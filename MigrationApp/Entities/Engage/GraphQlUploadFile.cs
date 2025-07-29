using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Engage
{
    public class GraphQlUploadFile
    {
        public Data data { get; set; }

        public class Data
        {
            public CreateGroupUploadSessionForNetwork createGroupUploadSessionForNetwork { get; set; }
        }

        public class CreateGroupUploadSessionForNetwork
        {
            public UploadSession uploadSession { get; set; }
        }

        public class UploadSession
        {
            public string __typename { get; set; }
            public string sessionId { get; set; }
            public string fileId { get; set; }
            public string fileVersionId { get; set; }
            public string url { get; set; }
        }
    }

}
