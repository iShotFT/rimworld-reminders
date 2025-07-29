using System;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Domain.Triggers;

namespace AdvancedReminders.Presentation.UI.ViewModels
{
    /// <summary>
    /// View model for reminder display logic, separating data from UI concerns.
    /// Provides computed properties for UI-specific display without modifying the core model.
    /// </summary>
    public class ReminderViewModel
    {
        private readonly Reminder _model;

        public ReminderViewModel(Reminder model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        #region Core Model Access

        /// <summary>
        /// The underlying reminder model
        /// </summary>
        public Reminder Model => _model;

        /// <summary>
        /// Reminder ID
        /// </summary>
        public int Id => _model.Id;

        /// <summary>
        /// Reminder title
        /// </summary>
        public string Title => _model.Title;

        /// <summary>
        /// Reminder description
        /// </summary>
        public string Description => _model.Description;

        /// <summary>
        /// Reminder severity level
        /// </summary>
        public SeverityLevel Severity => _model.Severity;

        /// <summary>
        /// Whether the reminder is active
        /// </summary>
        public bool IsActive => _model.IsActive;

        #endregion

        #region UI-Specific Computed Properties

        /// <summary>
        /// Display title with urgency indicators
        /// </summary>
        public string DisplayTitle
        {
            get
            {
                var title = _model.Title;
                if (IsOverdue)
                    return $"⚠ {title}";
                if (IsVeryUrgent)
                    return $"🔥 {title}";
                if (IsUrgent)
                    return $"⏰ {title}";
                return title;
            }
        }

        /// <summary>
        /// Truncated description for list display
        /// </summary>
        public string DisplayDescription
        {
            get
            {
                var desc = _model.Description ?? "";
                if (desc.Length <= 80) return desc;
                return desc.Substring(0, 77) + "...";
            }
        }

        /// <summary>
        /// Status text for display
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!_model.IsActive)
                    return "Completed";

                if (_model.Trigger is TimeTrigger timeTrigger)
                {
                    var ticksRemaining = timeTrigger.TargetTick - Find.TickManager.TicksGame;
                    if (ticksRemaining <= 0)
                        return "Ready to trigger!";
                    
                    return $"Active - {timeTrigger.TimeRemainingDescription} remaining";
                }

                return "Active";
            }
        }

        /// <summary>
        /// Color for status display based on urgency
        /// </summary>
        public Color StatusColor
        {
            get
            {
                if (!_model.IsActive) return Color.gray;
                if (IsOverdue) return Color.red;
                if (IsVeryUrgent) return new Color(1f, 0.4f, 0.4f);
                if (IsUrgent) return new Color(1f, 0.8f, 0.4f);
                return Color.white;
            }
        }

        /// <summary>
        /// Color for severity display
        /// </summary>
        public Color SeverityColor
        {
            get
            {
                return _model.Severity switch
                {
                    SeverityLevel.Low => new Color(0.4f, 0.8f, 0.4f),
                    SeverityLevel.Medium => new Color(0.8f, 0.8f, 0.4f),
                    SeverityLevel.High => new Color(0.8f, 0.6f, 0.4f),
                    SeverityLevel.Critical => new Color(0.8f, 0.4f, 0.4f),
                    _ => Color.white
                };
            }
        }

        /// <summary>
        /// Background color for the reminder item
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                var baseColor = new Color(0.1f, 0.1f, 0.1f);
                
                // Add subtle urgency tinting
                if (IsVeryUrgent)
                    baseColor.r += 0.05f;
                
                return baseColor;
            }
        }

        /// <summary>
        /// Opacity for the entire reminder display
        /// </summary>
        public float DisplayOpacity => _model.IsActive ? 1f : 0.5f;

        #endregion

        #region Urgency Logic

        /// <summary>
        /// Whether this reminder is overdue (past trigger time)
        /// </summary>
        public bool IsOverdue
        {
            get
            {
                if (!_model.IsActive) return false;
                
                if (_model.Trigger is TimeTrigger timeTrigger)
                {
                    return timeTrigger.TargetTick <= Find.TickManager.TicksGame;
                }
                
                return false;
            }
        }

        /// <summary>
        /// Whether this reminder is very urgent (less than 1 day)
        /// </summary>
        public bool IsVeryUrgent
        {
            get
            {
                if (!_model.IsActive || IsOverdue) return false;
                
                if (_model.Trigger is TimeTrigger timeTrigger)
                {
                    var ticksRemaining = timeTrigger.TargetTick - Find.TickManager.TicksGame;
                    return ticksRemaining <= 60000; // Less than 1 day
                }
                
                return _model.Severity == SeverityLevel.Critical;
            }
        }

        /// <summary>
        /// Whether this reminder is urgent (less than 3 days)
        /// </summary>
        public bool IsUrgent
        {
            get
            {
                if (!_model.IsActive || IsOverdue || IsVeryUrgent) return false;
                
                if (_model.Trigger is TimeTrigger timeTrigger)
                {
                    var ticksRemaining = timeTrigger.TargetTick - Find.TickManager.TicksGame;
                    return ticksRemaining <= 180000; // Less than 3 days
                }
                
                return _model.Severity >= SeverityLevel.High;
            }
        }

        #endregion

        #region Trigger Information

        /// <summary>
        /// Detailed trigger information for display
        /// </summary>
        public string TriggerDisplayText
        {
            get
            {
                if (_model.Trigger == null)
                    return "No trigger set";
                    
                if (_model.Trigger is TimeTrigger timeTrigger)
                {
                    var firstMap = Find.Maps.Count > 0 ? Find.Maps[0] : null;
                    var tile = firstMap?.Tile ?? 0;
                    var targetDate = GenDate.DateReadoutStringAt(timeTrigger.TargetTick, Find.WorldGrid.LongLatOf(tile));
                    var timeRemaining = timeTrigger.TimeRemainingDescription;
                    return $"Triggers on {targetDate} ({timeRemaining})";
                }
                
                if (_model.Trigger is QuestDeadlineTrigger questTrigger)
                {
                    var quest = questTrigger.GetAssociatedQuest();
                    var questName = quest?.name ?? "Unknown Quest";
                    return $"Quest deadline: {questName}";
                }
                
                return _model.Trigger.Description;
            }
        }

        /// <summary>
        /// Color for trigger display based on urgency
        /// </summary>
        public Color TriggerColor
        {
            get
            {
                if (!_model.IsActive) return Color.gray;
                if (IsOverdue) return Color.red;
                if (IsVeryUrgent) return new Color(1f, 0.4f, 0.4f);
                if (IsUrgent) return Color.yellow;
                return Color.white;
            }
        }

        #endregion

        #region Time Formatting

        /// <summary>
        /// Gets a compact time remaining string
        /// </summary>
        public string CompactTimeRemaining
        {
            get
            {
                if (_model.Trigger is TimeTrigger timeTrigger)
                {
                    var ticksRemaining = timeTrigger.TargetTick - Find.TickManager.TicksGame;
                    
                    if (ticksRemaining <= 0)
                        return "Now";
                    
                    var days = ticksRemaining / 60000;
                    var hours = (ticksRemaining % 60000) / 2500;
                    
                    if (days > 0) return $"{days}d";
                    if (hours > 0) return $"{hours}h";
                    return "<1h";
                }
                
                return "N/A";
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Whether this reminder can be edited
        /// </summary>
        public bool CanEdit => _model.IsActive;

        /// <summary>
        /// Whether this reminder can be deleted
        /// </summary>
        public bool CanDelete => true;

        /// <summary>
        /// Whether this reminder has an associated quest
        /// </summary>
        public bool HasQuest => _model.Trigger is QuestDeadlineTrigger;

        #endregion
    }

    /// <summary>
    /// Extension methods for easy ViewModel creation
    /// </summary>
    public static class ReminderViewModelExtensions
    {
        /// <summary>
        /// Converts a Reminder to a ReminderViewModel
        /// </summary>
        public static ReminderViewModel ToViewModel(this Reminder reminder)
        {
            return new ReminderViewModel(reminder);
        }
    }
}