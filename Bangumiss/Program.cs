namespace Bangumiss;

class Program
{
    static void Main(string[] args)
    {
        BgmRss bgmRss = new()
        {
            BgmUri = new Uri("https://bgm.tv/feed/user/samhou/timeline")
        };
        foreach (var item in bgmRss.GetRssItems())
        {
            Console.WriteLine(item);
        }
    }
}