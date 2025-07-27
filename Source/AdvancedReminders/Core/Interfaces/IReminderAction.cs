using AdvancedReminders.Core.Enums;
using AdvancedReminders.Core.Models;

namespace AdvancedReminders.Core.Interfaces
{
    public interface IReminderAction
    {
        ActionType Type { get; }
        string Description { get; }
        
        void Execute(Reminder reminder);
    }
}