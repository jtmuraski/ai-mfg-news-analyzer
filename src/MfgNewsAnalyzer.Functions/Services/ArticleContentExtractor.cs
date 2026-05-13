using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using Microsoft.Extensions.Logging;
using SmartReader;

namespace MfgNewsAnalyzer.Functions.Services;

public class ArticleContentExtractor : IArticleContentExtractor
{
    private readonly ILogger<ArticleContentExtractor> _logger;

    public ArticleContentExtractor(ILogger<ArticleContentExtractor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Takes the URL from a publication article and reads it using the SmartReader library. This strips out the HTML and unnecessary formatting to make it easier for the Claude API to analyze the article.
    /// </summary>
    /// <param name="url">URL of the article to be read</param>
    /// <returns></returns>
    public async Task<StrippedArticle> ExtractContent(string url, CancellationToken cancellationToken)
    {
        Reader reader = new SmartReader.Reader(url);
        SmartReader.Article article = await reader.GetArticleAsync();

        if (article.IsReadable && article.Completed)
        {
            StrippedArticle strippedArticle = new StrippedArticle
            {
                Author = article.Author,
                Title = article.Title,
                Url = string.IsNullOrEmpty(article.Uri.ToString()) ? url : article.Uri.ToString(),
                RawText = article.TextContent,
                Length = article.Length,
                TimeToRead = article.TimeToRead,
                IsSuccess = true,
                Errors = new List<Exception>()
            };

            return strippedArticle;
        }
        else
        {
            _logger.LogInformation("The article at {Url} is not readable. See below for deatils", url);
            foreach(Exception ex in article.Errors)
            {
                _logger.LogInformation("{ExceptionMessage}", ex.Message);
            }

            return new StrippedArticle
            {
                Url = url,
                IsSuccess = false,
                Errors = article.Errors
            };
        }
    }
}