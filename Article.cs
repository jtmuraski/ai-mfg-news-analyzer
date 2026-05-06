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
    public AiAnalysis? AiAnalysis { get; set; }

}