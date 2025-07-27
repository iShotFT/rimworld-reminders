using AdvancedReminders.Core.Enums;
using Verse;

namespace AdvancedReminders.Infrastructure.Localization
{
    public static class LocalizationHelper
    {
        /// <summary>
        /// Translates a key using RimWorld's translation system
        /// </summary>
        public static string Translate(string key, params object[] args)
        {
            return key.Translate(args);
        }
        
        /// <summary>
        /// Translates a key with fallback to the key itself if translation is missing
        /// </summary>
        public static string TranslateSafe(string key, params object[] args)
        {
            try
            {
                var translated = key.Translate(args);
                return translated.ToString();
            }
            catch
            {
                return args.Length > 0 ? string.Format(key, args) : key;
            }
        }
        
        /// <summary>
        /// Gets the localized name for a severity level
        /// </summary>
        public static string GetSeverityName(SeverityLevel severity)
        {
            return severity switch
            {
                SeverityLevel.Low => LocalizationKeys.SeverityLow.Translate(),
                SeverityLevel.Medium => LocalizationKeys.SeverityMedium.Translate(),
                SeverityLevel.High => LocalizationKeys.SeverityHigh.Translate(),
                SeverityLevel.Critical => LocalizationKeys.SeverityCritical.Translate(),
                SeverityLevel.Urgent => LocalizationKeys.SeverityUrgent.Translate(),
                _ => severity.ToString()
            };
        }
        
        /// <summary>
        /// Gets the localized name for a trigger type
        /// </summary>
        public static string GetTriggerTypeName(TriggerType triggerType)
        {
            return triggerType switch
            {
                TriggerType.Time => LocalizationKeys.TriggerTypeTime.Translate(),
                TriggerType.Event => LocalizationKeys.TriggerTypeEvent.Translate(),
                TriggerType.Resource => LocalizationKeys.TriggerTypeResource.Translate(),
                TriggerType.Pawn => LocalizationKeys.TriggerTypePawn.Translate(),
                TriggerType.Quest => LocalizationKeys.TriggerTypeQuest.Translate(),
                TriggerType.Calendar => LocalizationKeys.TriggerTypeCalendar.Translate(),
                TriggerType.Condition => LocalizationKeys.TriggerTypeCondition.Translate(),
                _ => triggerType.ToString()
            };
        }
        
        /// <summary>
        /// Gets the localized name for an action type
        /// </summary>
        public static string GetActionTypeName(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.Notification => LocalizationKeys.ActionTypeNotification.Translate(),
                ActionType.Pause => LocalizationKeys.ActionTypePause.Translate(),
                ActionType.Camera => "Camera Focus",
                ActionType.Chain => "Chain Action", 
                ActionType.Sound => "Sound Alert",
                ActionType.Quest => "Quest Action",
                _ => actionType.ToString()
            };
        }
        
        /// <summary>
        /// Gets the localized name for a quest phase
        /// </summary>
        public static string GetQuestPhaseName(QuestPhase phase)
        {
            return phase switch
            {
                QuestPhase.Preparation => LocalizationKeys.QuestPhasePreparation.Translate(),
                QuestPhase.Active => LocalizationKeys.QuestPhaseActive.Translate(),
                QuestPhase.Completion => LocalizationKeys.QuestPhaseCompletion.Translate(),
                QuestPhase.Expired => LocalizationKeys.QuestPhaseExpired.Translate(),
                _ => phase.ToString()
            };
        }
        
        /// <summary>
        /// Gets the localized name for a quadrum
        /// </summary>
        public static string GetQuadrumName(int quadrum)
        {
            return quadrum switch
            {
                0 => LocalizationKeys.QuadrumAprimay.Translate(),
                1 => LocalizationKeys.QuadrumJugust.Translate(),
                2 => LocalizationKeys.QuadrumSeptober.Translate(),
                3 => LocalizationKeys.QuadrumDecembary.Translate(),
                _ => quadrum.ToString()
            };
        }
        
        /// <summary>
        /// Formats a time string with proper pluralization
        /// </summary>
        public static string FormatTimeString(int days, int hours)
        {
            if (days <= 0 && hours <= 0)
                return LocalizationKeys.TimeRemainingNow.Translate();
            
            if (days > 0 && hours > 0)
            {
                var dayStr = days == 1 ? LocalizationKeys.Day.Translate() : LocalizationKeys.Days.Translate();
                var hourStr = hours == 1 ? LocalizationKeys.Hour.Translate() : LocalizationKeys.Hours.Translate();
                return $"{days} {dayStr}, {hours} {hourStr}";
            }
            else if (days > 0)
            {
                var dayStr = days == 1 ? LocalizationKeys.Day.Translate() : LocalizationKeys.Days.Translate();
                return $"{days} {dayStr}";
            }
            else
            {
                var hourStr = hours == 1 ? LocalizationKeys.Hour.Translate() : LocalizationKeys.Hours.Translate();
                return $"{hours} {hourStr}";
            }
        }
        
        /// <summary>
        /// Creates a formatted status message for reminder operations
        /// </summary>
        public static string FormatReminderMessage(string messageKey, string reminderTitle)
        {
            return messageKey.Translate(reminderTitle);
        }
    }
}