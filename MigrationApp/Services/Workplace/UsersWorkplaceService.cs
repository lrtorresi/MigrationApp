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
    public class UsersWorkplaceService
    {
        private readonly IConfiguration _configuration;
        private readonly LogManager _logManager;

        public UsersWorkplaceService(IConfiguration configuration, LogManager logManager)
        {
            _configuration = configuration;
            _logManager = logManager;
        }
       
        public async Task<User?> GetAll()
        {
            var token = GlobalConfig.TokenWorkplace;

            try
            {
                var allUsers = new List<User>();
                int startIndex = 1;
                int count = 100;
                int totalResults = 0;

                do
                {
                    var client = new RestClient("https://scim.workplace.com/Users");
                    var request = new RestRequest();
                    request.AddHeader("Authorization", $"Bearer {token}");
                    request.AddParameter("fields", "name,userName,profileUrl,emails,id");
                    request.AddParameter("startIndex", startIndex);
                    request.AddParameter("count", count);

                    var response = await client.ExecuteAsync(request, Method.Get);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logManager.LogError($"Erro na requisição: {response.StatusCode} - {response.Content}");
                        break;
                    }

                    var result = JsonConvert.DeserializeObject<User>(response.Content);

                    if (result == null || result.Resources == null || result.Resources.Count == 0)
                    {
                        _logManager.LogInformation("Nenhum usuário retornado na resposta.");
                        break;
                    }

                    //allUsers.AddRange(result.Resources);

                    totalResults = result.TotalResults;
                    startIndex += result.ItemsPerPage;

                    _logManager.LogInformation($"Buscando usuários: {allUsers.Count} de {totalResults}");

                } while (startIndex <= totalResults);

                _logManager.LogInformation("GetAllUsersAsync - Finalizado a busca de usuários no Workplace.");
                return null;
            }
            catch (HttpRequestException e)
            {
                var logObject = new { Class = "UsersWorkplaceService", Function = "GetAll", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<Users> GetUserByGroups(string groupId)
        {
            var token = GlobalConfig.TokenWorkplace;
            var api = _configuration["Workplace:Api"];
            var fields = "id,first_name,last_name,email,administrator";

            try
            {
                // Definir política de retry com Polly
                var retryPolicy = Policy.Handle<IOException>()
                    .Or<SocketException>()
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                    {
                        _logManager.LogError($"GetUserByGroups - Tentativa {retryCount} falhou. Erro: {exception.Message}");
                        await Task.CompletedTask;
                    });

                _logManager.LogInformation("GetUserByGroups - Iniciando a busca de usuários por grupo.");

                var allUsers = new List<Resource>(); // Lista para acumular todos os usuários
                string nextPageUrl = $"{api}/{groupId}/members?fields={fields}"; // URL inicial da requisição

                do
                {
                    // Executar a política de retry para cada requisição
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

                        var obj = JsonConvert.DeserializeObject<Users>(response.Content);
                        return obj;
                    });

                    if (result == null)
                    {
                        _logManager.LogError("GetUserByGroups - Falha ao buscar usuários no Workplace.");
                        break;
                    }

                    // Adicionar os usuários da página atual à lista total
                    if (result.Data != null && result.Data.Count > 0)
                    {
                        allUsers.AddRange(result.Data);
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

                

                // Criar o objeto Users para retornar
                var finalResult = new Users
                {
                    Data = allUsers
                };

                _logManager.LogInformation($"GetUserByGroups - Finalizando a busca de usuários por grupo. Total de usuários: {allUsers.Count}");
                
                return finalResult;
            }           
            catch (HttpRequestException e)
            {
                var logObject = new { Class = "UsersWorkplaceService", Function = "GetUserByGroups", Error = e.Message, Value = groupId };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<User?> GetUserById(string Id)
        {
            var token = GlobalConfig.TokenWorkplace;
            var api = _configuration["Workplace:Api"];
            var fields = "name,userName,profileUrl,emails,id";

            try
            {
                _logManager.LogInformation("GetUserById - Iniciando a busca de usuários por Id.");
                var client = new RestClient($"https://scim.workplace.com/Users/{Id}?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;
                var result = JsonConvert.DeserializeObject<User>(response.Content);

                _logManager.LogInformation("GetUserById - Finalizando a busca de usuários por Id.");
                return result;
            }
            catch (HttpRequestException e)
            {
                var logObject = new { Class = "UsersWorkplaceService", Function = "GetUserById", Error = e.Message, Value = Id };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<Users?> GetUserknowledgeLibrary(string knowledgeLibraryId)
        {
            var token = GlobalConfig.TokenWorkplace;
            var api = _configuration["Workplace:Api"];
            var fields = "id,email";

            try
            {
                _logManager.LogInformation("GetUserknowledgeLibrary - Iniciando a busca de usuários na Biblioteca de Conhecimento.");
                var client = new RestClient($"{api}/{knowledgeLibraryId}/seen?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;
                var result = JsonConvert.DeserializeObject<Users>(response.Content);

                _logManager.LogInformation("GetUserknowledgeLibrary - Finalizando a busca de usuários na Biblioteca de Conhecimento.");
                return result;
            }
            catch (HttpRequestException e)
            {
                var logObject = new { Class = "UsersWorkplaceService", Function = "GetUserknowledgeLibrary", Error = e.Message, Value = knowledgeLibraryId };
                _logManager.LogObject(logObject);
                throw;
            }
        }
    }
}
