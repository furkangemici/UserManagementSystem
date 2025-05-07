using System;

namespace WindowsFormsApp2
{
    public class Note
    {
        public int NoteId { get; set; } // Added an Id property to uniquely identify notes
        public string NoteContent { get; set; }
        public DateTime Timestamp { get; set; }

        public Note()
        {
            Timestamp = DateTime.Now;
        }

        public Note(int id, string content) : this()
        {
            NoteId = id;
            NoteContent = content;
        }
        public Note(int ıd, string content, DateTime timestamp) : this(ıd, content)
        {
            Timestamp = timestamp;
        }

        public void UpdateContent(string newContent)
        {
            NoteContent = newContent;
            Timestamp = DateTime.Now;
        }
    }
}
