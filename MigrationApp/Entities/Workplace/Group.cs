using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MigrationApp.Entities.Workplace.Post;

namespace MigrationApp.Entities.Workplace
{

    public class Cover
    {
        public string? CoverId { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public string? Source { get; set; }
    }

    public class Owner
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
    }

    public class Group
    {
        public string? Id { get; set; }
        public bool Archived { get; set; }
        public Cover? Cover { get; set; }
        public string? Icon { get; set; }
        public bool IsWorkplaceDefault { get; set; }
        public string? Name { get; set; }
        public Owner? Owner { get; set; }
        public string? Privacy { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string? Description { get; set; }
    }



    public class Paging
    {
        public Cursors? Cursors { get; set; }
        public string? Next {  get; set; }
    }

    public class Images
    {
        public string? source { get; set; }

    }

    public class Groups
    {
        public List<Group>? Data { get; set; }
        public Paging? Paging { get; set; }
        public List<Images>? Images { get; set; }

    }

}

