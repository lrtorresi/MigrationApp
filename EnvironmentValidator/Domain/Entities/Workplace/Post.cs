using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace EnvironmentValidator.Domain.Entities.Workplace
{
    public class Action
    {
        public string? Name { get; set; }
        public string? Link { get; set; }
    }

    public class From
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
    }


    public class Target
    {
        public string? Id { get; set; }
        public string? Url { get; set; }
    }

    public class Comment
    {
        public string? CreatedTime { get; set; }
        public From? From { get; set; }
        public string? Message { get; set; }
        public string? Id { get; set; }
    }

    public class Comments
    {
        public List<Comment>? Data { get; set; }
    }


    public class Post
    {
        public string? Id { get; set; }
        public List<Action>? Actions { get; set; }
        public string? CreatedTime { get; set; }
        public From? From { get; set; }
        public string? Icon { get; set; }
        public bool IsHidden { get; set; }
        public string? Link { get; set; }
        public string? Message { get; set; }
        public string? ObjectId { get; set; }
        public string? PermalinkUrl { get; set; }
        public string? Picture { get; set; }
        public string? Full_picture { get; set; }
        public string? Story { get; set; }
        public Comments? Comments { get; set; }
        public User? User { get; set; }
        public Attachments? attachments { get; set; }
    }

    public class Attachment
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? MediaType { get; set; }
        public Image? MediaImage { get; set; }
        public string? Type { get; set; }
    }

    public class Cursors
    {
        public string? Before { get; set; }
        public string? After { get; set; }
    }


    public class Root
    {
        public List<Post>? Data { get; set; }
        public Paging? Paging { get; set; }
    }
}
