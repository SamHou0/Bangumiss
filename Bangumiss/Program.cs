using System.Diagnostics;
using System.ServiceModel.Syndication;
using Bangumiss.Bangumi;
using Bangumiss.Data;
using Bangumiss.Misskey;
using Sentry;

namespace Bangumiss;

class Program
{
    private static readonly BgmRss _bgmRss = new();
    private static readonly BgmApi _bgmApi = new();
    private static MisskeyApi _misskeyApi = new();
    private static Database.Database _database = new();

    static async Task Main(string[] args)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(
            int.Parse(
                Environment.GetEnvironmentVariable("BANGUMISS_REFRESH_SECONDS")
                ?? throw new Exception("Refresh seconds not set.")
            )
        ));
        InitSentry();
        Console.WriteLine("Starting...");
        do
        {
            try
            {
                Console.WriteLine("Refreshing at " + DateTime.UtcNow);
                var pendingItems = await RefreshBgmRssAsync();
                await ProcessNotesAsync(pendingItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.Message);
                SentrySdk.CaptureException(ex);
                if (Debugger.IsAttached) throw;
            }
        } while (await timer.WaitForNextTickAsync());
    }
/// <summary>
/// Refresh bangumi rss and create final ActionItem.
/// </summary>
/// <returns></returns>
/// <exception cref="ArgumentNullException"></exception>
    static async Task<List<ActionItem>> RefreshBgmRssAsync()
    {
        IEnumerable<SyndicationItem> items;
        try
        {
            items = _bgmRss.GetRssItems();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Get rss failed, retry new time: " + ex.Message);
            Console.WriteLine(ex);
            return new List<ActionItem>();
        }

        List<ActionItem> pendingItems = new();
        foreach (var item in items)
        {
            Console.WriteLine(item.Title.Text + " at " + item.PublishDate);
            var bgmCollectionData = await _bgmApi.GetUserCollectByUrl(item.Links[0].Uri);
            ActionItem actionItem = new()
            {
                Action = NoteAction.Add,
                Comment = bgmCollectionData.Comment ?? "",
                BgmId = BgmApi.ExtractBgmId(item.Links[0].Uri),
                Rate = bgmCollectionData.Rate,
                Tags = bgmCollectionData.Tags ?? [],
                Title = item.Title.Text ??
                        throw new ArgumentNullException(nameof(item.Title)),
                State = (EntryState)bgmCollectionData.Type
            };
            // Get last state
            EntryState lastState = _database.GetItemState(actionItem.BgmId);
            if (lastState == EntryState.NotInDb || actionItem.State != lastState)
            {
                pendingItems.Add(actionItem);
            }
        }

        return pendingItems;
    }
/// <summary>
/// Process notes and post them to misskey
/// </summary>
/// <param name="items"></param>
    static async Task ProcessNotesAsync(List<ActionItem> items)
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
                text += "锐评（可能有轻微剧透但不影响观看兴趣）："
                        + Environment.NewLine;
                text += "$[blur " + item.Comment + "]";
            }

            var createdNote = await _misskeyApi.PostNote(text);
            _database.RemoveItemNote(item.BgmId);
            _database.AddItemNote(new()
            {
                ItemId = item.BgmId,
                NoteId = createdNote.Id ?? 
                         throw new ArgumentNullException(nameof(createdNote.Id)),
                State = item.State
            });
        }
    }

    static void InitSentry()
    {
        string? dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
        if (string.IsNullOrEmpty(dsn) || Debugger.IsAttached)
        {
            Console.WriteLine("Sentry not enabled! Stop init Sentry.");
            return;
        }
        else
        {
            Console.WriteLine("Sentry enabled using " + dsn);
        }

        SentrySdk.Init(options =>
        {
            // A Sentry Data Source Name (DSN) is required.
            // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
            // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
            options.Dsn = dsn;

            // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
            // This might be helpful, or might interfere with the normal operation of your application.
            // We enable it here for demonstration purposes when first trying Sentry.
            // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
            options.Debug = false;

            // This option is recommended. It enables Sentry's "Release Health" feature.
            options.AutoSessionTracking = true;
        });
    }
}