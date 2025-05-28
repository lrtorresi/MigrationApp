#nullable enable

using EnvironmentValidator.Domain.Entities.Global;
using EnvironmentValidator.Domain.Entities.Workplace;
using EnvironmentValidator.Service.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Service.Workplace
{
    public class GroupWorkplaceService
    {
        private readonly IConfiguration _configuration;
        private readonly LogManager _logManager;
        private readonly PostWorkplaceService _postWorkplaceService;
        

        public GroupWorkplaceService(IConfiguration configuration, LogManager logManager, PostWorkplaceService postWorkplaceService)
        {
            _configuration = configuration;
            _logManager = logManager;
            _postWorkplaceService = postWorkplaceService;
        }

        public async Task<Groups> GetAll(string token, IProgress<string> progress = null)
        {

            _logManager.LogInformation("GetAll - Iniciando a busca de grupos no Workplace.");

            var fields = "id,archived,cover,description,icon,is_workplace_default,name,owner,privacy,updated_time";
            var api = _configuration["Workplace:Api"];

            var allGroups = new List<Group>();
            string nextPageUrl = $"{api}/community/groups?fields={fields}";

            var postComplete = new PostComplete();
            var totalPost = 0;
            var count = 0;

            try
            {
                do
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

                    var result = JsonConvert.DeserializeObject<Groups>(response.Content);

                    if (result.Data != null && result.Data.Count > 0)
                    {
                        allGroups.AddRange(result.Data);
                    }

                    if (result.Paging != null && !string.IsNullOrEmpty(result.Paging.Next))
                    {
                        nextPageUrl = result.Paging.Next;
                    }
                    else
                    {
                        nextPageUrl = null;
                    }

                } while (!string.IsNullOrEmpty(nextPageUrl));

                allGroups = allGroups.OrderBy(x => x.Name).ToList();

                //Após pegar as comunidades, pega os posts.
                foreach (var item in allGroups)
                {
                    _logManager.LogInformation($"GetAll - Iniciando a busca de Posts. Grupos: {count++}/{allGroups.Count}");
                    progress.Report($"Buscando posts da comunidade: {item.Name}. {count++}/{allGroups.Count}");

                    var response = await _postWorkplaceService.GetAllPostByCommunityId(item.Id, token);
                    
                    try
                    {
                        if (response is not null)
                        {
                            totalPost = totalPost + response.Data.Count;

                            //verifica se o post tem Anexos (imagens, videos, documentos...)
                            foreach (var post in response.Data)
                            {
                                //Verifica se tem Anexos
                                var attachements = await _postWorkplaceService.GetAttachaments(post.Id, token);

                                //Realiza o upload dos arquivos
                                if (attachements is not null ||attachements?.Data?.Count > 0)
                                {
                                    var size = await _postWorkplaceService.GetUpload(attachements);
                                    
                                    if(size is not null && size.Count > 0)
                                        item.TotalMb += (int)Math.Max(1, Math.Round(size.Sum() / 1024.0 / 1024.0));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logManager.LogError($"GetAll - Erro ao realizar o somatorio de Uploads. Erro: {ex.Message}");
                        continue;
                    }
                }

                var finalResult = new Groups
                {
                    Data = allGroups,
                    TotalPosts = totalPost
                };

                

                _logManager.LogInformation($"GetAll - Total de comunidades encontradas: {allGroups.Count}");
                _logManager.LogInformation("GetAll - Finalizado a busca de grupos no Workplace.");

                return finalResult;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "GroupWorkplaceService", Function = "GetAll", Error = e.Message };
                _logManager.LogObject(logObject);
                return null;
            }
        }

        private async Task LogCommunity(Groups result)
        {
            

            StringBuilder log = new StringBuilder();
            foreach (var item in result.Data)
            {

                log.Append(item.Name).Append(";");                
            }

            _logManager.LogInformation($"Comunidades - {log}");
        }

        public async Task<Groups?> GetImageBannerByGroupId(string? groupId)
        {
            try
            {
                _logManager.LogInformation("GetImageBannerByGroupId - Iniciando a busca da imagem do Banner.");
                var token = _configuration["Workplace:Token"];
                var api = _configuration["Workplace:Api"];
                var fields = "id,images";

                var client = new RestClient($"{api}/v19.0/{groupId}?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<Groups>(response.Content);

                _logManager.LogInformation("GetImageBannerByGroupId - Finalizado a busca da imagem do Banner.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "GroupWorkplaceService", Function = "GetImageBannerByGroupId", Error = e.Message, Value = groupId };
                _logManager.LogObject(logObject);
                throw;
            }
        }
    }
}
