using Anthropic;
using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MfgNewsAnalyzer.Core.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Anthropic.Models.Beta.Messages;
using Anthropic.Models.Messages;

namespace MfgNewsAnalyzer.Functions.Services
{
    public class ArticleAnalyzer : IArticleAnalyzer
    {
        private readonly ILogger<ArticleAnalyzer> _logger;
        private readonly IOptions<ClaudeAnalyzerOptions> _options;
        private readonly AnthropicClient _client;

        // Fields
        private string? _systemPromptPath;
        private string? _systemPrompt;

        public ArticleAnalyzer(ILogger<ArticleAnalyzer> logger, IOptions<ClaudeAnalyzerOptions> options)
        {
            _logger = logger;
            _options = options;

            if(string.IsNullOrEmpty(_options.Value.MainApiKey))
            {
                _logger.LogError("Anthropic API key is not configured. Please set the MainApiKey in the configuration.");
                throw new InvalidApiKeyException("Anthropic API key is not configured. Please set the MainApiKey in the configuration.");
            }
            _client = new AnthropicClient() { ApiKey = _options.Value.MainApiKey };
        }

        /// <summary>
        /// If the system prompt has not yet been populated, read the prompt file and load it.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task GetSystemPromptAsync (CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPromptPath = Path.Combine(AppContext.BaseDirectory, _options.Value.SystemPromptPath);
                _systemPrompt = await File.ReadAllTextAsync(_systemPromptPath, cancellationToken);
                _logger.LogInformation($"System prompt has been loaded from {_systemPromptPath}");
            }

            return;
        }
        
        public async Task<AiAnalysis> AnalyzeAsync(string rawText, CancellationToken cancellationToken = default)
        {
            await GetSystemPromptAsync(cancellationToken);

            if(_systemPrompt == null)
            {
                _logger.LogError("The system prompt at {SystemPromptPath} is empty.", _systemPromptPath);
                throw new NullSystemPromptException($"The system prompt at {_systemPromptPath} is empty.");
            }

            // Build the JSON schema that the prompt will use to return results.
            // By providing this template, the Claude API will be required to provide its response to meet this schema.
            // Anthropic structured response doc: 
            var schemaTemplateObject = new
            {
                type = "object",
                properties = new
                {
                    claudeSummary = new { type = "string" },
                    tags = new { type = "array", items = new { type = "string" } },
                    recommendation = new { type = "integer", minimum = 1, maximum = 5 },
                    sentiment = new { type = "integer" }
                },
                required = new[] { "claudeSummary", "tags", "recommendation", "sentiment" },
                additionalProperties = false
            };

            // Convert the schema into a dictionary of JsonElements, which is the format required by the Anthropic .NET SDK for schema-based output formatting.
            var schemaTemplateDict = new Dictionary<string, JsonElement>()
            {
                ["type"] = JsonSerializer.SerializeToElement(schemaTemplateObject.type),
                ["properties"] = JsonSerializer.SerializeToElement(schemaTemplateObject.properties),
                ["required"] = JsonSerializer.SerializeToElement(schemaTemplateObject.required),
                ["additionalProperties"] = JsonSerializer.SerializeToElement(schemaTemplateObject.additionalProperties)
            };

            Anthropic.Models.Messages.MessageCreateParams parameters = new Anthropic.Models.Messages.MessageCreateParams()
            {
                Model = Model.ClaudeHaiku4_5_20251001,
                MaxTokens = 2000,
                System = _systemPrompt,
                Messages = new List<MessageParam>()
                {
                    new MessageParam() { Role = Anthropic.Models.Messages.Role.User, Content = rawText }
                },
                OutputConfig = new OutputConfig()
                {
                    Format = new JsonOutputFormat()
                    {
                        Schema = schemaTemplateDict
                    }
                }
            };

            try
            {
                var response = await _client.Messages.Create(parameters, cancellationToken);

                if (response.Content[0].TryPickText(out var textBlock))
                {
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(textBlock.Text);
                    if (result != null)
                    {
                        return new AiAnalysis()
                        {
                            ClaudeSummary = result["claudeSummary"]?.ToString() ?? string.Empty,
                            Tags = ((JsonElement)result["tags"]).Deserialize<List<string>>() ?? new List<string>(),
                            Recommendation = int.TryParse(result["recommendation"]?.ToString(), out int rec) ? rec : 0,
                            Sentiment = int.TryParse(result["sentiment"]?.ToString(), out int sent) ? sent : 0
                        };
                    }
                    else
                    {
                        _logger.LogError("Failed to deserialize the response from Claude. Response content: {ResponseContent}", textBlock.Text);
                        throw new Exception("Failed to deserialize the response from Claude.");
                    }
                }
                else
                {
                    _logger.LogError("Claude API did not return a proper response. Full response: {FullResponse}", JsonSerializer.Serialize(response));
                    throw new PromptFailureException($"Claude API did not return a proper response. Full response: {JsonSerializer.Serialize(response)}");
                }
            }
            catch (Anthropic.Exceptions.AnthropicBadRequestException ex)
            {
                // Write a diagnostic file containing the exception and the request payload (important for root-cause)
                try
                {
                    if (!Directory.Exists("Errors")) Directory.CreateDirectory("Errors");
                    var diag = new
                    {
                        Exception = ex.ToString(),
                        Request = new
                        {
                            Model = parameters.Model,
                            MaxTokens = parameters.MaxTokens,
                            System = parameters.System,
                            Messages = parameters.Messages?.Select(m => new { m.Role, m.Content }),
                            Schema = schemaTemplateObject
                        }
                    };
                    string title = "article";
                    File.WriteAllText($"Errors/anthropic_badrequest_{title ?? "article"}.json", JsonSerializer.Serialize(diag, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch { }

                throw;
            }
        }
    }
}