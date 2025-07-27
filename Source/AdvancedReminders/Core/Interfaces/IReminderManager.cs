using System.Collections.Generic;
using AdvancedReminders.Core.Models;

namespace AdvancedReminders.Core.Interfaces
{
    public interface IReminderManager
    {
        IReadOnlyList<Reminder> ActiveReminders { get; }
        
        void AddReminder(Reminder reminder);
        void UpdateReminder(Reminder reminder);
        void RemoveReminder(int reminderId);
        Reminder GetReminder(int reminderId);
        void ProcessTriggers();
    }
}