using ClosedXML.Excel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using EnvironmentValidator.Domain.Entities.Workplace;

namespace EnvironmentValidator.Helpers
{
    public class GenerateXlsx
    {
        public async Task<string> ExportToXlsx(object resultEnvironment)
        {
            var type = resultEnvironment.GetType();
            var dataGroups = type.GetProperty("Groups")?.GetValue(resultEnvironment);
            var dataLibrary = type.GetProperty("KnowledgeLibrary")?.GetValue(resultEnvironment);


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Comunidades");

                // Definir os títulos das colunas
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nome";
                worksheet.Cell(1, 3).Value = "Tamanho (MB)";

                // Preencher as linhas com os dados de resultEnvironment
                int currentRow = 2;
                foreach (var item in (IEnumerable<object>)dataGroups)
                {
                    worksheet.Cell(currentRow, 1).Value = item.GetType().GetProperty("Id")?.GetValue(item)?.ToString();
                    worksheet.Cell(currentRow, 2).Value = item.GetType().GetProperty("Name")?.GetValue(item)?.ToString();
                    worksheet.Cell(currentRow, 3).Value = Convert.ToInt32(item.GetType().GetProperty("TotalMb")?.GetValue(item)?.ToString());
                    currentRow++;
                }

                var worksheet2 = workbook.Worksheets.Add("Bibliotecas do Conhecimento");

                // Definir os títulos das colunas
                worksheet2.Cell(1, 1).Value = "ID";
                worksheet2.Cell(1, 2).Value = "Nome";
                worksheet2.Cell(1, 3).Value = "Tamanho (MB)";
                
                int currentRow2 = 2;
                if(dataLibrary is not null)
                {
                    foreach (var item in (IEnumerable<object>)dataLibrary)
                    {
                        worksheet2.Cell(currentRow2, 1).Value = item.GetType().GetProperty("Id")?.GetValue(item)?.ToString();
                        worksheet2.Cell(currentRow2, 2).Value = item.GetType().GetProperty("Title")?.GetValue(item)?.ToString();
                        worksheet2.Cell(currentRow2, 3).Value = item.GetType().GetProperty("Size")?.GetValue(item)?.ToString();
                        currentRow2++;
                    }
                }
                

                // Obtém a pasta de Downloads do usuário
                var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                // Define o nome do arquivo
                var filePath = Path.Combine(downloadsPath, "Migrador Workplace - Dados Exportados.xlsx");

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    workbook.SaveAs(stream);
                }

                return downloadsPath;
            }
        }
    }
}
