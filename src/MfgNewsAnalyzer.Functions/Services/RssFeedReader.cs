using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.Eventing.Reader;

namespace MfgNewsAnalyzer.Functions.Services
{
    public class RssFeedReader : IRssFeedReader
    {
        private readonly RssFeedReaderOptions _options;
        private readonly ILogger<RssFeedReader> _logger;

        public RssFeedReader(RssFeedReaderOptions options, ILogger<RssFeedReader> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Read an RSS feed from the specified URL. If successfully read, parse the feed into usable data.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="publisher"></param>
        /// <param name="canellationToken"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Article>> ReadFeedAsync(string url, string publisher, CancellationToken canellationToken)
        {
            _logger.LogInformation("Reading RSS feed from {Url} for publisher {Publisher}", url, publisher);

            var feed = await FeedReader.ReadAsync(url, canellationToken);

            List<Article> articles = new List<Article>();
            int skippedItems = 0;

            foreach (var item in feed.Items)
            {
                if (string.IsNullOrEmpty(item.Link))
                {
                    string title = string.IsNullOrEmpty(item.Title) ? "No Title" : item.Title;
                    _logger.LogWarning("Skipping feed item with missing link: {Title}", title);
                    skippedItems++;
                    continue;
                }
                

                articles.Add(new Article
                {
                    Id = Guid.NewGuid().ToString(),
                    Publisher = publisher,
                    Url = item.Link,
                    PulledDate = DateTime.UtcNow,
                    Title = item.Title ?? "No Title",
                    Description = item.Description,
                    Author = item.Author,
                    PublishDate = item.PublishingDate
                });
            }

            _logger.LogInformation("Finsihed reading feed from {Publisher}. Artciles Found {ArticlesFound} and skipped {SkippedItems} items.", articles.Count, skippedItems);
            return articles;
        }
    }
}
