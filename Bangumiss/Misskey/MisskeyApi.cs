using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bangumiss.Misskey;

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

    public async Task<CreatedNote> PostNote(string text, string contentWarningText = "", bool isLocalOnly = false)
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
        }

        StringContent content = new(JsonSerializer.Serialize(note),
            Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_misskeyUrl +
                                               "/notes/create", content);
        response.EnsureSuccessStatusCode();
        var responseObject = JsonSerializer.Deserialize<ResponseObject>(await response.Content.ReadAsStringAsync())
                             ?? throw new Exception("Empty response");
        return responseObject.CreatedNote ?? throw new Exception("Empty response");
    }

    public async Task EditNote(string id, string text, string contentWarningText = "", bool isLocalOnly = false)
    {
        var note = new EditingNote()
        {
            Id = id,
            Text = text,
            ContentWarningText = contentWarningText,
            IsLocalOnly = isLocalOnly
        };
        StringContent content = new(JsonSerializer.Serialize(note),
            Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_misskeyUrl + "/notes/edit", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteNote(string id)
    {
        var note = new DeletingNote()
        {
            Id = id
        };
        StringContent content = new(JsonSerializer.Serialize(note),
            Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_misskeyUrl + "/notes/delete", content);
        response.EnsureSuccessStatusCode();
    }
}

public class DeletingNote
{
    [JsonPropertyName("noteId")] public string? Id { get; set; }
}

public class MisskeyNote
{
    [JsonPropertyName("text")] public string? Text { get; set; }
    [JsonPropertyName("cw")] public string? ContentWarningText { get; set; }
    [JsonPropertyName("localOnly")] public bool IsLocalOnly { get; set; } = false;

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; } =
        Environment.GetEnvironmentVariable("BANGUMISS_MISSKEY_VISIBILITY") ?? "home";
}

public class EditingNote : MisskeyNote
{
    [JsonPropertyName("editId")] public string? Id { get; set; }
}

public class ResponseObject
{
    [JsonPropertyName("createdNote")] public CreatedNote? CreatedNote { get; set; }
}

public class CreatedNote
{
    [JsonPropertyName("id")] public string? Id { get; set; }
}