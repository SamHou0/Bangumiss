using Bangumiss;

namespace BangumissTest;

[TestClass]
public sealed class BgmApiTest
{
    [TestMethod]
    public async Task TestBgmApiGetCollection()
    {
        BgmApi bgmApi = new BgmApi();
        var collection = await bgmApi.GetUserCollectByUrl(new Uri("https://bgm.tv/subject/363612"));
        Assert.IsNotNull(collection);
        Assert.AreEqual
            ("Test bangumi RSS https://github.com/SamHou0/Bangumiss", 
                collection.Comment);
        Assert.AreEqual(
            7,collection.Rate);
    }
}