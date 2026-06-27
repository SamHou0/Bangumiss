namespace Bangumiss.Data;

public class DatabaseItem
{
    public required string ItemId;
    public required string NoteId;
    public EntryState State; // The state of an entry. See EntryState.cs
}