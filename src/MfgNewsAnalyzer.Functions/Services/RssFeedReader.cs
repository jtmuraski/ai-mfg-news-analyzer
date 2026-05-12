using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MfgNewsAnalyzer.Functions.Services
{
    public class RssFeedReader : IRssFeedReader
    {
        private readonly RssFeedReaderOptions _options;
        private readonly ILogger<RssFeedReader> _logger;

        public Task<IReadOnlyList<Article>> ReadFeedAsync(string url, string publisher, CancellationToken canellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
