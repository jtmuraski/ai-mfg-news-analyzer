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
    public string? ClaudeSummary { get; set; }
    public List<string>? Tags { get; set; }
    public int? Recommendation { get; set; } // 1-5 scale for relevance to me. 0 if Claude cannot anlayze or provide a recommendation.
    public string? Sentiment { get; set; } // Positive, Neutral, Negative

}