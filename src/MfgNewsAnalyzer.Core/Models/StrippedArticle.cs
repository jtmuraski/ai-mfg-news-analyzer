using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Core.Models
{
    public record StrippedArticle
    {
        public string? Title { get; init; }
        public string? Author { get; init; }
        public string? Url { get; init; }
        public string? RawText { get; init; }
        public int? Length { get; init; }
        public TimeSpan? TimeToRead { get; init; }

        public bool IsSuccess { get; init; }
        public List<Exception>? Errors { get; init; }
    }
}
