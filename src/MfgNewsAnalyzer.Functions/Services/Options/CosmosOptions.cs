using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Functions.Services.Options
{
    public record CosmosOptions
    {
        public const string SectionName = "Cosmos";
        public required string EndpointUri { get; init; } = string.Empty;
        public required string ContainerId { get; init; } = string.Empty;
        public required string Database { get; init; } = string.Empty;
    }
}
