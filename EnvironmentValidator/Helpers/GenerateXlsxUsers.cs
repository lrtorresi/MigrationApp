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
    public class GenerateXlsxUsers
    {
        public async Task<string> ExportToXlsxUsers()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");

                // Definir os títulos das colunas
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Email";

                // Preencher as linhas com os dados de resultEnvironment
                int currentRow = 2;
                foreach (var item in UserManager.UsersDataList)
                {
                    worksheet.Cell(currentRow, 1).Value = item.Id;
                    worksheet.Cell(currentRow, 2).Value = item.Name.Formatted;
                    worksheet.Cell(currentRow, 3).Value = item.UserName;
                    currentRow++;
                }

                // Obtém a pasta de Downloads do usuário
                var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                // Define o nome do arquivo
                var filePath = Path.Combine(downloadsPath, "Users - Dados Exportados.xlsx");

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    workbook.SaveAs(stream);
                }

                return downloadsPath;
            }
        }
    }
}
