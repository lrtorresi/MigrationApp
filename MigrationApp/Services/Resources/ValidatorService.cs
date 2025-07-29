using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationApp.Entities.Global;

namespace MigrationApp.Services.Resources
{
    public class ValidatorService
    {
        private readonly IConfiguration _configuration;
        private readonly LogManager _logManager;

        public ValidatorService(IConfiguration configuration, LogManager logManager)
        {
            _configuration = configuration;
            _logManager = logManager;
        }

        public async Task<int> KeyValidation(string key)
        {
            var api = _configuration["Impar:Api"];

            var client = new RestClient($"{api}/Migration/{key}");
            var request = new RestRequest();

            try
            {
                var response = await client.PostAsync(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logManager.LogInformation($"Erro ao validar a chave. Código de status: {response.StatusCode}");
                    return 0;
                }

                _logManager.LogInformation("1 - Chave validada com sucesso.");

                GlobalConfig.TotalCommunityCout = int.Parse(response.Content);
                return int.Parse(response.Content);
            }
            catch (Exception ex)
            {
                _logManager.LogError($"Exceção ao validar a chave: {ex.Message}");
                return 0;
            }
        }
    }
}
