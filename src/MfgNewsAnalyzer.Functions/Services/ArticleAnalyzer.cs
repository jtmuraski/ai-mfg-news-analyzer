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
            if (string.IsNullOrEmpty(_systemPromptPath))
            {
                _systemPromptPath = Path.Combine(AppContext.BaseDirectory, _options.Value.SystemPromptPath);
                _systemPrompt = await File.ReadAllTextAsync(_systemPromptPath, cancellationToken);
                _logger.LogInformation($"System prompt has been loaded from {_systemPromptPath}");
            }

            return;
        }
        
        public async Task<AiAnalysis> AnalyzeAsync(string rawText, CancellationToken cancellationToken = default)
        {
            GetSystemPromptAsync(cancellationToken).Wait(cancellationToken);

            if(_systemPrompt == null)
            {
                _logger.LogError("The system prompt at {SystemPromptPath} is empty.", _systemPromptPath);
                throw new NullSystemPromptException($"The system prompt at {_systemPromptPath} is empty.");
            }

            // Build the JSON schema that the prompt will use to return results
            var schemaTemplateObject = new
            {
                type = "object",
                properties = new
                {
                    claudeSummary = new { type = "string" },
                    tags = new { type = "array", items = new { type = "string" } },
                    recommendation = new { type = "integer", minimum = 1, maximum = 5 },
                    sentiment = new { type = "string" }
                },
                required = new[] { "claudeSummary", "tags", "recommendation", "sentiment" },
                additionalProperties = false
            };

            var schemaTemplateDict = new Dictionary<string, JsonElement>()
            {
                ["type"] = JsonSerializer.SerializeToElement(schemaTemplateObject.type),
                ["properties"] = JsonSerializer.SerializeToElement(schemaTemplateObject.properties),
                ["required"] = JsonSerializer.SerializeToElement(schemaTemplateObject.required),
                ["additionalProperties"] = JsonSerializer.SerializeToElement(schemaTemplateObject.additionalProperties)
            };

            Anthropic.Models.Messages.MessageCreateParams parameters = new Anthropic.Models.Messages.MessageCreateParams()
            {
                Model = "claude-haiku-4-5-20251001",
                // Reduce MaxTokens to avoid exceeding model limits which often causes BadRequest
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
                var response = await _client.Messages.Create(parameters);

                if (response.Content[0].TryPickText(out var message))
                {
                    var analysis = JsonSerializer.Deserialize<AiAnalysis>(message.Text);
                    if (analysis == null)
                    {
                        _logger.LogError("Failed to deserialize the model response into AiAnalysis. Response content: {ResponseContent}", message);
                        throw new JsonException("Failed to deserialize the model response into AiAnalysis.");
                    }
                    return analysis;
                }
                else
                {
                    _logger.LogError("The model response did not contain text content. Full response: {@Response}", response);
                    throw new InvalidOperationException("The model response did not contain text content.");
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