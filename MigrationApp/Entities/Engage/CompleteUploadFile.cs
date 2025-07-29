using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Engage
{
    public class CompleteUploadFile
    {
        public string? ODataContext { get; set; }
        public string? ContentDownloadUrl { get; set; }
        public CreatedBy? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? ETag { get; set; }
        public string? Id { get; set; }
        public LastModifiedBy? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDateTime { get; set; }
        public string? Name { get; set; }
        public ParentReference? ParentReference { get; set; }
        public string? WebUrl { get; set; }
        public string? CTag { get; set; }
        public FileDetails? File { get; set; }
        public FileSystemInfo? FileSystemInfo { get; set; }
        public Media? Media { get; set; }
        public Photo? Photo { get; set; }
        public Shared? Shared { get; set; }
        public long? Size { get; set; }
    }

    public class CreatedBy
    {
        public Application? Application { get; set; }
        public User? User { get; set; }
    }

    public class LastModifiedBy
    {
        public Application? Application { get; set; }
        public User? User { get; set; }
    }

    public class Application
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
    }

    public class User
    {
        public string? Email { get; set; }
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
    }

    public class ParentReference
    {
        public string? DriveType { get; set; }
        public string? DriveId { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? SiteId { get; set; }
    }

    public class FileDetails
    {
        public Hashes? Hashes { get; set; }
        public bool? IrmEffectivelyEnabled { get; set; }
        public bool? IrmEnabled { get; set; }
        public string? MimeType { get; set; }
    }

    public class Hashes
    {
        public string? QuickXorHash { get; set; }
    }

    public class FileSystemInfo
    {
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? LastModifiedDateTime { get; set; }
    }

    public class Media
    {
        public string? AboutVisibility { get; set; }
        public string? AnalyticsVisibility { get; set; }
        public bool? AreExtendedFeaturesEnabled { get; set; }
        public string? ChatVisibility { get; set; }
        public Interactivity? Interactivity { get; set; }
        public bool? IsNoiseSuppressionControlShown { get; set; }
        public bool? IsWatermarkEnabled { get; set; }
        public bool? NoiseSuppressionEnabledByDefault { get; set; }
        public string? NotesVisibility { get; set; }
        public string? TableOfContentsVisibility { get; set; }
        public Viewpoint? Viewpoint { get; set; }
    }

    public class Interactivity
    {
        public bool? IsInteractiveContentShown { get; set; }
    }

    public class Viewpoint
    {
        public bool? AreReactionsAllowed { get; set; }
        public bool? IsAutomaticTranscriptionAllowed { get; set; }
        public bool? IsTranscriptionAllowed { get; set; }
        public bool? IsTranscriptionTranslationAllowed { get; set; }
    }

    public class Photo
    {
    }

    public class Shared
    {
        public List<string>? EffectiveRoles { get; set; }
        public string? Scope { get; set; }
    }
}
