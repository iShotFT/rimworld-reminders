using AdvancedReminders.Core.Enums;
using AdvancedReminders.Core.Models;
using RimWorld;
using System.Linq;
using Verse;

namespace AdvancedReminders.Domain.Triggers
{
    public class QuestDeadlineTrigger : Trigger
    {
        public override TriggerType Type => TriggerType.Quest;
        
        public int QuestId { get; set; }
        public int HoursBeforeExpiry { get; set; }
        public int CalculatedTriggerTick { get; set; }
        
        public override string Description
        {
            get
            {
                var quest = GetAssociatedQuest();
                if (quest != null)
                {
                    return $"{HoursBeforeExpiry}h before '{quest.name}' expires";
                }
                return $"{HoursBeforeExpiry}h before quest expires";
            }
        }
        
        public QuestDeadlineTrigger()
        {
            HoursBeforeExpiry = 24; // Default to 1 day before
        }
        
        public QuestDeadlineTrigger(int questId, int hoursBeforeExpiry) : this()
        {
            QuestId = questId;
            HoursBeforeExpiry = hoursBeforeExpiry;
            CalculateTriggerTick();
        }
        
        public Quest GetAssociatedQuest()
        {
            if (QuestId == 0)
                return null;
                
            return Find.QuestManager.QuestsListForReading.Find(q => q.id == QuestId);
        }
        
        public void CalculateTriggerTick()
        {
            var quest = GetAssociatedQuest();
            if (quest != null && quest.TicksUntilExpiry > 0)
            {
                CalculatedTriggerTick = quest.acceptanceExpireTick - (HoursBeforeExpiry * GenDate.TicksPerHour);
                
                // Make sure we don't trigger in the past
                if (CalculatedTriggerTick <= Find.TickManager.TicksGame)
                {
                    CalculatedTriggerTick = Find.TickManager.TicksGame + 60; // 1 second from now
                }
            }
        }
        
        public override bool Evaluate()
        {
            if (IsTriggered)
                return false;
                
            var quest = GetAssociatedQuest();
            if (quest == null)
            {
                // Quest no longer exists or was removed
                IsTriggered = true;
                return false; // Don't trigger, just mark as completed
            }
            
            // If quest was accepted, mark trigger as completed without firing
            if (quest.State != QuestState.NotYetAccepted)
            {
                IsTriggered = true;
                return false; // Quest accepted, don't trigger reminder
            }
            
            // Recalculate trigger tick if needed (in case quest data changed)
            if (CalculatedTriggerTick == 0)
            {
                CalculateTriggerTick();
            }
            
            var currentTick = Find.TickManager.TicksGame;
            bool shouldTrigger = currentTick >= CalculatedTriggerTick;
            
            if (shouldTrigger)
            {
                IsTriggered = true;
            }
            
            return shouldTrigger;
        }
        
        public override void Reset()
        {
            IsTriggered = false;
            CalculateTriggerTick();
        }
        
        public int TicksRemaining
        {
            get
            {
                if (CalculatedTriggerTick == 0)
                    CalculateTriggerTick();
                    
                var remaining = CalculatedTriggerTick - Find.TickManager.TicksGame;
                return remaining > 0 ? remaining : 0;
            }
        }
        
        public string TimeRemainingDescription
        {
            get
            {
                var remaining = TicksRemaining;
                if (remaining <= 0)
                    return "Now";
                    
                var days = (int)GenDate.TicksToDays(remaining);
                var hours = (remaining % GenDate.TicksPerDay) / GenDate.TicksPerHour;
                var minutes = (remaining % GenDate.TicksPerHour) / 60; // 60 ticks per minute
                
                if (days > 0)
                    return hours > 0 ? $"{days}d {hours}h" : $"{days}d";
                else if (hours > 0)
                    return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
                else
                    return $"{minutes}m";
            }
        }
        
        public string DetailedTriggerDescription
        {
            get
            {
                if (CalculatedTriggerTick == 0)
                    CalculateTriggerTick();
                    
                var quest = GetAssociatedQuest();
                if (quest != null)
                {
                    // Get trigger date and time
                    var firstMap = Find.Maps.FirstOrDefault();
                    var tile = firstMap?.Tile ?? 0;
                    var targetDate = GenDate.DateReadoutStringAt(CalculatedTriggerTick, Find.WorldGrid.LongLatOf(tile));
                    var timeRemaining = TimeRemainingDescription;
                    return $"Triggers on: {targetDate} (in {timeRemaining})";
                }
                return "Quest deadline trigger";
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            
            int questId = QuestId;
            int hoursBeforeExpiry = HoursBeforeExpiry;
            int calculatedTriggerTick = CalculatedTriggerTick;
            
            Scribe_Values.Look(ref questId, "questId", 0);
            Scribe_Values.Look(ref hoursBeforeExpiry, "hoursBeforeExpiry", 24);
            Scribe_Values.Look(ref calculatedTriggerTick, "calculatedTriggerTick", 0);
            
            QuestId = questId;
            HoursBeforeExpiry = hoursBeforeExpiry;
            CalculatedTriggerTick = calculatedTriggerTick;
            
            // Recalculate trigger tick after loading if it wasn't saved properly
            if (CalculatedTriggerTick == 0)
            {
                CalculateTriggerTick();
            }
        }
    }
}