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
using AdvancedReminders.Presentation.Components;

namespace AdvancedReminders.Presentation.Windows
{
    public class Dialog_EditReminder : Window
    {
        private Reminder reminder;
        private ReminderFormComponent formComponent;
        
        public override Vector2 InitialSize => new Vector2(750f, 650f);
        
        public Dialog_EditReminder(Reminder reminderToEdit)
        {
            this.reminder = reminderToEdit;
            formComponent = new ReminderFormComponent();
            formComponent.IsEditMode = true;
            formComponent.InitializeFromReminder(reminder);
            
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
        
        private void UpdateReminder()
        {
            // Validate using form component
            if (!formComponent.ValidateForm(out string errorMessage))
            {
                Messages.Message(errorMessage, MessageTypeDefOf.RejectInput);
                return;
            }
            
            try
            {
                // Update the reminder
                reminder.Title = formComponent.ReminderTitle;
                reminder.Description = formComponent.ReminderDescription;
                reminder.Severity = formComponent.Severity;
                
                // Update trigger based on reminder type
                if (formComponent.ReminderType == ReminderType.Quest)
                {
                    // Create quest deadline trigger
                    var questTrigger = new QuestDeadlineTrigger(formComponent.SelectedQuest.id, formComponent.QuestHoursBeforeExpiry);
                    reminder.Trigger = questTrigger;
                }
                else
                {
                    // Create time trigger
                    var timeTrigger = new TimeTrigger();
                    int hours = formComponent.TimeUnit == ReminderFormComponent.TimeUnitType.Days ? formComponent.TimeValue * 24 : formComponent.TimeValue;
                    timeTrigger.SetHoursFromNow(hours);
                    reminder.Trigger = timeTrigger;
                }
                
                // Update notification action
                reminder.Actions.Clear();
                var notificationAction = new NotificationAction(formComponent.PauseGame);
                reminder.Actions.Add(notificationAction);
                
                // If the reminder was completed, reactivate it
                if (!reminder.IsActive)
                {
                    reminder.IsActive = true;
                }
                
                // Show success message
                Messages.Message($"Reminder '{formComponent.ReminderTitle}' has been updated", MessageTypeDefOf.PositiveEvent);
                
                Close();
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Failed to update reminder: {ex}");
                Messages.Message("Failed to update reminder", MessageTypeDefOf.RejectInput);
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
            Widgets.Label(titleRect, "Edit Reminder");
            currentY += Text.LineHeight + 5f;
            
            // Subtitle
            Text.Font = GameFont.Small;
            var subtitleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            GUI.color = Color.gray;
            Widgets.Label(subtitleRect, "Modify your reminder settings below");
            GUI.color = Color.white;
            
            Text.Anchor = TextAnchor.UpperLeft;
        }
        
        private void DrawInputSection(Rect rect)
        {
            // Section background
            Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.6f));
            Widgets.DrawBox(rect);
            
            var innerRect = rect.ContractedBy(15f);
            formComponent.DrawForm(innerRect);
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
            
            // Show original reminder info
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            var originalInfoRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            Widgets.Label(originalInfoRect, $"Original: {reminder.Title}");
            GUI.color = Color.white;
            currentY += Text.LineHeight + 8f;
            Text.Font = GameFont.Small;
            
            // Preview the reminder as it would appear
            if (!string.IsNullOrWhiteSpace(formComponent.ReminderTitle))
            {
                // Title with severity color
                var titleColor = GetSeverityColor(formComponent.Severity);
                var previewTitleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
                GUI.color = titleColor;
                Widgets.Label(previewTitleRect, formComponent.ReminderTitle);
                GUI.color = Color.white;
                currentY += Text.LineHeight + 5f;
                
                // Description
                if (!string.IsNullOrWhiteSpace(formComponent.ReminderDescription))
                {
                    Text.Font = GameFont.Tiny;
                    var descHeight = Text.CalcHeight(formComponent.ReminderDescription, innerRect.width);
                    var previewDescRect = new Rect(innerRect.x, currentY, innerRect.width, descHeight);
                    GUI.color = Color.gray;
                    Widgets.Label(previewDescRect, formComponent.ReminderDescription);
                    GUI.color = Color.white;
                    currentY += descHeight + 8f;
                    Text.Font = GameFont.Small;
                }
                
                // Timing info
                var timingInfo = formComponent.GetPreviewTimingText();
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
                Widgets.Label(severityInfoRect, $"Priority: {LocalizationHelper.GetSeverityName(formComponent.Severity)}");
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                currentY += Text.LineHeight + 5f;
                
                // Options
                if (formComponent.PauseGame)
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
            
            // Update button
            var updateRect = new Rect(startX, rect.y + 5f, buttonWidth, 35f);
            var canUpdate = formComponent.ValidateForm(out _);
            
            GUI.enabled = canUpdate;
            var updateColor = canUpdate ? ColoredText.FactionColor_Ally : Color.gray;
            GUI.color = updateColor;
            if (Widgets.ButtonText(updateRect, "Update", drawBackground: true))
            {
                UpdateReminder();
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
    }
}