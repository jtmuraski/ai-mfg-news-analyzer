using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MfgNewsAnalyzer.Functions.Functions
{
    public class TestCosmosRepo
    {
        private readonly ILogger<TestCosmosRepo> _logger;
        private readonly IArticleRepository _repo;

        public TestCosmosRepo(ILogger<TestCosmosRepo> logger, IArticleRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        [Function(nameof(TestCosmosRepo))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            // Create a dummy Article to test Cosmos DB connectivity
            Article testArticle = new Article()
            {
                Id = Guid.NewGuid().ToString(),
                Publisher = "Test Publisher",
                Url = "https://example.com/test-article",
                PulledDate = DateTime.UtcNow,
                Title = "Test Article Title",
                Description = "This is a test article for Cosmos DB connectivity.",
                Author = "John Doe",
                PublishDate = DateTime.UtcNow.AddDays(-1),
                RawText = "This is the raw text of the test article.",
                AiAnalysisResults = new AiAnalysis()
                {
                    ClaudeSummary = "This is a summary of the test article.",
                    Sentiment = 1,
                    Tags = new List<string> { "test", "article", "cosmos db" },
                    Recommendation = 4
                }
            };

            bool articleExists = await _repo.ExistByUrlAsync(testArticle.Url, cancellationToken);
            if (articleExists)
            {
                _logger.LogInformation("Test article already exists in the database. Check database before trying again");
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            else
            {
                await _repo.SaveAsync(testArticle, cancellationToken);
                _logger.LogInformation("Test article added to the database successfully.");

                // NOTE: We do not need to wait to ensure propgation here. Because Cosmos DB uses SESSION CONSISTENCy by default.
                //       So since the same client that did the save is reading the container, it is GUARANTEED to see the new article immediately.

                bool articleNowExists = await _repo.ExistByUrlAsync(testArticle.Url, cancellationToken);
                if (articleNowExists)
                {
                    _logger.LogInformation("Verified that the test article exists in the database.");
                    return new OkObjectResult("Test article added and verified in the database successfully.");
                }
                else
                {
                    _logger.LogError("Failed to verify that the test article was added to the database.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
        }
    }
}
