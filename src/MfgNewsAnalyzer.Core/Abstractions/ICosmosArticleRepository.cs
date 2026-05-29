using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MfgNewsAnalyzer.Core.Models;

namespace MfgNewsAnalyzer.Core.Abstractions
{
    public interface ICosmosArticleRepository
    {
        Task<Article> AddArticleAsync(Article article, CancellationToken cancellationToken = default);

    }
}
