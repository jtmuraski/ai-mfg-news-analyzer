using MfgNewsAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Core.Abstractions
{
    public interface IArticleAnalyzer
    {
        Task<AiAnalysis> AnalyzeAsync(string rawText, CancellationToken cancellationToken);
    }
}
