using EnvironmentValidator.Service.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Service.Workplace
{
    public class EnvironmentValidatorServices
    {
        private readonly LogManager _logManager;
        private readonly UsersWorkplaceService _users;
        private readonly GroupWorkplaceService _group;
        private readonly PostWorkplaceService _postService;
        private readonly knowledgeLibraryService _knowledgeLibraryService;
        

        public EnvironmentValidatorServices(LogManager logManager, UsersWorkplaceService users, GroupWorkplaceService group, PostWorkplaceService postService, knowledgeLibraryService knowledgeLibraryService)
        {
            _logManager = logManager;
            _users = users;
            _group = group;
            _postService = postService;
            _knowledgeLibraryService = knowledgeLibraryService;
            
        }

        public async Task<object> EnvironmentValidator(string token, IProgress<string> progress)
        {

            #region Funções
            //log
            _logManager.LogInformation("Validação de ambiente iniciada.");
            progress?.Report("Iniciando validação geral do ambiente...");

            //Retorna o total de usuários no tenant do Workplace
            //var totalUsers = await _users.GetAll(token);
            var totalUsers = await _users.GetAllUsersRecursive(token, progress);
            
            if (totalUsers == null)
                _logManager.LogInformation("Não foi possível buscar os usuários. Tentando buscar as comunidades como próximo passo.");

            //Retorna o total de comunidades no Workplace
            var totalGroups = await _group.GetAll(token, progress);
            if (totalGroups == null)
                return null;

            //Retorna o total de Bibliotecas de Conhecimento no Workplace
            var totalKnowledgeLibrary = await _knowledgeLibraryService.GetAll(token);

            _logManager.LogInformation("Finalizado a validação de ambiente.");
            #endregion

            var obj = new
            {
                TotalUsers = totalUsers?.Count,
                TotalGroups = totalGroups?.Data?.Count,
                TotalPosts = totalGroups?.TotalPosts,
                TotalKnowledgeLibrary = totalKnowledgeLibrary?.Data?.Count ?? 0,
                Groups = totalGroups?.Data,
                KnowledgeLibrary = totalKnowledgeLibrary?.Data,
            };

            _logManager.LogInformation($"Total de usuários no Workplace: {totalUsers?.Count}");
            _logManager.LogInformation($"Total de comunidades no Workplace: {totalGroups?.Data?.Count ?? 0}");
            _logManager.LogInformation($"Total de Posts no Workplace: {totalGroups?.TotalPosts}");
            _logManager.LogInformation($"Total de Bibliotecas de Conhecimento: {totalKnowledgeLibrary?.Data?.Count ?? 0}");

            return obj;
        }
    }
}
