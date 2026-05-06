using CodeHollow.FeedReader;
using SmartReader;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

// string newsApiKey = "ce88b43e298d430cbd7bda272abbc07a";

// The list of RSS feed links to read
Dictionary<string, string> rssLinks = new Dictionary<string, string>()
{
    //"https://www.industryweek.com/rss"              // XML parsing error
    {"Plant Engineering", "https://www.plantengineering.com/feed/"}        // This one works just fine
    //"https://www.manufacturing.net/feed"              
    //"https://manufacturing-today.com/feed/"           
    //"https://www.manufacturingdive.com/feeds/news/"  
    //"https://www.assemblymag.com/rss/17"              
};


List<Article>? existingArticles = new List<Article>();
List<Article>? unreadableArticles = new List<Article>();

string jsonPath = "articles.json";
using(FileStream fs = File.Open(jsonPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
{
    if(fs.Length > 0)
    {
        var deserialized = JsonSerializer.Deserialize<List<Article>>(fs);
        if (deserialized is not null)
        {
            existingArticles.AddRange(deserialized);
        }
    }
}

// Loop through each RSS feed link and read the feed using the CodeHollow.FeedReader library
List<Article> foundArticles = new List<Article>();
int newArticlesFound = 0;
if(!Directory.Exists("Errors"))
{
    Directory.CreateDirectory("Errors");
}
string errorLogPath = "Errors/{0}.txt";

foreach (KeyValuePair<string, string> link in rssLinks)
{
    var feed = await CodeHollow.FeedReader.FeedReader.ReadAsync(link.Value);

    foreach (var item in feed.Items)
    {
        Article article = new Article
        {
            Title = item.Title,
            Description = item.Description,
            Url = item.Link,
            PublishDate = item.PublishingDate,
            Author = item.Author,
            PulledDate = DateTime.Now,
            Publisher = link.Key
        };

        if (!existingArticles.Any(a => a.Url == article.Url))
        {
            SmartReader.Reader reader = new SmartReader.Reader(item.Link);
            SmartReader.Article readArticle = await reader.GetArticleAsync();

            if(readArticle.IsReadable && readArticle.Completed)
            {
                article.RawText = readArticle.TextContent;
                foundArticles.Add(article);
                newArticlesFound++;
            }
            else if(readArticle.IsReadable && !readArticle.Completed)
            {
                SmartReader.Article retryRead = await reader.GetArticleAsync();
                if(retryRead.IsReadable && retryRead.Completed)
                {
                    article.RawText = retryRead.TextContent;
                    foundArticles.Add(article);
                    newArticlesFound++;
                }
                else
                {
                     unreadableArticles.Add(article);
                    if(readArticle.Errors.Count > 0)
                    {
                        foreach(var exception in readArticle.Errors)
                        {
                        File.WriteAllText(string.Format(errorLogPath, article.Title.Replace(" ", "_"  )), exception.Message);
                        }
                    }
                }
            }
            else
            {
                unreadableArticles.Add(article);
                if(readArticle.Errors.Count > 0)
                {
                    foreach(var exception in readArticle.Errors)
                    {
                    File.WriteAllText(string.Format(errorLogPath, article.Title.Replace(" ", "_"  )), exception.Message);
                    }
                }
            }
        }
    }
}

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(jsonPath, JsonSerializer.Serialize(foundArticles, options));

string unreadableJsonPath = "unreadable_articles.json";
File.WriteAllText(unreadableJsonPath, JsonSerializer.Serialize(unreadableArticles, options));

Console.WriteLine($"New Articles Found: {newArticlesFound}");
Console.WriteLine("Finished reading RSS feeds.");
Console.ReadLine();