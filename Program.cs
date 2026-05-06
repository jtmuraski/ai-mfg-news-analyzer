using CodeHollow.FeedReader;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using SmartReader;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

// string newsApiKey = "ce88b43e298d430cbd7bda272abbc07a";

// The list of RSS feed links to read
List<string> rssLinks = new List<string>()
{
    //"https://www.industryweek.com/rss"              // XML parsing error
    "https://www.plantengineering.com/feed/"        // This one works just fine
    //"https://www.manufacturing.net/feed"              
    //"https://manufacturing-today.com/feed/"           
    //"https://www.manufacturingdive.com/feeds/news/"  
    //"https://www.assemblymag.com/rss/17"              
};

// TODO: Read the JSON file of the currently found articles
List<Article>? existingArticles = new List<Article();
string jsonPath = "found_articles.json";
using(FileStream fs = File.Open(jsonPath, FileMode.OpenOrCreate))
{
    existingArticles = await JsonSerializer.DeserializeAsync<List<Article>>(fs);
}

// Loop through each RSS feed link and read the feed using the CodeHollow.FeedReader library
List<Article> foundArticles = new List<Article>();

foreach (string link in rssLinks)
{
    var feed = await CodeHollow.FeedReader.FeedReader.ReadAsync(link);

    // Print the title of the feed and the titles of each item in the feed
    Console.WriteLine($"Feed Title: {feed.Title}");
    Console.WriteLine($"Feed Link: {feed.Link}");
    Console.WriteLine($"Feed Description: {feed.Description}");
    Console.WriteLine($"Number of Items: {feed.Items.Count}");

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
            Publisher = link
        };

        // TODO: Check if the article already exists in the JSON file of found articles. If it does, skip it. If it doesn't, add it to the list of found articles and save the updated list back to the JSON file.

        bool articleExists = existingArticles.Any(a => a.Url == article.Url);
        if (!articleExists)
        {
            SmartReader.Reader reader = new SmartReader.Reader(item.Link);
            SmartReader.Article readArticle = reader.GetArticle();

            if(readArticle.IsReadable)
            {
                article.RawText = readArticle.TextContent;
            }

            foundArticles.Add(article);
        }
    }

    // Add the new articles to the JSON file
    var options = new JsonSerializerOptions { WriteIndented = true };
    List<Article> existingArticles = new List<Article>();
}

Console.WriteLine("Finished reading RSS feeds.");
Console.ReadLine();