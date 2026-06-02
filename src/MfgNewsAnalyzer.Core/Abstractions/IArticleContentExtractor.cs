using MfgNewsAnalyzer.Core.Models;

namespace MfgNewsAnalyzer.Core.Abstractions
{
    public interface IArticleContentExtractor
    {
        Task<StrippedArticle> ExtractContentAsync(string url, CancellationToken cancellationToken = default);
    }
}
