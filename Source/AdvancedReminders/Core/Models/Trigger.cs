using AdvancedReminders.Core.Interfaces;
using AdvancedReminders.Core.Enums;
using Verse;

namespace AdvancedReminders.Core.Models
{
    public abstract class Trigger : ITrigger
    {
        public abstract TriggerType Type { get; }
        public virtual bool IsTriggered { get; protected set; }
        public abstract string Description { get; }
        
        public abstract bool Evaluate();
        public abstract void Reset();
        
        public virtual void ExposeData()
        {
            // Base trigger serialization - override in derived classes
        }
    }
}