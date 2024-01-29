using ElevenNote.Data;
using ElevenNote.Models.Note;
using Microsoft.EntityFrameworkCore;

namespace ElevenNote.Services.Note
{
    public interface INoteService
    {  
        Task<NoteListItem?> CreateNoteAsync(NoteCreate request);
        Task<IEnumerable<NoteListItem>> GetAllNotesAsync(); 
        // Task<NoteDetail?> GetNoteByIdAsync(int noteId);
    }
}