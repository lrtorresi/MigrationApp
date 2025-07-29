using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Engage
{
    public class UploadLargeFileEngage
    {
        public CompleteSharePointUploadSessionData Data { get; set; }

        public class CompleteSharePointUploadSessionData
        {   

            public class CompleteSharePointUploadSession
            {
                public VideoFile File { get; set; }

                public class VideoFile
                {
                    public string __typename { get; set; }
                    public string Id { get; set; }
                    public string DisplayName { get; set; }
                    public string FileDescription { get; set; }
                    public string DownloadLink { get; set; }
                    public string PreviewImage { get; set; }
                    public string DatabaseId { get; set; }
                    public string State { get; set; }
                    public int Width { get; set; }
                    public int Height { get; set; }
                    
                    public object AzureVideoSource { get; set; }
                    

                    public class Group
                    {
                        public string DatabaseId { get; set; }
                        public string TelemetryId { get; set; }
                        public string Id { get; set; }
                        public string OfficeUnifiedGroupId { get; set; }
                        
                        public string AvatarUrlTemplate { get; set; }
                        public bool HasDefaultAvatar { get; set; }
                        public string DisplayName { get; set; }
                        public string Description { get; set; }
                        public bool IsExternal { get; set; }
                        public bool IsOfficial { get; set; }
                        public int GuestsCount { get; set; }
                        public string ThreadStarterSmallFileUploadUrl { get; set; }
                        public bool IsNetworkQuestionGroup { get; set; }
                        public bool IsMultiTenantOrganizationGroup { get; set; }
                        public bool IsMoveThreadToThisGroupRestricted { get; set; }

                        public class Network
                        {
                            public string Id { get; set; }
                        }
                    }

                    public class SharePointVideoSource
                    {
                        public string EmbeddablePreviewUrl { get; set; }
                        public string StreamUrlProvider { get; set; }
                    }
                }
            }
        }
    }

}
