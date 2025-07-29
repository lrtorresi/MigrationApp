using EnvironmentValidator.Domain.Entities.Workplace;
using EnvironmentValidator.Service.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace EnvironmentValidator.Service.Workplace
{
    public class PostWorkplaceService
    {
        private readonly IConfiguration _configuration;

        private readonly LogManager _logManager;

        public PostWorkplaceService(IConfiguration configuration, LogManager logManager)
        {
            _configuration = configuration;

            _logManager = logManager;
        }

        //public async Task<object> GetTotalPost(string token)
        //{
        //    try
        //    {
        //        _logManager.LogInformation("GetTotalPost - Iniciando a busca pelo total de Post.");
        //        var getGroups = await _groupService.GetAll(token);
        //        int totalPosts = 0;

        //        var api = _configuration["Workplace:Api"];

        //        if (getGroups == null)
        //            return 0;

        //        foreach (var getGroup in getGroups.Data)
        //        {
        //            var posts = await GetAllPostByCommunityId(getGroup.Id, token);
        //            totalPosts += posts.Data.Count;
        //        }

        //        _logManager.LogInformation("GetTotalPost - Finalizado a busca pelo total de Post.");

        //        return totalPosts;
        //    }
        //    catch (Exception e)
        //    {
        //        var logObject = new { Class = "PostWorkplaceService", Function = "GetTotalPost", Error = e.Message };
        //        _logManager.LogObject(logObject);
        //        throw;
        //    }
        //}

        public async Task<Root?> GetAllPostByCommunityId(string communityId, string token)
        {
            _logManager.LogInformation("GetAllPostByCommunityId - Iniciando a busca de Post pelo ID da Comunidade.");

            var api = _configuration["Workplace:Api"];
            var fields = "id,admin_creator,actions,application,caption,created_time,description,feed_targeting,from,icon,is_hidden,link,message,message_tags,name,object_id,parent_id,permalink_url,picture,place,properties,shares,source,story,full_picture,expanded_height,comments,attachments";

            // Definir política de retry com Polly
            var retryPolicy = Policy.Handle<IOException>()
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                {
                    _logManager.LogError($"Tentativa {retryCount} falhou. Erro: {exception.Message}");
                    await Task.CompletedTask;
                });


            try
            {
                var allPosts = new List<Post>(); //Lista para acumular todos os posts
                string nextPageUrl = $"{api}/{communityId}/feed?fields={fields}";

                do
                {
                    // Executar a política de retry
                    var result = await retryPolicy.ExecuteAsync(async () =>
                    {
                        var client = new RestClient(nextPageUrl);
                        var request = new RestRequest();
                        request.AddHeader("Authorization", $"Bearer {token}");
                        var response = await client.ExecuteAsync(request);

                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            _logManager.LogError($"GetAllPostByCommunityId - Não foi possível buscar os Posts da comunidade. ID: {communityId}. Erro: {response.Content}");
                            return null;
                        }

                        var obj = JsonConvert.DeserializeObject<Root>(response.Content);
                        return obj;
                    });

                    if (result == null)
                    {
                        _logManager.LogError($"GetAllPostByCommunityId - Falha ao buscar posts na comunidade. ID: {communityId}");
                        break;
                    }

                    // Adicionar os posts da página atual à lista total
                    if (result.Data != null && result.Data.Count > 0)
                    {
                        allPosts.AddRange(result.Data);
                    }

                    // Verificar se há próxima página
                    if (result.Paging != null && !string.IsNullOrEmpty(result.Paging.Next))
                    {
                        nextPageUrl = result.Paging.Next;
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
                return null;
            }
        }

        public async Task<PostComplete?> GetPostCompleteById(string postId, string token)
        {
            try
            {
                _logManager.LogInformation("GetAllPostByCommunityId - Iniciando a busca do Post completo.");
                var api = _configuration["Workplace:Api"];
                var fields = "id,admin_creator,actions,application,caption,created_time,description,feed_targeting,from,icon,is_hidden,link,message,message_tags,name,object_id,parent_id,permalink_url,picture,place,properties,shares,source,story,full_picture,expanded_height,comments,attachments";

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
                throw;
            }
        }

        public async Task<AttachmentsFiles?> GetAttachaments(string postId, string token)
        {
            try
            {
                _logManager.LogInformation("GetAttachaments - Iniciando a busca de anexos no Post.");
                var api = _configuration["Workplace:Api"];

                var client = new RestClient($"{api}/{postId}/attachments");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<AttachmentsFiles>(response.Content);



                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "PostWorkplaceService", Function = "GetAttachaments", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<Vote?> GetVotesByPostId(string postId)
        {
            try
            {
                _logManager.LogInformation("GetVotesByPostId - Iniciando a busca de de votos por Enquete.");
                var token = _configuration["Workplace:Token"];
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

        public async Task<List<double>> GetUpload(AttachmentsFiles files)
        {
            _logManager.LogInformation($"uploadFile - Iniciando o Get de arquivos (upload)");

            try
            {
                var listFiles = new List<RestResponse>();
                var image = files?.Data?.Where(x => x.Type == "photo" || x.Type == "video_inline" || x.Type == "share").ToList();               
                var archives = (files?.Data != null && files.Data.Count > 0) ? files.Data[0]?.Subattachments?.Data?.ToList() : null;
                var listFilesSize = new List<double>(); // Lista para armazenar os tamanhos dos arquivos

                if (image.Count > 0)
                {
                    _logManager.LogInformation($"GetAll - Total de anexos encontrados: {image.Count}");

                    foreach (var item in image)
                    {
                        if (item?.Media?.Image?.Src == null && item?.Media?.source == null)
                            continue;


                        var url = item.Type.Equals("photo") || item.Type.Equals("share") ? item.Media.Image.Src : item.Media.source;


                        _logManager.LogInformation($"uploadFile - image.count > 0");
                        _logManager.LogInformation($"URL do vídeo: {url}");

                        // Get File (URL to File)
                        var options = new RestClientOptions(url)
                        {
                            ThrowOnAnyError = true,
                            Timeout = TimeSpan.FromMilliseconds(9200000)
                        };
                        var client_img = new RestClient(options);

                        try
                        {
                            var request_img = new RestRequest();

                            // Definir política de retry com Polly
                            var retryPolicy = Policy.Handle<IOException>()
                                .Or<SocketException>()
                                .Or<HttpRequestException>()
                                .WaitAndRetryAsync(8, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                                      onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                                {
                                    _logManager.LogError($"Tentativa {retryCount} falhou. Erro: {exception.Message}");
                                    await Task.CompletedTask;
                                });


                            // Executar a política de retry
                            await retryPolicy.ExecuteAsync(async () =>
                            {
                                // Cria a requisição HEAD
                                var request = new RestRequest(url, Method.Head);

                                // Executa a requisição
                                var response1 = await client_img.ExecuteAsync(request);

                                if (response1 == null)
                                    return; // Se não houver resposta, não continua

                                var contentLengthHeader = response1.ContentLength;
                                long fileSize = long.Parse(contentLengthHeader.Value.ToString());

                                _logManager.LogInformation($"Tamanho do vídeo [MB]: {(int)Math.Max(1, Math.Round(fileSize / 1024.0 / 1024.0))}");

                                listFilesSize.Add(fileSize);

                                // Criar objeto Files e adicionar à lista compartilhada
                                var fileData = new Files
                                {   
                                    Name = url ?? "Desconhecido",
                                    Type = item.Type,
                                    Size = (int)Math.Max(1, Math.Round(fileSize / 1024.0 / 1024.0))
                                };

                                FileManager.FilesDataList.Add(fileData);
                            });
                        }
                        catch (Exception ex)
                        {
                            _logManager.LogError($"Erro ao fazer download: {ex.Message}");
                            continue;
                        }
                        finally
                        {
                            client_img.Dispose();
                        }

                        _logManager.LogInformation($"uploadFile - File adicionado");
                    }
                }

                else
                {
                    if (archives == null) { return null; }
                    _logManager.LogInformation($"GetAll - Total de anexos encontrados: {archives.Count}");

                    foreach (var item in archives)
                    {
                        if (item?.Type == "option") { continue; }

                        // Get File (URL to File)
                        var client_img = item?.Type == "photo" ? new RestClient(item?.Media?.Image?.Src) : new RestClient(item?.Url);
                        try
                        {

                            _logManager.LogInformation($"uploadFile - Type == 'photo'");
                            _logManager.LogInformation($"URL do arquivo: {item.Media?.Image?.Src ?? item.Url} | Nome: {item.Title}");


                            var request_img = new RestRequest();

                            // Cria a requisição HEAD
                            var request = new RestRequest(item.Media?.Image?.Src ?? item.Url, Method.Head);

                            // Executa a requisição
                            var response = await client_img.ExecuteAsync(request);

                            if (response == null)
                                continue; //Se não houver resposta, não continua

                            var contentLengthHeader = response.ContentLength;
                            long fileSize = long.Parse(contentLengthHeader.Value.ToString());

                            _logManager.LogInformation($"Tamanho do arquivo [MB]: {(int)Math.Max(1, Math.Round(fileSize / 1024.0 / 1024.0))}");

                            listFilesSize.Add(fileSize);

                            // Criar objeto Files e adicionar à lista compartilhada
                            var fileData = new Files
                            {   
                                Name = item.Media?.Image?.Src ?? item.Url,
                                Type = item.Type,
                                Size = (int)Math.Max(1, Math.Round(fileSize / 1024.0 / 1024.0))
                            };

                            FileManager.FilesDataList.Add(fileData);
                        }
                        catch (Exception ex)
                        {
                            _logManager.LogError($"Erro ao fazer download: {ex.Message}");
                            continue;
                        }
                        finally
                        {
                            client_img.Dispose(); 
                        }


                        _logManager.LogInformation($"uploadFile - File adicionado");
                    }
                }

                _logManager.LogInformation($"uploadFile - Get de arquivos concluído");
                return listFilesSize;
            }
            catch (Exception e)
            {
                _logManager.LogError(e.ToString());
                return null;
            }
        }

    }
}
