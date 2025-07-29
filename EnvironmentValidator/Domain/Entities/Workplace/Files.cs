using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Workplace
{
    public class Files
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Size { get; set; }
    }

    public class FileManager
    {
        // Lista estática para armazenar os dados dos arquivos durante a execução da aplicação
        public static List<Files> FilesDataList { get; } = new List<Files>();
    }
}
