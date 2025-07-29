using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using MigrationApp.Entities.Engage;
using MigrationApp.Entities.Global;
using MigrationApp.Services.Resources;
using MigrationApp.Services.Workplace;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;




namespace MigrationApp.Services.Engage
{
    public class UserEngageService
    {
        private readonly IConfiguration _configuration;
        private readonly LogManager _logManager;
        private readonly UsersWorkplaceService _usersWorkplaceService;
        

        public UserEngageService(IConfiguration configuration, LogManager logManager, UsersWorkplaceService usersWorkplaceService)
        {
            _configuration = configuration;
            _logManager = logManager;
            _usersWorkplaceService = usersWorkplaceService;
        }

        public async Task<Auth> GetCurrentUser()
        {
            try
            {
                _logManager.LogInformation($"GetCurrentUser - Buscando CurrentUser");

                //var token = GlobalConfig.TokenEngage;
                var token = await GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];

                var client = new RestClient($"{api}/v1/users/current.json");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.GetAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<Auth>(response.Content);

                return result;
            }
            catch (Exception)
            {

                _logManager.LogError("GetCurrentUser - Erro ao buscar usuario");
                return null;
            }
        }
        public async Task<Auth?> GetUserTokenById(string id)
        {
            try
            {
                _logManager.LogInformation($"GetUserTokenById - Get Token do Usuario pelo ID. ID: {id}");

                //var token = GlobalConfig.TokenEngage;
                var token = await GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];
                var clientId = GlobalConfig.ClientIdEngage;

                var client = new RestClient($"{api}/v1/oauth.json?user_id={id}&consumer_key={clientId}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {GlobalConfig.TokenEngage ?? token}");

                var response = await client.PostAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<Auth>(response.Content);

                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "UserEngageService", Function = "GetUserTokenById", Error = e.Message, ID_User = id };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<string> GetUserTokenEngage(string[] scopes)
        {
            try
            {
                _logManager.LogInformation($"GetUserTokenEngage - Autenticando usuário");

                var tenantId = GlobalConfig.TenatId;
                var userName = GlobalConfig.Username;
                var password = GlobalConfig.Password;
                var clientId = GlobalConfig.ClientId; 
                
                var app = PublicClientApplicationBuilder.Create(clientId)
                    .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                    .WithDefaultRedirectUri() 
                    .Build();
                
                // Configurar a persistência do cache de tokens
                ConfigureTokenCache(app.UserTokenCache);

                AuthenticationResult result = null;

                try
                {
                    // Tenta adquirir o token silenciosamente
                    var accounts = await app.GetAccountsAsync();
                    result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                       .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // Autenticação Interativa
                    result = await app.AcquireTokenInteractive(scopes)
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }

                _logManager.LogInformation("Token de acesso obtido com sucesso!");
                GlobalConfig.Domain = result.Account.Username.Split('@')[1] ?? null;

                 return result.AccessToken;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "UserEngageService", Function = "GetUserTokenEngage", Error = e.Message };
                _logManager.LogObject(logObject);
                return null;
            }
        }

        public async Task AuthLimparCacheAsync()
        {
            var clientId = GlobalConfig.ClientId;
            var tenantId = GlobalConfig.TenatId;

            var app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithDefaultRedirectUri()
                .Build();

            // Configurar o cache da mesma forma que na autenticação
            ConfigureTokenCache(app.UserTokenCache);

            var accounts = await app.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await app.RemoveAsync(account);
            }

            _logManager.LogInformation("Cache de tokens limpo com sucesso.");
        }

        public async Task<UserEngage?> GetUserByEmail(string email)
        {
            try
            {
                //var token = GlobalConfig.TokenEngage;
                var token = await GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];
                var clientId = GlobalConfig.ClientIdEngage;

                var client = new RestClient($"{api}/v1/users/by_email.json?email={email}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<List<UserEngage>>(response.Content);


                return result[0];
            }
            catch (Exception e)
            {
                var logObject = new { Class = "UserEngageService", Function = "GetUserByEmail", Error = e.Message, UserEmail = email };
                _logManager.LogObject(logObject);
                throw;
            }
        }

        public async Task<string?> Auth(string userId)
        {
            try
            {
                var userEmail = await _usersWorkplaceService.GetUserById(userId);
                if (userEmail is null)
                    return null;

                var email = userEmail?.Emails != null ? userEmail?.Emails[0]?.Value : userEmail.UserName;
                
                var userIdEngange = await GetUserByEmail(email);
                if (userIdEngange == null)
                {
                    _logManager.LogInformation($"Auth - Não foi possivel buscar o email [{email}] no O365");
                    return null;
                }

                var userToken = await GetUserTokenById(userIdEngange.Id);

                return userToken.Token;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "UserEngageService", Function = "Auth", Error = e.Message, UserID = userId };
                _logManager.LogObject(logObject);
                return null;
            }
        }

        private static void ConfigureTokenCache(ITokenCache tokenCache)
        {
            // Definir as propriedades de armazenamento
            var cacheFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "msalcache.bin3");
            var storageProperties = new StorageCreationPropertiesBuilder("msalcache.bin3", Path.GetDirectoryName(cacheFilePath))
                .WithLinuxKeyring(
                    schemaName: "com.microsoft.adal.cache",
                    collection: MsalCacheHelper.LinuxKeyRingDefaultCollection,
                    secretLabel: "MSAL token cache",
                    attribute1: new KeyValuePair<string, string>("Version", "1"),
                    attribute2: new KeyValuePair<string, string>("ProductGroup", "MSAL"))
                .WithMacKeyChain(
                    serviceName: "com.microsoft.adal.cache",
                    accountName: "MSAL token cache")
                .Build();

            // Criar o cache helper
            var cacheHelper = MsalCacheHelper.CreateAsync(storageProperties).GetAwaiter().GetResult();

            // Registrar o cache
            cacheHelper.RegisterCache(tokenCache);
        }
    }
}
