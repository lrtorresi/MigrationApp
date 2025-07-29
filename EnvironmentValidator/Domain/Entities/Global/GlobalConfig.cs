using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Global
{
    public static class GlobalConfig
    {
        public static string? TokenWorkplace { get; set; }
        public static string? TokenEngage { get; set; }
        public static string? ClientIdEngage { get; set; }
        public static string? ApiImpar { get; set; } = "https://localhost:7252/api";
        public static string? Domain { get; set; }
    }
}
