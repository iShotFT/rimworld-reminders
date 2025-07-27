using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Domain.Triggers;
using AdvancedReminders.Domain.Actions;
using AdvancedReminders.Application.Services;
using AdvancedReminders.Infrastructure.Localization;

namespace AdvancedReminders.Presentation.Windows
{
    public class Dialog_CreateReminder : Window
    {
        private string reminderTitle = "";
        private string reminderDescription = "";
        private int timeValue = 1;
        private TimeUnit timeUnit = TimeUnit.Hours;
        private SeverityLevel severity = SeverityLevel.Medium;
        private bool pauseGame = false;
        
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 descScrollPosition = Vector2.zero;
        
        public override Vector2 InitialSize => new Vector2(750f, 650f);
        
        private enum TimeUnit
        {
            Hours,
            Days
        }
        
        public Dialog_CreateReminder()
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            // Calculate section heights dynamically
            var headerHeight = CalculateHeaderHeight(inRect.width);
            var inputSectionHeight = CalculateInputSectionHeight(inRect.width);
            var previewSectionHeight = CalculatePreviewSectionHeight(inRect.width);
            var buttonHeight = 45f;
            
            var spacing = 10f;
            var currentY = inRect.y;
            
            // Header Section
            var headerRect = new Rect(inRect.x, currentY, inRect.width, headerHeight);
            DrawHeaderSection(headerRect);
            currentY += headerHeight + spacing;
            
            // Main content area (side by side)
            var remainingHeight = inRect.height - headerHeight - buttonHeight - (spacing * 3);
            var leftWidth = inRect.width * 0.6f;
            var rightWidth = inRect.width * 0.38f;
            
            // Input Section (left side)
            var inputRect = new Rect(inRect.x, currentY, leftWidth, remainingHeight);
            DrawInputSection(inputRect);
            
            // Preview Section (right side)
            var previewRect = new Rect(inRect.x + leftWidth + spacing, currentY, rightWidth, remainingHeight);
            DrawPreviewSection(previewRect);
            
            currentY += remainingHeight + spacing;
            
            // Action Buttons
            var buttonRect = new Rect(inRect.x, currentY, inRect.width, buttonHeight);
            DrawActionButtons(buttonRect);
        }
        
        private void CreateReminder()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(reminderTitle))
            {
                Messages.Message(LocalizationKeys.ErrorTitleRequired.Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            
            if (timeValue < 1)
            {
                Messages.Message("Time must be at least 1", MessageTypeDefOf.RejectInput);
                return;
            }
            
            try
            {
                // Create the reminder
                var reminder = new Reminder(reminderTitle, reminderDescription, severity);
                
                // Create time trigger based on selected time unit
                var timeTrigger = new TimeTrigger();
                int hours = timeUnit == TimeUnit.Days ? timeValue * 24 : timeValue;
                timeTrigger.SetHoursFromNow(hours);
                reminder.Trigger = timeTrigger;
                
                // Create notification action
                var notificationAction = new NotificationAction(pauseGame);
                reminder.Actions.Add(notificationAction);
                
                // Add to manager
                ReminderManager.Instance?.AddReminder(reminder);
                
                // Show success message
                Messages.Message(LocalizationHelper.FormatReminderMessage(LocalizationKeys.ReminderCreated, reminderTitle), 
                    MessageTypeDefOf.PositiveEvent);
                
                Close();
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Failed to create reminder: {ex}");
                Messages.Message(LocalizationKeys.ErrorFailedToCreate.Translate(), MessageTypeDefOf.RejectInput);
            }
        }
        
        private float CalculateHeaderHeight(float width)
        {
            float height = 20f; // Padding
            Text.Font = GameFont.Medium;
            height += Text.LineHeight + 5f; // Title
            Text.Font = GameFont.Small;
            height += Text.LineHeight + 10f; // Subtitle
            return height;
        }
        
        private float CalculateInputSectionHeight(float width)
        {
            // This is dynamic based on content, but we'll calculate in the actual drawing
            return 300f; // Rough estimate
        }
        
        private float CalculatePreviewSectionHeight(float width)
        {
            return 300f; // Rough estimate
        }
        
        private void DrawHeaderSection(Rect rect)
        {
            // Header background
            Widgets.DrawBoxSolid(rect, new Color(0.15f, 0.15f, 0.2f, 0.8f));
            Widgets.DrawBox(rect);
            
            var innerRect = rect.ContractedBy(10f);
            float currentY = innerRect.y;
            
            // Main title
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, LocalizationKeys.CreateReminderTitle.Translate());
            currentY += Text.LineHeight + 5f;
            
            // Subtitle
            Text.Font = GameFont.Small;
            var subtitleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            GUI.color = Color.gray;
            Widgets.Label(subtitleRect, "Configure your reminder settings below");
            GUI.color = Color.white;
            
            Text.Anchor = TextAnchor.UpperLeft;
        }
        
        private void DrawInputSection(Rect rect)
        {
            // Section background
            Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.6f));
            Widgets.DrawBox(rect);
            
            var innerRect = rect.ContractedBy(15f);
            float currentY = innerRect.y;
            
            // Section title
            Text.Font = GameFont.Small;
            var sectionTitleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(sectionTitleRect, "Reminder Details");
            GUI.color = Color.white;
            currentY += Text.LineHeight + 12f;
            
            // Reminder Title
            var titleLabelRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            Widgets.Label(titleLabelRect, LocalizationKeys.ReminderTitle.Translate() + ":");
            currentY += Text.LineHeight + 3f;
            
            var titleInputRect = new Rect(innerRect.x, currentY, innerRect.width, 25f);
            reminderTitle = Widgets.TextField(titleInputRect, reminderTitle);
            currentY += 30f;
            
            // Reminder Description
            var descLabelRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            Widgets.Label(descLabelRect, LocalizationKeys.ReminderDescription.Translate() + ":");
            currentY += Text.LineHeight + 3f;
            
            var descInputRect = new Rect(innerRect.x, currentY, innerRect.width, 50f);
            reminderDescription = Widgets.TextArea(descInputRect, reminderDescription);
            currentY += 55f;
            
            // Time Configuration Section
            currentY += 10f;
            var timeSectionRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(timeSectionRect, "Timing");
            GUI.color = Color.white;
            currentY += Text.LineHeight + 8f;
            
            // Time value and unit on same line
            var timeValueWidth = 80f;
            var timeValueRect = new Rect(innerRect.x, currentY, timeValueWidth, 25f);
            var timeValueStr = timeValue.ToString();
            timeValueStr = Widgets.TextField(timeValueRect, timeValueStr);
            if (int.TryParse(timeValueStr, out int newTimeValue) && newTimeValue > 0)
            {
                timeValue = newTimeValue;
            }
            
            var timeUnitRect = new Rect(innerRect.x + timeValueWidth + 10f, currentY, 80f, 25f);
            if (Widgets.ButtonText(timeUnitRect, timeUnit.ToString()))
            {
                var timeUnitOptions = new List<FloatMenuOption>
                {
                    new FloatMenuOption("Hours", () => timeUnit = TimeUnit.Hours),
                    new FloatMenuOption("Days", () => timeUnit = TimeUnit.Days)
                };
                Find.WindowStack.Add(new FloatMenu(timeUnitOptions));
            }
            
            var timeDescRect = new Rect(innerRect.x + timeValueWidth + 100f, currentY + 5f, innerRect.width - timeValueWidth - 100f, Text.LineHeight);
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(timeDescRect, "from now");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            currentY += 35f;
            
            // Severity Section
            currentY += 10f;
            var severitySectionRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(severitySectionRect, "Priority");
            GUI.color = Color.white;
            currentY += Text.LineHeight + 8f;
            
            var severityRect = new Rect(innerRect.x, currentY, innerRect.width, 25f);
            var severityColor = GetSeverityColor(severity);
            GUI.color = severityColor;
            if (Widgets.ButtonText(severityRect, LocalizationHelper.GetSeverityName(severity), drawBackground: true))
            {
                var severityOptions = new List<FloatMenuOption>();
                foreach (SeverityLevel sev in System.Enum.GetValues(typeof(SeverityLevel)))
                {
                    var color = GetSeverityColor(sev);
                    severityOptions.Add(new FloatMenuOption(
                        LocalizationHelper.GetSeverityName(sev).Colorize(color), 
                        () => severity = sev
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(severityOptions));
            }
            GUI.color = Color.white;
            currentY += 35f;
            
            // Options Section
            currentY += 10f;
            var optionsSectionRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(optionsSectionRect, "Options");
            GUI.color = Color.white;
            currentY += Text.LineHeight + 8f;
            
            var pauseRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            Widgets.CheckboxLabeled(pauseRect, LocalizationKeys.PauseGame.Translate(), ref pauseGame);
        }
        
        private void DrawPreviewSection(Rect rect)
        {
            // Preview background
            Widgets.DrawBoxSolid(rect, new Color(0.05f, 0.1f, 0.05f, 0.6f));
            Widgets.DrawBox(rect);
            
            var innerRect = rect.ContractedBy(15f);
            float currentY = innerRect.y;
            
            // Section title
            Text.Font = GameFont.Small;
            var sectionTitleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            GUI.color = ColoredText.TipSectionTitleColor;
            Widgets.Label(sectionTitleRect, "Preview");
            GUI.color = Color.white;
            currentY += Text.LineHeight + 12f;
            
            // Preview the reminder as it would appear
            if (!string.IsNullOrWhiteSpace(reminderTitle))
            {
                // Title with severity color
                var titleColor = GetSeverityColor(severity);
                var previewTitleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
                GUI.color = titleColor;
                Widgets.Label(previewTitleRect, reminderTitle);
                GUI.color = Color.white;
                currentY += Text.LineHeight + 5f;
                
                // Description
                if (!string.IsNullOrWhiteSpace(reminderDescription))
                {
                    Text.Font = GameFont.Tiny;
                    var descHeight = Text.CalcHeight(reminderDescription, innerRect.width);
                    var previewDescRect = new Rect(innerRect.x, currentY, innerRect.width, descHeight);
                    GUI.color = Color.gray;
                    Widgets.Label(previewDescRect, reminderDescription);
                    GUI.color = Color.white;
                    currentY += descHeight + 8f;
                    Text.Font = GameFont.Small;
                }
                
                // Timing info
                var timingInfo = GetPreviewTimingText();
                var timingRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
                GUI.color = ColoredText.DateTimeColor;
                Text.Font = GameFont.Tiny;
                Widgets.Label(timingRect, timingInfo);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                currentY += Text.LineHeight + 5f;
                
                // Severity indicator
                var severityInfoRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
                GUI.color = titleColor;
                Text.Font = GameFont.Tiny;
                Widgets.Label(severityInfoRect, $"Priority: {LocalizationHelper.GetSeverityName(severity)}");
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                currentY += Text.LineHeight + 5f;
                
                // Options
                if (pauseGame)
                {
                    var pauseInfoRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
                    GUI.color = ColoredText.TipSectionTitleColor;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(pauseInfoRect, "⏸ Will pause game when triggered");
                    Text.Font = GameFont.Small;
                    GUI.color = Color.white;
                }
            }
            else
            {
                // Empty state
                var emptyRect = new Rect(innerRect.x, currentY + 50f, innerRect.width, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.gray;
                Widgets.Label(emptyRect, "Enter a title to see preview");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }
        
        private void DrawActionButtons(Rect rect)
        {
            var buttonWidth = 120f;
            var spacing = 15f;
            var totalButtonWidth = (buttonWidth * 2) + spacing;
            var startX = rect.x + (rect.width - totalButtonWidth) / 2f;
            
            // Create button
            var createRect = new Rect(startX, rect.y + 5f, buttonWidth, 35f);
            var canCreate = !string.IsNullOrWhiteSpace(reminderTitle) && timeValue > 0;
            
            GUI.enabled = canCreate;
            var createColor = canCreate ? ColoredText.FactionColor_Ally : Color.gray;
            GUI.color = createColor;
            if (Widgets.ButtonText(createRect, LocalizationKeys.ButtonCreate.Translate(), drawBackground: true))
            {
                CreateReminder();
            }
            GUI.color = Color.white;
            GUI.enabled = true;
            
            // Cancel button
            var cancelRect = new Rect(startX + buttonWidth + spacing, rect.y + 5f, buttonWidth, 35f);
            if (Widgets.ButtonText(cancelRect, LocalizationKeys.ButtonCancel.Translate()))
            {
                Close();
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
        
        private string GetPreviewTimingText()
        {
            int totalHours = timeUnit == TimeUnit.Days ? timeValue * 24 : timeValue;
            var triggerTick = Find.TickManager.TicksGame + (totalHours * GenDate.TicksPerHour);
            
            var firstMap = Find.Maps.FirstOrDefault();
            var tile = firstMap?.Tile ?? 0;
            var targetDate = GenDate.DateReadoutStringAt(GenDate.TickGameToAbs(triggerTick), Find.WorldGrid.LongLatOf(tile));
            
            return $"Will trigger on: {targetDate}";
        }
    }
}