
using Microsoft.Extensions.Configuration;
using MigrationApp.Entities.Engage;
using MigrationApp.Entities.Global;
using MigrationApp.Entities.Workplace;
using MigrationApp.Services.Resources;
using MigrationApp.Services.Workplace;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;
using Windows.Networking.NetworkOperators;
using static System.Collections.Specialized.BitVector32;

namespace MigrationApp.Services.Engage
{
    public class CommunityEngageService
    {
        private readonly IConfiguration _configuration;
        private readonly UsersWorkplaceService _usersWorkplaceService;
        private readonly LogManager _logManager;
        private readonly UserEngageService _userEngageService;

        public CommunityEngageService(IConfiguration configuration, UsersWorkplaceService usersWorkplaceService, LogManager logManager, UserEngageService userEngageService)
        {
            _configuration = configuration;
            _usersWorkplaceService = usersWorkplaceService;
            _logManager = logManager;
            _userEngageService = userEngageService;
        }

        public async Task<CommunityEngage?> AddCommunity(string name, bool privacy)
        {
            try
            {
                _logManager.LogInformation($"AddCommunity - Adicionando ao Engage a comunidade: {name}");

                name = Regex.Replace(name, @"[^a-zA-Z0-9\s\-_]", "");

                // Definir política de retry com Polly
                var retryPolicy = Policy
                    .Handle<IOException>()
                    .Or<SocketException>()
                    .Or<HttpRequestException>()
                    .Or<Exception>(ex => ex.Message == "DISPLAY_NAME_ALREADY_EXISTS")
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                    {
                        _logManager.LogError($"AddCommunity - Tentativa {retryCount} falhou. Erro: {exception.Message}");

                        // Atualizar o nome da comunidade com timestamp para evitar duplicidade
                        name = $"{name} - {DateTime.Now:HH:mm:ss}";
                        await Task.CompletedTask;
                    });


                var token = GlobalConfig.TokenEngage;
                var api = _configuration["Yammer:Api"];

                #region Yammer
                //var result = await retryPolicy.ExecuteAsync(async () =>
                //{
                //    var client = new RestClient($"{api}/v1/groups.json?name={name}&private={privacy}");
                //    var request = new RestRequest();
                //    request.AddHeader("Authorization", $"Bearer {token}");

                //    var response = await client.PostAsync(request);

                //    if (response.StatusCode != System.Net.HttpStatusCode.Created)
                //    {
                //        _logManager.LogError($"AddCommunity - Não foi possivel adicionar a comunidade: {name} ");
                //        return null; // Retorna null explicitamente
                //    }

                //    // Desserializar a resposta
                //    var obj = JsonConvert.DeserializeObject<CommunityEngage>(response.Content);
                //    return obj;
                //});

                #endregion

                #region Graph

                #region Opção 1
                //var result = await retryPolicy.ExecuteAsync(async () =>
                //{
                //    var token = await _userEngageService.GetUserTokenEngage(["https://graph.microsoft.com/.default"]);

                //    var client = new RestClient($"https://graph.microsoft.com/beta/employeeExperience/communities");
                //    var request = new RestRequest();
                //    request.AddHeader("Authorization", $"Bearer {token}");
                //    request.AddHeader("Content-type", "application/json");
                //    request.AddJsonBody(new
                //    {
                //        displayName = name,
                //        description = name,
                //        privacy = privacy ? "Private" : "Public"
                //    });

                //    var response = await client.PostAsync(request);

                //    if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                //    {
                //        _logManager.LogError($"AddCommunity - Não foi possivel adicionar a comunidade: {name} ");
                //        return null; // Retorna null explicitamente
                //    }

                //    var locationHeader = response.Headers
                //                                 .FirstOrDefault(h => h.Name.Equals("Location", StringComparison.OrdinalIgnoreCase))?.Value?.ToString();

                //    var match = Regex.Match(locationHeader ?? "", @"engagementAsyncOperations\('(.+)'\)");
                //    if (!match.Success)
                //    {
                //        _logManager.LogError("AddCommunity - Formato inesperado do header Location.");
                //        return null;
                //    }

                //    var operationId = match.Groups[1].Value;

                //    var statusClient = new RestClient($"https://graph.microsoft.com/beta/employeeExperience/engagementAsyncOperations/{operationId}");
                //    var statusRequest = new RestRequest();
                //    statusRequest.AddHeader("Authorization", $"Bearer {token}");
                //    statusRequest.AddHeader("Content-Type", "application/json");

                //    var statusCommunity = await statusClient.GetAsync(statusRequest);


                //    // Desserializar a resposta
                //    var obj = JsonConvert.DeserializeObject<CommunityEngage>(statusCommunity.Content);
                //    //var getProperties = await getNetworkId(obj.Data.CreateGroup.Group.Id);
                //    //var getProperties = await GetCommunityWithFilter(name, token);

                //    var statusJson = JObject.Parse(statusCommunity.Content ?? "{}");
                //    //obj.Id = obj.resourceId;

                //    return obj;
                //});
                #endregion

                #region Opção 2
                var result = await retryPolicy.ExecuteAsync(async () =>
                {
                    var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);

                    var client = new RestClient($"https://engage.cloud.microsoft/graphql?operationName=CreateGroupClients&apiVnext=2");
                    var request = new RestRequest();
                    request.AddHeader("Authorization", $"Bearer {token}");
                    request.AddHeader("Content-type", "application/json");
                    request.AddJsonBody(new
                    {
                        operationName = "CreateGroupClients",
                        query = "mutation CreateGroupClients($displayName: String!, $description: String, $isExternal: Boolean!, $isPrivate: Boolean!, $isUnlisted: Boolean, $addMemberUserIds: [ID!], $classificationName: String, $officeSensitivityLabelId: String = null, $threadStarterDefaultContentType: MessageContentType = null, $isThreadStarterRestricted: Boolean = null, $isNetworkQuestionGroup: Boolean = null, $groupType: GroupType = null, $isMoveThreadToThisGroupRestricted: Boolean = null) { createGroup(input: {displayName: $displayName, description: $description, isExternal: $isExternal, isPrivate: $isPrivate, isUnlisted: $isUnlisted, addMemberUserIds: $addMemberUserIds, classificationName: $classificationName, officeSensitivityLabelId: $officeSensitivityLabelId, threadStarterDefaultContentType: $threadStarterDefaultContentType, isThreadStarterRestricted: $isThreadStarterRestricted, isNetworkQuestionGroup: $isNetworkQuestionGroup, groupType: $groupType, isMoveThreadToThisGroupRestricted: $isMoveThreadToThisGroupRestricted}) { group { id } } }",
                        variables = new
                        {
                            displayName = name,
                            description = name,
                            isExternal = false,
                            isPrivate = privacy,
                            isUnlisted = false,
                            addMemberUserIds = new string[] { },
                            threadStarterDefaultContentType = "NORMAL",
                            isMoveThreadToThisGroupRestricted = true
                        }
                    });

                    var response = await client.PostAsync(request);

                    #region Validação de Comunidade Existente
                    // Verificar se há erro lógico na resposta JSON (mesmo com 200 OK)
                    var content = response.Content;
                    var json = JObject.Parse(content ?? "{}");

                    var errorCode = json["errors"]?[0]?["extensions"]?["code"]?.ToString();
                    if (errorCode == "DISPLAY_NAME_ALREADY_EXISTS")
                    {
                        throw new Exception("DISPLAY_NAME_ALREADY_EXISTS");
                    }
                    #endregion

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logManager.LogError($"AddCommunity - Não foi possivel adicionar a comunidade: {name} ");
                        return null; // Retorna null explicitamente
                    }

                    var obj = JsonConvert.DeserializeObject<CommunityEngage>(response.Content);
                    var getProperties = await getNetworkId(obj.Data.CreateGroup.Group.Id);

                    #region NetWorkId
                    var match = Regex.Match(getProperties.Data.Group.ThreadStarterSmallFileUploadUrl, @"network/(\d+)/group/");
                    if (match.Success)
                    {
                        var networkId = match.Groups[1].Value;
                        getProperties.Data.Group.Network.Id = networkId;
                    }
                    #endregion

                    obj.Network_id = getProperties.Data.Group.Network.Id;
                    obj.Id = getProperties.Data.Group.DatabaseId;

                    return obj;
                });
                #endregion

                #endregion

                if (result == null)
                    _logManager.LogError($"AddCommunity - Não foi possivel adicionar a comunidade: {name}");

                _logManager.LogInformation($"AddCommunity - Comunidade adicionada: {name}");
                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "CommunityEngageService", Function = "AddCommunity", Error = e.Message };
                _logManager.LogObject(logObject);
                return null;
            }
        }

        public async Task<bool> AddBannerCommunity(string communityId, string source)
        {
            try
            {
                if (communityId == null || source == null) { return false; }
                _logManager.LogInformation($"AddBannerCommunity - Adicionando o Banner a comunidade. ID da Comunidade: {communityId}");

                //var token = GlobalConfig.TokenEngage;
                var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];

                //Get Banner (URL to File)
                var client2 = new RestClient(source);
                var request2 = new RestRequest();
                RestResponse response2 = await client2.ExecuteAsync(request2);

                var client = new RestClient($"{api}/v2/group_cover_image");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddFile("image", response2.RawBytes, "banner");
                request.AddParameter("group_id", communityId);


                var response = await client.PostAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return false;

                _logManager.LogInformation($"AddBannerCommunity - Adicionado o Banner a comunidade. ID da Comunidade: {communityId}. URL: {source}");

                return true;
            }
            catch (Exception e)
            {
                _logManager.LogError($"AddBannerCommunity - Erro ao adicionar o Banner da comunidade: {communityId}. URL do Banner: {source}");
                var logObject = new { Class = "CommunityEngageService", Function = "AddBannerCommunity", Error = e.Message, CommunityID = communityId, Source = source };
                _logManager.LogObject(logObject);
                return false;
            }
        }

        public async Task<bool> AddUsersToCommunity(string groupWorkplaceId, string communityEngangeId)
        {
            try
            {
                _logManager.LogInformation($"AddUsersToCommunity - Adicionando o Usuarios a comunidade a comunidade. ID da Comunidade: {communityEngangeId}.");

                //Get All user (Workplace) by community
                var users = await _usersWorkplaceService.GetUserByGroups(groupWorkplaceId);

                if (users == null)
                    return false;

                var listUserError = new List<string>();
                
                var count = 0;
                var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                _logManager.LogWarning($"Token gerado: {token}");
                foreach (var user in users.Data)
                {
                    if (user == null)
                        continue;

                    try
                    {
                        var resultAddUser = await AddUserCommunityEngage(communityEngangeId, user, token);

                        if (resultAddUser)
                        {
                            count++;
                            _logManager.LogInformation($"Total de usuários adicionado ao Engage: {count}/{users.Data.Count}");
                            await Task.Delay(30000);
                        }
                        else
                        {
                            listUserError.Add(user.Email);
                        }

                            
                    }
                    catch (Exception)
                    {
                        _logManager.LogError($"AddUsersToCommunity - Falha ao adicionar o usuario na comunidade do Engage: {user.Email}");
                        continue; // Se falhar ao adicionar um usuário, continua com os próximos
                    }

                }


                _logManager.LogInformation($"Total de usuários adicionados a comunidade do Engage: {count++}");

                // Resultado final: e-mails separados por ;
                var emailsComErro = string.Join(";", listUserError);
                _logManager.LogWarning($"Usuários que não foram adicionados: {emailsComErro}");

                return true;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "CommunityEngageService", Function = "AddUsersToCommunity", Error = e.Message, GroupWorkplaceId = groupWorkplaceId, CommunityEngangeId = communityEngangeId };
                _logManager.LogObject(logObject);
                return false;
            }
        }

        public async Task<Boolean> AddUserCommunityEngage(string communityEngangeId, Resource user, string token)
        {
            try
            {
                //var token = GlobalConfig.TokenEngage;
                //var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                var api = _configuration["Yammer:Api"];
                var domain = GlobalConfig.Domain;

                var emaildomain = user.Email.Split('@')[1];
                if (emaildomain != domain)
                    return false;


                //Add user to community (Engage)
                var client = new RestClient($"{api}/v1/group_memberships.json");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("Content-type", "application/json");
                request.AddJsonBody(new
                {
                    group_id = communityEngangeId,
                    email = user.Email
                });
                var response = await client.PostAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    _logManager.LogError($"Falha ao adicionar colaborador a comunidade. E-mail: {user.Email} - ErroCode: {response.StatusCode}");
                    return false;
                }
                    

                var result = JsonConvert.DeserializeObject<UserEngage>(response.Content);

                //Se usuário for administrador, promove ele a admin da comunidade
                if (user.administrator == true)
                {
                    _logManager.LogInformation($"Promovendo colaborador a Administrador da comunidade. E-mail: {user.Email}");

                    var client2 = new RestClient($"{api}/v1/groups/{communityEngangeId}/make_admin?user_id={result.User_Id}");
                    var request2 = new RestRequest(); // NOVO request
                    request2.AddHeader("Authorization", $"Bearer {token}");

                    var admin = await client2.PostAsync(request2);

                    if (admin.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logManager.LogError($"Falha ao promover colaborador a admin. E-mail: {user.Email} - ErroCode: {admin.StatusCode} - {admin.Content}");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "CommunityEngageService", Function = "AddUserCommunityEngage", Error = e.Message, CommunityID = communityEngangeId, User = user };
                _logManager.LogObject(logObject);
                return false;
            }

        }

        private async Task<CommunityEngage> getNetworkId(string communityId)
        {
            //var token = GlobalConfig.TokenEngage;
            var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);

            var client = new RestClient($"https://engage.cloud.microsoft/graphql?operationName=GroupHeaderClients&apiVnext=2");
            var request = new RestRequest();
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(new
            {
                operationName = "GroupHeaderClients",
                query = "query GroupHeaderClients($groupId: ID!, $includeGroupFeaturePermissions: Boolean = false) { group: node(id: $groupId) { ... on Group { ...GroupHeaderFields } } } fragment GroupHeaderFields on Group { ...GroupFields ...ViewerCommunityRelationshipFields ...ViewerHasFavoritedGroupFields category classification { name } sensitivityLabel { officeSensitivityLabelId displayName description isGuestGroupAccessEnabled } participatingNetworks(excludeHostNetwork: true) { id } ...GroupCoverImageFields hiddenForNetworkInDiscovery hiddenForViewerInDiscovery viewerCanMarkAsMultiTenantOrganizationGroup ...GroupFeaturePermissions @include(if: $includeGroupFeaturePermissions) } fragment ViewerHasFavoritedGroupFields on Group { id viewerHasFavorited } fragment ViewerCommunityRelationshipFields on Group { id privacy state viewerMembershipStatus viewerIsAdmin viewerHasFavorited viewerCanStartThread isThreadStarterRestricted isDynamicMembership isAllCompanyGroup } fragment GroupFeaturePermissions on Group { id groupType isM365CopilotAdoptionGroup viewerCanAccessDivisionalGroupFeatures } fragment GroupCoverImageFields on Group { coverImageUrlTemplate hasDefaultCoverImage defaultCoverImageUrlTemplate } fragment GroupFields on Group { databaseId telemetryId id officeUnifiedGroupId network { id } ...GroupAvatarFields displayName description isExternal isOfficial guestsCount threadStarterSmallFileUploadUrl isNetworkQuestionGroup isMultiTenantOrganizationGroup isMoveThreadToThisGroupRestricted } fragment GroupAvatarFields on Group { avatarUrlTemplate hasDefaultAvatar }",
                variables = new
                {
                    groupId = communityId,
                    includeGroupFeaturePermissions = false
                }
            });

            var response = await client.PostAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var result = JsonConvert.DeserializeObject<CommunityEngage>(response.Content);
            return result;
        }

        private async Task<CommunityEngageResponse> GetCommunityWithFilter(string displayName, string token)
        {
            try
            {
                var client = new RestClient("https://graph.microsoft.com/beta/employeeExperience/communities");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("$filter", $"displayName eq '{displayName}'");

                var response = await client.GetAsync(request);

                if (!response.IsSuccessful)
                {
                    _logManager.LogError($"GetCommunityWithFilter - Erro ao consultar comunidade: {response.Content}");
                    return null;
                }

                var result = JsonConvert.DeserializeObject<CommunityEngageResponse>(response.Content);
                return result;
            }
            catch (Exception ex)
            {
                _logManager.LogError($"GetCommunityWithFilter - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> PromoteUserToAdminAsync(string groupId, string userId, string token)
        {
            var client = new RestClient("https://graph.microsoft.com/v1.0");
            var request = new RestRequest($"/groups/{groupId}/owners/$ref", Method.Post);

            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                // Microsoft exige o @odata.id para referenciar o usuário
                @odataid = $"https://graph.microsoft.com/v1.0/users/{userId}"
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                _logManager.LogInformation($"Usuário {userId} promovido a owner (admin) do grupo {groupId}");
                return true;
            }
            else
            {
                _logManager.LogError($"Erro ao promover usuário: {response.StatusCode} - {response.Content}");
                return false;
            }
        }
    }
}
