using MfgNewsAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Core.Abstractions
{
    public interface IArticleRepository
    {
        Task<bool> ExistByUrlAsync(string url, CancellationToken cancellationToken);

        Task SaveAsync(Article article, CancellationToken cancellationToken);
    }
}
