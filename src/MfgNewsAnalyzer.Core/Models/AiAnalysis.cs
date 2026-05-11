using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MfgNewsAnalyzer.Core.Models;

public record AiAnalysis
{
    [JsonPropertyName("claudeSummary")]
    public required string ClaudeSummary { get; init; }

    [JsonPropertyName("recommendation")]
    public required int Recommendation { get; init; }

    [JsonPropertyName("sentiment")]
    public required int Semantic { get; init; }

    [JsonPropertyName("tags")]
    public required IReadOnlyList<string> Tags { get; init; } = new List<string>();
}
