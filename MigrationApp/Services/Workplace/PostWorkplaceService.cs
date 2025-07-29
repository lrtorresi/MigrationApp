using Microsoft.Extensions.Configuration;
using MigrationApp.Entities.Global;
using MigrationApp.Entities.Workplace;
using MigrationApp.Services.Resources;
using Newtonsoft.Json;
using Polly;
using PuppeteerSharp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MigrationApp.Services.Workplace
{
    public class PostWorkplaceService
    {
        private readonly IConfiguration _configuration;
        private readonly GroupWorkplaceService _groupService;
        private readonly LogManager _logManager;

        public PostWorkplaceService(IConfiguration configuration, GroupWorkplaceService groupService, LogManager logManager)
        {
            _configuration = configuration;
            _groupService = groupService;
            _logManager = logManager;
        }

        public async Task<int> GetTotalPost()
        {
            try
            {
                _logManager.LogInformation("GetTotalPost - Iniciando a busca pelo total de Post.");
                var getGroups = await _groupService.GetAll();
                int totalPosts = 0;

                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];

                var fields = "id,admin_creator,actions,application,caption,created_time,description,feed_targeting,from,icon,is_hidden,link,message,message_tags,name,object_id,parent_id,permalink_url,picture,place,properties,shares,source,story,full_picture,expanded_height,comments";

                if (getGroups == null)
                    return 0;

                foreach (var getGroup in getGroups.Data)
                {
                    var client = new RestClient($"{api}/{getGroup.Id}/feed?fields={fields}");
                    var request = new RestRequest();
                    request.AddHeader("Authorization", $"Bearer {token}");
                    var response = await client.ExecuteAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        continue;

                    Root root = JsonConvert.DeserializeObject<Root>(response.Content);
                    totalPosts += root.Data.Count;
                }

                _logManager.LogInformation("GetTotalPost - Finalizado a busca pelo total de Post.");
                return totalPosts;

            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetTotalPost", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<Root?> GetAllPostByCommunityId(string communityId)
        {
            _logManager.LogInformation("GetAllPostByCommunityId - Iniciando a busca de Post pelo ID da Comunidade.");
            var token = GlobalConfig.TokenWorkplace;
            var api = _configuration["Workplace:Api"];
            //var fields = "id,admin_creator,application,caption,created_time,description,feed_targeting,from,icon,is_hidden,link,message,message_tags,name,object_id,parent_id,permalink_url,picture,place,privacy,properties,shares,source";
            var fields = "id, message, from";

            string nextPageUrl = $"{api}/{communityId}/feed?fields={fields}";

            // Definir política de retry com Polly
            var retryPolicy = Policy
                    .Handle<IOException>()
                    .Or<SocketException>()
                    .Or<HttpRequestException>()
                    .OrResult<RestResponse>(r =>
                    {
                        // retry se HTTP ≠ 200 e não for subcode 99
                        if (r.StatusCode != HttpStatusCode.OK &&
                           !(r.Content?.Contains("\"error_subcode\": 99") ?? false))
                            return true;
                        return false;
                    })
                    .WaitAndRetryAsync(
                        retryCount: 5,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetryAsync: async (outcome, timespan, retryCount, context) =>
                        {
                            // se foi um 413 Request Entity Too Large (payload grande)
                            if (outcome.Result?.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                if (retryCount == 5)
                                {
                                    _logManager.LogError($"Não foi possivel continuar a busca por Posts.");
                                    nextPageUrl = null;
                                }
                                _logManager.LogWarning($"Payload muito grande detectado. Ajustando ‘until’ em 4 meses para URL atual.");
                                nextPageUrl = AjustarUntil(nextPageUrl, -4);
                                _logManager.LogInformation($"Nova URL: {nextPageUrl}");
                            }

                            // seu log de erros existente
                            if (outcome.Exception != null)
                            {
                                _logManager.LogError($"Tentativa {retryCount} falhou (exceção): {outcome.Exception.Message}");
                            }
                            else if (outcome.Result != null)
                            {
                                _logManager.LogError($"Tentativa {retryCount} falhou. StatusCode: {outcome.Result.StatusCode}, Conteúdo: {outcome.Result.Content}");
                            }
                            await Task.CompletedTask;
                        }
                    );


            try
            {
                var allPosts = new List<Post>(); // Lista para acumular todos os posts

                do
                {
                    // Executar a política de retry
                    var result = await retryPolicy.ExecuteAsync(async () =>
                    {
                        var client = new RestClient(nextPageUrl);
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {token}");
                        var response = await client.ExecuteAsync(request);

                        return response;
                    });

                    if (!string.IsNullOrEmpty(result.Content) && result.Content.Contains("\"error_subcode\": 99"))
                    {
                        _logManager.LogError($"Erro crítico (subcode 99) detectado na API do Workplace. Finalizando paginação. URL: {nextPageUrl}");
                        break;
                    }

                    if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logManager.LogError($"GetAllPostByCommunityId - Falha ao buscar posts na comunidade. ID: {communityId}. Erro: {result?.Content}");
                        continue;
                    }

                    var obj = JsonConvert.DeserializeObject<Root>(result.Content);
                    if (obj == null)
                    {
                        _logManager.LogError($"GetAllPostByCommunityId - Erro ao desserializar a resposta. ID: {communityId}");
                        continue;
                    }

                    // Adicionar os posts da página atual à lista total
                    if (obj.Data != null && obj.Data.Count > 0)
                    {
                        allPosts.AddRange(obj.Data);
                    }

                    // Verificar se há próxima página
                    if (obj.Paging != null && !string.IsNullOrEmpty(obj.Paging.Next))
                    {
                        nextPageUrl = obj.Paging.Next;
                    }
                    else
                    {
                        nextPageUrl = null;
                    }

                } while (!string.IsNullOrEmpty(nextPageUrl));

                _logManager.LogInformation($"GetAllPostByCommunityId - Finalizado a busca de Posts. Total de posts encontrados: {allPosts.Count}");

                // Criar o objeto final para retornar
                var finalResult = new Root
                {
                    Data = allPosts
                };

                return finalResult;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetAllPostByCommunityId", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<PostComplete?> GetPostCompleteById(string postId)
        {
            try
            {
                _logManager.LogInformation("GetAllPostByCommunityId - Iniciando a busca do Post completo.");
                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];
                var fields = "id,message,full_picture,picture,attachments{media,subattachments,media_type,type,url},created_time,updated_time,from,story,link,name,description,caption,source,status_type,type,object_id,shares,reactions,comments,is_published,permalink_url";

                var client = new RestClient($"{api}/{postId}?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<PostComplete>(response.Content);

                _logManager.LogInformation("GetAllPostByCommunityId - Finalizado a busca do Post completo.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetPostCompleteById", Error = e.Message };
                _logManager.LogObject(logObject);
                return null;
            }
        }

        public async Task<AttachmentsFiles?> GetAttachaments(string postId)
        {
            try
            {
                _logManager.LogInformation("GetAttachaments - Iniciando a busca de anexos no Post.");
                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];

                var client = new RestClient($"{api}/{postId}/attachments");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<AttachmentsFiles>(response.Content);

                _logManager.LogInformation("GetAttachaments - Finalizado a busca de anexos no Post.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetAttachaments", Error = e.Message };
                _logManager.LogObject(logObject);
                return null;
            }
        }

        public async Task<Vote?> GetVotesByPostId(string postId)
        {
            try
            {
                _logManager.LogInformation("GetVotesByPostId - Iniciando a busca de de votos por Enquete.");
                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];
                var fields = "message,poll{options{name,vote_count,votes}}";

                var client = new RestClient($"{api}/{postId}?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<Vote>(response.Content);

                _logManager.LogInformation("GetVotesByPostId - Finalizado a busca de de votos por Enquete.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetVotesByPostId", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<Comments> GetCommentsByPostId(string postId)
        {
            try
            {
                _logManager.LogInformation("GetCommentsByPostId - Iniciando a busca de comentários por Post ID.");
                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];
                var fields = "message,poll{options{name,vote_count,votes}}";

                var retryPolicy = Policy.Handle<IOException>()
                    .Or<SocketException>()
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                    {
                        _logManager.LogError($"Tentativa {retryCount} falhou. Erro: {exception.Message}");
                        await Task.CompletedTask;
                    });

                var allComments = new List<Comment>(); // Lista para acumular todos os comentários
                string nextPageUrl = $"{api}/{postId}/comments?fields={fields}";

                do
                {
                    var result = await retryPolicy.ExecuteAsync(async () =>
                    {
                        var client = new RestClient(nextPageUrl);
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {token}");

                        var response = await client.ExecuteAsync(request);

                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            _logManager.LogError($"Erro na requisição: {response.StatusCode} - {response.Content}");
                            return null;
                        }


                        return response;
                    });

                    var obj = JsonConvert.DeserializeObject<Comments>(result.Content);

                    if (result == null)
                    {
                        _logManager.LogError("GetCommentsByPostId - Falha ao buscar comentários.");
                        continue;
                    }

                    // Adicionar os comentários da página atual à lista total
                    if (obj.Data != null && obj.Data.Count > 0)
                    {
                        allComments.AddRange(obj.Data);
                    }

                    // Verificar se há próxima página
                    if (obj.Paging != null && !string.IsNullOrEmpty(obj.Paging.Next))
                    {
                        nextPageUrl = obj.Paging.Next;
                    }
                    else
                    {
                        nextPageUrl = null;
                    }

                } while (!string.IsNullOrEmpty(nextPageUrl));

                _logManager.LogInformation("GetCommentsByPostId - Finalizado a busca de comentários.");

                // Criar o objeto Comments para retornar
                var finalResult = new Comments
                {
                    Data = allComments
                };

                return finalResult;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetCommentsByPostId", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<Reactions> GetReactionsByPostId(string postId)
        {
            try
            {
                _logManager.LogInformation("GetReactionsByPostId - Iniciando a busca de likes.");
                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];


                var retryPolicy = Policy.Handle<IOException>()
                    .Or<SocketException>()
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                    {
                        _logManager.LogError($"Tentativa {retryCount} falhou. Erro: {exception.Message}");
                        await Task.CompletedTask;
                    });

                var allLikes = new List<ReactionData>(); // Lista para acumular todos os comentários
                string nextPageUrl = $"{api}/{postId}/reactions";

                do
                {
                    var result = await retryPolicy.ExecuteAsync(async () =>
                    {
                        var client = new RestClient(nextPageUrl);
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {token}");

                        var response = await client.ExecuteAsync(request);

                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            _logManager.LogError($"Erro na requisição: {response.StatusCode} - {response.Content}");
                            return null;
                        }


                        return response;
                    });

                    var obj = JsonConvert.DeserializeObject<Reactions>(result.Content);

                    if (result == null)
                    {
                        _logManager.LogError("GetReactionsByPostId - Falha ao buscar likes.");
                        continue;
                    }

                    // Adicionar os comentários da página atual à lista total
                    if (obj.Data != null && obj.Data.Count > 0)
                    {
                        allLikes.AddRange(obj.Data);
                    }

                    // Verificar se há próxima página
                    if (obj.Paging != null && !string.IsNullOrEmpty(obj.Paging.Next))
                    {
                        nextPageUrl = obj.Paging.Next;
                    }
                    else
                    {
                        nextPageUrl = null;
                    }

                } while (!string.IsNullOrEmpty(nextPageUrl));

                _logManager.LogInformation("GetReactionsByPostId - Finalizado a busca de likes.");

                // Criar o objeto Comments para retornar
                var finalResult = new Reactions
                {
                    Data = allLikes
                };

                return finalResult;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetCommentsByPostId", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        private string AjustarUntil(string url, int offsetMonths)
        {
            // Regex que captura "until=1234567890"
            var m = Regex.Match(url, @"(?<=until=)(\d+)");
            if (!m.Success)
                return url; // se não achar, retorna sem alterações

            // converte timestamp existente
            var ts = long.Parse(m.Value);
            var novoTs = DateTimeOffset
                               .FromUnixTimeSeconds(ts)
                               .AddMonths(offsetMonths)
                               .ToUnixTimeSeconds();

            // substitui apenas o número, mantendo o resto da URL intacto
            return Regex.Replace(url,
                                 @"(?<=until=)\d+",
                                 novoTs.ToString());
        }
    }
}
