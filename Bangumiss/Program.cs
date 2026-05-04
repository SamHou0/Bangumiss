namespace Bangumiss;

class Program
{
    private static readonly BgmRss _bgmRss = new();
    private static readonly BgmApi _bgmApi = new();
    private static MisskeyApi _misskeyApi = new();
    private static DateTime _lastUpdate = DateTime.UtcNow;

    static async Task Main(string[] args)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(
            int.Parse(
                Environment.GetEnvironmentVariable("BANGUMISS_REFRESH_SECONDS")
                ?? throw new Exception("Refresh seconds not set.")
            )
        ));
        Console.WriteLine("Starting...");
        while (await timer.WaitForNextTickAsync())
        {
            try
            {
                Console.WriteLine("Refreshing at " + DateTime.UtcNow +
                                  " Last Update " + _lastUpdate);
                var pendingItems = await RefreshBgmRssAsync();
                _lastUpdate = DateTime.UtcNow;
                await PostNotesAsync(pendingItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    static async Task<List<BgmItem>> RefreshBgmRssAsync()
    {
        var items = _bgmRss.GetRssItems();
        List<BgmItem> pendingItems = new();
        foreach (var item in items)
        {
            Console.WriteLine(item.Title.Text + " at " + item.PublishDate);
            if (item.PublishDate > _lastUpdate)
            {
                var bgmCollectionData = await _bgmApi.GetUserCollectByUrl(item.Links[0].Uri);
                pendingItems.Add(new()
                {
                    Comment = bgmCollectionData.Comment ?? "",
                    Link = item.Links[0].Uri,
                    Rate = bgmCollectionData.Rate,
                    Tags = bgmCollectionData.Tags ??[],
                    Title = item.Title.Text ??
                            throw new ArgumentNullException(nameof(item.Title)),
                });
            }
        }
        return pendingItems;
    }

    static async Task PostNotesAsync(List<BgmItem> items)
    {
        foreach (var item in items)
        {
            string text = item.Title + Environment.NewLine;
            text += "评分： " + item.Rate + "/10" + Environment.NewLine;
            text += Environment.NewLine;
            foreach (var tag in item.Tags)
            {
                text += "#" + tag + " ";
            }

            if (!string.IsNullOrEmpty(item.Comment))
            {
                text += Environment.NewLine;
                text += "锐评（或许有轻微剧透但不影响观看兴趣）："
                        + Environment.NewLine;
                text += item.Comment;
            }

            await _misskeyApi.PostNote(text);
        }
    }
}