using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bangumiss;

public class BgmApi
{
    private readonly string _apiKey =
        Environment.GetEnvironmentVariable("BANGUMISS_BGM_TOKEN") ??
        throw new Exception("Bangumiss Bgm API Key not set");

    private readonly HttpClient _client = new HttpClient();
    private const string BaseUrl = "https://api.bgm.tv";
    private readonly string _userName;

    public BgmApi()
    {
        InitializeClient();
        _userName = GetUserName();
    }

    /// <summary>
    /// Add authorization and user-agent
    /// </summary>
    private void InitializeClient()
    {
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _client.DefaultRequestHeaders.Add(
            "User-Agent", "SamHou0.Bangumiss.1.0.0");
    }

    private string GetUserName()
    {
        var response = _client.GetAsync(BaseUrl + "/v0/me").Result;
        response.EnsureSuccessStatusCode();
        BgmUser user = JsonSerializer.Deserialize<BgmUser>
            (response.Content.ReadAsStringAsync().Result)
            ?? throw new Exception("Empty get user response");
        return user.UserName ?? throw new Exception("Empty user name");
    }

    public async Task<BgmCollectionData> GetUserCollectByUrl(Uri link)
    {
        string id = ExtractBgmId(link);
        var response = await
            _client.GetAsync(BaseUrl+$"/v0/users/{_userName}/collections/{id}");
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<BgmCollectionData>(
                   response.Content.ReadAsStringAsync().Result)
               ?? throw new Exception("Empty get user collection response");
    }

    /// <summary>
    /// Extract the id from bgm link
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    private string ExtractBgmId(Uri link) =>
        link.AbsolutePath.Split('/')
            .Last().TrimEnd('/');
}

public class BgmCollectionData
{
    [JsonPropertyName("comment")] public string? Comment { get; init; }
    [JsonPropertyName("tags")] public string[]? Tags { get; init; }
    [JsonPropertyName("rate")] public int Rate { get; init; }
}

public class BgmUser
{
    [JsonPropertyName("username")]
    public string? UserName { get; init; }
}