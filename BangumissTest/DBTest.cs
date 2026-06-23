using Bangumiss.Data;
using Bangumiss.Database;

namespace BangumissTest;
[TestClass]
public class DBTest
{
    [TestMethod]
    public async Task TestDBCreation()
    {
        Database db = new Database();
        Assert.IsNotNull(db);
        Assert.IsTrue(File.Exists("bangumiss.db"));
    }

    [TestMethod]
    public async Task TestItemAddition()
    {
        Database db = new Database();
        DatabaseItem item = new DatabaseItem()
        {
            ItemId = "-114514",
            NoteId = "test_note_id",
            State = EntryState.Done
        };
        db.RemoveItemNote(item.ItemId);
        db.AddItemNote(item);
        Assert.AreEqual(db.GetItemState(item.ItemId),item.State);
        db.RemoveItemNote(item.ItemId);
    }
}