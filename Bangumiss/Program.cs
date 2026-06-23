using System.Diagnostics;
using Bangumiss.Bangumi;
using Bangumiss.Data;
using Bangumiss.Misskey;

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
        Console.WriteLine("Starting...");
        while (await timer.WaitForNextTickAsync())
        {
            try
            {
                Console.WriteLine("Refreshing at " + DateTime.UtcNow);
                var pendingItems = await RefreshBgmRssAsync();
                await ProcessNotesAsync(pendingItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (Debugger.IsAttached) throw;
            }
        }
    }

    static async Task<List<ActionItem>> RefreshBgmRssAsync()
    {
        var items = _bgmRss.GetRssItems();
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
                NoteId = createdNote.Id,
                State = item.State
            });
        }
    }
}