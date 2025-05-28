
using DocumentFormat.OpenXml.Wordprocessing;
using EnvironmentValidator.Domain.Entities.Global;
using EnvironmentValidator.Domain.Entities.Workplace;
using EnvironmentValidator.Helpers;
using EnvironmentValidator.Service.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Service.Workplace
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

        public async Task<User?> GetAll(string token)
        {
            try
            {
                _logManager.LogInformation("GetAll - Iniciando a busca de usuários no Workplace.");
                var client = new RestClient("https://scim.workplace.com/Users?fields=name,userName,profileUrl,emails,id");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;
                var result = JsonConvert.DeserializeObject<User>(response.Content);

                _logManager.LogInformation("GetAll - Finalizado a busca de usuários no Workplace.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "UsersWorkplaceService", Function = "GetAll", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<Users> GetUserByGroups(string groupId, string token)
        {
            var api = _configuration["Workplace:Api"];
            var fields = "id,first_name,last_name,email,administrator";

            try
            {
                _logManager.LogInformation("GetUserByGroups - Iniciando a busca de usuários por grupo.");
                var client = new RestClient($"{api}/{groupId}/members?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;
                var result = JsonConvert.DeserializeObject<Users>(response.Content);

                _logManager.LogInformation("GetUserByGroups - Finalizando a busca de usuários por grupo.");
                return result;
            }
            catch (HttpRequestException e)
            {
                var logObject = new { Class = "UsersWorkplaceService", Function = "GetUserByGroups", Error = e.Message, Value = groupId };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<User?> GetUserById(string Id, string token)
        {
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
            var token = _configuration["Workplace:Token"];
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

        public async Task<List<Resource>> GetAllUsersRecursive(string token, IProgress<string> progress = null, int startIndex = 1, int count = 100, List<Resource>? allUsers = null)
        {
            try
            {
                if (allUsers == null)
                    allUsers = new List<Resource>();

                _logManager.LogInformation($"GetAllUsersRecursive - Buscando usuários a partir do índice {startIndex}");
                progress.Report($"Total de usuários mapeados: {allUsers.Count}…");


                // Monta a URL com os parâmetros de paginação
                var url = $"https://scim.workplace.com/Users?fields=name,userName,profileUrl,emails,id&startIndex={startIndex}&count={count}";
                var client = new RestClient(url);
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logManager.LogInformation("GetAllUsersRecursive - Erro ao buscar usuários");
                    return null;
                }

                // Desserializa a resposta para a classe User, que contém os metadados e a lista de usuários em Resources
                var result = JsonConvert.DeserializeObject<User>(response.Content);
                if (result?.Resources != null)
                {
                    allUsers.AddRange(result.Resources);
                    _logManager.LogInformation($"GetAllUsersRecursive - {result.Resources.Count} usuários encontrados na página iniciando em {startIndex}");

                    UserManager.AddUsers(result.Resources);
                }
                else
                {
                    _logManager.LogInformation("GetAllUsersRecursive - Nenhum usuário retornado nesta página");
                }

                // Verifica se o número de usuários retornados é igual ao count solicitado.
                // Caso seja, pode haver mais registros e, portanto, a chamada recursiva é feita.
                if (result != null && result.Resources != null && result.Resources.Count == count)
                {
                    return await GetAllUsersRecursive(token, progress, startIndex + count, count, allUsers);
                }
                else
                {
                    _logManager.LogInformation("GetAllUsersRecursive - Busca completa de usuários finalizada.");
                    return allUsers;
                }
            }
            catch (Exception e)
            {
                var logObject = new { Class = "UsersWorkplaceService", Function = "GetAllUsersRecursive", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }
    }
}
