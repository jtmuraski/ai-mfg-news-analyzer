using Anthropic;
using Anthropic.Models.Messages;
using System.IO;

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

    // Method to perform AI analysis on the article
    public void PerformAiAnalysis(AnthropicClient anthropicClient)
    {
        FileStream fileStream = new FileStream("systemprompt.txt", FileMode.Open, FileAccess.Read);
        string systemPrompt = new StreamReader(fileStream).ReadToEnd();
        fileStream.Close();

        AnthropicClient client = new AnthropicClient();

        MessageCreateParams parameters = new MessageCreateParams
        {
            Model = "claude-haiku-4-5",
            MaxTokens = 5000
        };

        parameters.Messages.Add(new)


    }

}