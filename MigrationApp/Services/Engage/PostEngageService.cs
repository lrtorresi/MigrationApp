

using Microsoft.Extensions.Configuration;
using MigrationApp.Entities.Engage;
using MigrationApp.Entities.Global;
using MigrationApp.Entities.Workplace;
using MigrationApp.Services.Resources;
using MigrationApp.Services.Workplace;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Polly;
using System.Net;

namespace MigrationApp.Services.Engage
{
    public class PostEngageService
    {
        private readonly IConfiguration _configuration;
        private readonly PostWorkplaceService _postWorkplaceService;
        private readonly UserEngageService _userEngageService;
        private readonly UsersWorkplaceService _usersWorkplaceService;
        private readonly GenerateHtml _generateHtml;
        private readonly LogManager _logManager;

        public PostEngageService(IConfiguration configuration, PostWorkplaceService postWorkplaceService, UserEngageService userEngageService, UsersWorkplaceService usersWorkplaceService, GenerateHtml generateHtml, LogManager logManager)
        {
            _configuration = configuration;
            _postWorkplaceService = postWorkplaceService;
            _userEngageService = userEngageService;
            _usersWorkplaceService = usersWorkplaceService;
            _generateHtml = generateHtml;
            _logManager = logManager;
        }
        public async Task<bool> AddPostsToCommunity(string groupWorkplaceId, string communityEngageId, string communityEngageNetworkId, string graphCommunitEngageId, IProgress<string> progress = null)
        {
            _logManager.LogInformation($"AddPostsToCommunity - Adicionando post a comunidade. groupWorkplaceId: {groupWorkplaceId}, communityEngageId: {communityEngageId}, communityEngageNetworkId: {communityEngageNetworkId}");

            var token = "";
            var api = _configuration["Yammer:Api"];
            var clientId = GlobalConfig.ClientIdEngage;
            var count = 0;

            try
            {
                //Get Posts from Workplace By Group Id
                var postsWorkplace = await _postWorkplaceService.GetAllPostByCommunityId(groupWorkplaceId);
                if (postsWorkplace is null || postsWorkplace.Data == null) { return false; }

                postsWorkplace.Data.Reverse(); //orderna o objeto

                foreach (var post in postsWorkplace.Data)
                {
                   

                    try
                    {
                        var attachmentsFiles = new List<string>();
                        bool isQuestion = false;

                        //Get User Token by User Id (proprietario do post)
                        var userToken = await _userEngageService.Auth(post.From.Id);

                        //Verifica se tem o token do usuario
                        token = userToken ?? await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);

                        //Get Post - Complete Workplace (comentários, likes, etc)
                        var completePostWorkplace = await _postWorkplaceService.GetPostCompleteById(post.Id);

                        if (post.Message == null && completePostWorkplace.Attachments == null)
                        {
                            _logManager.LogError($"AddPostsToCommunity - Comunidade sem post. communityEngageId: {communityEngageId}. PostID: {post.Id}. PostType: {completePostWorkplace.Type}");
                            continue;
                        }

                        _logManager.LogInformation($"AddPostsToCommunity - Adicionando post a comunidade. communityEngageId: {communityEngageId} | Post: {count++}/{postsWorkplace.Data.Count}");
                        progress?.Report($"Adicionando Post: {count++}/{postsWorkplace.Data.Count}");

                        //Verifica se tem Anexos
                        var attachements = await _postWorkplaceService.GetAttachaments(post.Id);

                        //Realiza o upload dos arquivos
                        if (attachements?.Data?.Count > 0)
                        {
                            attachmentsFiles = await uploadFile(token, api, attachements, communityEngageId, communityEngageNetworkId, graphCommunitEngageId);
                            if (attachmentsFiles is null)
                                _logManager.LogError($"Erro ao adicionar Upload no Post (ID Workplace): {completePostWorkplace.Id}");
                        }


                        //Adicionando o Post na Comunidade
                        var client = new RestClient($"{api}/v1/messages.json");
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {token}");
                        request.AddParameter("body", post.Message ?? "_");
                        request.AddParameter("group_id", communityEngageId);

                        if (attachmentsFiles != null && attachmentsFiles.Count > 0)
                            attachmentsFiles.ForEach(attachment => request.AddParameter("attached_objects[]", $"uploaded_file:{attachment}"));

                        if (attachements?.Data.Count > 0 && attachements?.Data[0]?.Type == "question")
                        {
                            request.AddParameter("message_type", "poll");
                            attachements?.Data[0].Subattachments.Data.ForEach(async item =>
                            {
                                request.AddParameter("poll_options[]", item?.Title);
                            });

                            isQuestion = true;
                        };

                        var response = await client.PostAsync(request);


                        if (response.StatusCode != System.Net.HttpStatusCode.Created)
                        {
                            _logManager.LogError($"AddPostsToCommunity - Erro ao adicionar o Post: ID: {post.Id}");
                            continue;
                        }

                        var result = JsonConvert.DeserializeObject<PostEngage>(response.Content);

                        //Se tiver comentários no post, adiciona os comentários
                        if (completePostWorkplace.Comments != null)
                        {
                            //Adiciona os comentários
                            progress?.Report($"Adicionando comentários na postagem.");
                            await AddCommentsToPost(completePostWorkplace, result.Messages[0].Id);
                        }

                        //Se tiver likes no post, adiciona os likes
                        if (completePostWorkplace.Reactions != null)
                        {
                            //Adiciona os likes
                            progress?.Report($"Adicionando likes na postagem.");
                            await AddLikesToPost(completePostWorkplace, result.Messages[0].Id);
                        }

                        //Se for enquete, adiciona os votos
                        if (isQuestion)
                        {
                            //Adiciona os votos

                            await AddVotesToQuestion(completePostWorkplace, result.Messages[0].Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logManager.LogError($"AddPostsToCommunity - Adicionando dados da comunidade. Erro: {ex.Message}");
                        continue;
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostEngageService", Function = "AddPostsToCommunity", Error = e.Message, GroupWorkplaceId = groupWorkplaceId, CommunityEngageId = communityEngageId, CommunityEngageNetworkId = communityEngageNetworkId };
                _logManager.LogObject(logObject);
                await AddPostsToCommunity(groupWorkplaceId, communityEngageId, communityEngageNetworkId, graphCommunitEngageId);
                return false;
            }
        }

        private async Task<string> createPostEngage(List<string> attachmentsFiles, string userToken, string token, string communityEngageNetworkId, string graphCommunitEngageId)
        {
            var client = new RestClient($"https://engage.cloud.microsoft/graphql?operationName=CreateGroupUploadSessionClients&apiVnext=2");
            var request = new RestRequest();
            request.AddHeader("Authorization", $"Bearer {userToken ?? token}");
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(new
            {
                operationName = "CreateGroupUploadSessionClients",
                query = @"mutation CreateGroupUploadSessionClients($fileName: String!, $groupId: ID!, $networkId: ID!, $threadId: ID) { createGroupUploadSessionForNetwork(input: {filename: $fileName, groupId: $groupId, networkId: $networkId, threadId: $threadId}) { uploadSession { ...UploadSessionFields } } } mutation CreateNetworkQuestionUploadSessionClients($fileName: String!, $networkId: ID!, $threadId: ID) { createNetworkQuestionUploadSessionForNetwork(input: {filename: $fileName, networkId: $networkId, threadId: $threadId}) { uploadSession { ...UploadSessionFields } } } mutation CreateUserStoryUploadSessionClients($fileName: String!, $userId: ID!, $networkId: ID!, $threadId: ID, $onBehalfOfSenderId: ID) { createUserStoryUploadSessionForNetwork(input: {filename: $fileName, userId: $userId, networkId: $networkId, threadId: $threadId, onBehalfOfSenderId: $onBehalfOfSenderId}) { uploadSession { ...UploadSessionFields } } } mutation CreateDirectMessageUploadSessionClients($fileName: String!, $networkId: ID!, $threadId: ID) { createDirectMessageUploadSessionForNetwork(input: {filename: $fileName, networkId: $networkId, threadId: $threadId}) { uploadSession { ...UploadSessionFields } } } mutation CreateUserMomentUploadSessionClients($fileName: String!, $userId: ID!, $networkId: ID!, $threadId: ID, $onBehalfOfSenderId: ID) { createUserMomentUploadSessionForNetwork(input: {filename: $fileName, userId: $userId, networkId: $networkId, threadId: $threadId, onBehalfOfSenderId: $onBehalfOfSenderId}) { uploadSession { ...UploadSessionFields } } } fragment AzureUploadSessionFields on AzureUploadSession { sessionId fileId fileVersionId url sasValidator sasTokenExpirationTime } fragment SharePointUploadSessionFields on SharePointUploadSession { sessionId fileId fileVersionId url } fragment UploadSessionFields on UploadSession { __typename ...AzureUploadSessionFields ...SharePointUploadSessionFields }",
                variables = new
                {
                    fileName = "video.mp4",
                    groupId = graphCommunitEngageId,
                    networkId = communityEngageNetworkId
                }
            });

            var response = await client.PostAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;


            var resultUploadSession = JsonConvert.DeserializeObject<GraphQlUploadFile>(response.Content);
            return "null";
        }

        private async Task AddVotesToQuestion(PostComplete completePostWorkplace, string? id)
        {
            try
            {
                _logManager.LogInformation($"AddVotesToQuestion - Adicionando Votos na Enquete.");
                //var token = GlobalConfig.TokenEngage;
                var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];
                var clientId = GlobalConfig.ClientIdEngage;

                //Pega os Votos da Enquete
                var poll = await _postWorkplaceService.GetVotesByPostId(completePostWorkplace.Id);

                if (poll != null && poll.Poll.Options.Data.Count > 0)
                {
                    for (int i = 0; i < poll.Poll.Options.Data.Count; i++)
                    {
                        if (poll.Poll.Options.Data[i].Votes == null) { continue; }

                        //Pega o Token do Usuário que votou
                        foreach (var user in poll.Poll.Options.Data[i].Votes.Data)
                        {
                            var userToken = await _userEngageService.Auth(user.Id);

                            //Adiciona o Voto no Engage
                            var client = new RestClient($"{api}/v2/votes");
                            var request = new RestRequest();
                            request.AddHeader("Authorization", $"Bearer {userToken ?? token}");
                            request.AddHeader("Content-type", "application/json");
                            request.AddJsonBody(new
                            {
                                option = i,
                                poll_id = id
                            });
                            await client.PostAsync(request);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostEngageService", Function = "AddVotesToQuestion", Error = e.Message, CompletePostWorkplace = completePostWorkplace, ID = id };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        private async Task AddLikesToPost(PostComplete postComplete, string? postIdEngage)
        {
            try
            {
                _logManager.LogInformation($"AddLikesToPost - Adicionando Likes no Post.");
                //var token = GlobalConfig.TokenEngage;
                var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];
                var clientId = GlobalConfig.ClientIdEngage;

                //Se tiver 25 likes, verifica se exite mais likes
                if (postComplete.Reactions.Data.Count == 25)
                {
                    var reactions = await _postWorkplaceService.GetReactionsByPostId(postComplete.Id);
                    postComplete.Reactions.Data = reactions.Data;
                }

                foreach (var like in postComplete.Reactions.Data)
                {
                    try
                    {
                        //Pega o Token do Usuário que fez o like
                        var userToken = await _userEngageService.Auth(like.Id);

                        //Adiciona o Like
                        var client = new RestClient($"{api}/v1/messages/liked_by/current.json?message_id={postIdEngage}");
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {userToken ?? token}");
                        var response = await client.PostAsync(request);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                   
                }
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostEngageService", Function = "AddLikesToPost", Error = e.Message, PostComplete = postComplete, PostIdEngage = postIdEngage };
                _logManager.LogObject(logObject);
                throw;
            }

        }

        private async Task AddCommentsToPost(PostComplete postComplete, string? postIdEngage)
        {
            try
            {
                _logManager.LogInformation($"AddCommentsToPost - Adicionando comentarios no Post.");

                //var token = GlobalConfig.TokenEngage;
                var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];
                var clientId = GlobalConfig.ClientIdEngage;

                //Se tiver 25 comentarios, verifica se exite mais comentarios
                if (postComplete.Comments.Data.Count == 25)
                {
                    var comments = await _postWorkplaceService.GetCommentsByPostId(postComplete.Id);
                    postComplete.Comments.Data = comments.Data;
                }

                foreach (var comment in postComplete.Comments.Data)
                {
                    try
                    {
                        //Pega o Token do Usuário que fez o comentário
                        var userToken = await _userEngageService.Auth(comment?.From?.Id ?? comment?.Id);
                        if (comment.Message == "") { comment.Message = "-"; };

                        //Adiciona o comentário
                        var client = new RestClient($"{api}/v1/messages.json");
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {userToken ?? token}");
                        request.AddParameter("body", comment.Message);
                        request.AddParameter("replied_to_id", postIdEngage);
                        var response = await client.PostAsync(request);
                    }
                    catch (Exception)
                    {
                        _logManager.LogError("AddCommentsToPost - Erro ao adicionar o comentário.");
                        continue;
                    }

                }
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostEngageService", Function = "AddCommentsToPost", Error = e.Message, PostComplete = postComplete, PostIdEngage = postIdEngage };
                _logManager.LogObject(logObject);
                return;
            }

        }

        private async Task<List<string>> uploadFile(string? token, string? api, AttachmentsFiles files, string communityEngageId, string communityEngageNetworkId, string graphCommunitEngageId)
        {
            _logManager.LogInformation($"uploadFile - Iniciando o Upload de arquivos");
            _logManager.LogInformation($"uploadFile - ComunidadeID: {communityEngageId}, Total de arquivos: {files.Data.Count} ");

            try
            {
                var listUrlFiles = new List<string>();
                RestResponse file = new RestResponse();
                var image = files?.Data?.Where(x => x.Type == "photo" || x.Type == "video_inline" || x.Type == "share" | x.Type ==  "animated_image_video").ToList();
                var archives = files?.Data[0]?.Subattachments?.Data?.ToList();


                token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);

                // Definir política de retry com Polly
                var retryPolicy = Policy.Handle<IOException>()
                    .Or<SocketException>()
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                    {
                        _logManager.LogError($"Tentativa {retryCount} falhou. Erro: {exception.Message}");
                        await Task.CompletedTask;
                    });


                if (image.Count > 0)
                {
                    foreach (var item in image)
                    {
                        try
                        {
                            if (item?.Media?.Image?.Src == null)
                                continue;

                            //Get File (URL to File)
                            await retryPolicy.ExecuteAsync(async () =>
                            {
                                var client_img = new RestClient(item.Type.Equals("photo") || item.Type.Equals("share") ? item.Media.Image.Src : item.Media.source);
                                var request_img = new RestRequest();
                                file = await client_img.ExecuteAsync(request_img);
                            });

                            if (file.StatusCode != System.Net.HttpStatusCode.OK)
                                continue;

                            //upload Large File
                            if (file.RawBytes.Length > 5 * 1024 * 1024) // 5 MB em bytes  
                            {
                                listUrlFiles = await uploadLargeFile(token, file, communityEngageNetworkId, communityEngageId, graphCommunitEngageId);
                                return listUrlFiles;
                            }

                            //upload Small File
                            else
                            {
                                var client = new RestClient($"https://filesng.yammer.com/v4/uploadSmallFile/network/{communityEngageNetworkId}/group/{communityEngageId}?thirdpartycookiefix=true");
                                var request = new RestRequest();
                                request.AddHeader("Authorization", $"Bearer {token}");
                                request.AddFile("file", file.RawBytes, "image.png");
                                request.AddParameter("filename", "image.png");

                                var response = await client.ExecuteAsync(request, Method.Post);
                                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                                    continue;

                                var result = JsonConvert.DeserializeObject<UploadFile>(response.Content);

                                listUrlFiles.Add(result.Id);
                            }
                        }
                        catch (Exception)
                        {
                            _logManager.LogError($"uploadFile - image.Count > 0 - Erro ao realizar o upload do arquivo. ComunidadeID: {communityEngageId} ");
                            continue;
                        }

                    }
                }
                else
                {
                    if (archives == null) { return null; }
                    foreach (var item in archives)
                    {
                        if (item.Type == "option") { continue; }
                        //Get File (URL to File)
                        var client_img = item.Type == "photo" ? new RestClient(item.Media.Image.Src) : new RestClient(item.Url);


                        //Tratamento para Videos
                        if (item.Type == "video")
                        {
                            client_img = new RestClient(item?.Media?.source) ?? client_img;
                            item.Title = $"{item.Target.Id}.mp4";
                        }

                        //Get File (URL to File)
                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            var request_img = new RestRequest();
                            file = await client_img.ExecuteAsync(request_img);
                        });

                        if (file.StatusCode != System.Net.HttpStatusCode.OK)
                            continue;

                        //upload Large File
                        if (file.RawBytes.Length > 5 * 1024 * 1024) // 5 MB em bytes  
                        {
                            listUrlFiles = await uploadLargeFile(token, file, communityEngageNetworkId, communityEngageId, graphCommunitEngageId);
                            return listUrlFiles;
                        }

                        var client = new RestClient($"https://filesng.yammer.com/v4/uploadSmallFile/network/{communityEngageNetworkId}/group/{communityEngageId}?thirdpartycookiefix=true");
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {token}");
                        request.AddFile("file", file.RawBytes, "file");
                        request.AddParameter("filename", item.Title ?? "image.png");

                        var response = await client.ExecuteAsync(request, Method.Post);
                        if (response.StatusCode != System.Net.HttpStatusCode.Created)
                            continue;

                        var result = JsonConvert.DeserializeObject<UploadFile>(response.Content);

                        listUrlFiles.Add(result.Id);
                    }

                }

                _logManager.LogInformation($"uploadFile - Upload concluído");
                return listUrlFiles;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostEngageService", Function = "uploadFile", Error = e.Message, AttachmentsFiles = files, CommunityEngageId = communityEngageId, CommunityEngageNetworkId = communityEngageNetworkId };
                _logManager.LogObject(logObject);
                return null;
            }

        }

        private async Task<List<string>> uploadLargeFile(string token, RestResponse file, string communityEngageNetworkId, string communityEngageId, string graphCommunitEngageId)
        {
            var api = _configuration["Yammer:Api"];
            var domain = GlobalConfig.Domain;

            var listUrlFiles = new List<string>();
            int fragSize = file.ContentLength > 21122856 ? 1024 * 1024 * 4 : 1024 * 1024 * 21;
            var arrayBatches = ByteArrayIntoBatches(file.RawBytes, fragSize).ToList();
            int start = 0;

            Random random = new Random();
            int randomNumber = random.Next(10000, 100000);
            string fileName = file.ContentType == "application/pdf" ? $"file{randomNumber}.pdf" : $"video{randomNumber}.mp4";

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (ex, ts, count, ctx) =>
                {
                    _logManager.LogWarning($"uploadLargeFile - Tentativa {count} falhou: {ex.Message}");
                });

            try
            {
                _logManager.LogInformation($"uploadLargeFile - Iniciando upload");

                
                var result = await retryPolicy.ExecuteAsync(async () =>
                {
                    var client = new RestClient($"https://filesng.yammer.com/v3/createUploadSession");
                    var request = new RestRequest();
                    request.AddHeader("Authorization", $"Bearer {token}");
                    request.AddHeader("Content-type", "application/json");
                    request.AddJsonBody(new
                    {
                        filename = fileName,
                        is_all_company = "false",
                        group_id = communityEngageId,
                        network_id = communityEngageNetworkId
                    });

                    var response = await client.PostAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new Exception("Falha ao iniciar a sessão de upload");

                    _logManager.LogInformation($"uploadLargeFile - Session criada com sucesso.");
                    return JsonConvert.DeserializeObject<UploadLargeFiles>(response.Content);
                });

                // 🔁 Upload de chunks com retry
                using (var client2 = new HttpClient())
                {
                    client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    foreach (var byteArray in arrayBatches)
                    {
                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            int byteArrayLength = byteArray.Length;
                            var contentRange = $"bytes {start}-{start + byteArrayLength - 1}/{file.RawBytes.Length}";

                            var content = new ByteArrayContent(byteArray);
                            content.Headers.Add("Content-Range", contentRange);
                            content.Headers.Add("x-ms-blob-type", "BlockBlob");
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                            var responsePartial = await client2.PutAsync(result.url, content);
                            if (responsePartial.StatusCode != HttpStatusCode.Created && responsePartial.StatusCode != HttpStatusCode.Accepted)
                                throw new Exception("Falha ao enviar chunk");

                            var responseContent = await responsePartial.Content.ReadAsStringAsync();
                            var responseContentObject = JsonConvert.DeserializeObject<CompleteUploadFile>(responseContent);
                            result.sharepoint_id = responseContentObject.Id;

                            start += byteArrayLength;
                        });
                    }
                }

                _logManager.LogInformation($"uploadLargeFile - Chunks adicionados com sucesso.");

                // 🔁 Finalização do upload com retry
                var response3 = await retryPolicy.ExecuteAsync(async () =>
                {
                    var client3 = new RestClient($"https://filesng.yammer.com/v3/completeThreadAttachmentUploadSession");
                    var request3 = new RestRequest();
                    request3.AddHeader("Authorization", $"Bearer {token}");
                    request3.AddHeader("Content-type", "application/json");
                    request3.AddJsonBody(new
                    {
                        is_new_file = true,
                        filename = fileName,
                        group_id = communityEngageId,
                        network_id = communityEngageNetworkId,
                        uploaded_file_id = result.uploaded_file_id,
                        uploaded_file_version_id = result.uploaded_file_version_id,
                        sharepoint_id = result.sharepoint_id,
                        is_all_company = false
                    });

                    var response = await client3.ExecuteAsync(request3, Method.Post);
                    if (response.StatusCode != System.Net.HttpStatusCode.Created)
                        throw new Exception("Falha ao concluir o upload");

                    return response;
                });

                var result2 = JsonConvert.DeserializeObject<UploadLargeFiles>(response3.Content);
                listUrlFiles.Add(result2.Id);

                _logManager.LogInformation($"uploadLargeFile - Upload concluído");
                return listUrlFiles;
            }
            catch (Exception ex)
            {
                _logManager.LogError($"uploadLargeFile - Erro: {ex.Message}. URL-File: {file.ResponseUri}");
                return null;
            }
        }


        private async Task<string> CreateSessionUploadFile(string communityEngageId, string communityEngageNetworkId, List<byte[]> arrayBatches, RestResponse file, string graphCommunitEngageId)
        {
            var api = _configuration["Yammer:Api"];
            var domain = GlobalConfig.Domain;
            var listUrlFiles = new List<string>();
            var uploadSessionId = new UploadLargeFiles();

            try
            {
                //Gera um token da conta do Admin do Engage
                var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);

                #region Create Upload Session
                var client = new RestClient($"https://engage.cloud.microsoft/graphql?operationName=CreateGroupUploadSessionClients&apiVnext=2");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("Content-type", "application/json");
                request.AddJsonBody(new
                {
                    operationName = "CreateGroupUploadSessionClients",
                    query = @"mutation CreateGroupUploadSessionClients($fileName: String!, $groupId: ID!, $networkId: ID!, $threadId: ID) { createGroupUploadSessionForNetwork(input: {filename: $fileName, groupId: $groupId, networkId: $networkId, threadId: $threadId}) { uploadSession { ...UploadSessionFields } } } mutation CreateNetworkQuestionUploadSessionClients($fileName: String!, $networkId: ID!, $threadId: ID) { createNetworkQuestionUploadSessionForNetwork(input: {filename: $fileName, networkId: $networkId, threadId: $threadId}) { uploadSession { ...UploadSessionFields } } } mutation CreateUserStoryUploadSessionClients($fileName: String!, $userId: ID!, $networkId: ID!, $threadId: ID, $onBehalfOfSenderId: ID) { createUserStoryUploadSessionForNetwork(input: {filename: $fileName, userId: $userId, networkId: $networkId, threadId: $threadId, onBehalfOfSenderId: $onBehalfOfSenderId}) { uploadSession { ...UploadSessionFields } } } mutation CreateDirectMessageUploadSessionClients($fileName: String!, $networkId: ID!, $threadId: ID) { createDirectMessageUploadSessionForNetwork(input: {filename: $fileName, networkId: $networkId, threadId: $threadId}) { uploadSession { ...UploadSessionFields } } } mutation CreateUserMomentUploadSessionClients($fileName: String!, $userId: ID!, $networkId: ID!, $threadId: ID, $onBehalfOfSenderId: ID) { createUserMomentUploadSessionForNetwork(input: {filename: $fileName, userId: $userId, networkId: $networkId, threadId: $threadId, onBehalfOfSenderId: $onBehalfOfSenderId}) { uploadSession { ...UploadSessionFields } } } fragment AzureUploadSessionFields on AzureUploadSession { sessionId fileId fileVersionId url sasValidator sasTokenExpirationTime } fragment SharePointUploadSessionFields on SharePointUploadSession { sessionId fileId fileVersionId url } fragment UploadSessionFields on UploadSession { __typename ...AzureUploadSessionFields ...SharePointUploadSessionFields }",
                    persistedQuery = new
                    {
                        version = 1,
                        sha256Hash = "f3bb6669b056ded93ee46898e6c4db4f874ffbd40b7b72ee32e89c94d72b94a7"
                    },
                    #region extension
                    extensions = new
                    {
                        yammerTreatments = new
                        {
                            version = "1",
                            treatments = new[]
                            {
                                new { project = "ClientForward", key = "BackendGraphQLApiVNextRolloutProd25", value = "true" },
                                new { project = "ClientForward", key = "BackendGraphQLApiVNextRolloutProd50", value = "false" },
                                new { project = "ClientForward", key = "BackendGraphQLApiVNextRolloutProd100", value = "true" },
                                new { project = "ClientForward", key = "BackendStreamieCosmos", value = "true" },
                                new { project = "ClientForward", key = "CampaignSashAvatar", value = "true" },
                                new { project = "ClientForward", key = "DraftSorting", value = "true" },
                                new { project = "ClientForward", key = "GroupsFromGrouper", value = "true" },
                                new { project = "ClientForward", key = "MTOBadge", value = "true" },
                                new { project = "ClientForward", key = "MTOBlendedInbox", value = "true" },
                                new { project = "ClientForward", key = "MTOGraphVNextResolvers", value = "true" },
                                new { project = "ClientForward", key = "MultiTenantOrganization", value = "true" },
                                new { project = "ClientForward", key = "OauthTokenExpires", value = "true" },
                                new { project = "ClientForward", key = "ThirdPartyCookieFix", value = "true" },
                                new { project = "ClientForward", key = "UserGroupsFromGrouper", value = "enabled" },
                                new { project = "ClientForward", key = "VivaEngageCopilotBE", value = "enabled" },
                                new { project = "ClientForward", key = "VivaEngageCopilotWAGpt4", value = "enabled" },
                                new { project = "ClientForward", key = "NoWFMessageBodyInGroupMessageCreationResponse", value = "enabled" },
                                new { project = "CrossPlatform", key = "ExpandedReactionsEnabled", value = "true" },
                                new { project = "CrossPlatform", key = "DomainMigration", value = "true" },
                                new { project = "CrossPlatform", key = "StorylineMediaPostGA", value = "true" }
                            }
                        }
                    },
                    #endregion
                    variables = new
                    {
                        fileName = "video.mp4",
                        groupId = graphCommunitEngageId,
                        networkId = communityEngageNetworkId
                    }
                });

                var response = await client.PostAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;


                var resultUploadSession = JsonConvert.DeserializeObject<GraphQlUploadFile>(response.Content);
                #endregion

                #region Upload Chunks
                int start = 0;
                using (var client2 = new HttpClient())
                {
                    foreach (var byteArray in arrayBatches)
                    {
                        int byteArrayLength = byteArray.Length;
                        var contentRange = " bytes " + start + "-" + (start + (byteArrayLength - 1)) + "/" + file.RawBytes.Length;

                        var content = new ByteArrayContent(byteArray);
                        content.Headers.Add("Content-Range", contentRange);
                        content.Headers.Add("x-ms-blob-type", "BlockBlob");
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        content.Headers.ContentLength = byteArrayLength;
                        var url = resultUploadSession.data.createGroupUploadSessionForNetwork.uploadSession.url;
                        var responsePartial = await client2.PutAsync(url, content);
                        if (responsePartial.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            continue;
                        }

                        var responseContent = await responsePartial.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseContent))
                            uploadSessionId = JsonConvert.DeserializeObject<UploadLargeFiles>(responseContent);

                        start = start + byteArrayLength;
                    }
                }

                #endregion

                #region Complete Upload Session

                //Quando finaliza o upload, pega o ID do Upload.

                //regra para pegar o FileID
                string[] parts = resultUploadSession.data.createGroupUploadSessionForNetwork.uploadSession.url.Split(new string[] { "items/" }, StringSplitOptions.None);
                string[] fileId = parts[1].Split(new string[] { "/uploadSession" }, StringSplitOptions.None);

                var client3 = new RestClient($"https://engage.cloud.microsoft/graphql?operationName=CompleteSharePointUploadSessionClients&apiVnext=2");
                var request3 = new RestRequest();
                request3.AddHeader("Authorization", $"Bearer {token}");
                request3.AddHeader("Content-type", "application/json");
                request3.AddJsonBody(new
                {
                    operationName = "CompleteSharePointUploadSessionClients",
                    query = "mutation CompleteSharePointUploadSessionClients($sessionId: String!, $sharePointFileId: String!) { completeSharePointUploadSession(input: {sessionId: $sessionId, sharePointFileId: $sharePointFileId}) { file { __typename ...VideoFileFields ...ImageFileFields ...FileFields } } } fragment FileFields on File { __typename id displayName fileDescription: description fullPageEditorUrl mimeType downloadLink previewImage embeddablePreviewUrl databaseId state group { ...GroupFields } } fragment ImageFileFields on ImageFile { __typename id displayName fileDescription: description downloadLink width height smallImage mediumImage largeImage databaseId state storageProvider } fragment VideoFileFields on VideoFile { __typename id displayName fileDescription: description downloadLink previewImage databaseId state width height group { ...GroupFields } azureVideoSource { streamUrlProvider transcodingStatus } sharePointVideoSource { embeddablePreviewUrl streamUrlProvider } } fragment GroupFields on Group { databaseId telemetryId id officeUnifiedGroupId network { id } ...GroupAvatarFields displayName description isExternal isOfficial guestsCount threadStarterSmallFileUploadUrl isNetworkQuestionGroup isMultiTenantOrganizationGroup isMoveThreadToThisGroupRestricted } fragment GroupAvatarFields on Group { avatarUrlTemplate hasDefaultAvatar }",
                    variables = new
                    {
                        sessionId = resultUploadSession.data.createGroupUploadSessionForNetwork.uploadSession.sessionId,
                        sharePointFileId = fileId[0]
                    }
                });
                var response3 = await client3.PostAsync(request3);
                if (response3.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result2 = JsonConvert.DeserializeObject<UploadLargeFileEngage>(response3.Content);

                return null;
                #endregion
            }
            catch (Exception)
            {
                return null;
            }



        }

        private static IEnumerable<byte[]> ByteArrayIntoBatches(byte[] bArray, int intBufforLengt)
        {
            int bArrayLenght = bArray.Length;
            byte[] bReturn = null;

            int i = 0;
            for (; bArrayLenght > (i + 1) * intBufforLengt; i++)
            {
                bReturn = new byte[intBufforLengt];
                Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLengt);
                yield return bReturn;
            }

            int intBufforLeft = bArrayLenght - i * intBufforLengt;
            if (intBufforLeft > 0)
            {
                bReturn = new byte[intBufforLeft];
                Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLeft);
                yield return bReturn;
            }
        }
    }
}
