using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Service.Workplace
{
    using Domain.Entities.Workplace;
    using EnvironmentValidator.Helpers;
    using EnvironmentValidator.Service.Resources;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


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


        public async Task<KnowledgeLibrary?> GetAll(string token)
        {
            try
            {
                var initCount = 0;
                _logManager.LogInformation("GetAll - Iniciando a busca pelo total de Biblioteca de conhecimento.");
                var fields = "id,title,content,status,last_editor,color,icon,last_updated,json_content,read_audience{audience_type,groups,static_users,profile_set_conditions}";
                var api = _configuration["Workplace:Api"];

                var client = new RestClient($"{api}/community/knowledge_library_categories?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<KnowledgeLibrary>(response.Content);

                if (result.Data is null || result.Data.Count <= 0)
                {
                    _logManager.LogInformation($"Total de Bibliotecas de Conhecimento: {result.Data.Count}");
                    return null;
                }


                _logManager.LogInformation($"Total de Bibliotecas de Conhecimento: {result.Data.Count}");

                var t = new GeneratePdf();
                foreach (var item in result.Data)
                {
                    try
                    {
                        _logManager.LogInformation($"Bibliotecas de Conhecimento convertida em Doc: {initCount++}/{result.Data.Count}");
                        var pdfSizeInMB = await t.GenerateHtmlToPdf(item, token);
                        item.Size = pdfSizeInMB;
                    }
                    catch (Exception ex)
                    {
                        _logManager.LogError($"GetAll - Erro ao adicionar Biblioteca do conhecimento - {ex.Message}");
                        continue;
                    }

                }

                _logManager.LogInformation("GetAll - Finalizado a busca pelo total de Biblioteca de conhecimento.");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "knowledgeLibraryService", Function = "GetAll", Error = e.Message };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<knowledgeLibrary?> GetById(string Id)
        {
            try
            {
                _logManager.LogInformation("GetById - Iniciando a busca pela Biblioteca de conhecimento por ID.");
                var fields = "id,title,content,status,last_editor,color,icon,last_updated,json_content,read_audience{audience_type,groups,static_users,profile_set_conditions}";

                var token = _configuration["Workplace:Token"];
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
                throw;
            }
        }
    }
}


