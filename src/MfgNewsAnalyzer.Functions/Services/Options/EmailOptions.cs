using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Functions.Services.Options
{
    public record EmailOptions
    {
        public const string SectionName = "Email";
        public required string AzureEmailServiceEndpoint { get; init; } = string.Empty;
        public required string DestinationAddress { get; init; } = string.Empty;
        public required string SenderAddress { get; init; } = string.Empty;

    }
}
