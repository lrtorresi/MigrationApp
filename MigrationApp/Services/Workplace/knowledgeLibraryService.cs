using Microsoft.Extensions.Configuration;
using MigrationApp.Entities.Global;
using MigrationApp.Entities.Workplace;
using MigrationApp.Services.Resources;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Services.Workplace
{
    public class knowledgeLibraryService
    {
        private readonly IConfiguration _configuration;
        private readonly GroupWorkplaceService _groupService;
        private readonly LogManager _logManager;

        public knowledgeLibraryService(IConfiguration configuration, GroupWorkplaceService groupService, LogManager logManager)
        {
            _configuration = configuration;
            _groupService = groupService;
            _logManager = logManager;
        }


        public async Task<KnowledgeLibraryRoot?> GetAll()
        {
            try
            {
                _logManager.LogInformation("GetAll - Iniciando a busca pelo total de Biblioteca de conhecimento.");
                var fields = "id,title";
                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];

                var client = new RestClient($"{api}/community/knowledge_library_categories?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<KnowledgeLibraryRoot>(response.Content);

                _logManager.LogInformation("GetAll - Finalizado a busca pelo total de Biblioteca de conhecimento.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "knowledgeLibraryService", Function = "GetAll", Error = e.Message };
                _logManager.LogObject(logObject);
                return null;
            }
        }

        public async Task<knowledgeLibrary?> GetById(string Id)
        {
            try
            {
                _logManager.LogInformation("GetById - Iniciando a busca pela Biblioteca de conhecimento por ID.");
                var fields = "id,title,content,status,last_editor,color,icon,last_updated,json_content,read_audience{audience_type,groups,static_users,profile_set_conditions}";

                var token = GlobalConfig.TokenWorkplace;
                var api = _configuration["Workplace:Api"];

                var client = new RestClient($"{api}/{Id}?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<knowledgeLibrary>(response.Content);

                _logManager.LogInformation("GetById - Finalizado a busca pela Biblioteca de conhecimento por ID.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "knowledgeLibraryService", Function = "GetById", Error = e.Message };
                _logManager.LogObject(logObject);
                return null;
            }
        }
    }
}
