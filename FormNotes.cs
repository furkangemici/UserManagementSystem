using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Drawing;

namespace WindowsFormsApp2
{
    public partial class FormNotes : Form, IReminderObserver
    {
        private User currentUser;
        private Point originalPosition;

        public FormNotes(User currentUser)
        {
            InitializeComponent();
            this.currentUser = currentUser;
            notesDataGridView.Columns.Add("Id", "Id");
            notesDataGridView.Columns.Add("Timestamp", "Timestamp");

            notesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            notesDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            currentUser.NotesManager.LoadNotesFromExcel($"{currentUser.Username}_Notes.xlsx");
            LoadNotes();

            currentUser.TaskReminderManager.RegisterObserver(this);
            originalPosition = this.Location;

        }

        public void RefreshHeader(string summary)
        {
            GlobalSettings.UpdateHeader(this, summary);
        }

        public void ShakeWindow()
        {
            GlobalSettings.ShakeWindow(this);
        }
        private void FormNotes_Load(object sender, EventArgs e)
        {
            
        }

        private void LoadNotes()
        {
            notesDataGridView.Rows.Clear();
            var notes = currentUser.NotesManager.GetNotesList();

            foreach (var note in notes)
            {
                bool exists = false;
                foreach (DataGridViewRow row in notesDataGridView.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[0].Value.ToString() == note.NoteId.ToString())
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    notesDataGridView.Rows.Add(note.NoteId, note.Timestamp);
                }
            }
        }


        private void btnAddNote_Click(object sender, EventArgs e)
        {
            string noteContent = noteContentTextBox.Text;
            if (!string.IsNullOrEmpty(noteContent))
            {
                currentUser.NotesManager.InsertNote(noteContent);
                noteContentTextBox.Clear();
                LoadNotes();
                currentUser.NotesManager.WriteNotesToExcelFile(Convert.ToString( currentUser.Username));
            }
            else
            {
                MessageBox.Show("Not içeriği boş olamaz.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateNote_Click(object sender, EventArgs e)
        {
            if (notesDataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = notesDataGridView.SelectedRows[0];

                int selectedNoteId = Convert.ToInt32(selectedRow.Cells[0].Value);

                string updatedContent = noteContentTextBox.Text;

                try
                {
                    currentUser.NotesManager.EditNote(selectedNoteId, updatedContent);
                    LoadNotes(); 
                    currentUser.NotesManager.WriteNotesToExcelFile(currentUser.Username); 
                    MessageBox.Show("Not başarıyla güncellendi.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Not güncellenemedi: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Güncellemek için lütfen bir not seçin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (notesDataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = notesDataGridView.SelectedRows[0];

                int selectedNoteId = Convert.ToInt32(selectedRow.Cells[0].Value);

                try
                {
                    currentUser.NotesManager.DeleteNote(selectedNoteId);
                    LoadNotes(); // Refresh the DataGridView
                    currentUser.NotesManager.WriteNotesToExcelFile(currentUser.Username); // Export to Excel
                    MessageBox.Show("Not başarıyla silindi.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Not silinemedi: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Silmek için lütfen bir not seçin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void btnAlt_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized; 
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult cikis = MessageBox.Show("Uygulamadan çıkış yapmak istediğinize emin misiniz?", "Uyarı", MessageBoxButtons.OKCancel);
            if (cikis == DialogResult.OK)
            {
                this.Close();
                Application.Exit();
            }
            else { return; }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < notesDataGridView.Rows.Count)
            {
                DataGridViewRow row = notesDataGridView.Rows[e.RowIndex];
                if (row.Cells[0].Value != null) // Ensure the cell value is not null
                {
                    int selectedNoteId = Convert.ToInt32(row.Cells[0].Value); // Get the ID from the first column
                    var selectedNote = currentUser.NotesManager.GetNoteById(selectedNoteId); // Get the selected note by ID
                    noteContentTextBox.Text = selectedNote.NoteContent; // Set the content of the selected note to the textBox1
                }
            }
        }
    }

}
