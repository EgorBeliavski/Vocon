using Microsoft.Maui.Storage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Vocon.Models;

namespace Vocon.Services
{
    public interface INoteRepository{
        Task InitializeAsync();
        Task<int> SaveNoteAsync(Note note);
        Task<List<Note>> GetAllNotesAsync();
        Task<int> DeleteNoteAsync(Note note);
        Task<int> UpdateNoteAsync(Note note);

    }
    public class NoteRepository : INoteRepository
    {
        private SQLiteAsyncConnection _connection;

        public async Task InitializeAsync()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vocon.db3");
            Debug.WriteLine($"File exists after create: {(dbPath)}");
            _connection = new SQLiteAsyncConnection(dbPath);
            await _connection.CreateTableAsync<Note>().ConfigureAwait(false);
        }

        public async Task<int> SaveNoteAsync(Note note)
        {
            return await _connection.InsertAsync(note).ConfigureAwait(false);
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            return await _connection.Table<Note>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<int> DeleteNoteAsync(Note note)
        {
            return await _connection.DeleteAsync(note).ConfigureAwait(false);
        }
        public async Task<int> UpdateNoteAsync(Note note)
        {
            return await _connection.UpdateAsync(note);
        }
    }
}
