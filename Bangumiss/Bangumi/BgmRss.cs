using System.ServiceModel.Syndication;
using System.Xml;

namespace Bangumiss.Bangumi;

public class BgmRss
{
    private readonly Uri _bgmUri = new(Environment.GetEnvironmentVariable("BANGUMISS_BGM_RSS")
                                      ??throw new Exception("RSS url not set"));

    public IEnumerable<SyndicationItem> GetRssItems()
    {
        List<string> result = new();
        using XmlReader reader = XmlReader.Create(_bgmUri.ToString());
        SyndicationFeed feed = SyndicationFeed.Load(reader);
        Console.WriteLine("Reading feed from " + _bgmUri.ToString());
        return feed.Items;
    }
}