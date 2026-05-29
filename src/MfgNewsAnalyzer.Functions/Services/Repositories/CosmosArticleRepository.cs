using Azure;
using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using MfgNewsAnalyzer.Functions.Services.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MfgNewsAnalyzer.Functions.Services.Repositories
{
    public class CosmosArticleRepository :IArticleRepository
    {
        private readonly IOptions<CosmosOptions> _options;
        private readonly Container _container;
        private readonly ILogger<CosmosArticleRepository> _logger;

        public CosmosArticleRepository(CosmosClient client, IOptions<CosmosOptions> options, ILogger<CosmosArticleRepository> logger)
        {
            _options = options;
            _logger = logger;
            _container = client.GetContainer(_options.Value.Database, _options.Value.ContainerId);
        }

        public async Task SaveAsync(Article article, CancellationToken cancellationToken = default)
        {
            var response = await _container.CreateItemAsync(article, new PartitionKey(article.Publisher), cancellationToken: cancellationToken);
            return;
        }


        public async Task<bool> ExistByUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.url = @url")
                .WithParameter("@url", url);
            var iterator = _container.GetItemQueryIterator<int>(query);

            while(iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                return response.Resource.FirstOrDefault() > 0;
            }

            return false;
        }
    }
}
