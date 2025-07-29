using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Workplace
{
    public class PostComplete
    {
        public string? Id { get; set; }
        public string? Message { get; set; }
        public string? FullPicture { get; set; }
        public string? Picture { get; set; }
        public AttachmentsFiles? Attachments { get; set; }
        public string? created_time { get; set; }
        public string? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public From? From { get; set; }
        public string? Link { get; set; }
        public string? StatusType { get; set; }
        public string? Type { get; set; }
        public string? ObjectId { get; set; }
        public Reactions? Reactions { get; set; }
        public Comments? Comments { get; set; }
        public bool? IsPublished { get; set; }
        public string? PermalinkUrl { get; set; }
    }

    public class Attachments
    {
        public List<Data>? Data { get; set; }
    }

    public class Media
    {
        public Image? Image { get; set; }
        public string? source { get; set; }
    }


    public class Data
    {
        public Media? Media { get; set; }
        public string? MediaType { get; set; }
        public string? Type { get; set; }
        public string? Url { get; set; }

    }



    public class Image
    {
        public int? Height { get; set; }
        public string? Src { get; set; }
        public int? Width { get; set; }
    }


    public class Reactions
    {
        public List<ReactionData>? Data { get; set; }
        public Paging? Paging { get; set; }
    }

    public class ReactionData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
    }

    public class CommentData
    {
        public DateTime? CreatedTime { get; set; }
        public From? From { get; set; }
        public string? Message { get; set; }
        public string? Id { get; set; }
    }
}
