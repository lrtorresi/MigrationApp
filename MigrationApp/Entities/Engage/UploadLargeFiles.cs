using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Engage
{
    public class UploadLargeFiles
    {
        public string? url { get; set; }
        public string? filename { get; set; }
        public long uploaded_file_id { get; set; }
        public long uploaded_file_version_id { get; set; }
        public bool is_new_file { get; set; }
        public string? storage_type { get; set; }
        public long sas_token_expiration_time { get; set; }
        public string? sas_validator { get; set; }
        public string? web_url { get; set; }
        public string? Id { get; set; }
        public string? sharepoint_id { get; set; }
    }
}
