using AdvancedReminders.Core.Enums;
using AdvancedReminders.Core.Interfaces;
using Verse;

namespace AdvancedReminders.Core.Models
{
    public abstract class ReminderAction : IReminderAction
    {
        public abstract ActionType Type { get; }
        public abstract string Description { get; }
        
        public abstract void Execute(Reminder reminder);
        
        public virtual void ExposeData()
        {
            // Base action serialization - override in derived classes
        }
    }
}