using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Entities.Global
{
    public static class GlobalConfig
    {
        public static string? TokenWorkplace { get; set; }
        public static string? TokenEngage { get; set; }
        public static string? ClientIdEngage { get; set; }
        public static string? ApiImpar { get; set; } = "https://localhost:7252/api";
        public static string? Domain { get; set; }
        public static int TotalCommunityCout { get; set; }
        public static string? TenatId { get; set; }
        public static string? ClientId { get; set; }
        public static string? Username { get; set; }
        public static string? Password { get; set; }
    }
}
