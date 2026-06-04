using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;
using System.Text;
using MfgNewsAnalyzer.Functions.Services.Options;
using Azure;
using Microsoft.Extensions.Options;

namespace MfgNewsAnalyzer.Functions.Services
{
    public class EmailService : IEmailService
    {
        ILogger<EmailService> _logger;
        EmailClient _client;
        IOptions<EmailOptions> _options;

        public EmailService(ILogger<EmailService> logger, EmailClient client, IOptions<EmailOptions> options)
        {
            _logger = logger;
            _client = client;
            _options = options;
        }
        public async Task SendEmailAsync(IReadOnlyList<Article> articles, CancellationToken cancellationToken)
        {
            string headerMessage = $"Here are the top {articles.Count} new Manufacturing related news articles from the last week:";

            List<Article> sortedArticles = articles.OrderByDescending(art => art.AiAnalysisResults.Recommendation).Where(art => art.AiAnalysisResults.Recommendation >= 3).ToList();
            StringBuilder formattedArticles = new StringBuilder();
            formattedArticles.Append(@"<style>
                                            table.article-table {
                                                border-collapse: collapse;
                                                width: 100%;
                                                margin-bottom: 10px;
                                            }
                                            table.article-table td {
                                                border: 1px solid black;
                                                padding: 6px 8px;
                                            }
                                        </style>");
            foreach(Article article in sortedArticles)
            {
                string html = @$"<table class='article-table'>
                                    <tr>
                                        <td><a href='{article.Url}'>{article.Title}</a></td>
                                        <td>{article.Publisher}</td>
                                        <td>{article.PublishDate?.ToString("yyyy-MM-dd")}</td>
                                        <td>{ToStars(article.AiAnalysisResults.Recommendation)}</td>
                                    </tr>
                                    <tr>
                                        <td colspan='4'>{article.AiAnalysisResults.ClaudeSummary} </td>
                                    </tr>
                                </table>
                                <br>";
                formattedArticles.Append(html);
            }

            var emailMessage = new EmailMessage(
                senderAddress: _options.Value.SenderAddress,
                content: new EmailContent("Manufacturing News Update")
                {
                    PlainText = headerMessage,
                    Html = formattedArticles.ToString()
                },
                recipients: new EmailRecipients(new List<EmailAddress>
                { 
                    new EmailAddress(_options.Value.DestinationAddress)
                })
                );

            EmailSendOperation sendOperation = await _client.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken);

            return;
        }

        private static string ToStars(int recommendation)
        {
            return new string('★', Math.Clamp(recommendation, 0, 5));
        }
    }
}
