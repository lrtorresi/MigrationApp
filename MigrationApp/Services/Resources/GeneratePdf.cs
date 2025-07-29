using Microsoft.Extensions.Configuration;
using MigrationApp.Entities.Engage;
using MigrationApp.Entities.Global;
using MigrationApp.Entities.Workplace;
using MigrationApp.Services.Engage;
using MigrationApp.Services.Workplace;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace MigrationApp.Services.Resources
{
    public class GeneratePdf
    {
        private readonly knowledgeLibraryService _knowledgeLibraryService;
        private readonly UserEngageService _userEngageService;
        private readonly GenerateHtml _generateHtml;
        private readonly CommunityEngageService _communityEngageService;
        private readonly IConfiguration _configuration;
        private readonly UsersWorkplaceService _usersWorkplaceService;

       
        public GeneratePdf(knowledgeLibraryService knowledgeLibraryService, UserEngageService userEngageService, GenerateHtml generateHtml, CommunityEngageService communityEngageService, IConfiguration configuration, UsersWorkplaceService usersWorkplaceService)
        {
            _knowledgeLibraryService = knowledgeLibraryService;
            _userEngageService = userEngageService;
            _generateHtml = generateHtml;
            _communityEngageService = communityEngageService;
            _configuration = configuration;
            _usersWorkplaceService = usersWorkplaceService;
        }
        public async Task<knowledgeLibrary> KnowledgeLibrary(string knowledgeLibraryId)
        {
            //Get knowledgeLibrary
            var knowledgeLibrary = await _knowledgeLibraryService.GetById(knowledgeLibraryId);

            //Convert to HTML
            var htmlContent = await _generateHtml.ConvertToJsonHtml(knowledgeLibrary);

            //Convert Object to PDF
            await CreatePdf(knowledgeLibrary, htmlContent);

            return knowledgeLibrary;
        }

        private async Task CreatePdf(knowledgeLibrary knowledgeLibrary, string htmlContent)
        {
            try
            {

                //// Iniciar o navegador
                //await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                //{
                //    Headless = true,
                //    ExecutablePath = GlobalConfig.PuppeteerSharpPath.ExecutablePath
                //});
                var browserFetcher = new BrowserFetcher();
                var revisionInfo = await browserFetcher.DownloadAsync();

                var options = new LaunchOptions { Headless = true, ExecutablePath = revisionInfo.GetExecutablePath() };

                // Abrir uma nova página
                await using var browser = await Puppeteer.LaunchAsync(options);
                await using var page = await browser.NewPageAsync();

                // Definir o conteúdo da página
                await page.SetContentAsync(htmlContent);

                // Gerar o PDF
                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4
                });

                // Cria a Comunidade com o Nome da Bibioteca
                var community = await _communityEngageService.AddCommunity($"Biblioteca do conhecimento: {knowledgeLibrary.Title}", true);

                // Faz o upload do arquivo
                var uploadId = await UploadFile(pdfBytes, community, knowledgeLibrary.Title);

                // Adiciona o Post na Comunidade
                await CreatePdfPost(community, knowledgeLibrary, uploadId);

                // Adiciona os usuarios na Comunidade
                var usersknowledgeLibrary = await _usersWorkplaceService.GetUserknowledgeLibrary(knowledgeLibrary.Id);

                var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
                foreach (var user in usersknowledgeLibrary.Data)
                {
                    await _communityEngageService.AddUserCommunityEngage(community.Id, user, token);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private async Task CreatePdfPost(CommunityEngage community, knowledgeLibrary knowledgeLibrary, string uploadId)
        {
            //Adicionando o Post na Comunidade
            var token = GlobalConfig.TokenEngage;
            var api = _configuration["Yammer:Api"];

            var clientId = GlobalConfig.ClientIdEngage;
            var client = new RestClient($"{api}/v1/messages.json");
            var request = new RestRequest();
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddParameter("body", $"Biblioteca do Conhecimento: {knowledgeLibrary.Title}");
            request.AddParameter("group_id", community.Id);
            request.AddParameter("attached_objects[]", $"uploaded_file:{uploadId}");

            var response = await client.PostAsync(request);
            var result = JsonConvert.DeserializeObject<PostEngage>(response.Content);
        }

        private async Task<string> UploadFile(byte[] pdfBytes, CommunityEngage communityEngage, string DocTitle)
        {
            var token = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
            var api = _configuration["Yammer:Api"];
            var clientId = GlobalConfig.ClientIdEngage;

            var client = new RestClient($"https://filesng.yammer.com/v4/uploadSmallFile/network/{communityEngage.Network_id}/group/{communityEngage.Id}?thirdpartycookiefix=true");
            var request = new RestRequest();
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddFile("file", pdfBytes, "file.pdf");
            request.AddParameter("filename", $"{DocTitle}.pdf");

            var response = await client.PostAsync(request);
            var result = JsonConvert.DeserializeObject<UploadFile>(response.Content);

            return result.Id;
        }


    }

}
