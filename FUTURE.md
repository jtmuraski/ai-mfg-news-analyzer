## Refactor Items for V2
1. Update security practices (set URI's in config for example
2. Review system prompt for updates

## New Features for V2
1. Add calls for NewsAPI sources
2. Tool to analyze different publications for useful articles (which publication writes the most useful articles for me?)


## Code Quality Tasks
1. Ivnestigate batch processing for Cosmos
2. Investigate batch processing for Claude API calls with articles
3. Delete Test Function?


## Implement the following in the Cosmos repository:
```
        public async Task<IEnumerable<Article>> GetByPublisherAsync(string publisher, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.publisher = @publisher").WithParameter("@publisher", publisher);
            var iterator = _container.GetItemQueryIterator<Article>(query);

            List<Article> articles = new List<Article>();
            while(iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                articles.AddRange(response.Resource);
            }

            return articles;
        }

        public async Task<Article> UpdateArticleAsync(Article article, CancellationToken cancellationToken = default)
        {
            var response = await _container.UpsertItemAsync(article, new PartitionKey(article.Publisher), cancellationToken: cancellationToken);
            return response.Resource;
        }
        public async Task DeleteArticleAsync(string id, string publisher, CancellationToken cancellationToken = default)
        {
            var response = await _container.DeleteItemAsync<Article>(id, new PartitionKey(publisher), cancellationToken: cancellationToken);
            return;
        }
```