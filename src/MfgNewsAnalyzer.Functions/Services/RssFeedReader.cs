using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MfgNewsAnalyzer.Functions.Services.Options;
using System.Xml;

namespace MfgNewsAnalyzer.Functions.Services
{
    public class RssFeedReader : IRssFeedReader
    {
        private readonly IOptions< RssFeedReaderOptions> _options;
        private readonly ILogger<RssFeedReader> _logger;

        public RssFeedReader(IOptions<RssFeedReaderOptions> options, ILogger<RssFeedReader> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Read an RSS feed from the specified URL. If successfully read, parse the feed into usable data.
        /// </summary>
        /// <param name="url">Url of the RSS Feed</param>
        /// <param name="publisher">The name of the publication that publishes and manages the RSS feed</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Article>> ReadFeedAsync(string url, string publisher, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reading RSS feed from {Url} for publisher {Publisher}", url, publisher);
            List<Article> articles = new List<Article>();
            int skippedItems = 0;

            try
            {
                var feed = await FeedReader.ReadAsync(url, cancellationToken);

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
            }
            catch (XmlException ex)
            {
                _logger.LogError(ex, "Error parsing feed item: {Publisher}. Skipping this publisher.", publisher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing feed item: {Publisher}. Skipping this publisher.", publisher);
            }

            _logger.LogInformation("Finished reading feed from {Publisher}. Articles Found {ArticlesFound} and skipped {SkippedItems} items.", publisher, articles.Count, skippedItems);
            return articles;
        }
    }
}
