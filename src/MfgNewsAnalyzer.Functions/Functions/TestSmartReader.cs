using MfgNewsAnalyzer.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using MfgNewsAnalyzer.Core.Models;

namespace MfgNewsAnalyzer.Functions.Functions;

public class TestSmartReader
{
    private readonly ILogger<TestSmartReader> _logger;
    private readonly IRssFeedReader _feedReader;
    private readonly IArticleContentExtractor _smartReader;

    public TestSmartReader(ILogger<TestSmartReader> logger, IRssFeedReader feedReader, IArticleContentExtractor smartReader)
    {
        _logger = logger;
        _feedReader = feedReader;
        _smartReader = smartReader;
    }

    [Function(nameof(TestSmartReader))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        const string feedUrl = "https://www.plantengineering.com/feed/";
        const string publisherName = "Plant Engineering";

        _logger.LogInformation("TestSmartReader invoked");

        // Get and Parse the RSS feed
        var articles = await _feedReader.ReadFeedAsync(feedUrl, publisherName, cancellationToken);

        List<Article> strippedArticles = new List<Article>();
        List<Article> unreadableArticles = new List<Article>();

        if (articles.Count > 0)
        {
            _logger.LogInformation("{ArticleCount} articles found in the feed.", articles.Count);

            foreach(Article article in articles)
            {
                StrippedArticle strippedArticle = await _smartReader.ExtractContentAsync(article.Url, cancellationToken);
                
                if(strippedArticle.IsSuccess)
                {
                    _logger.LogInformation("Successfully stripped article at {Url}", article.Url);
                    string updatedTitle = (string.IsNullOrEmpty(article.Title) && !string.IsNullOrEmpty(strippedArticle.Title)) ? strippedArticle.Title : article.Title;

                    var updatedArticle = article with
                    {
                        Title = updatedTitle,
                        RawText = strippedArticle.RawText
                    };
                    strippedArticles.Add(updatedArticle);
                }
                else
                {
                    unreadableArticles.Add(article);
                }

            }
        }

        return new OkObjectResult(new
        {
            FeedUrl = feedUrl,
            Publisher = publisherName,
            ArticleCount = articles.Count,
            StrippedArticles = strippedArticles,
            UnreadableArticles = unreadableArticles
        });
    }
}
