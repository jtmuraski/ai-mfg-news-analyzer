namespace MfgNewsAnalyzer.Functions.Services.Options;

public record ClaudeAnalyzerOptions
{
    public const string SectionName = "Anthropoc";
    public required string MainApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "claude-haiku-4-5-20251001";
    public int MaxTokens { get; init; } = 2000;
    public string SystemPromptPath { get; init; } = "systemprompt.txt";
}
