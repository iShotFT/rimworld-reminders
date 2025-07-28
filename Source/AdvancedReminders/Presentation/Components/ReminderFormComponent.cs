using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Domain.Triggers;
using AdvancedReminders.Domain.Actions;
using AdvancedReminders.Infrastructure.Localization;

namespace AdvancedReminders.Presentation.Components
{
    public class ReminderFormComponent
    {
        // Form data
        public string ReminderTitle { get; set; } = "";
        public string ReminderDescription { get; set; } = "";
        public int TimeValue { get; set; } = 1;
        public TimeUnitType TimeUnit { get; set; } = TimeUnitType.Hours;
        public SeverityLevel Severity { get; set; } = SeverityLevel.Medium;
        public bool PauseGame { get; set; } = false;
        public ReminderType ReminderType { get; set; } = ReminderType.Time;
        public Quest SelectedQuest { get; set; } = null;
        public int QuestHoursBeforeExpiry { get; set; } = 24;
        public TimeUnitType QuestTimeUnit { get; set; } = TimeUnitType.Days;
        
        // Scroll positions
        private Vector2 inputScrollPosition = Vector2.zero;
        public Vector2 InputScrollPosition 
        { 
            get => inputScrollPosition; 
            set => inputScrollPosition = value; 
        }
        
        // State
        public bool IsEditMode { get; set; } = false;
        
        // Events
        public System.Action OnFormChanged { get; set; }
        
        public enum TimeUnitType
        {
            Hours,
            Days
        }
        
        public void InitializeFromReminder(Reminder reminder)
        {
            ReminderTitle = reminder.Title;
            ReminderDescription = reminder.Description;
            Severity = reminder.Severity;
            
            // Detect reminder type based on trigger
            if (reminder.Trigger is QuestDeadlineTrigger questTrigger)
            {
                ReminderType = ReminderType.Quest;
                SelectedQuest = questTrigger.GetAssociatedQuest();
                QuestHoursBeforeExpiry = questTrigger.HoursBeforeExpiry;
                
                // Set appropriate quest time unit based on the hours value
                if (QuestHoursBeforeExpiry >= 24 && QuestHoursBeforeExpiry % 24 == 0)
                {
                    QuestTimeUnit = TimeUnitType.Days;
                }
                else
                {
                    QuestTimeUnit = TimeUnitType.Hours;
                }
            }
            else if (reminder.Trigger is TimeTrigger timeTrigger)
            {
                ReminderType = ReminderType.Time;
                var remainingTicks = timeTrigger.TargetTick - Find.TickManager.TicksGame;
                if (remainingTicks > 0)
                {
                    var remainingHours = remainingTicks / GenDate.TicksPerHour;
                    if (remainingHours >= 24)
                    {
                        TimeValue = remainingHours / 24;
                        TimeUnit = TimeUnitType.Days;
                    }
                    else
                    {
                        TimeValue = Mathf.Max(1, remainingHours);
                        TimeUnit = TimeUnitType.Hours;
                    }
                }
                else
                {
                    // Already triggered or past due - set a default
                    TimeValue = 1;
                    TimeUnit = TimeUnitType.Hours;
                }
            }
            
            // Extract pause setting from actions
            if (reminder.Actions != null)
            {
                var notificationAction = reminder.Actions.OfType<NotificationAction>().FirstOrDefault();
                if (notificationAction != null)
                {
                    PauseGame = notificationAction.PauseGame;
                }
            }
        }
        
        public bool ValidateForm(out string errorMessage)
        {
            errorMessage = null;
            
            if (string.IsNullOrWhiteSpace(ReminderTitle))
            {
                errorMessage = LocalizationKeys.ErrorTitleRequired.Translate();
                return false;
            }
            
            if (ReminderType == ReminderType.Time)
            {
                if (TimeValue < 1)
                {
                    errorMessage = LocalizationKeys.ErrorInvalidDays.Translate();
                    return false;
                }
            }
            else if (ReminderType == ReminderType.Quest)
            {
                if (SelectedQuest == null)
                {
                    errorMessage = LocalizationKeys.PleaseSelectQuest.Translate();
                    return false;
                }
                
                if (QuestHoursBeforeExpiry < 1)
                {
                    errorMessage = LocalizationKeys.HoursBeforeExpiryRequired.Translate();
                    return false;
                }
                
                // Validate that reminder time doesn't exceed quest expiration
                var questHoursRemaining = SelectedQuest.TicksUntilExpiry / (float)GenDate.TicksPerHour;
                if (QuestHoursBeforeExpiry >= questHoursRemaining)
                {
                    errorMessage = $"Reminder time ({QuestHoursBeforeExpiry}h) exceeds quest expiration ({questHoursRemaining:F1}h). Please choose a shorter time.";
                    return false;
                }
            }
            
            return true;
        }
        
        public void DrawForm(Rect rect)
        {
            // Calculate content height dynamically
            var contentHeight = CalculateContentHeight(rect.width - 16f);
            var viewRect = new Rect(0f, 0f, rect.width - 16f, contentHeight);
            
            // Use scroll view if content exceeds available height
            if (contentHeight > rect.height)
            {
                Widgets.BeginScrollView(rect, ref inputScrollPosition, viewRect);
                DrawFormContent(viewRect);
                Widgets.EndScrollView();
            }
            else
            {
                DrawFormContent(rect);
            }
        }
        
        private float CalculateContentHeight(float width)
        {
            float height = 0f;
            
            // Section title
            height += Text.LineHeight + 12f;
            
            // Reminder Type Selection
            height += Text.LineHeight + 3f + 25f + 35f;
            
            // Quest Selection (if quest type)
            if (ReminderType == ReminderType.Quest)
            {
                height += Text.LineHeight + 3f + 25f + 35f; // Quest dropdown
                
                if (SelectedQuest != null)
                {
                    height += Text.LineHeight + 8f; // Quest expiration display
                    height += Text.LineHeight + 3f + 25f + 35f; // Quest timing
                }
            }
            
            // Time Configuration (if time type)
            if (ReminderType == ReminderType.Time)
            {
                height += 10f + Text.LineHeight + 8f + 35f; // Time section
            }
            
            // Title input
            height += Text.LineHeight + 3f + 25f + 30f;
            
            // Description input  
            height += Text.LineHeight + 3f + 50f + 55f;
            
            // Severity section
            height += 10f + Text.LineHeight + 8f + 25f + 35f;
            
            // Options section
            height += 10f + Text.LineHeight + 8f + Text.LineHeight + 10f;
            
            return height;
        }
        
        private void DrawFormContent(Rect rect)
        {
            float currentY = rect.y;
            
            // Section title
            Text.Font = GameFont.Small;
            var sectionTitleRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(sectionTitleRect, LocalizationKeys.ReminderDetails.Translate());
            GUI.color = Color.white;
            currentY += Text.LineHeight + 12f;
            
            // Reminder Type Selection (only show in create mode)
            if (!IsEditMode)
            {
                var typeLabelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
                Widgets.Label(typeLabelRect, LocalizationKeys.ReminderType.Translate());
                currentY += Text.LineHeight + 3f;
                
                var typeButtonRect = new Rect(rect.x, currentY, rect.width, 25f);
                var typeButtonText = ReminderType == ReminderType.Time ? LocalizationKeys.ReminderTypeTime.Translate() : LocalizationKeys.ReminderTypeQuest.Translate();
                if (Widgets.ButtonText(typeButtonRect, typeButtonText, drawBackground: true))
                {
                    var typeOptions = new List<FloatMenuOption>
                    {
                        new FloatMenuOption(LocalizationKeys.ReminderTypeTime.Translate(), () => {
                            ReminderType = ReminderType.Time;
                            SelectedQuest = null;
                            UpdateTitleAndDescription();
                            OnFormChanged?.Invoke();
                        }),
                        new FloatMenuOption(LocalizationKeys.ReminderTypeQuest.Translate(), () => {
                            ReminderType = ReminderType.Quest;
                            UpdateTitleAndDescription();
                            OnFormChanged?.Invoke();
                        })
                    };
                    Find.WindowStack.Add(new FloatMenu(typeOptions));
                }
                currentY += 35f;
            }
            
            // Quest Selection (only for quest reminders)
            if (ReminderType == ReminderType.Quest)
            {
                var questLabelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
                Widgets.Label(questLabelRect, LocalizationKeys.SelectQuest.Translate());
                currentY += Text.LineHeight + 3f;
                
                var questButtonRect = new Rect(rect.x, currentY, rect.width, 25f);
                var questButtonText = SelectedQuest != null ? SelectedQuest.name : LocalizationKeys.SelectQuestPrompt.Translate().ToString();
                if (Widgets.ButtonText(questButtonRect, questButtonText, drawBackground: true))
                {
                    ShowQuestSelectionMenu();
                }
                currentY += 35f;
                
                if (SelectedQuest != null)
                {
                    currentY = DrawQuestTimingControls(rect, currentY);
                }
            }
            
            // Reminder Title
            var titleLabelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            Widgets.Label(titleLabelRect, LocalizationKeys.ReminderTitle.Translate() + ":");
            currentY += Text.LineHeight + 3f;
            
            var titleInputRect = new Rect(rect.x, currentY, rect.width, 25f);
            var newTitle = Widgets.TextField(titleInputRect, ReminderTitle);
            if (newTitle != ReminderTitle)
            {
                ReminderTitle = newTitle;
                OnFormChanged?.Invoke();
            }
            currentY += 30f;
            
            // Reminder Description
            var descLabelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            Widgets.Label(descLabelRect, LocalizationKeys.ReminderDescription.Translate() + ":");
            currentY += Text.LineHeight + 3f;
            
            var descInputRect = new Rect(rect.x, currentY, rect.width, 50f);
            var newDesc = Widgets.TextArea(descInputRect, ReminderDescription);
            if (newDesc != ReminderDescription)
            {
                ReminderDescription = newDesc;
                OnFormChanged?.Invoke();
            }
            currentY += 55f;
            
            // Time Configuration Section (only for time-based reminders)
            if (ReminderType == ReminderType.Time)
            {
                currentY = DrawTimeControls(rect, currentY);
            }
            
            // Severity Section
            currentY = DrawSeverityControls(rect, currentY);
            
            // Options Section
            DrawOptionsControls(rect, currentY);
        }
        
        private float DrawQuestTimingControls(Rect rect, float currentY)
        {
            // Display quest expiration deadline for reference
            if (SelectedQuest != null)
            {
                var questExpirationRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
                GUI.color = ColoredText.TipSectionTitleColor;
                Text.Font = GameFont.Tiny;
                var hoursUntilExpiry = SelectedQuest.TicksUntilExpiry / (float)GenDate.TicksPerHour;
                var expirationText = $"Quest expires in: {hoursUntilExpiry:F1} hours";
                Widgets.Label(questExpirationRect, expirationText);
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                currentY += Text.LineHeight + 8f;
            }
            
            // Timing for quest reminders
            var questTimeLabelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            Widgets.Label(questTimeLabelRect, LocalizationKeys.RemindMe.Translate());
            currentY += Text.LineHeight + 3f;
            
            // Use same format as time-based reminders for consistency
            var questTimeValueWidth = 80f;
            var questTimeValueRect = new Rect(rect.x, currentY, questTimeValueWidth, 25f);
            
            // Calculate display value based on current unit
            int questTimeValue;
            if (QuestTimeUnit == TimeUnitType.Days)
            {
                questTimeValue = Mathf.Max(1, Mathf.RoundToInt(QuestHoursBeforeExpiry / 24f));
            }
            else
            {
                questTimeValue = QuestHoursBeforeExpiry;
            }
            
            var questTimeValueStr = questTimeValue.ToString();
            questTimeValueStr = Widgets.TextField(questTimeValueRect, questTimeValueStr);
            if (int.TryParse(questTimeValueStr, out int newQuestTimeValue) && newQuestTimeValue > 0)
            {
                // Update QuestHoursBeforeExpiry based on current unit
                if (QuestTimeUnit == TimeUnitType.Days)
                {
                    QuestHoursBeforeExpiry = newQuestTimeValue * 24;
                }
                else
                {
                    QuestHoursBeforeExpiry = newQuestTimeValue;
                }
                OnFormChanged?.Invoke();
            }
            
            var questTimeUnitRect = new Rect(rect.x + questTimeValueWidth + 10f, currentY, 80f, 25f);
            if (Widgets.ButtonText(questTimeUnitRect, QuestTimeUnit.ToString()))
            {
                var questTimeUnitOptions = new List<FloatMenuOption>
                {
                    new FloatMenuOption("Hours", () => {
                        QuestTimeUnit = TimeUnitType.Hours;
                        // Keep the same total hours, just change how we display it
                        OnFormChanged?.Invoke();
                    }),
                    new FloatMenuOption("Days", () => {
                        QuestTimeUnit = TimeUnitType.Days;
                        // Convert to nearest day if switching from hours
                        if (QuestHoursBeforeExpiry % 24 != 0)
                        {
                            QuestHoursBeforeExpiry = Mathf.Max(24, Mathf.RoundToInt(QuestHoursBeforeExpiry / 24f) * 24);
                        }
                        OnFormChanged?.Invoke();
                    })
                };
                Find.WindowStack.Add(new FloatMenu(questTimeUnitOptions));
            }
            
            var questTimeDescRect = new Rect(rect.x + questTimeValueWidth + 100f, currentY + 5f, rect.width - questTimeValueWidth - 100f, Text.LineHeight);
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(questTimeDescRect, LocalizationKeys.BeforeQuestExpires.Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            currentY += 35f;
            
            return currentY;
        }
        
        private float DrawTimeControls(Rect rect, float currentY)
        {
            currentY += 10f;
            var timeSectionRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(timeSectionRect, LocalizationKeys.Timing.Translate());
            GUI.color = Color.white;
            currentY += Text.LineHeight + 8f;
            
            // Time value and unit on same line
            var timeValueWidth = 80f;
            var timeValueRect = new Rect(rect.x, currentY, timeValueWidth, 25f);
            var timeValueStr = TimeValue.ToString();
            timeValueStr = Widgets.TextField(timeValueRect, timeValueStr);
            if (int.TryParse(timeValueStr, out int newTimeValue) && newTimeValue > 0)
            {
                TimeValue = newTimeValue;
                OnFormChanged?.Invoke();
            }
            
            var timeUnitRect = new Rect(rect.x + timeValueWidth + 10f, currentY, 80f, 25f);
            if (Widgets.ButtonText(timeUnitRect, TimeUnit.ToString()))
            {
                var timeUnitOptions = new List<FloatMenuOption>
                {
                    new FloatMenuOption("Hours", () => {
                        TimeUnit = TimeUnitType.Hours;
                        OnFormChanged?.Invoke();
                    }),
                    new FloatMenuOption("Days", () => {
                        TimeUnit = TimeUnitType.Days;
                        OnFormChanged?.Invoke();
                    })
                };
                Find.WindowStack.Add(new FloatMenu(timeUnitOptions));
            }
            
            var timeDescRect = new Rect(rect.x + timeValueWidth + 100f, currentY + 5f, rect.width - timeValueWidth - 100f, Text.LineHeight);
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(timeDescRect, LocalizationKeys.FromNow.Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            currentY += 35f;
            
            return currentY;
        }
        
        private float DrawSeverityControls(Rect rect, float currentY)
        {
            currentY += 10f;
            var severitySectionRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(severitySectionRect, LocalizationKeys.Priority.Translate());
            GUI.color = Color.white;
            currentY += Text.LineHeight + 8f;
            
            var severityRect = new Rect(rect.x, currentY, rect.width, 25f);
            var severityColor = GetSeverityColor(Severity);
            GUI.color = severityColor;
            if (Widgets.ButtonText(severityRect, LocalizationHelper.GetSeverityName(Severity), drawBackground: true))
            {
                var severityOptions = new List<FloatMenuOption>();
                foreach (SeverityLevel sev in System.Enum.GetValues(typeof(SeverityLevel)))
                {
                    var color = GetSeverityColor(sev);
                    severityOptions.Add(new FloatMenuOption(
                        LocalizationHelper.GetSeverityName(sev).Colorize(color), 
                        () => {
                            Severity = sev;
                            OnFormChanged?.Invoke();
                        }
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(severityOptions));
            }
            GUI.color = Color.white;
            currentY += 35f;
            
            return currentY;
        }
        
        private void DrawOptionsControls(Rect rect, float currentY)
        {
            currentY += 10f;
            var optionsSectionRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(optionsSectionRect, LocalizationKeys.Options.Translate());
            GUI.color = Color.white;
            currentY += Text.LineHeight + 8f;
            
            var pauseRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            bool newPauseGame = PauseGame;
            Widgets.CheckboxLabeled(pauseRect, LocalizationKeys.PauseGame.Translate(), ref newPauseGame);
            if (newPauseGame != PauseGame)
            {
                PauseGame = newPauseGame;
                OnFormChanged?.Invoke();
            }
        }
        
        private void ShowQuestSelectionMenu()
        {
            var availableQuests = Find.QuestManager.QuestsListForReading
                .Where(q => q.State == QuestState.NotYetAccepted && q.TicksUntilExpiry > 0)
                .ToList();
                
            if (!availableQuests.Any())
            {
                Messages.Message(LocalizationKeys.NoQuestsWithDeadlines.Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            
            var questOptions = new List<FloatMenuOption>();
            foreach (var quest in availableQuests)
            {
                var hoursUntilExpiry = quest.TicksUntilExpiry / (float)GenDate.TicksPerHour;
                var questText = LocalizationKeys.QuestExpiresIn.Translate(quest.name, hoursUntilExpiry.ToString("F1"));
                
                questOptions.Add(new FloatMenuOption(questText, () => {
                    SelectedQuest = quest;
                    UpdateTitleAndDescription();
                    OnFormChanged?.Invoke();
                }));
            }
            
            Find.WindowStack.Add(new FloatMenu(questOptions));
        }
        
        private void UpdateTitleAndDescription()
        {
            if (ReminderType == ReminderType.Quest && SelectedQuest != null)
            {
                ReminderTitle = $"Quest Deadline: {SelectedQuest.name}";
                ReminderDescription = $"Remember to accept quest '{SelectedQuest.name}' before it expires!";
            }
            else if (ReminderType == ReminderType.Time)
            {
                // Reset to empty for manual entry
                if (ReminderTitle.StartsWith("Quest Deadline:"))
                {
                    ReminderTitle = "";
                    ReminderDescription = "";
                }
            }
        }
        
        private Color GetSeverityColor(SeverityLevel severity)
        {
            return severity switch
            {
                SeverityLevel.Low => ColoredText.FactionColor_Ally, // Green
                SeverityLevel.Medium => ColoredText.FactionColor_Neutral, // Blue  
                SeverityLevel.High => ColoredText.TipSectionTitleColor, // Yellow
                SeverityLevel.Critical => ColoredText.ThreatColor, // Red-orange
                SeverityLevel.Urgent => ColoredText.WarningColor, // Red
                _ => Color.white
            };
        }
        
        public string GetPreviewTimingText()
        {
            if (ReminderType == ReminderType.Quest && SelectedQuest != null)
            {
                var questTrigger = new QuestDeadlineTrigger(SelectedQuest.id, QuestHoursBeforeExpiry);
                return questTrigger.DetailedTriggerDescription;
            }
            else
            {
                int totalHours = TimeUnit == TimeUnitType.Days ? TimeValue * 24 : TimeValue;
                var triggerTick = Find.TickManager.TicksGame + (totalHours * GenDate.TicksPerHour);
                
                var firstMap = Find.Maps.FirstOrDefault();
                var tile = firstMap?.Tile ?? 0;
                var targetDate = GenDate.DateReadoutStringAt(GenDate.TickGameToAbs(triggerTick), Find.WorldGrid.LongLatOf(tile));
                
                return $"Will trigger on: {targetDate}";
            }
        }
    }
}