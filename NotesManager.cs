using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace WindowsFormsApp2
{
    public class NotesManager
    {
        private List<Note> noteList;
        private int nextAvailableId;

        public NotesManager()
        {
            noteList = new List<Note>();
            nextAvailableId = 1; // Initialize nextId to 1
        }

        public void InsertNote(string noteContent)
        {
            var currentNote = new Note(nextAvailableId, noteContent);
            noteList.Add(currentNote);
            nextAvailableId++;
        }

        public void EditNote(int id, string newContent)
        {
            var note = noteList.FirstOrDefault(n => n.NoteId == id);
            if (note != null)
            {
                note.UpdateContent(newContent);
            }
            else
            {
                throw new Exception("Not bulunamadı.");
            }
        }

        public void DeleteNote(int id)
        {
            var note = noteList.FirstOrDefault(n => n.NoteId == id);
            if (note != null)
            {
                noteList.Remove(note);
            }
            else
            {
                throw new Exception("Not bulunamadı.");
            }
        }

        public List<Note> GetNotesList()
        {
            return noteList;
        }

        public Note GetNoteById(int id)
        {
            var note = noteList.FirstOrDefault(n => n.NoteId == id);
            if (note != null)
            {
                return note;
            }
            else
            {
                throw new Exception("Not bulunamadı.");
            }
        }

        public void WriteNotesToExcelFile(string userName)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Notes");

                worksheet.Cell("A1").Value = "ID";
                worksheet.Cell("B1").Value = "Content";
                worksheet.Cell("C1").Value = "Timestamp";

                for(int i = 0; i < noteList.Count; i++)
                {
                    var note = noteList[i];
                    worksheet.Cell(i + 2, 1).Value = note.NoteId;
                    worksheet.Cell(i + 2, 2).Value = note.NoteContent;
                    worksheet.Cell(i + 2, 3).Value = note.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                }
                string fileName = $"{userName}_Notes.xlsx";
                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(fileName);
            }
        }

        public void LoadNotesFromExcel(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                    foreach (var row in rows)
                    {
                        try
                        {
                            var id = row.Cell(1).GetValue<int>();
                            var content = row.Cell(2).GetValue<string>();
                            var timestampString = row.Cell(3).GetValue<string>(); // Get the timestamp as a string

                            DateTime timestamp;
                            if (DateTime.TryParseExact(timestampString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
                            {
                                var note = new Note(id, content, timestamp);
                                noteList.Add(note);

                                if (id >= nextAvailableId)
                                {
                                    nextAvailableId = id + 1; // Ensure nextId is always greater than any existing ID
                                }
                            }
                            else
                            {
                                throw new Exception($"Geçersiz tarih formatı: {timestampString}");
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine($"İlgili satır işlenirken bir hata oluştu: {row.RowNumber()}: {exception.Message}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Dosya mevcut değil.");
            }
        }

    }
}
