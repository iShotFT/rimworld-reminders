using AdvancedReminders.Core.Enums;
using AdvancedReminders.Core.Interfaces;
using Verse;

namespace AdvancedReminders.Core.Models
{
    public abstract class Trigger : IExposable, ITrigger
    {
        public abstract TriggerType Type { get; }
        public bool IsTriggered { get; protected set; }
        public abstract string Description { get; }
        
        public abstract bool Evaluate();
        
        public virtual void Reset()
        {
            IsTriggered = false;
        }
        
        public virtual void ExposeData()
        {
            bool isTriggered = IsTriggered;
            Scribe_Values.Look(ref isTriggered, "isTriggered", false);
            IsTriggered = isTriggered;
        }
    }
}