using MigrationApp.Entities.Workplace;
using MigrationApp.Services.Workplace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MigrationApp.Services.Resources
{
    public class GenerateHtml
    {
        private readonly GroupWorkplaceService _groupService;

        public GenerateHtml(GroupWorkplaceService groupService)
        {
            _groupService = groupService;
        }

        public async Task<string> ConvertStringToHtml(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            // Substitui marcadores markdown por tags HTML correspondentes
            // # Titulo -> <h1>Titulo</h1>
            content = Regex.Replace(content, @"(^|\s)#\s(.+)", "$1<h1>$2</h1>");

            // **TEXTO** -> <strong>TEXTO</strong>
            content = Regex.Replace(content, @"\*\*(.+?)\*\*", "<strong>$1</strong>");

            // ***TEXTO*** -> <em>TEXTO</em>
            content = Regex.Replace(content, @"\*\*\*(.+?)\*\*\*", "<em>$1</em>");

            // \n\n -> <br>
            content = content.Replace("\n\n", "<br>");

            // [link](URL) -> <a href="URL">link</a>
            content = Regex.Replace(content, @"\[([^\]]+)\]\(([^)]+)\)", "<br><br><a href=\"$2\">$1</a>");

            // Quebra o markdown em linhas e adiciona <p> ao redor de cada linha
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                sb.Append("<p>").Append(line).Append("</p>");
            }

            return sb.ToString();
        }

        public async Task<string> ConvertToJsonHtml(knowledgeLibrary knowledgeLibrary)
        {
            var sb = new StringBuilder();

            await ConvertJsonToHtmlAsync(knowledgeLibrary, sb);
            var html = $"<!DOCTYPE html><html lang=\"pt-br\"><head><title></title><meta charset=\"utf-8\"></head><body>{sb.ToString()}</body></html>";

            return html;
        }

        public async Task ConvertJsonToHtmlAsync(knowledgeLibrary knowledgeLibrary, StringBuilder sb)
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
                    var urlImage = await _groupService.GetImageBannerByGroupId(content.image_data.Id);

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
    }

}
