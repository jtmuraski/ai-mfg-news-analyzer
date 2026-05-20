using MfgNewsAnalyzer.Core.Models;

namespace MfgNewsAnalyzer.Core.Abstractions
{
    public interface IArticleAnalyzer
    {
        Task<AiAnalysis> AnalyzeAsync(string rawText, CancellationToken cancellationToken = default);
    }
}
