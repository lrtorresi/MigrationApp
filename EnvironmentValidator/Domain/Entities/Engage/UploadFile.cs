using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Engage
{
    public class UploadFile
    {
        public string? Id { get; set; }
        public string? Network_Id { get; set; }
        public string? Url { get; set; }
        public string? Web_Url { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Original_Name { get; set; }
        public string? Full_Name { get; set; }
        public string? Description { get; set; }
        public string? Content_Type { get; set; }
        public string? Content_Class { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Owner_Id { get; set; }
        public bool Official { get; set; }
        public string? Storage_Type { get; set; }
        public string? Target_Type { get; set; }
        public string? Storage_State { get; set; }
        public string? Sharepoint_Id { get; set; }
        public string? Sharepoint_Web_Url { get; set; }
        public string? Small_Icon_Url { get; set; }
        public string? Large_Icon_Url { get; set; }
        public string? Download_Url { get; set; }
        public string? Thumbnail_Url { get; set; }
        public string? Preview_Url { get; set; }
        public string? Large_Preview_Url { get; set; }
        public string? Size { get; set; }
        public string? Owner_Type { get; set; }
        public DateTime? Last_Uploaded_At { get; set; }
        public string? Last_Uploaded_By_Id { get; set; }
        public string? Last_Uploaded_By_Type { get; set; }
        public string? Uuid { get; set; }
        public string? Transcoded { get; set; }
        public string? Streaming_Url { get; set; }
        public string? Path { get; set; }
        public string? YId { get; set; }
        public string? Overlay_Url { get; set; }
        public string? Privacy { get; set; }
        public string? GroupId { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string? ScaledUrl { get; set; }
        public ImageDetails? Image { get; set; }
        public string? LatestVersionId { get; set; }
        public string? Status { get; set; }
        public LatestVersionDetails? LatestVersion { get; set; }
        public StatsDetails? Stats { get; set; }
        public string? ZencoderJobId { get; set; }
        public string? FileUploadId { get; set; }
    }

    public class ImageDetails
    {
        public string? Url { get; set; }
        public string? Size { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class LatestVersionDetails
    {
        public string? Id { get; set; }
        public string? FileId { get; set; }
        public string? ContentType { get; set; }
        public string? Size { get; set; }
        public string? UploaderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Path { get; set; }
        public string? DownloadUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PreviewUrl { get; set; }
        public string? LargePreviewUrl { get; set; }
        public string? PostProcessedId { get; set; }
        public string? StreamingUrl { get; set; }
        public string? RevertUrl { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string? ScaledUrl { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? PreviewPath { get; set; }
        public string? LargePreviewPath { get; set; }
        public string? Status { get; set; }
    }

    public class StatsDetails
    {
        public int Followers { get; set; }
    }
}
