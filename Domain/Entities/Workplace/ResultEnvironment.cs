using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Workplace
{
    public class ResultEnvironment
    {
        public int TotalUsers { get; set; }
        public int TotalGroups { get; set; }
        public int TotalPosts { get; set; }
        public int TotalKnowledgeLibrary { get; set; }
    }
}
