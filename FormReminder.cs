using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class FormReminder : Form, IReminderObserver
    {
        private const int shakeIntensity = 5;
        private const int shakeDuration = 1000; 
        private Point originalPosition;

        // Form taşıma işlemleri için mouse olayları
        private bool dragging = false;
        private Point startPoint;

        private User currentUser;
        private UserManager userManager;

        private int selectedRowIndex = -1;

        public FormReminder(User currentUser, UserManager userManager)
        {
            InitializeComponent();
            this.currentUser = currentUser;
            this.userManager = userManager;
            currentUser.TaskReminderManager.RegisterObserver(this);
            UpdateReminderList();
            originalPosition = this.Location;
            
            // ComboBox'a varsayılan değerleri ekle
            reminderTypeComboBox.Items.Clear();
            reminderTypeComboBox.Items.Add("Task");
            reminderTypeComboBox.Items.Add("Meeting");
            reminderTypeComboBox.SelectedIndex = 0;
        }
        
        // Form taşıma işlemleri için mouse olayları
        private void FormReminder_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void FormReminder_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void FormReminder_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }
        
        private void FormReminder_Load(object sender, EventArgs e)
        {
            UpdateReminderList();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult cikis = MessageBox.Show("Uygulamayı Kapatmak İstediğinize Emin Misiniz??", "Uyarı", MessageBoxButtons.OKCancel);
            if (cikis == DialogResult.OK)
            {
                this.Close();
                Application.Exit();
            }
            else { return; }
        }

        private void btnAlt_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAddReminder_Click(object sender, EventArgs e)
        {
            ReminderFactory factory = reminderTypeComboBox.SelectedItem.ToString() == "Meeting"
                ? (ReminderFactory)new MeetingReminderFactory()
                : new TaskReminderFactory();

            Reminder reminder = factory.CreateReminder();
            reminder.ReminderDate = dateTimePicker1.Value;
            reminder.Summary = summaryTextBox.Text;
            reminder.FullDescription = descriptionTextBox.Text;

            // Set IsTriggered to false when adding a new reminder
            reminder.IsTriggered = false;

            currentUser.TaskReminderManager.InsertReminder(reminder);
            UpdateReminderList();
            currentUser.TaskReminderManager.ExportRemindersToExcel(Convert.ToString(currentUser.Username));
            MessageBox.Show("Başarıyla Eklendi");
            descriptionTextBox.Text = "";
            summaryTextBox.Text = "";
            remindersDataGridView.ClearSelection();
        }

        private void btnUpdateReminder_Click(object sender, EventArgs e)
        {
            if (remindersDataGridView.SelectedRows.Count > 0)
            {
                int index = remindersDataGridView.SelectedRows[0].Index;
                Reminder reminder = currentUser.TaskReminderManager.ListReminders()[index];
                reminder.ReminderDate = dateTimePicker1.Value;
                reminder.Summary = summaryTextBox.Text;
                reminder.FullDescription = descriptionTextBox.Text;
                currentUser.TaskReminderManager.EditReminder(index, reminder);
                UpdateReminderList();
                currentUser.TaskReminderManager.ExportRemindersToExcel(Convert.ToString(currentUser.Username));
                MessageBox.Show("Baaşrıyla Güncellendi!");
                summaryTextBox.Text = "";
                descriptionTextBox.Text = "";
            }
            else
            {
                MessageBox.Show("Güncellenecek Bilgiyi Seçiniz");
            }
        }

        private void btnDeleteReminder_Click(object sender, EventArgs e)
        {
            if (remindersDataGridView.SelectedRows.Count > 0)
            {
                int index = remindersDataGridView.SelectedRows[0].Index;
                currentUser.TaskReminderManager.DeleteReminder(index);
                UpdateReminderList();
                currentUser.TaskReminderManager.ExportRemindersToExcel(Convert.ToString(currentUser.Username));
                MessageBox.Show("Başarıyla Silindi");
                descriptionTextBox.Text = "";
                summaryTextBox.Text = "";
                remindersDataGridView.ClearSelection();
            }
            else
            {
                MessageBox.Show("Silinecek Bilgiyi Seçiniz");
            }
        }

        private void UpdateReminderList()
        {
            remindersDataGridView.Rows.Clear();
            foreach (Reminder reminder in currentUser.TaskReminderManager.ListReminders())
            {
                remindersDataGridView.Rows.Add(reminder.ReminderDate.ToString("dd.MM.yyyy HH:mm"), reminder.Summary, reminder.FullDescription);
            }
        }
        public void RefreshHeader(string summary)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(RefreshHeader), summary);
            }
            else
            {
                this.Text = summary;
            }
        }
        public void ShakeWindow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShakeWindow));
            }
            else
            {
                Random random = new Random();
                int shakeIterations = shakeDuration / 5; // Shake faster by reducing iterations
                Point originalPosition = this.Location; // Ensure originalPosition is set correctly

                for (int i = 0; i < shakeIterations; i++)
                {
                    int offsetX = random.Next(-shakeIntensity, shakeIntensity + 1);
                    int offsetY = random.Next(-shakeIntensity, shakeIntensity + 1);
                    this.Location = new Point(originalPosition.X + offsetX, originalPosition.Y + offsetY);
                    System.Threading.Thread.Sleep(5); // Shake faster by reducing sleep duration
                    this.Location = originalPosition; // Reset to original position after each shake iteration
                }

                this.Location = originalPosition; // Ensure the form is in its original position at the end
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < remindersDataGridView.Rows.Count)
            {
                // Set the selected row index
                selectedRowIndex = e.RowIndex;
                // Retrieve data from the selected row
                DataGridViewRow selectedRow = remindersDataGridView.Rows[e.RowIndex];
                summaryTextBox.Text = selectedRow.Cells["Özet"].Value.ToString();
                descriptionTextBox.Text = selectedRow.Cells["Açıklama"].Value.ToString();
                dateTimePicker1.Value = DateTime.ParseExact(selectedRow.Cells["Zaman"].Value.ToString(), "dd.MM.yyyy HH:mm", null);
                remainderTabControl1.SelectedTab = tabPage1;
            }
        }

     
    }

}
