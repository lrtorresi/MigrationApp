using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MigrationApp.Entities.Workplace.Group;

namespace MigrationApp.Entities.Workplace
{

    public class User
    {
        public List<string>? Schemas { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public int ItemsPerPage { get; set; }
        public List<Resource>? Resources { get; set; }
        public Name? Name { get; set; }
        public string? UserName { get; set; }
        public string? ProfileUrl { get; set; }
        public List<Email>? Emails { get; set; }
        public string? Id { get; set; }
        public string? Email { get; set; }
    }

    public class Resource
    {
        public Name? Name { get; set; }
        public string? UserName { get; set; }
        public string? ProfileUrl { get; set; }
        public List<Email>? Emails { get; set; }
        public string? Id { get; set; }
        public string? Email { get; set; }
        public bool administrator { get; set; }
        public string? Type { get; set; }
        public ResourceFile? ResourceFile { get; set; }
        public string? Description { get; set; }
        public Paging? Paging { get; set; }
    }

    public class Name
    {
        public string? Formatted { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenName { get; set; }
    }

    public class Email
    {
        public string? Value { get; set; }
        public string? Display { get; set; }
        public bool Primary { get; set; }

    }

    public class Users
    {
        public List<Resource>? Data { get; set; }
        public Paging? Paging { get; set; }
    }

    public class Seen
    {
        public List<UserDetails>? Data { get; set; }
        public Paging? Paging { get; set; }
    }

    public class UserDetails
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

}

