using System.Collections.Generic;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Core.Interfaces;
using Verse;

namespace AdvancedReminders.Core.Models
{
    public class Reminder : IExposable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public SeverityLevel Severity { get; set; }
        public bool IsActive { get; set; }
        public bool IsRepeating { get; set; }
        public int CreatedTick { get; set; }
        public int LastTriggeredTick { get; set; }
        
        public ITrigger Trigger { get; set; }
        public List<IReminderAction> Actions { get; set; }
        
        public Reminder()
        {
            Actions = new List<IReminderAction>();
            IsActive = true;
            CreatedTick = Find.TickManager.TicksGame;
        }
        
        public Reminder(string title, string description, SeverityLevel severity = SeverityLevel.Medium) : this()
        {
            Title = title;
            Description = description;
            Severity = severity;
        }
        
        public bool ShouldTrigger()
        {
            return IsActive && Trigger != null && Trigger.Evaluate();
        }
        
        public void TriggerActions()
        {
            if (Actions != null)
            {
                foreach (var action in Actions)
                {
                    action.Execute(this);
                }
            }
            
            LastTriggeredTick = Find.TickManager.TicksGame;
            
            if (!IsRepeating)
            {
                IsActive = false;
            }
            else
            {
                Trigger?.Reset();
            }
        }
        
        public virtual void ExposeData()
        {
            int id = Id;
            string title = Title;
            string description = Description;
            SeverityLevel severity = Severity;
            bool isActive = IsActive;
            bool isRepeating = IsRepeating;
            int createdTick = CreatedTick;
            int lastTriggeredTick = LastTriggeredTick;
            
            Scribe_Values.Look(ref id, "id", 0);
            Scribe_Values.Look(ref title, "title", "");
            Scribe_Values.Look(ref description, "description", "");
            Scribe_Values.Look(ref severity, "severity", SeverityLevel.Medium);
            Scribe_Values.Look(ref isActive, "isActive", true);
            Scribe_Values.Look(ref isRepeating, "isRepeating", false);
            Scribe_Values.Look(ref createdTick, "createdTick", 0);
            Scribe_Values.Look(ref lastTriggeredTick, "lastTriggeredTick", 0);
            
            Id = id;
            Title = title;
            Description = description;
            Severity = severity;
            IsActive = isActive;
            IsRepeating = isRepeating;
            CreatedTick = createdTick;
            LastTriggeredTick = lastTriggeredTick;
            
            // Note: Trigger and Actions serialization will be handled by derived classes
            // or specialized serialization system due to interface/abstract types
        }
    }
}