using CodeHollow.FeedReader;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;

string newsApiKey = "ce88b43e298d430cbd7bda272abbc07a";

// Test the NewsApi API
var newsApiClient = new NewsApiClient(newsApiKey);
var articles =  await newsApiClient.GetEverythingAsync(new EverythingRequest
{
    Q = "Manufacturing Technology || Industry 4.0 || Smart Manufacturing",
    SortBy = SortBys.Popularity,
    Language = Languages.EN,
    
    From = new DateTime(2026,04,25)
});

Console.WriteLine($"Total Results: {articles.TotalResults}");
foreach (var article in articles.Articles)
{
    Console.WriteLine($"Title: {article.Title}");
    Console.WriteLine($"Description: {article.Description}");
    Console.WriteLine($"URL: {article.Url}");
    Console.WriteLine($"Published At: {article.PublishedAt}");
    Console.WriteLine();
}

/*
// The list of RSS feed links to read
List<string> rssLinks = new List<string>()
{
    //"https://www.industryweek.com/rss"
    "https://www.plantengineering.com/feed/"        // This one works just fine
};

// Loop through each RSS feed link and read the feed using the CodeHollow.FeedReader library
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
        Console.WriteLine($"Item Title- {item.Title}");
        Console.WriteLine($"Item Link: {item.Link}");
        Console.WriteLine($"Item Publish Date: {item.PublishingDate}");
        Console.WriteLine($"Item Description: {item.Description}");
    }
}
*/

Console.WriteLine("Finished reading RSS feeds.");
Console.ReadLine();