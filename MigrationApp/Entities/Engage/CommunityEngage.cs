using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Engage
{
    public class CommunityEngage
    {
        public string? Type { get; set; }
        public string? Id { get; set; }
        public string? ResourceId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Network_id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Privacy { get; set; }
        public string? Url { get; set; }
        public string? WebUrl { get; set; }
        public string? MugshotUrl { get; set; }
        public string? MugshotRedirectUrl { get; set; }
        public string? MugshotUrlTemplate { get; set; }
        public string? MugshotRedirectUrlTemplate { get; set; }
        public object? MugshotId { get; set; }
        public string? ShowInDirectory { get; set; }
        public string? CreatedAt { get; set; }
        public int AADGuests { get; set; }
        public string? Color { get; set; }
        public bool External { get; set; }
        public bool Moderated { get; set; }
        public string? HeaderImageUrl { get; set; }
        public string? Category { get; set; }
        public string? DefaultThreadStarterType { get; set; }
        public bool RestrictedPosting { get; set; }
        public bool CompanyGroup { get; set; }
        public string? CreatorType { get; set; }
        public long CreatorId { get; set; }
        public string? State { get; set; }
        public bool Member { get; set; }
        public bool Pending { get; set; }
        public bool Admin { get; set; }
        public bool HasAdmin { get; set; }
        public bool CanAddMembers { get; set; }
        public bool CanInvite { get; set; }
        public string? NetworkName { get; set; }
        public bool CanStartThread { get; set; }
        public bool Favorite { get; set; }
        public string ThreadStarterSmallFileUploadUrl { get; set; }

        public Data Data { get; set; }
    }

    public class Data
    {
        public CreateGroup CreateGroup { get; set; }
        public Group Group { get; set; }
    }

    public class CreateGroup
    {
        public Group Group { get; set; }
    }

    public class Group
    {
        public string DatabaseId { get; set; }
        public string TelemetryId { get; set; }
        public string Id { get; set; }
        public string OfficeUnifiedGroupId { get; set; }
        public Network Network { get; set; }
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
        public string Privacy { get; set; }
        public string State { get; set; }
        public string ViewerMembershipStatus { get; set; }
        public bool ViewerIsAdmin { get; set; }
        public bool ViewerHasFavorited { get; set; }
        public bool ViewerCanStartThread { get; set; }
        public bool IsThreadStarterRestricted { get; set; }
        public bool IsDynamicMembership { get; set; }
        public bool IsAllCompanyGroup { get; set; }
        public string Category { get; set; }
        public object Classification { get; set; } 
        public object SensitivityLabel { get; set; } 
        public List<Network> ParticipatingNetworks { get; set; }
        public string CoverImageUrlTemplate { get; set; }
        public bool HasDefaultCoverImage { get; set; }
        public string DefaultCoverImageUrlTemplate { get; set; }
        public string HiddenForNetworkInDiscovery { get; set; }
        public string HiddenForViewerInDiscovery { get; set; }
        public bool ViewerCanMarkAsMultiTenantOrganizationGroup { get; set; }
    }

    public class Network
    {
        public string Id { get; set; }
    }

    public class CommunityEngageResponse
    {
        [JsonProperty("value")]
        public List<CommunityEngageItem> Value { get; set; }
    }

    public class CommunityEngageItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("privacy")]
        public string Privacy { get; set; }

        [JsonProperty("groupId")]
        public string GroupId { get; set; }
    }
}
