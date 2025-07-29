using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Workplace
{
    public class UsersResponse
    {
        [JsonProperty("data")]
        public List<User> Data { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }
    }
}
