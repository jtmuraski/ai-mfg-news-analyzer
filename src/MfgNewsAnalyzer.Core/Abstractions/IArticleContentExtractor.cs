using MfgNewsAnalyzer.Core.Models;

namespace MfgNewsAnalyzer.Core.Abstractions
{
    public interface IArticleContentExtractor
    {
        Task<StrippedArticle> ExtractContent(string url, CancellationToken cancellationToken = default);
    }
}
