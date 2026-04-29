using CodeHollow.FeedReader;

// The list of RSS feed links to read
List<string> rssLinks = new List<string>()
{
    "https://www.industryweek.com/rss"
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

Console.WriteLine("Finished reading RSS feeds.");
Console.ReadLine();