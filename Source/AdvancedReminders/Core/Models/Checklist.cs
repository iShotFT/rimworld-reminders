using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AdvancedReminders.Core.Models
{
    public class Checklist : IExposable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Reminder> Reminders { get; set; }
        
        public Checklist()
        {
            Reminders = new List<Reminder>();
        }
        
        public Checklist(string name, string description = "") : this()
        {
            Name = name;
            Description = description;
        }
        
        public IEnumerable<Reminder> ActiveReminders => 
            Reminders?.Where(r => r.IsActive) ?? Enumerable.Empty<Reminder>();
        
        public IEnumerable<Reminder> CompletedReminders => 
            Reminders?.Where(r => !r.IsActive) ?? Enumerable.Empty<Reminder>();
        
        public int TotalCount => Reminders?.Count ?? 0;
        public int ActiveCount => ActiveReminders.Count();
        public int CompletedCount => CompletedReminders.Count();
        
        public float CompletionPercentage
        {
            get
            {
                if (TotalCount == 0) return 100f;
                return (float)CompletedCount / TotalCount * 100f;
            }
        }
        
        public void AddReminder(Reminder reminder)
        {
            if (reminder != null)
            {
                Reminders.Add(reminder);
            }
        }
        
        public void RemoveReminder(Reminder reminder)
        {
            if (reminder != null)
            {
                Reminders.Remove(reminder);
            }
        }
        
        public void RemoveReminder(int reminderId)
        {
            var reminder = Reminders.FirstOrDefault(r => r.Id == reminderId);
            if (reminder != null)
            {
                Reminders.Remove(reminder);
            }
        }
        
        public void ExposeData()
        {
            string name = Name;
            string description = Description;
            
            Scribe_Values.Look(ref name, "name", "");
            Scribe_Values.Look(ref description, "description", "");
            List<Reminder> reminders = Reminders;
            Scribe_Collections.Look(ref reminders, "reminders", LookMode.Deep);
            Reminders = reminders;
            
            Name = name;
            Description = description;
        }
    }
}