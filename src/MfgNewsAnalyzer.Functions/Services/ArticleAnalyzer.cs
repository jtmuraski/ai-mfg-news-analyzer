using Anthropic;
using MfgNewsAnalyzer.Core.Abstractions;
using MfgNewsAnalyzer.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Functions.Services
{
    public class ArticleAnalyzer : IArticleAnalyzer
    {
        private readonly ILogger<ArticleAnalyzer> _logger;
        private readonly IOptions<ClaudeAnalyzerOptions> _options;
        private readonly AnthropicClient _client;

        // Fields
        private string? _systemPromptPath;
        private string? _systemPrompt;

        public ArticleAnalyzer(ILogger<ArticleAnalyzer> logger, IOptions<ClaudeAnalyzerOptions> options)
        {
            _logger = logger;
            _options = options;
            _client = new AnthropicClient() { ApiKey = _options.Value.MainApiKey };
        }

        /// <summary>
        /// If the system prompt has not yet been populated, read the prompt file and load it.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task GetSystemPromptAsync (CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_systemPromptPath))
            {
                _systemPromptPath = Path.Combine(AppContext.BaseDirectory, _options.Value.SystemPromptPath);
                _systemPrompt = await File.ReadAllTextAsync(_systemPromptPath, cancellationToken);
                _logger.LogInformation($"System prompt has been loaded from {_systemPromptPath}");
            }

            return;
        }
        
        public Task<AiAnalysis> AnalyzeAsync(string rawText, CancellationToken cancellationToken = default)
        {
            GetSystemPromptAsync(cancellationToken).Wait(cancellationToken);

            if(_systemPrompt == null)
            {
                throw new Exception()
            }


        }
    }
}