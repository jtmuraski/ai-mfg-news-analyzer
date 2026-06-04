# ai-mfg-news-analyzer

An AI-powered manufacturing news digest tool built on Azure Functions. It reads manufacturing RSS feeds on a schedule, deduplicates articles against a Cosmos DB database, analyzes each article using the Claude AI API, and delivers a ranked email digest every week — sorted by relevance to manufacturing automation, industrial software engineering, and OT/IT topics.

This project serves a dual purpose: a practical personal tool for staying current on manufacturing news, and a learning vehicle for working with AI APIs, Azure cloud services, and modern .NET patterns.

---

## What It Does

1. **Reads RSS feeds** from a configured list of manufacturing publications
2. **Deduplicates** articles against Cosmos DB so you never see the same article twice
3. **Strips article content** from web pages using SmartReader (removes HTML noise)
4. **Analyzes each article** via the Claude API (Haiku model), producing:
   - A 3–6 sentence summary
   - 2–5 classification tags
   - A recommendation score (1–5, or 0 if indeterminate)
   - A sentiment score (-1, 0, or 1)
5. **Saves results** to Azure Cosmos DB
6. **Emails a digest** via Azure Communication Services, sorted by recommendation score, filtered to articles scoring 3 or higher

The digest runs every Monday at noon (UTC) via a Timer Trigger.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Azure Functions (Isolated Worker)           │
│                                                                 │
│  [Timer Trigger - Monday 12:00 UTC]                             │
│         │                                                       │
│         ▼                                                       │
│  OrchestrateArticleReading                                      │
│    ├── IRssFeedReader         → Reads RSS feeds                 │
│    ├── IArticleRepository     → Dedup check via Cosmos DB       │
│    ├── IArticleContentExtractor → Strips HTML via SmartReader   │
│    ├── IArticleAnalyzer       → Analyzes via Claude API         │
│    ├── IArticleRepository     → Saves analyzed article          │
│    └── IEmailService          → Sends digest email              │
└─────────────────────────────────────────────────────────────────┘

┌───────────────────────┐    ┌──────────────────────┐
│   Azure Key Vault     │    │   Azure Cosmos DB     │
│  (API keys / secrets) │    │  (Article storage)    │
└───────────────────────┘    └──────────────────────┘

┌───────────────────────┐    ┌──────────────────────┐
│  Anthropic Claude API │    │  Azure Communication  │
│  (Article analysis)   │    │  Services (Email)     │
└───────────────────────┘    └──────────────────────┘
```

### Project Structure

```
ai-mfg-news-analyzer/
├── src/
│   ├── MfgNewsAnalyzer.Core/           # Models, interfaces, exceptions — zero infrastructure deps
│   │   ├── Abstractions/               # IRssFeedReader, IArticleAnalyzer, IArticleRepository, etc.
│   │   ├── Exceptions/                 # Custom exceptions (InvalidApiKeyException, NullSystemPromptException, etc.)
│   │   └── Models/                     # Article, AiAnalysis, StrippedArticle
│   │
│   └── MfgNewsAnalyzer.Functions/      # Azure Functions host + all implementations
│       ├── Functions/                  # HTTP & Timer trigger functions
│       ├── Services/                   # Implementations of Core interfaces
│       │   ├── Options/                # Strongly-typed config records
│       │   └── Repositories/          # CosmosArticleRepository
│       ├── systemprompt.txt            # Claude system prompt (copied to output)
│       └── Program.cs                  # DI registration, Key Vault setup
├── FUTURE.md                           # Deferred work and V2 ideas
└── requests.http                       # Local test endpoints
```

**Core principle:** `MfgNewsAnalyzer.Core` has zero infrastructure dependencies — only models and abstractions. All implementations live in `MfgNewsAnalyzer.Functions`.

---

## Configured RSS Feeds (V1)

| Publication | Feed URL |
|---|---|
| Plant Engineering | `https://www.plantengineering.com/feed/` |
| Manufacturing Today | `https://manufacturing-today.com/feed/` |
| Manufacturing Dive | `https://www.manufacturingdive.com/feeds/news/` |
| Assembly Magazine | `https://www.assemblymag.com/rss/17` |

> Feeds that return malformed XML are logged and skipped rather than crashing the run.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10, Azure Functions v4 (Isolated Worker) |
| AI | Anthropic Claude API (claude-haiku-4-5) |
| Database | Azure Cosmos DB (NoSQL) |
| Secrets | Azure Key Vault + `DefaultAzureCredential` |
| Email | Azure Communication Services |
| Content extraction | SmartReader 0.11.0 |
| RSS parsing | CodeHollow.FeedReader 1.2.6 |
| Observability | Application Insights |

---

## Prerequisites

- .NET 10 SDK
- Azure Functions Core Tools v4
- An Azure subscription with:
  - Azure Functions App
  - Azure Cosmos DB account (NoSQL API)
  - Azure Key Vault
  - Azure Communication Services resource
- An [Anthropic API key](https://console.anthropic.com/)
- Azure CLI (for local development authentication)

---

## Local Development Setup

### 1. Clone the repo

```bash
git clone https://github.com/jtmuraski/ai-mfg-news-analyzer.git
cd ai-mfg-news-analyzer
```

### 2. Log in to Azure CLI

Authentication uses `DefaultAzureCredential`, which resolves to `AzureCliCredential` locally.

```bash
az login
```

### 3. Create `local.settings.json`

Create this file at `src/MfgNewsAnalyzer.Functions/local.settings.json`. It is gitignored and should **never** be committed.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Anthropic__Model": "claude-haiku-4-5-20251001",
    "Anthropic__MaxTokens": "2000",
    "Anthropic__SystemPromptPath": "systemprompt.txt",
    "Cosmos__EndpointUri": "<your-cosmos-endpoint>",
    "Cosmos__ContainerId": "<your-container-id>",
    "Cosmos__Database": "<your-database-name>",
    "Email__AzureEmailServiceEndpoint": "<your-acs-endpoint>",
    "Email__DestinationAddress": "<your-email>",
    "Email__SenderAddress": "<your-sender-address>"
  }
}
```

> The Anthropic API key is **not** stored here — it is retrieved at runtime from Azure Key Vault under the secret name `Anthropoc--MainApiKey`.

### 4. Run locally

```bash
cd src/MfgNewsAnalyzer.Functions
func start
```

---

## Test Endpoints

The project includes HTTP-triggered test functions for validating each integration independently before running the full orchestration.

| Endpoint | Purpose |
|---|---|
| `GET /api/TestRssReader` | Reads the Plant Engineering RSS feed |
| `GET /api/TestSmartReader` | Reads + strips article content from Plant Engineering |
| `GET /api/TestKeyVault` | Verifies Key Vault connectivity and secret retrieval |
| `GET /api/TestConfig` | Confirms the Anthropic API key is loaded from Key Vault |
| `GET /api/TestCosmosRepo` | Writes and reads a test article to/from Cosmos DB |
| `POST /admin/functions/OrchestrateArticleReading` | Manually triggers the full orchestration pipeline |

All test requests are also available in `requests.http` at the repo root.

---

## Configuration Reference

| Setting | Section | Description |
|---|---|---|
| `MainApiKey` | `Anthropic` | Anthropic API key — sourced from Key Vault, not `local.settings.json` |
| `Model` | `Anthropic` | Claude model to use (default: `claude-haiku-4-5-20251001`) |
| `MaxTokens` | `Anthropic` | Max tokens per analysis call (default: 2000) |
| `SystemPromptPath` | `Anthropic` | Path to system prompt file, relative to output directory |
| `EndpointUri` | `Cosmos` | Cosmos DB endpoint URI |
| `ContainerId` | `Cosmos` | Cosmos DB container name |
| `Database` | `Cosmos` | Cosmos DB database name |
| `AzureEmailServiceEndpoint` | `Email` | Azure Communication Services endpoint |
| `DestinationAddress` | `Email` | Email address to deliver the digest to |
| `SenderAddress` | `Email` | Verified sender address in ACS |

---

## Authentication Model

This project uses `DefaultAzureCredential` with the following providers enabled (in resolution order):

1. `ManagedIdentityCredential` — used when deployed to Azure
2. `AzureCliCredential` — used for local development

All other providers (Visual Studio, VS Code, Interactive Browser, etc.) are explicitly excluded to keep the resolution path predictable.

---

## Article Analysis Schema

Claude returns structured JSON conforming to this schema, which maps directly to the `AiAnalysis` model:

```json
{
  "claudeSummary": "string",
  "tags": ["string"],
  "recommendation": 0,
  "sentiment": 0
}
```

| Field | Type | Description |
|---|---|---|
| `claudeSummary` | string | 3–6 sentence article summary |
| `tags` | string[] | 2–5 classification tags |
| `recommendation` | int | 1–5 relevance score (0 = indeterminate) |
| `sentiment` | int | 1 = positive, 0 = neutral, -1 = negative |

---

## License

MIT — see [LICENSE](LICENSE) for details.
