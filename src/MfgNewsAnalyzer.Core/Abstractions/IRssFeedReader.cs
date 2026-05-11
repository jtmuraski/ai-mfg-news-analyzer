using MfgNewsAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Core.Abstractions
{
    public interface IRssFeedReader
    {
        Task<IReadOnlyList<Article>> ReadFeedAsync(string url, string publisher, CancellationToken canellationToken);
    }
}
