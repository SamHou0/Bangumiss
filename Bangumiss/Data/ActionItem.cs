namespace Bangumiss.Data;

public class ActionItem
{
    public string NoteId = String.Empty;
    public NoteAction Action;
    public required string Title;
    public required string BgmId;
    public required string Comment;
    public required string[] Tags;
    public required int Rate;
    public required EntryState State;
}