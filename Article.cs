using AngleSharp.Io;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

public class Article
{
    // Article Information
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Publisher { get; set; }
    public string? Author { get; set; }
    public string? Url { get; set; }
    public DateTime? PublishDate { get; set; }
    public DateTime? PulledDate { get; set; }
    public string? RawText { get; set; }

    // AI Analysis Results
    public bool AiAnalysisCompleted { get; set; } = false;
    public AiAnalysis? AiAnalysisResults { get; set; }

    // Method to perform AI analysis on the article
    public async Task PerformAiAnalysis()
    {
        var config = new ConfigurationBuilder()
                    .AddUserSecrets<Program>()
                    .Build();

        AnthropicClient client = new AnthropicClient()
        {
            ApiKey = config["ANTHROPIC_API_KEY"]
        };

        if (string.IsNullOrWhiteSpace(client.ApiKey))
        {
            throw new InvalidOperationException("Anthropic API key is not configured. Add ANTHROPIC_API_KEY to user secrets.");
        }

        // Read system prompt using a using statement to ensure the file is closed promptly
        string systemPrompt;
        using (var sr = new StreamReader("C:\\Github Repos\\ai-mfg-news-analyzer\\systemprompt.txt"))
        {
            systemPrompt = await sr.ReadToEndAsync();
        }

        // Ensure we have content to send
        if (string.IsNullOrEmpty(RawText))
        {
            // Nothing to analyze; mark as completed and return early
            AiAnalysisCompleted = true;
            return;
        }

        // Build the JSON schema as a single object for diagnostic clarity
        var schemaObject = new
        {
            type = "object",
            properties = new
            {
                ClaudeSummary = new { type = "string" },
                Tags = new { type = "array", items = new { type = "string" } },
                Recommendation = new { type = "integer"},
                Sentiment = new { type = "integer"}
            },
            required = new[] { "ClaudeSummary", "Tags", "Recommendation", "Sentiment" },
            additionalProperties = false
        };

        // Serialize the schema parts into JsonElements for the SDK types used previously
        var schemaDict = new Dictionary<string, JsonElement>()
        {
            ["type"] = JsonSerializer.SerializeToElement(schemaObject.type),
            ["properties"] = JsonSerializer.SerializeToElement(schemaObject.properties),
            ["required"] = JsonSerializer.SerializeToElement(schemaObject.required),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(schemaObject.additionalProperties)
        };

        MessageCreateParams parameters = new()
        {
            // Consider using a stable production model name; adjust if your environment requires a different model
            Model = "claude-haiku-4-5-20251001",
            // Reduce MaxTokens to avoid exceeding model limits which often causes BadRequest
            MaxTokens = 2000,
            System = systemPrompt,
            Messages = new List<MessageParam>()
            {
                new MessageParam() { Role = Role.User, Content = RawText }
            },
            OutputConfig = new OutputConfig()
            {
                Format = new JsonOutputFormat()
                {
                    Schema = schemaDict
                }
            }
        };

        try
        {
            var response = await client.Messages.Create(parameters);
            if (response.Content[0].TryPickText(out var textBlock))
            {
                // JSON is guaranteed to match the schema
                var articleReview = JsonSerializer.Deserialize<Dictionary<string, object>>(textBlock.Text)!;
                Console.WriteLine($"{articleReview["ClaudeSummary"]}");
                Console.WriteLine($"{articleReview["Tags"]}");
                Console.WriteLine($"{articleReview["Recommendation"]}");
                Console.WriteLine($"{articleReview["Sentiment"]}");
                Console.WriteLine();

                AiAnalysisCompleted = true;
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
                        Schema = schemaObject
                    }
                };
                File.WriteAllText($"Errors/anthropic_badrequest_{Title ?? "article"}.json", JsonSerializer.Serialize(diag, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }

            throw;
        }
        catch (Exception)
        {
            // Re-throw after letting the caller handle general exceptions; optional: add logging here
            throw;
        }
    }
}