using System.Text.Json.Serialization;

namespace MfgNewsAnalyzer.Core.Models;
public record Article
{
    [JsonPropertyName("id")]
    public required string Id { get; init; } = Guid.NewGuid().ToString();

    // ---Article Properties---

    // --Required Properties--
    [JsonPropertyName("publisher")]
    public required string Publisher { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("pulledDate")]
    public required DateTime PulledDate { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    // --Optional Properties--
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("author")]
    public string? Author { get; init; }

    [JsonPropertyName("publishDate")]
    public DateTime? PublishDate { get; init; }

    [JsonPropertyName("rawText")]
    public string? RawText { get; init; }

    // ---AI Analysis Properties---
    [JsonPropertyName("aiAnalysisResults")]
    public AiAnalysis? AiAnalysisResults { get; init; }
}
