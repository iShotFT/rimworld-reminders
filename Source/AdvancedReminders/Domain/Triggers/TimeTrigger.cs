using AdvancedReminders.Core.Enums;
using AdvancedReminders.Core.Models;
using RimWorld;
using System.Linq;
using Verse;

namespace AdvancedReminders.Domain.Triggers
{
    public class TimeTrigger : Trigger
    {
        public override TriggerType Type => TriggerType.Time;
        
        public int TargetTick { get; set; }
        public int DaysFromNow { get; set; }
        public bool IsRelativeTime { get; set; }
        
        public override string Description
        {
            get
            {
                if (IsRelativeTime)
                {
                    // If it's less than 24 hours, show as hours
                    if (DaysFromNow < 24)
                    {
                        return $"In {DaysFromNow} hour{(DaysFromNow == 1 ? "" : "s")}";
                    }
                    else
                    {
                        return $"In {DaysFromNow} day{(DaysFromNow == 1 ? "" : "s")}";
                    }
                }
                else
                {
                    var firstMap = Find.Maps.FirstOrDefault();
                    var tile = firstMap?.Tile ?? 0;
                    var targetDate = GenDate.DateReadoutStringAt(TargetTick, Find.WorldGrid.LongLatOf(tile));
                    return $"On {targetDate}";
                }
            }
        }
        
        public TimeTrigger()
        {
            IsRelativeTime = true;
            DaysFromNow = 1;
        }
        
        public TimeTrigger(int daysFromNow) : this()
        {
            SetDaysFromNow(daysFromNow);
        }
        
        public TimeTrigger(int targetTick, bool isAbsolute) : this()
        {
            if (isAbsolute)
            {
                TargetTick = targetTick;
                IsRelativeTime = false;
            }
            else
            {
                SetDaysFromNow((int)GenDate.TicksToDays(targetTick - Find.TickManager.TicksGame));
            }
        }
        
        public void SetDaysFromNow(int days)
        {
            DaysFromNow = days;
            IsRelativeTime = true;
            TargetTick = Find.TickManager.TicksGame + GenDate.DaysToTicks(days);
        }
        
        public void SetHoursFromNow(int hours)
        {
            DaysFromNow = hours; // Store as hours for display purposes
            IsRelativeTime = true;
            TargetTick = Find.TickManager.TicksGame + (hours * GenDate.TicksPerHour);
        }
        
        public void SetAbsoluteTick(int tick)
        {
            TargetTick = tick;
            IsRelativeTime = false;
            DaysFromNow = (int)GenDate.TicksToDays(tick - Find.TickManager.TicksGame);
        }
        
        public override bool Evaluate()
        {
            if (IsTriggered)
                return false;
                
            var currentTick = Find.TickManager.TicksGame;
            
            // If we set it as relative time, calculate the actual target tick if needed
            if (IsRelativeTime && TargetTick == 0)
            {
                TargetTick = currentTick + GenDate.DaysToTicks(DaysFromNow);
            }
            
            bool shouldTrigger = currentTick >= TargetTick;
            
            if (shouldTrigger)
            {
                IsTriggered = true;
            }
            
            return shouldTrigger;
        }
        
        public override void Reset()
        {
            base.Reset();
            
            // If this is a relative trigger, recalculate the target tick from current time
            if (IsRelativeTime)
            {
                TargetTick = Find.TickManager.TicksGame + GenDate.DaysToTicks(DaysFromNow);
            }
        }
        
        public int TicksRemaining
        {
            get
            {
                var remaining = TargetTick - Find.TickManager.TicksGame;
                return remaining > 0 ? remaining : 0;
            }
        }
        
        public int DaysRemaining => (int)GenDate.TicksToDays(TicksRemaining);
        
        public string TimeRemainingDescription
        {
            get
            {
                var remaining = TicksRemaining;
                if (remaining <= 0)
                    return "Now";
                    
                var days = (int)GenDate.TicksToDays(remaining);
                var hours = (remaining % GenDate.TicksPerDay) / GenDate.TicksPerHour;
                
                if (days > 0)
                    return $"{days}d {hours}h";
                else
                    return $"{hours}h";
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            
            int targetTick = TargetTick;
            int daysFromNow = DaysFromNow;
            bool isRelativeTime = IsRelativeTime;
            
            Scribe_Values.Look(ref targetTick, "targetTick", 0);
            Scribe_Values.Look(ref daysFromNow, "daysFromNow", 1);
            Scribe_Values.Look(ref isRelativeTime, "isRelativeTime", true);
            
            TargetTick = targetTick;
            DaysFromNow = daysFromNow;
            IsRelativeTime = isRelativeTime;
        }
    }
}