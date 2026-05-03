using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bangumiss;

public class MisskeyApi
{
    private readonly string _apiKey =
        Environment.GetEnvironmentVariable("BANGUMISS_MISSKEY_KEY")
        ?? throw new Exception("Missing BANGUMISS_MISSKEY_KEY environment variable.");

    private readonly string _misskeyUrl =
        (Environment.GetEnvironmentVariable("BANGUMISS_MISSKEY_URL")
        ?? throw new Exception("Missing BANGUMISS_MISSKEY_URL environment variable."))
        + "/api";

    private readonly HttpClient _client = new HttpClient();

    public MisskeyApi()
    {
        Initialize();
    }

    private void Initialize()
    {
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _client.DefaultRequestHeaders.Add(
            "User-Agent", "SamHou0.Bangumiss.1.0.0");
    }

    public async Task PostNote(string text,string contentWarningText = "",bool isLocalOnly = false)
    {

        var note = new MisskeyNote();
        if (string.IsNullOrEmpty(contentWarningText))
        {
            note.Text = text;
            note.IsLocalOnly = isLocalOnly;
        }
        else
        {
            note.Text = text;
            note.ContentWarningText = contentWarningText;
            note.IsLocalOnly = isLocalOnly;
        };
        StringContent content = new(JsonSerializer.Serialize(note), 
            Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_misskeyUrl+
            "/notes/create",content);
        response.EnsureSuccessStatusCode();
    }
}

public class MisskeyNote
{
    [JsonPropertyName("text")] public string? Text { get; set; }
    [JsonPropertyName("cw")] public string? ContentWarningText { get; set; }
    [JsonPropertyName("localOnly")] public bool IsLocalOnly { get; set; }
    
}