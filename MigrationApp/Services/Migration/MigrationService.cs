using MigrationApp.Services.Engage;
using MigrationApp.Services.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Services.Migration
{
    public class MigrationService
    {
        private readonly LogManager _logManager;
        private readonly CommunityEngageService _communityEngageService;
        private readonly PostEngageService _postEngageService;

        public MigrationService(LogManager logManager, CommunityEngageService communityEngageService, PostEngageService postEngageService)
        {
            _logManager = logManager;
            _communityEngageService = communityEngageService;
            _postEngageService = postEngageService;
        }

        #region Groups
        public async Task<bool> MigrateGroups(string nameWorkplace, string idWorkplace, string communityPrivacy, string cover, IProgress<string> progress = null)
        {
            _logManager.LogInformation("MigrateGroups - Iniciando a migração de dados.");
            progress?.Report($"Iniciando migração da comunidade {nameWorkplace}");

            if (idWorkplace == null)
            {
                _logManager.LogInformation("MigrateGroups - groupsWorkplace é nulo");
                return false;
            }

            var privacy = communityPrivacy == "CLOSED" ? true : false;

            // 1 - Add community (Engage)
            progress?.Report($"Iniciando a criação da comunidade: {nameWorkplace}");
            var communitEngage = await _communityEngageService.AddCommunity(nameWorkplace, privacy);
            if (communitEngage is null)
            {
                _logManager.LogError($"Erro ao migrar dados da comunidade {communitEngage.Name}");
                return false;
            }

            // 2 - Add Banner community (Engage)
            progress?.Report($"Adicionando o banner da comunidade: {nameWorkplace}");
            var addBannerCommunity = await _communityEngageService.AddBannerCommunity(communitEngage.Id, cover);

            // 3 - Add users to community (Engage)
            progress?.Report($"Adicionando usuários a comunidade: {nameWorkplace}");
            var addUsersToCommunity = await _communityEngageService.AddUsersToCommunity(idWorkplace, communitEngage.Id);

            // 4 - Add posts/Comments/Likes to community (Engage)
            progress?.Report($"Adicionando os dados da comunidade {nameWorkplace}");
            var addPostsToCommunity = await _postEngageService.AddPostsToCommunity(idWorkplace, communitEngage.Id, communitEngage.Network_id, communitEngage?.Data?.CreateGroup?.Group?.Id, progress);

            if (communitEngage != null && addUsersToCommunity && addPostsToCommunity)
            {
                _logManager.LogInformation($"MigrateGroups - Dados da comunidade {nameWorkplace} migrados com sucesso.");
                _logManager.LogInformation($"==============================================================================================");
                _logManager.LogInformation($"==============================================================================================");
                return true;
            }
            else
            {
                _logManager.LogError($"Erro ao migrar dados da comunidade {communitEngage.Name}");
                _logManager.LogObject(new { communitEngage = communitEngage, addBannerCommunity = addBannerCommunity, addUsersToCommunity = addUsersToCommunity, addPostsToCommunity = addUsersToCommunity });
                return false;
            }
        }

    }
    #endregion
}

