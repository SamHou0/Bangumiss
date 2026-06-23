using Bangumiss;
using Bangumiss.Misskey;

namespace BangumissTest;

[TestClass]
public sealed class MisskeyApiTest
{
    [TestMethod]
    public async Task TestNotePost()
    {
        MisskeyApi misskeyApi = new();
        await misskeyApi.PostNote("Test Note","Hide Text",true);
        await misskeyApi.PostNote("Test Note",isLocalOnly:true);
    }

    [TestMethod]
    public async Task TestNoteEditWithDelete()
    {
        MisskeyApi misskeyApi = new();
        var note = await misskeyApi.PostNote("Test Note", "Hide Text", true);
        await misskeyApi.EditNote(note.Id, "Edited text", "Hide Text", true);
        // This will give 10 sec observe time to see whether it's up and running.
        Thread.Sleep(10000);
        await misskeyApi.DeleteNote(note.Id);
    }
}