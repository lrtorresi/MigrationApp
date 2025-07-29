using Microsoft.Extensions.Configuration;
using MigrationApp.Entities.Global;
using MigrationApp.Entities.Workplace;
using MigrationApp.Services.Resources;
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

namespace MigrationApp.Services.Workplace
{
    public class GroupWorkplaceService
    {
        private readonly IConfiguration _configuration;
        private readonly LogManager _logManager;

        public GroupWorkplaceService(IConfiguration configuration, LogManager logManager)
        {
            _configuration = configuration;
            _logManager = logManager;
        }
        public async Task<Groups?> GetAll()
        {

            _logManager.LogInformation("GetAll - Iniciando a busca de grupos no Workplace.");

            var fields = "id,archived,cover,description,icon,is_workplace_default,name,owner,privacy,updated_time";
            var token = GlobalConfig.TokenWorkplace;
            var api = _configuration["Workplace:Api"];

            var allGroups = new List<Group>();
            string nextPageUrl = $"{api}/community/groups?fields={fields}";

            // Definir política de retry com Polly
            var retryPolicy = Policy
                .Handle<IOException>()
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .OrResult<RestResponse>(r => r.StatusCode != System.Net.HttpStatusCode.OK) // Adicionar condição para o status de resposta
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetryAsync: async (response, timeSpan, retryCount, context) =>
                {
                    if (response.Exception != null)
                    {
                        _logManager.LogError($"Tentativa {retryCount} falhou devido a uma exceção. Erro: {response.Exception.Message}");
                    }
                    else if (response.Result != null)
                    {
                        _logManager.LogError($"Tentativa {retryCount} falhou. StatusCode: {response.Result.StatusCode}, Erro: {response.Result.Content}");
                    }
                    await Task.CompletedTask;
                });

            try
            {
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

                    
                    if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logManager.LogError($"Erro na requisição: {result.StatusCode} - {result.Content}");
                        continue;
                    }

                    var obj = JsonConvert.DeserializeObject<Groups>(result.Content);

                    if (obj.Data != null && obj.Data.Count > 0)
                    {
                        allGroups.AddRange(obj.Data);
                    }

                    if (obj.Paging != null && !string.IsNullOrEmpty(obj.Paging.Next))
                    {
                        nextPageUrl = obj.Paging.Next;
                    }
                    else
                    {
                        nextPageUrl = null;
                    }

                } while (!string.IsNullOrEmpty(nextPageUrl));

                allGroups = allGroups.OrderBy(x => x.Name).ToList();

                var finalResult = new Groups
                {
                    Data = allGroups
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

        public async Task<Groups?> GetImageBannerByGroupId(string? groupId)
        {
            try
            {
                _logManager.LogInformation("GetImageBannerByGroupId - Iniciando a busca da imagem do Banner.");
                var token = GlobalConfig.TokenWorkplace;
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
