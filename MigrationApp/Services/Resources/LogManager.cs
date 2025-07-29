using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationApp.Services.Resources
{
    public class LogManager
    {
        private readonly ILogger<LogManager> _logger;

        public LogManager(ILogger<LogManager> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        public void LogObject(object obj)
        {
            // Serializar o objeto como JSON para fins de logging
            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            _logger.LogInformation("Objeto: {0}", json);
        }
    }
}
