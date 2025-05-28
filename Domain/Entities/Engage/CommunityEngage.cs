using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Engage
{
    public class CommunityEngage
    {
        public string? Type { get; set; }
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int Network_id { get; set; }
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
    }
}
