using Bangumiss.Data;
using Microsoft.Data.Sqlite;

namespace Bangumiss.Database;

public class Database
{
    private SqliteConnection _connection = new SqliteConnection(ConnectionString);
    const string ConnectionString = @"Data Source=data/bangumiss.db";
    public Database()
    {
        _connection.Open();
        InitDB();
    }
    ~Database()
    {
        _connection.Close();
    }

    public void InitDB()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS item_notes (
                                  item_id TEXT PRIMARY KEY NOT NULL,
                                  item_state INTEGER NOT NULL,
                                  note_id TEXT NOT NULL
                              );
                              """;
        command.ExecuteNonQuery();
    }
    public string GetItemNoteId(string itemId)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              SELECT note_id FROM item_notes
                              WHERE item_id = @itemId
                              """;
        command.Parameters.AddWithValue("@itemId",itemId);
        using var reader = command.ExecuteReader();
        reader.Read();
        return reader["note_id"].ToString() ?? string.Empty;
    }

    public void AddItemNote(DatabaseItem item)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              INSERT INTO item_notes
                              VALUES (@itemId,@itemState,@noteId)
                              """;
        command.Parameters.AddWithValue("@itemId", item.ItemId);
        command.Parameters.AddWithValue("@itemState", item.State);
        command.Parameters.AddWithValue("@noteId", item.NoteId);
        command.ExecuteNonQuery();
    }

    public void RemoveItemNote(string itemId)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              Delete FROM item_notes
                              WHERE item_id = @itemId
                              """;
        command.Parameters.AddWithValue("@itemId", itemId);
        command.ExecuteNonQuery();
    }
    public EntryState GetItemState(string itemId)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
                              SELECT item_id,item_state FROM item_notes
                              WHERE item_id = @itemId
                              """;
        command.Parameters.AddWithValue("@itemId",itemId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
            return (EntryState)Convert.ToInt32(reader["item_state"]);
        else return EntryState.NotInDb;
    }
    // Editing item is not allowed and no use. Add new notes instead.
}