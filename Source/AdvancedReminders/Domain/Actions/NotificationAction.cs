using AdvancedReminders.Core.Enums;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Domain.Triggers;
using RimWorld;
using Verse;

namespace AdvancedReminders.Domain.Actions
{
    public class NotificationAction : ReminderAction
    {
        public override ActionType Type => ActionType.Notification;
        
        public LetterDef LetterType { get; set; }
        public bool PauseGame { get; set; }
        public string CustomTitle { get; set; }
        public string CustomText { get; set; }
        
        public override string Description => $"Show notification{(PauseGame ? " and pause" : "")}";
        
        public NotificationAction()
        {
            LetterType = LetterDefOf.NeutralEvent;
            PauseGame = false;
        }
        
        public NotificationAction(bool pauseGame, LetterDef letterType = null) : this()
        {
            PauseGame = pauseGame;
            if (letterType != null)
                LetterType = letterType;
        }
        
        public override void Execute(Reminder reminder)
        {
            if (reminder == null)
            {
                Log.Warning("[AdvancedReminders] NotificationAction: Cannot execute with null reminder");
                return;
            }
            
            try
            {
                var title = !string.IsNullOrEmpty(CustomTitle) ? CustomTitle : reminder.Title;
                var text = !string.IsNullOrEmpty(CustomText) ? CustomText : reminder.Description;
                
                // Determine letter type based on severity if not explicitly set
                var letterDef = DetermineLetterType(reminder.Severity);
                
                // Check if this is a quest reminder to add quest link
                Quest relatedQuest = null;
                if (reminder.Trigger is QuestDeadlineTrigger questTrigger)
                {
                    relatedQuest = questTrigger.GetAssociatedQuest();
                }
                
                // Create and send the letter with quest reference if applicable
                ChoiceLetter letter;
                if (relatedQuest != null)
                {
                    // Create letter with quest link - this automatically adds the "View Quest" button
                    letter = LetterMaker.MakeLetter(title, text, letterDef, relatedFaction: null, quest: relatedQuest);
                }
                else
                {
                    // Create regular letter without quest link
                    letter = LetterMaker.MakeLetter(title, text, letterDef);
                }
                
                Find.LetterStack.ReceiveLetter(letter);
                
                // Pause game if requested or if severity is critical/urgent
                if (PauseGame || reminder.Severity >= SeverityLevel.Critical)
                {
                    Find.TickManager.Pause();
                }
                
                Log.Message($"[AdvancedReminders] Notification sent: {title}" + 
                           (relatedQuest != null ? $" (with quest link: {relatedQuest.name})" : ""));
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Error executing notification action: {ex}");
            }
        }
        
        private LetterDef DetermineLetterType(SeverityLevel severity)
        {
            if (LetterType != null && LetterType != LetterDefOf.NeutralEvent)
                return LetterType;
                
            return severity switch
            {
                SeverityLevel.Low => LetterDefOf.PositiveEvent,      // Green
                SeverityLevel.Medium => LetterDefOf.NeutralEvent,    // Blue  
                SeverityLevel.High => LetterDefOf.NegativeEvent,     // Yellow
                SeverityLevel.Critical => LetterDefOf.ThreatSmall,   // Orange
                SeverityLevel.Urgent => LetterDefOf.ThreatBig,       // Red
                _ => LetterDefOf.NeutralEvent
            };
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            
            bool pauseGame = PauseGame;
            string customTitle = CustomTitle;
            string customText = CustomText;
            
            Scribe_Values.Look(ref pauseGame, "pauseGame", false);
            Scribe_Values.Look(ref customTitle, "customTitle", "");
            Scribe_Values.Look(ref customText, "customText", "");
            LetterDef letterType = LetterType;
            Scribe_Defs.Look(ref letterType, "letterType");
            LetterType = letterType;
            
            PauseGame = pauseGame;
            CustomTitle = customTitle;
            CustomText = customText;
        }
    }
}