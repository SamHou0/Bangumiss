using System.Xml;
using System.ServiceModel.Syndication;
namespace Bangumiss;

public class BgmRss
{
    public required Uri BgmUri { get; set; }

    public List<string> GetRssItems()
    {
        List<string> result = new();
        using (XmlReader reader = XmlReader.Create(BgmUri.ToString()))
        {
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            Console.WriteLine("Reading feed from " + BgmUri.ToString());
            Console.WriteLine(feed.Title.Text);
            foreach (var item in feed.Items)
            {
                result.Add("Title:"+item.Title.Text+
                           Environment.NewLine
                +"link:"+ item.Links[0].Uri.AbsoluteUri);
            }
        }

        return result;
    }
}