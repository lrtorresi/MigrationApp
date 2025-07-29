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
    public class GenerateXlsxFIles
    {
        public async Task<string> ExportToXlsxFiles()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Comunidades");

                // Definir os títulos das colunas
                worksheet.Cell(1, 1).Value = "URL";
                worksheet.Cell(1, 2).Value = "Type";
                worksheet.Cell(1, 3).Value = "Tamanho (MB)";

                // Preencher as linhas com os dados de resultEnvironment
                int currentRow = 2;
                foreach (var item in FileManager.FilesDataList)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Name;
                    worksheet.Cell(currentRow, 2).Value = item.Type;
                    worksheet.Cell(currentRow, 3).Value = item.Size;
                    currentRow++;
                }

                // Obtém a pasta de Downloads do usuário
                var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                // Define o nome do arquivo
                var filePath = Path.Combine(downloadsPath, "Files - Dados Exportados.xlsx");

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    workbook.SaveAs(stream);
                }

                return downloadsPath;
            }
        }
    }
}
