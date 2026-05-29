namespace MfgNewsAnalyzer.Functions.Services.Options;

public record RssFeedReaderOptions
{
    public const string SectionName = "RssFeedReader";
    public int TimeoutSeconds { get; init; } = 30;
    public string UserAgent { get; init; } = "MfgNewsAnalyzer/1.0 (+https://github.com/jtmuraski/ai-mfg-news-analyzer)";
}
