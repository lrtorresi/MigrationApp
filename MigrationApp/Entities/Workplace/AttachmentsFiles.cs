using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Workplace
{
    public class AttachmentsFiles
    {
        public List<DataItem>? Data { get; set; }
    }

    public class DataItem
    {
        public Subattachments? Subattachments { get; set; }
        public string? Type { get; set; }
        public string? description { get; set; }
        public Media? Media { get; set; }
    }

    public class Subattachments
    {
        public List<SubattachmentItem>? Data { get; set; }
    }

    public class SubattachmentItem
    {
        public Media? Media { get; set; }
        public Target? Target { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Url { get; set; }
    }

}
