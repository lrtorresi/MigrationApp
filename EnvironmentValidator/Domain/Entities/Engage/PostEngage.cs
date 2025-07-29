using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Engage
{
    public class PostEngage
    {
        
        public List<Message>? Messages { get; set; }
        public List<Reference>? References { get; set; }
        public List<object>? ExternalReferences { get; set; }
        public Meta? Meta { get; set; }
    }

    public class Message
    {
        public string? Id { get; set; }
        public string? SenderId { get; set; }
        public object? DelegateId { get; set; }
        public object? RepliedToId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public int NetworkId { get; set; }
        public string? MessageType { get; set; }
        public string? SenderType { get; set; }
        public string? Url { get; set; }
        public string? WebUrl { get; set; }
        public string? GroupId { get; set; }
        public Body? Body { get; set; }
        public string? ThreadId { get; set; }
        public string? ClientType { get; set; }
        public string? ClientUrl { get; set; }
        public bool SystemMessage { get; set; }
        public bool DirectMessage { get; set; }
        public object? ChatClientSequence { get; set; }
        public object? Language { get; set; }
        public List<object>? NotifiedUserIds { get; set; }
        public string? Privacy { get; set; }
        public List<object>? Attachments { get; set; }
        public LikedBy? LikedBy { get; set; }
        public bool SupplementalReply { get; set; }
        public string? ContentExcerpt { get; set; }
        public string? GroupCreatedId { get; set; }
    }

    public class Body
    {
        public string? Parsed { get; set; }
        public string? Plain { get; set; }
        public string? Rich { get; set; }
    }

    public class LikedBy
    {
        public int Count { get; set; }
        public List<object>? Names { get; set; }
    }

    public class Reference
    {
        public string? Type { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? State { get; set; }
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }
        public int NetworkId { get; set; }
        public string? MugshotUrl { get; set; }
        public string? MugshotRedirectUrl { get; set; }
        public string? MugshotUrlTemplate { get; set; }
        public string? MugshotRedirectUrlTemplate { get; set; }
        public string? Url { get; set; }
        public string? WebUrl { get; set; }
        public DateTime ActivatedAt { get; set; }
        public bool AutoActivated { get; set; }
        public Stats? Stats { get; set; }
        public string? Email { get; set; }
        public bool AadGuest { get; set; }
    }



    public class Meta
    {
        public bool OlderAvailable { get; set; }
        public int RequestedPollInterval { get; set; }
        public object? LastSeenMessageId { get; set; }
        public string? CurrentUserId { get; set; }
        public List<object>? FollowedReferences { get; set; }
        public List<object>? Ymodules { get; set; }
        public object? NewestMessageDetails { get; set; }
        public string? FeedName { get; set; }
        public string? FeedDesc { get; set; }
        public bool DirectFromBody { get; set; }
    }
}
