using AdvancedReminders.Core.Enums;

namespace AdvancedReminders.Core.Interfaces
{
    public interface ITrigger
    {
        TriggerType Type { get; }
        bool IsTriggered { get; }
        string Description { get; }
        
        bool Evaluate();
        void Reset();
    }
}