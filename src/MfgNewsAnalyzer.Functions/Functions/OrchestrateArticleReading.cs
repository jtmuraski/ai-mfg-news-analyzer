using System;
using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MfgNewsAnalyzer.Functions.Functions;

public class OrchestrateArticleReading
{
    private readonly ILogger _logger;
    private readonly IRssFeedReader _feedReader;
    private readonly IArticleRepository _articleRepository;
    private readonly IArticleAnalyzer _articleAnalyzer;
    private readonly IArticleContentExtractor _contentExtractor;

    public OrchestrateArticleReading(ILoggerFactory loggerFactory, 
        IRssFeedReader feedReader, 
        IArticleRepository articleRepository, 
        IArticleAnalyzer articleAnalyzer,
        IArticleContentExtractor contentExtractor)
    {
        _logger = loggerFactory.CreateLogger<OrchestrateArticleReading>();
        _feedReader = feedReader;
        _articleRepository = articleRepository;
        _articleAnalyzer = articleAnalyzer;
        _contentExtractor = contentExtractor;
    }

    [Function("OrchestrateArticleReading")]
    public async Task Run([TimerTrigger("0 0 12 * * 1")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }

        // RSS Feeds
        Dictionary<string, string> rssLinks = new Dictionary<string, string>()
        {
            // "https://www.industryweek.com/rss"              // XML parsing error
            {"Plant Engineering", "https://www.plantengineering.com/feed/"},        // This one works just fine
            {"Manufacturing.Net", "https://www.manufacturing.net/feed" },
            {"Manufacturing Today", "https://manufacturing-today.com/feed/" },
            {"ManufacturingDrive", "https://www.manufacturingdive.com/feeds/news/" },
            {"Assembly Mag",  "https://www.assemblymag.com/rss/17" }           
        };

        // Call the RSS Feed Reader
        List<Article> articlesToSave = new List<Article>();
        foreach(var feedLink in rssLinks)
        {
            _logger.LogInformation($"Getting feed for {feedLink.Key} from {feedLink.Value}");
            var feedResponse = await _feedReader.ReadFeedAsync(feedLink.Value, feedLink.Key);

            _logger.LogInformation(feedResponse.Count == 0
                ? $"No articles found for {feedLink.Key}"
                : $"Found {feedResponse.Count} articles for {feedLink.Key}");

            List<Article> analyzedArticles = new List<Article>();
            // Verify that the article is not already in the database
            foreach (var article in feedResponse)
            {
                bool isUnique = await _articleRepository.ExistByUrlAsync(article.Url);
                if(isUnique)
                {
                    _logger.LogInformation($"Article '{article.Title}' is unique and will be added be analyzed.");
                    StrippedArticle strippedArticle = await _contentExtractor.ExtractContentAsync(article.Url);
                    if (strippedArticle.IsSuccess)
                    {
                        // If the article was successfully read and stripped, run it through the analyzer
                        AiAnalysis aiAnalysis = await _articleAnalyzer.AnalyzeAsync(strippedArticle.RawText);
                        if (aiAnalysis != null)
                        {
                            // Combine all the gathered information into a single Article object. This object will be saved to the DB
                            Article analyzedArticle = article with
                            {
                                RawText = strippedArticle.RawText,
                                AiAnalysisResults = aiAnalysis
                            };
                            articlesToSave.Add(analyzedArticle);
                        }

                    }
                }
            }
        }

        // Save each of the analyzed articles to the database
        foreach(Article article in articlesToSave)
        {
            await _articleRepository.SaveAsync(article);
            _logger.LogInformation($"Article '{article.Title}' saved to the database.");
        }
    }
}