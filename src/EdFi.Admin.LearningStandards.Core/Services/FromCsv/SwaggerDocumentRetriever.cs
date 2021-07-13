using System;
using System.Net;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    public class SwaggerDocumentRetriever : ISwaggerDocumentRetriever
    {
        private readonly ILogger<SwaggerDocumentRetriever> _logger;

        public SwaggerDocumentRetriever(ILogger<SwaggerDocumentRetriever> logger)
        {
            _logger = logger;
        }

        public async Task<string> LoadJsonString(string metaDataUri)
        {
            string swaggerDocument;
            using var webClient = new WebClient();
            {
                _logger.LogInformation($"Loading swagger document from {metaDataUri}.");
                swaggerDocument = await webClient.DownloadStringTaskAsync(new Uri(metaDataUri));
            }
            return swaggerDocument;
        }
    }
}
