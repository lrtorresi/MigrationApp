using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Engage
{
    public class UserEngage
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? Group_Id { get; set; }
        public string? User_Id { get; set; }
        public string? State { get; set; }
        public string? added_by_user_id { get; set; }



        public int NetworkId { get; set; }

        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public string? Interests { get; set; }
        public string? Summary { get; set; }
        public string? Expertise { get; set; }
        public string? FullName { get; set; }
        public DateTime ActivatedAt { get; set; }
        public bool AutoActivated { get; set; }
        public bool ShowAskForPhoto { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NetworkName { get; set; }
        public List<string>? NetworkDomains { get; set; }
        public string? Url { get; set; }
        public string? WebUrl { get; set; }
        public string? Name { get; set; }
        public string? MugshotUrl { get; set; }
        public string? MugshotRedirectUrl { get; set; }
        public string? MugshotUrlTemplate { get; set; }
        public string? MugshotRedirectUrlTemplate { get; set; }
        public string? BirthDate { get; set; }
        public string? BirthDateComplete { get; set; }
        public string? Timezone { get; set; }
        public List<string>? ExternalUrls { get; set; }
        public bool Admin { get; set; }
        public bool VerifiedAdmin { get; set; }
        public bool M365YammerAdmin { get; set; }
        public bool SupervisorAdmin { get; set; }
        public bool O365TenantAdmin { get; set; }
        public bool AnswersAdmin { get; set; }
        public bool CorporateCommunicator { get; set; }
        public bool CanBroadcast { get; set; }
        public string? Department { get; set; }
        public string? Email { get; set; }
        public bool Guest { get; set; }
        public bool AadGuest { get; set; }
        public bool CanViewDelegations { get; set; }
        public bool CanCreateNewNetwork { get; set; }
        public bool CanBrowseExternalNetworks { get; set; }
        public string? ReactionAccentColor { get; set; }
        public string? SignificantOther { get; set; }
        public string? KidsNames { get; set; }
        public List<string>? PreviousCompanies { get; set; }
        public List<string>? Schools { get; set; }
        public Contact? Contact { get; set; }
        public Stats? Stats { get; set; }
        public Settings? Settings { get; set; }
        public bool ShowInviteLightbox { get; set; }
    }

    public class Contact
    {
        public IM? Im { get; set; }
        public List<string>? PhoneNumbers { get; set; }
        public List<EmailAddress>? EmailAddresses { get; set; }
        public bool HasFakeEmail { get; set; }
    }

    public class IM
    {
        public string? Provider { get; set; }
        public string? Username { get; set; }
    }

    public class EmailAddress
    {
        public string? Type { get; set; }
        public string? Address { get; set; }
    }

    public class Stats
    {
        public int Updates { get; set; }
        public int Following { get; set; }
        public int Followers { get; set; }
    }

    public class Settings
    {
        public string? XdrProxy { get; set; }
    }

    public class Auth
    {
        public string? UserId { get; set; }
        public int NetworkId { get; set; }
        public string? NetworkPermalink { get; set; }
        public string? NetworkName { get; set; }
        public bool NetworkCanonical { get; set; }
        public bool NetworkPrimary { get; set; }
        public string? Token { get; set; }
        public string? Secret { get; set; }
        public bool ViewMembers { get; set; }
        public bool ViewGroups { get; set; }
        public bool ViewMessages { get; set; }
        public bool ViewSubscriptions { get; set; }
        public bool ModifySubscriptions { get; set; }
        public bool ModifyMessages { get; set; }
        public bool ViewTags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime AuthorizedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
