using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Application.Services;
using AdvancedReminders.Infrastructure.Localization;
using AdvancedReminders.Presentation.Windows;
using AdvancedReminders.Domain.Triggers;
using AdvancedReminders.Core.Enums;

namespace AdvancedReminders.Presentation.MainTab
{
    public class MainTabWindow_Reminders : MainTabWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private List<Reminder> displayedReminders = new List<Reminder>();
        private bool showCompleted = true;
        private SortMode currentSort = SortMode.TriggerTime;
        
        public override Vector2 RequestedTabSize => new Vector2(800f, 600f);
        
        public enum SortMode
        {
            TriggerTime,
            Created,
            Severity,
            Status,
            Title
        }

        public MainTabWindow_Reminders()
        {
            RefreshRemindersList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            RefreshRemindersList();
            
            // Calculate header height dynamically
            var headerHeight = CalculateHeaderHeight(inRect.width);
            var headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
            var contentRect = new Rect(inRect.x, inRect.y + headerHeight + 5f, inRect.width, inRect.height - headerHeight - 5f);
            
            DrawHeader(headerRect);
            DrawContent(contentRect);
        }
        
        private float CalculateHeaderHeight(float width)
        {
            float height = 10f; // Top padding
            
            // Title height
            Text.Font = GameFont.Medium;
            height += Text.LineHeight + 5f;
            
            // Stats height
            Text.Font = GameFont.Small;
            height += Text.LineHeight + 5f;
            
            // Controls height (sorting and filter controls)
            height += Text.LineHeight + 5f;
            
            // Button height (since they're the tallest element)
            height += 35f + 10f; // Button height + bottom padding
            
            Text.Font = GameFont.Small;
            return height;
        }
        
        private void DrawHeader(Rect rect)
        {
            // Background panel for header
            Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.8f));
            Widgets.DrawBox(rect);
            
            var innerRect = rect.ContractedBy(10f);
            float currentY = innerRect.y;
            
            // Title
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(innerRect.x, currentY, innerRect.width * 0.6f, Text.LineHeight);
            Widgets.Label(titleRect, LocalizationKeys.RemindersTabTitle.Translate());
            currentY += Text.LineHeight + 5f;
            
            // Statistics
            Text.Font = GameFont.Small;
            var statsRect = new Rect(innerRect.x, currentY, innerRect.width * 0.6f, Text.LineHeight);
            var activeCount = displayedReminders.Count(r => r.IsActive);
            var upcomingCount = displayedReminders.Count(r => r.IsActive && IsUpcoming(r));
            Widgets.Label(statsRect, $"Active: {activeCount} | Upcoming: {upcomingCount}");
            currentY += Text.LineHeight + 5f;
            
            // Controls row
            DrawControls(new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight));
            
            // Action buttons (positioned from the right side) - dynamic sizing
            var buttonHeight = 35f;
            var buttonY = innerRect.y + 5f;
            
            // Calculate button widths based on text content
            Text.Font = GameFont.Small;
            var createButtonText = LocalizationKeys.CreateNewReminder.Translate();
            var clearButtonText = "Clear Completed";
            var createButtonWidth = Text.CalcSize(createButtonText).x + 20f; // Add padding
            var clearButtonWidth = Text.CalcSize(clearButtonText).x + 20f; // Add padding
            
            var createButtonRect = new Rect(innerRect.xMax - createButtonWidth, buttonY, createButtonWidth, buttonHeight);
            if (Widgets.ButtonText(createButtonRect, createButtonText))
            {
                var dialog = new Dialog_CreateReminder();
                Find.WindowStack.Add(dialog);
            }
            
            var clearButtonRect = new Rect(innerRect.xMax - (createButtonWidth + clearButtonWidth + 10f), buttonY, clearButtonWidth, buttonHeight);
            if (Widgets.ButtonText(clearButtonRect, clearButtonText))
            {
                ReminderManager.Instance?.ClearCompletedReminders();
                RefreshRemindersList();
            }
            
            Text.Font = GameFont.Small;
        }
        
        private void DrawControls(Rect rect)
        {
            Text.Font = GameFont.Tiny;
            float currentX = rect.x;
            
            // Sort label
            var sortLabelRect = new Rect(currentX, rect.y, 30f, rect.height);
            Widgets.Label(sortLabelRect, "Sort:");
            currentX += 35f;
            
            // Sort buttons
            var sortOptions = new[] { 
                ("Time", SortMode.TriggerTime), 
                ("Created", SortMode.Created), 
                ("Severity", SortMode.Severity), 
                ("Status", SortMode.Status), 
                ("Title", SortMode.Title) 
            };
            
            foreach (var (label, mode) in sortOptions)
            {
                var buttonWidth = Text.CalcSize(label).x + 10f;
                var buttonRect = new Rect(currentX, rect.y, buttonWidth, rect.height);
                
                bool isSelected = currentSort == mode;
                GUI.color = isSelected ? Color.yellow : Color.white;
                
                if (Widgets.ButtonText(buttonRect, label, drawBackground: true, doMouseoverSound: true))
                {
                    currentSort = mode;
                    RefreshRemindersList();
                }
                
                GUI.color = Color.white;
                currentX += buttonWidth + 5f;
            }
            
            // Show completed toggle - calculate width based on text
            currentX += 20f; // Spacing
            Text.Font = GameFont.Small; // Use same font as the rest of the UI
            var toggleText = "Show completed";
            var toggleWidth = Text.CalcSize(toggleText).x + 25f; // Add space for checkbox
            var toggleRect = new Rect(currentX, rect.y, toggleWidth, rect.height);
            bool newShowCompleted = showCompleted;
            Widgets.CheckboxLabeled(toggleRect, toggleText, ref newShowCompleted);
            if (newShowCompleted != showCompleted)
            {
                showCompleted = newShowCompleted;
                RefreshRemindersList();
            }
            
            Text.Font = GameFont.Small;
        }
        
        private void DrawContent(Rect rect)
        {
            if (displayedReminders.Count == 0)
            {
                DrawEmptyState(rect);
            }
            else
            {
                DrawRemindersList(rect);
            }
        }
        
        private void DrawEmptyState(Rect rect)
        {
            var messageRect = new Rect(rect.x, rect.y + rect.height * 0.3f, rect.width, 100f);
            
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(messageRect, LocalizationKeys.NoActiveReminders.Translate());
            
            Text.Font = GameFont.Small;
            var instructionRect = new Rect(rect.x, messageRect.yMax + 20f, rect.width, 50f);
            Widgets.Label(instructionRect, "Click 'Create New Reminder' or press Y to get started!");
            
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawRemindersList(Rect rect)
        {
            // Sort reminders based on current sort mode
            var sortedReminders = GetSortedReminders(displayedReminders).ToList();
            
            // Calculate total height needed for all items
            float totalHeight = 5f; // Top padding
            var itemHeights = new List<float>();
            
            for (int i = 0; i < sortedReminders.Count; i++)
            {
                var itemHeight = CalculateReminderItemHeight(sortedReminders[i], rect.width - 26f); // Account for scrollbar and margins
                itemHeights.Add(itemHeight);
                totalHeight += itemHeight + 5f; // Item height + spacing
            }
            
            var viewRect = new Rect(0f, 0f, rect.width - 16f, totalHeight);
            
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            
            float currentY = 5f;
            for (int i = 0; i < sortedReminders.Count; i++)
            {
                var reminder = sortedReminders[i];
                var itemRect = new Rect(5f, currentY, viewRect.width - 10f, itemHeights[i]);
                
                DrawReminderItem(itemRect, reminder, i);
                currentY += itemHeights[i] + 5f;
            }
            
            Widgets.EndScrollView();
        }

        private float CalculateReminderItemHeight(Reminder reminder, float width)
        {
            float height = 16f; // Top and bottom padding (8f each)
            
            // Title height
            Text.Font = GameFont.Small;
            height += Text.LineHeight + 2f;
            
            // Description height
            Text.Font = GameFont.Tiny;
            string desc = reminder.Description;
            if (desc.Length > 80) desc = desc.Substring(0, 77) + "...";
            var descWidth = width - 152f; // Account for urgency bar, buttons, and margins
            height += Text.CalcHeight(desc, descWidth) + 2f;
            
            // Trigger info height
            height += Text.LineHeight + 2f;
            
            // Status height
            height += Text.LineHeight + 2f;
            
            Text.Font = GameFont.Small;
            
            // Ensure minimum height for buttons
            return Mathf.Max(height, 70f);
        }
        
        private IEnumerable<Reminder> GetSortedReminders(List<Reminder> reminders)
        {
            return currentSort switch
            {
                SortMode.TriggerTime => reminders.OrderBy(r => GetSortPriority(r)),
                SortMode.Created => reminders.OrderByDescending(r => r.CreatedTick),
                SortMode.Severity => reminders.OrderByDescending(r => (int)r.Severity).ThenBy(r => GetSortPriority(r)),
                SortMode.Status => reminders.OrderBy(r => r.IsActive ? 0 : 1).ThenBy(r => GetSortPriority(r)),
                SortMode.Title => reminders.OrderBy(r => r.Title),
                _ => reminders.OrderBy(r => GetSortPriority(r))
            };
        }
        
        private void DrawReminderItem(Rect rect, Reminder reminder, int index)
        {
            // Apply opacity for completed reminders
            float opacity = reminder.IsActive ? 1f : 0.5f;
            GUI.color = new Color(1f, 1f, 1f, opacity);
            
            // Background with urgency color coding
            var bgColor = GetReminderBackgroundColor(reminder, index);
            bgColor.a *= opacity; // Apply opacity to background too
            Widgets.DrawBoxSolid(rect, bgColor);
            Widgets.DrawBox(rect);

            var innerRect = rect.ContractedBy(8f);
            
            // Left side - Urgency indicator
            var urgencyRect = new Rect(innerRect.x, innerRect.y, 6f, innerRect.height);
            var urgencyColor = GetUrgencyColor(reminder);
            Widgets.DrawBoxSolid(urgencyRect, urgencyColor);
            
            // Content area (excluding urgency bar and buttons)
            var contentRect = new Rect(innerRect.x + 12f, innerRect.y + 2f, innerRect.width - 140f, innerRect.height - 4f);
            float currentY = contentRect.y;
            
            // Title with severity color
            Text.Font = GameFont.Small;
            var titleRect = new Rect(contentRect.x, currentY, contentRect.width, Text.LineHeight);
            var titleColor = GetSeverityColor(reminder.Severity);
            GUI.color = new Color(titleColor.r, titleColor.g, titleColor.b, GUI.color.a); // Preserve opacity
            Widgets.Label(titleRect, reminder.Title);
            GUI.color = new Color(1f, 1f, 1f, opacity); // Reset to base opacity
            currentY += Text.LineHeight + 2f;
            
            // Description
            Text.Font = GameFont.Tiny;
            string desc = reminder.Description;
            if (desc.Length > 80) desc = desc.Substring(0, 77) + "...";
            var descHeight = Text.CalcHeight(desc, contentRect.width);
            var descRect = new Rect(contentRect.x, currentY, contentRect.width, descHeight);
            Widgets.Label(descRect, desc);
            currentY += descHeight + 2f;
            
            // Trigger info with detailed timing
            var triggerRect = new Rect(contentRect.x, currentY, contentRect.width, Text.LineHeight);
            string triggerInfo = GetDetailedTriggerDisplayText(reminder);
            var triggerColor = GetTriggerStatusColor(reminder);
            GUI.color = triggerColor;
            Widgets.Label(triggerRect, triggerInfo);
            GUI.color = Color.white;
            currentY += Text.LineHeight + 2f;
            
            // Status and time remaining
            var statusRect = new Rect(contentRect.x, currentY, contentRect.width, Text.LineHeight);
            string statusText = GetStatusDisplayText(reminder);
            Widgets.Label(statusRect, statusText);
            
            Text.Font = GameFont.Small;
            
            // Action buttons
            var buttonWidth = 60f;
            var buttonHeight = 25f;
            var buttonY = innerRect.y + 5f;
            
            var editButtonRect = new Rect(innerRect.xMax - buttonWidth, buttonY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(editButtonRect, "Edit"))
            {
                Messages.Message("Edit functionality coming soon", MessageTypeDefOf.NeutralEvent);
            }
            
            var deleteButtonRect = new Rect(innerRect.xMax - buttonWidth, buttonY + 30f, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(deleteButtonRect, "Delete"))
            {
                if (ReminderManager.Instance != null)
                {
                    ReminderManager.Instance.RemoveReminder(reminder.Id);
                    RefreshRemindersList();
                }
            }
            
            // Reset GUI color
            GUI.color = Color.white;
        }

        private string GetDetailedTriggerDisplayText(Reminder reminder)
        {
            if (reminder.Trigger == null)
                return "No trigger set";
                
            if (reminder.Trigger is TimeTrigger timeTrigger)
            {
                // Show when it will trigger, not the original description
                var firstMap = Find.Maps.FirstOrDefault();
                var tile = firstMap?.Tile ?? 0;
                var targetDate = GenDate.DateReadoutStringAt(GenDate.TickGameToAbs(timeTrigger.TargetTick), Find.WorldGrid.LongLatOf(tile));
                return $"Triggers on: {targetDate}";
            }
            
            return $"Trigger: {reminder.Trigger.GetType().Name}";
        }
        
        private string GetStatusDisplayText(Reminder reminder)
        {
            if (!reminder.IsActive)
                return "Completed";
                
            if (reminder.Trigger is TimeTrigger timeTrigger)
            {
                var remaining = timeTrigger.TimeRemainingDescription;
                return $"Time remaining: {remaining}";
            }
            
            return "Active";
        }
        
        private Color GetReminderBackgroundColor(Reminder reminder, int index)
        {
            if (!reminder.IsActive)
                return new Color(0.3f, 0.3f, 0.3f, 0.3f); // Grayed out for completed
                
            if (IsUrgent(reminder))
                return new Color(0.4f, 0.1f, 0.1f, 0.3f); // Red tint for urgent
                
            if (IsUpcoming(reminder))
                return new Color(0.3f, 0.3f, 0.1f, 0.2f); // Yellow tint for upcoming
                
            // Alternating background
            return index % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f, 0.1f) : new Color(0.1f, 0.1f, 0.1f, 0.1f);
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
        
        private Color GetUrgencyColor(Reminder reminder)
        {
            if (!reminder.IsActive)
                return ColoredText.SubtleGrayColor;
                
            // Use severity-based coloring for the urgency bar too
            return GetSeverityColor(reminder.Severity);
        }
        
        private Color GetTriggerStatusColor(Reminder reminder)
        {
            if (!reminder.IsActive)
                return ColoredText.SubtleGrayColor;
                
            // For timing urgency, still use time-based colors
            if (IsUrgent(reminder))
                return ColoredText.WarningColor; // Red for urgent timing
                
            if (IsUpcoming(reminder))
                return ColoredText.TipSectionTitleColor; // Yellow for upcoming
                
            return Color.white;
        }
        
        private bool IsUrgent(Reminder reminder)
        {
            if (!reminder.IsActive || reminder.Trigger is not TimeTrigger timeTrigger)
                return false;
                
            var remaining = timeTrigger.TicksRemaining;
            var hoursRemaining = remaining / (float)GenDate.TicksPerHour;
            
            return hoursRemaining <= 6f; // Less than 6 hours
        }
        
        private bool IsUpcoming(Reminder reminder)
        {
            if (!reminder.IsActive || reminder.Trigger is not TimeTrigger timeTrigger)
                return false;
                
            var remaining = timeTrigger.TicksRemaining;
            var hoursRemaining = remaining / (float)GenDate.TicksPerHour;
            
            return hoursRemaining <= 24f && hoursRemaining > 6f; // Between 6-24 hours
        }
        
        private int GetSortPriority(Reminder reminder)
        {
            if (!reminder.IsActive)
                return 1000; // Completed reminders at the bottom
                
            if (reminder.Trigger is TimeTrigger timeTrigger)
            {
                return timeTrigger.TicksRemaining; // Sort by time remaining
            }
            
            return 500; // Other trigger types in the middle
        }

        private void RefreshRemindersList()
        {
            displayedReminders.Clear();
            
            if (ReminderManager.Instance != null)
            {
                var allReminders = ReminderManager.Instance.AllReminders;
                if (allReminders != null)
                {
                    // Filter based on showCompleted setting
                    var filteredReminders = showCompleted 
                        ? allReminders 
                        : allReminders.Where(r => r.IsActive);
                    
                    displayedReminders.AddRange(filteredReminders);
                }
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            RefreshRemindersList();
        }
    }
}