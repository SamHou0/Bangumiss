using Bangumiss;

namespace BangumissTest;

[TestClass]
public sealed class MisskeyApiTest
{
    [TestMethod]
    public async Task TestNotePost()
    {
        MisskeyApi misskeyApi = new();
        await misskeyApi.PostNote("Test Note","Hide Text",true);
        
    }
}