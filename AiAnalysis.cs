public class AiAnalysis
{
    public string? ClaudeSummary { get; set; }
    public List<string>? Tags { get; set; }
    public int? Recommendation { get; set; } // 1-5 scale for relevance to me. 0 if Claude cannot anlayze or provide a recommendation.
    public int? Sentiment { get; set; } // 1 == Positive, 0 == Neutral, -1 == Negative
}