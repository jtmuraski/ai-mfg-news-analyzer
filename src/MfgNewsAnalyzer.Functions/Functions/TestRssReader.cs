using CodeHollow.FeedReader;
using MfgNewsAnalyzer.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Functions.Functions;

public class TestRssReader
{
    private readonly IRssFeedReader _feedReader;
    private readonly ILogger<TestRssReader> _logger;

    public TestRssReader(IRssFeedReader feedReader, ILogger<TestRssReader> logger)
    {
        _feedReader = feedReader;
        _logger = logger;
    }

    [Function(nameof(TestRssReader))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        const string feedUrl = "https://www.plantengineering.com/feed/";
        const string publisherName = "Plant Engineering";

        _logger.LogInformation("TestRssReader invoked");

        var articles = await _feedReader.ReadFeedAsync(feedUrl, publisherName, cancellationToken);

        return new OkObjectResult(new
        {
            FeedUrl = feedUrl,
            Publisher = publisherName,
            ArticleCount = articles.Count,
            Articles = articles
        });
    }
}
