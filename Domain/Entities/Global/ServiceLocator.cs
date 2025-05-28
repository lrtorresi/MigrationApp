using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Global
{
    public class ServiceLocator
    {
        public static IServiceProvider ServiceProvider { get; set; }
    }
}
