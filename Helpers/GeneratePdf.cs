
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using System.IO;
using EnvironmentValidator.Domain.Entities.Workplace;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using PuppeteerSharp;
using RestSharp;
using EnvironmentValidator.Service.Resources;



namespace EnvironmentValidator.Helpers
{
    public class GeneratePdf
    {
        private readonly LogManager _logManager;

        public GeneratePdf()
        {
            
        }

        public async Task<int> GenerateHtmlToPdf(knowledgeLibrary library, string token)
        {
            try
            {
                //converte para HTML
                var sb = new StringBuilder();

                await ConvertJsonToHtmlAsync(library, sb, token);
                var html = $"<!DOCTYPE html><html lang=\"pt-br\"><head><title></title><meta charset=\"utf-8\"></head><body>{sb.ToString()}</body></html>";


                // Baixa a versão mais recente do Chromium necessária para o Puppeteer
                await new BrowserFetcher().DownloadAsync();

                // Inicia o browser em modo headless
                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

                // Cria uma nova página
                using var page = await browser.NewPageAsync();

                // Define o conteúdo da página
                await page.SetContentAsync(html);

                // Gera o PDF e obtém os dados em um array de bytes
                var pdfBytes = await page.PdfDataAsync();

                // Calcula o tamanho do PDF em megabytes
                var pdfSizeInMB = (int)Math.Max(1, Math.Round(pdfBytes.Length / 1024.0 / 1024.0));
                
                return pdfSizeInMB;

            }
            catch (Exception ex)
            {
                
                throw;
            }


        }

        private async Task ConvertJsonToHtmlAsync(knowledgeLibrary knowledgeLibrary, StringBuilder sb, string token)
        {
            foreach (var content in knowledgeLibrary.Json_Content)
            {
                if (content?.Children != null)
                {
                    foreach (var item in content.Children)
                    {
                        await ProcessItem(item, sb);
                    }
                }
                if (content?.Type == "image")
                {
                    //Get URL File
                    var urlImage = await GetImageBannerByGroupId(content.image_data.Id, token);

                    sb.Append($"<img src=\"{urlImage.Images[0].source}\" alt=\"Descrição da Imagem\" style=\"max-width:100%; height:auto;\">");
                }
            }
        }

        private async Task ProcessItem(Child? item, StringBuilder sb)
        {
            if (item != null)
            {
                // Verifica se o item possui o campo "type"
                if (item.Type != null)
                {
                    // Adiciona a tag HTML correspondente ao tipo
                    if (item.Type != "text")
                    {
                        if (item.Type == "a")
                        {
                            sb.Append("<").Append(item.Type).Append($" href={item.Href}").Append(">");
                        }
                        else
                        {
                            sb.Append("<").Append(item.Type).Append(">");
                        }
                    }


                    if (item.Children != null)
                    {
                        foreach (var child in item.Children)
                        {
                            await ProcessItem(child, sb);
                        }
                    }


                    // Fecha a tag HTML correspondente
                    if (item.Type != "text")
                        sb.Append("</").Append(item.Type).Append(">");
                }

                // Verifica se o item possui o campo "text"
                if (item.Text != null)
                {
                    sb.Append(item.Text);
                }
            }
        }

        private async Task<Groups?> GetImageBannerByGroupId(string? groupId, string token)
        {
            try
            {
                var fields = "id,images";

                var client = new RestClient($"https://graph.workplace.com/v19.0/{groupId}?fields={fields}");
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {token}");

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return null;

                var result = JsonConvert.DeserializeObject<Groups>(response.Content);


                return result;
            }
            catch (Exception e)
            {
                var logObject = new { Class = "GroupWorkplaceService", Function = "GetImageBannerByGroupId", Error = e.Message, Value = groupId };
                return null;
            }
        }


    }
}
