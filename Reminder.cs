using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers; // Explicitly using System.Timers
using System.Windows.Forms;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;


namespace WindowsFormsApp2
{
    public interface IReminderObserver
    {
        void RefreshHeader(string summary);
        void ShakeWindow();
    }

    public abstract class Reminder
    {
        public DateTime ReminderDate { get; set; }
        public string Summary { get; set; }
        public string FullDescription { get; set; }

        public bool IsTriggered { get; set; }

        public virtual string GetReminderType()
        {
            return "Reminder";
        }

        public abstract void Notify();
    }

    public class MeetingReminder : Reminder
    {
        public override void Notify()
        {
            Console.WriteLine("Meeting Reminder: " + Summary);
        }
        public override string GetReminderType()
        {
            return "Meeting Reminder";
        }
    }

    public class TaskReminder : Reminder
    {
        public override void Notify()
        {
            Console.WriteLine("Task Reminder: " + Summary);
        }

        public override string GetReminderType()
        {
            return "Task Reminder";
        }
    }

    public abstract class ReminderFactory
    {
        public abstract Reminder CreateReminder();
    }

    public class MeetingReminderFactory : ReminderFactory
    {
        public override Reminder CreateReminder()
        {
            return new MeetingReminder();
        }
    }

    public class TaskReminderFactory : ReminderFactory
    {
        public override Reminder CreateReminder()
        {
            return new TaskReminder();
        }
    }

    public class ReminderManager
    {
        private List<Reminder> reminders = new List<Reminder>();
        private List<IReminderObserver> observers = new List<IReminderObserver>();
        private System.Timers.Timer checkTimer;

        public ReminderManager()
        {
            checkTimer = new System.Timers.Timer(1000); // Check every second
            checkTimer.Elapsed += HandleTimerElapsed;
            checkTimer.Start();
        }

        public void InsertReminder(Reminder reminder)
        {
            reminders.Add(reminder);
            NotifyObservers(reminder.Summary);
        }

        public void EditReminder(int index, Reminder reminder)
        {
            if (index >= 0 && index < reminders.Count)
            {
                var oldReminder = reminders[index];
                reminders[index] = reminder;

                // If the reminder is already triggered, check if the new date is in the past
                if (oldReminder.IsTriggered && reminder.ReminderDate <= DateTime.Now)
                {
                    NotifyShake(reminder);
                }

                NotifyObservers(reminder.Summary);
            }
        }


        public void DeleteReminder(int index)
        {
            if (index >= 0 && index < reminders.Count)
            {
                reminders.RemoveAt(index);
            }
        }
        public List<Reminder> GetReminders()
        {
            return reminders;
        }
        public void RemoveAllReminders()
        {
            reminders.Clear();
        }
        public List<Reminder> ListReminders()
        {
            return new List<Reminder>(reminders);
        }

        public void RegisterObserver(IReminderObserver observer)
        {
            observers.Add(observer);
        }

        public void UnregisterObserver(IReminderObserver observer)
        {
            observers.Remove(observer);
        }

        private void NotifyObservers(string summary)
        {
            foreach (var observer in observers)
            {
                observer.RefreshHeader(summary);
            }
        }

        private void NotifyShake(Reminder reminder)
        {
            // Pencereyi titretme işlemini UI thread'inde yap
            if (Application.OpenForms[0].InvokeRequired)
            {
                Application.OpenForms[0].Invoke(new Action(() =>
                {
                    foreach (var observer in observers)
                    {
                        observer.ShakeWindow();
                    }
                    // MessageBox'ı titretme işleminden sonra doğrudan UI thread'inde göster
                    MessageBox.Show($"{reminder.GetReminderType()}: {reminder.Summary}", "Reminder Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            else
            {
                foreach (var observer in observers)
                {
                    observer.ShakeWindow();
                }
                // MessageBox'ı doğrudan UI thread'inde göster
                MessageBox.Show($"{reminder.GetReminderType()}: {reminder.Summary}", "Reminder Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var current = DateTime.Now;

            foreach (var reminder in reminders.Where(r => !r.IsTriggered && r.ReminderDate <= current))
            {
                reminder.IsTriggered = true;
                NotifyShake(reminder);
            }
        }


        public void ExportRemindersToExcel(string userName)
        {
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Reminders");

                // Add headers
                sheet.Cell(1, 1).Value = "Date";
                sheet.Cell(1, 2).Value = "Summary";
                sheet.Cell(1, 3).Value = "Description";
                sheet.Cell(1, 4).Value = "Type";

                // Add reminder entries
                for (int i = 0; i < reminders.Count; i++)
                {
                    var reminder = reminders[i];
                    sheet.Cell(i + 2, 1).Value = reminder.ReminderDate;
                    sheet.Cell(i + 2, 2).Value = reminder.Summary;
                    sheet.Cell(i + 2, 3).Value = reminder.FullDescription;
                    sheet.Cell(i + 2, 4).Value = reminder.GetReminderType();
                }

                string fileName = $"{userName}_Reminder.xlsx";
                workbook.SaveAs(fileName);
            }
        }

        public void ImportRemindersFromExcel(string filePath, out string errMassage)
        {
            errMassage = string.Empty;

            if (!File.Exists(filePath))
            {
                errMassage = "Dosya mevcut değil.";
                return;
            }

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed();

                    foreach (var row in rows)
                    {
                        if (row.RowNumber() == 1) continue; // Skip header row

                        var dateTime = row.Cell(1).GetDateTime();
                        var summary = row.Cell(2).GetString();
                        var description = row.Cell(3).GetString();
                        var type = row.Cell(4).GetString();

                        Reminder reminder;
                        if (type == "Meeting Reminder")
                        {
                            reminder = new MeetingReminder();
                        }
                        else if (type == "Task Reminder")
                        {
                            reminder = new TaskReminder();
                        }
                        else
                        {
                            errMassage = $"Bilinmeyen hatırlatıcı(reminder) türü: {type}";
                            return;
                        }

                        reminder.ReminderDate = dateTime;
                        reminder.Summary = summary;
                        reminder.FullDescription = description;

                        // Set IsTriggered based on whether the reminder time has passed
                        reminder.IsTriggered = dateTime <= DateTime.Now;

                        reminders.Add(reminder);
                    }
                }
            }
            catch (Exception ex)
            {
                errMassage = $"Dosya içe aktarılırken bir hata oluştu: {ex.Message}";
            }
        }



    }
}
