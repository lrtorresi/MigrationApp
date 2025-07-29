using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Workplace
{
    public class knowledgeLibrary
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Status { get; set; }
        public Editor? Last_Editor { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public DateTime? LastUpdated { get; set; }
        public List<Json_Content>? Json_Content { get; set; }
        public int Size { get; set; }
    }

    public class Editor
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    public class ContentBlock
    {
        public string? Type { get; set; }
        public List<ContentItem>? Children { get; set; }
    }
    public class ContentItem
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
        public ImageData? image_data { get; set; }
        public int? ImageWidth { get; set; }
        public string? HAlign { get; set; }
        public ResourceFileData? FileData { get; set; }
    }

    public class ResourceFileData
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
    }

    public class Json_Content
    {
        public string? Type { get; set; }
        public List<Child>? Children { get; set; }
        public ImageData? image_data { get; set; }
        public int ImageWidth { get; set; }
        public string? HAlign { get; set; }
        public List<Resource>? Resources { get; set; }
    }

    public class Child
    {
        public string? Type { get; set; }
        public List<Child>? Children { get; set; }
        public string? Text { get; set; }
        public string? Href { get; set; }
        public ImageData? image_data { get; set; }
    }

    public class ImageData
    {
        public DateTime? CreatedTime { get; set; }
        public string? Id { get; set; }
    }

    public class ResourceFile
    {
        public string? Id { get; set; }
    }

    public class KnowledgeLibrary
    {
        public List<knowledgeLibrary>? Data { get; set; }
        public Paging? Paging { get; set; }
    }
}
