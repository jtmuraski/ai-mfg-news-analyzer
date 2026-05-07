using Anthropic;
using Anthropic.Models.Messages;
using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        FileStream fileStream = new FileStream("C:\\Github Repos\\ai-mfg-news-analyzer\\systemprompt.txt", FileMode.Open, FileAccess.Read);
        string systemPrompt = new StreamReader(fileStream).ReadToEnd();
        fileStream.Close();

        AnthropicClient client = new AnthropicClient();

        MessageCreateParams parameters = new()
        {
            Model = "claude-haiku-4-5",
            MaxTokens = 5000,
            System = systemPrompt,
            Messages = new List<MessageParam>()
            {
                new MessageParam() {Role = Role.User, Content = RawText}
            }
        };

        var message = await client.Messages.Create(parameters);
        if(message is not null)
        {
            AiAnalysisCompleted = true;
            Console.WriteLine(message.ToString());
        }
    }

}