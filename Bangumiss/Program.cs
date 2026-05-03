namespace Bangumiss;

class Program
{
    private static BgmRss _bgmRss = new();
    private static BgmApi _bgmApi = new();
    private static DateTime _lastUpdate = DateTime.Now;

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
            var pendingItems = await RefreshBgmRssAsync();
            _lastUpdate = DateTime.Now;
        }
    }

    static async Task<List<BgmItem>> RefreshBgmRssAsync()
    {
        var items = _bgmRss.GetRssItems();
        List<BgmItem> pendingItems = new();
        foreach (var item in items)
        {
            if (item.LastUpdatedTime > _lastUpdate)
            {
                var bgmCollectionData = await _bgmApi.GetUserCollectByUrl(item.Links[0].Uri);
                pendingItems.Add(new()
                {
                    Comment = bgmCollectionData.Comment ??
                              throw new ArgumentNullException(nameof(bgmCollectionData.Comment)),
                    Link = item.Links[0].Uri,
                    Rate = bgmCollectionData.Rate,
                    Tags = bgmCollectionData.Tags ??
                           throw new ArgumentNullException(nameof(bgmCollectionData.Tags)),
                    Title = item.Title.ToString() ??
                            throw new ArgumentNullException(nameof(item.Title)),
                });
            }
        }

        return pendingItems;
    }

    static async Task PostNotesAsync(List<BgmItem> items)
    {
        
    }
}