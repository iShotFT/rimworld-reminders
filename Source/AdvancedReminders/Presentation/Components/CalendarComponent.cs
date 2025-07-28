using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Application.Services;

namespace AdvancedReminders.Presentation.Components
{
    /// <summary>
    /// General-purpose calendar component for RimWorld that displays days in quadrum/day grid format
    /// Following vanilla UI patterns from TimeAssignmentSelector and PawnColumnWorker_Timetable
    /// </summary>
    public class CalendarComponent
    {
        // Calendar state
        public int DisplayYear { get; set; }
        public int SelectedDay { get; set; } = -1; // Day of year (0-59), -1 = none selected
        
        // Events
        public Action<int> OnDaySelected { get; set; } // Called when a day is clicked (day of year)
        public Action<int> OnDayDoubleClicked { get; set; } // Called when a day is double-clicked
        
        // UI Constants (following vanilla patterns)
        private const float CELL_WIDTH = 24f;
        private const float CELL_HEIGHT = 20f;
        private const float CELL_PADDING = 1f;
        private const float QUADRUM_HEADER_HEIGHT = 18f;
        private const float NAV_BUTTON_WIDTH = 30f;
        private const float NAV_BUTTON_HEIGHT = 24f;
        
        // Colors (following vanilla color schemes)
        private static readonly Color CELL_NORMAL = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color CELL_HOVER = new Color(0.3f, 0.3f, 0.3f);
        private static readonly Color CELL_SELECTED = new Color(0.4f, 0.4f, 0.6f);
        private static readonly Color CELL_TODAY = new Color(0.2f, 0.4f, 0.2f);
        private static readonly Color CELL_REMINDER = new Color(0.6f, 0.4f, 0.2f);
        
        // Quadrum data (following RimWorld time system)
        private static readonly string[] QuadrumNames = { "Aprimay", "Jugust", "Septober", "Decembary" };
        private static readonly Color[] QuadrumColors = {
            new Color(0.3f, 0.6f, 0.3f), // Spring green
            new Color(0.6f, 0.6f, 0.2f), // Summer yellow
            new Color(0.6f, 0.4f, 0.2f), // Fall orange  
            new Color(0.4f, 0.5f, 0.6f)  // Winter blue
        };
        
        // Reminder data cache
        private Dictionary<int, List<Reminder>> remindersByDay = new Dictionary<int, List<Reminder>>();
        private int lastReminderCacheUpdate = -1;
        
        // Double-click detection
        private int lastClickedDay = -1;
        private float lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.5f;
        
        public CalendarComponent()
        {
            DisplayYear = GenLocalDate.Year(Find.CurrentMap);
        }
        
        /// <summary>
        /// Calculate the total size needed for the calendar
        /// </summary>
        public Vector2 GetCalendarSize()
        {
            var width = 15 * (CELL_WIDTH + CELL_PADDING) - CELL_PADDING + 120f; // Extra space for navigation and quadrum headers
            var height = 4 * (CELL_HEIGHT + CELL_PADDING) - CELL_PADDING + 40f; // Extra space for nav
            return new Vector2(width, height);
        }
        
        /// <summary>
        /// Force calendar to refresh reminder cache
        /// </summary>
        public void ForceRefresh()
        {
            UpdateReminderCache(forceUpdate: true);
        }
        
        /// <summary>
        /// Draw the calendar component
        /// </summary>
        public void DrawCalendar(Rect rect)
        {
            UpdateReminderCache();
            
            // Calculate layout
            var navRect = new Rect(rect.x, rect.y, rect.width, NAV_BUTTON_HEIGHT);
            var calendarRect = new Rect(rect.x, rect.y + NAV_BUTTON_HEIGHT + 5f, rect.width, rect.height - NAV_BUTTON_HEIGHT - 5f);
            
            DrawNavigationBar(navRect);
            DrawCalendarGrid(calendarRect);
        }
        
        /// <summary>
        /// Draw navigation bar with year controls
        /// </summary>
        private void DrawNavigationBar(Rect rect)
        {
            var prevRect = new Rect(rect.x, rect.y, NAV_BUTTON_WIDTH, NAV_BUTTON_HEIGHT);
            var nextRect = new Rect(rect.xMax - NAV_BUTTON_WIDTH, rect.y, NAV_BUTTON_WIDTH, NAV_BUTTON_HEIGHT);
            var yearRect = new Rect(prevRect.xMax + 5f, rect.y, nextRect.x - prevRect.xMax - 10f, NAV_BUTTON_HEIGHT);
            
            // Previous year button
            if (Widgets.ButtonText(prevRect, "<"))
            {
                DisplayYear--;
                SelectedDay = -1; // Clear selection when changing years
                UpdateReminderCache(forceUpdate: true);
            }
            
            // Next year button  
            if (Widgets.ButtonText(nextRect, ">"))
            {
                DisplayYear++;
                SelectedDay = -1;
                UpdateReminderCache(forceUpdate: true);
            }
            
            // Year display
            var yearText = $"Year {DisplayYear}";
            var currentYear = GenLocalDate.Year(Find.CurrentMap);
            if (DisplayYear == currentYear)
                yearText += " (Current)";
                
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(yearRect, yearText);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        
        /// <summary>
        /// Draw the main calendar grid with quadrums and days
        /// </summary>
        private void DrawCalendarGrid(Rect rect)
        {
            var currentTick = Find.TickManager.TicksGame;
            var currentYear = GenLocalDate.Year(Find.CurrentMap);
            var currentDayOfYear = GenLocalDate.DayOfYear(Find.CurrentMap);
            
            var gridRect = new Rect(rect.x + 60f, rect.y, // Leave space for quadrum headers
                                   15 * (CELL_WIDTH + CELL_PADDING) - CELL_PADDING,
                                   4 * (CELL_HEIGHT + CELL_PADDING) - CELL_PADDING);
            
            // Draw quadrum headers (left side)
            for (int quadrum = 0; quadrum < 4; quadrum++)
            {
                var headerRect = new Rect(rect.x, rect.y + quadrum * (CELL_HEIGHT + CELL_PADDING), 
                                         55f, CELL_HEIGHT); // Fixed width and height for headers
                
                GUI.color = QuadrumColors[quadrum];
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(headerRect, QuadrumNames[quadrum]);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            
            // Draw day grid
            for (int quadrum = 0; quadrum < 4; quadrum++)
            {
                for (int dayInQuadrum = 0; dayInQuadrum < 15; dayInQuadrum++)
                {
                    var dayOfYear = quadrum * 15 + dayInQuadrum;
                    var cellRect = new Rect(
                        gridRect.x + dayInQuadrum * (CELL_WIDTH + CELL_PADDING),
                        gridRect.y + quadrum * (CELL_HEIGHT + CELL_PADDING),
                        CELL_WIDTH,
                        CELL_HEIGHT
                    );
                    
                    DrawDayCell(cellRect, dayOfYear, currentYear, currentDayOfYear);
                }
            }
        }
        
        /// <summary>
        /// Draw an individual day cell
        /// </summary>
        private void DrawDayCell(Rect cellRect, int dayOfYear, int currentYear, int currentDayOfYear)
        {
            var isToday = (DisplayYear == currentYear && dayOfYear == currentDayOfYear);
            var isSelected = (dayOfYear == SelectedDay);
            var hasReminders = remindersByDay.ContainsKey(dayOfYear) && remindersByDay[dayOfYear].Any();
            var isHovered = Mouse.IsOver(cellRect);
            
            // Determine cell background color
            Color backgroundColor = CELL_NORMAL;
            if (isSelected)
                backgroundColor = CELL_SELECTED;
            else if (isToday)
                backgroundColor = CELL_TODAY;
            else if (hasReminders)
                backgroundColor = CELL_REMINDER;
            else if (isHovered)
                backgroundColor = CELL_HOVER;
            
            // Draw cell background
            GUI.color = backgroundColor;
            GUI.DrawTexture(cellRect, BaseContent.WhiteTex);
            
            // Draw cell border
            GUI.color = Color.gray;
            Widgets.DrawBox(cellRect, 1);
            
            // Draw special border for today
            if (isToday)
            {
                GUI.color = Color.yellow;
                Widgets.DrawBox(cellRect, 2);
            }
            
            // Draw day number
            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            var dayNumber = (dayOfYear + 1).ToString();
            Widgets.Label(cellRect, dayNumber);
            Text.Anchor = TextAnchor.UpperLeft;
            
            // Handle mouse interaction
            if (Widgets.ButtonInvisible(cellRect))
            {
                HandleDayClick(dayOfYear);
            }
            
            // Tooltip for reminders
            if (hasReminders && Mouse.IsOver(cellRect))
            {
                var reminders = remindersByDay[dayOfYear];
                var tooltipText = $"Reminders on day {dayOfYear + 1}:\n" + 
                                 string.Join("\n", reminders.Select(r => $"• {r.Title}"));
                TooltipHandler.TipRegion(cellRect, tooltipText);
            }
            
            GUI.color = Color.white;
        }
        
        /// <summary>
        /// Handle day cell clicks with double-click detection
        /// </summary>
        private void HandleDayClick(int dayOfYear)
        {
            var currentTime = Time.realtimeSinceStartup;
            
            // Check for double-click
            if (lastClickedDay == dayOfYear && (currentTime - lastClickTime) < DOUBLE_CLICK_TIME)
            {
                OnDayDoubleClicked?.Invoke(dayOfYear);
                lastClickedDay = -1; // Reset to prevent triple-click
            }
            else
            {
                SelectedDay = dayOfYear;
                OnDaySelected?.Invoke(dayOfYear);
                lastClickedDay = dayOfYear;
                lastClickTime = currentTime;
            }
        }
        
        /// <summary>
        /// Update the reminder cache for the current display year
        /// </summary>
        private void UpdateReminderCache(bool forceUpdate = false)
        {
            var currentTick = Find.TickManager.TicksGame;
            
            // Only update cache periodically to avoid performance issues
            if (!forceUpdate && Math.Abs(currentTick - lastReminderCacheUpdate) < 2500) // Update every hour
                return;
                
            lastReminderCacheUpdate = currentTick;
            remindersByDay.Clear();
            
            if (ReminderManager.Instance == null)
                return;
                
            // Get all active reminders
            var activeReminders = ReminderManager.Instance.AllReminders
                .Where(r => r.IsActive && r.Trigger != null)
                .ToList();
            
            // Group reminders by day of year for the display year
            foreach (var reminder in activeReminders)
            {
                var reminderDays = GetReminderDaysInYear(reminder, DisplayYear);
                foreach (var day in reminderDays)
                {
                    if (!remindersByDay.ContainsKey(day))
                        remindersByDay[day] = new List<Reminder>();
                    remindersByDay[day].Add(reminder);
                }
            }
        }
        
        /// <summary>
        /// Get the days in the specified year when this reminder will trigger
        /// </summary>
        private List<int> GetReminderDaysInYear(Reminder reminder, int year)
        {
            var days = new List<int>();
            
            // Calculate year boundaries in ticks
            var yearStartTick = GenDate.TicksPerYear * year;
            var yearEndTick = yearStartTick + GenDate.TicksPerYear;
            
            if (reminder.Trigger is Domain.Triggers.TimeTrigger timeTrigger)
            {
                if (timeTrigger.TargetTick >= yearStartTick && timeTrigger.TargetTick < yearEndTick)
                {
                    var dayOfYear = GenDate.DayOfYear(timeTrigger.TargetTick, 0f);
                    if (dayOfYear >= 0 && dayOfYear < 60)
                        days.Add(dayOfYear);
                }
            }
            else if (reminder.Trigger is Domain.Triggers.QuestDeadlineTrigger questTrigger)
            {
                var quest = questTrigger.GetAssociatedQuest();
                if (quest != null && quest.TicksUntilExpiry > 0)
                {
                    var triggerTick = Find.TickManager.TicksGame + quest.TicksUntilExpiry - (questTrigger.HoursBeforeExpiry * GenDate.TicksPerHour);
                    if (triggerTick >= yearStartTick && triggerTick < yearEndTick)
                    {
                        var dayOfYear = GenDate.DayOfYear(triggerTick, 0f);
                        if (dayOfYear >= 0 && dayOfYear < 60)
                            days.Add(dayOfYear);
                    }
                }
            }
            
            return days;
        }
        
        /// <summary>
        /// Get reminders for a specific day of year
        /// </summary>
        public List<Reminder> GetRemindersForDay(int dayOfYear)
        {
            return remindersByDay.ContainsKey(dayOfYear) ? remindersByDay[dayOfYear] : new List<Reminder>();
        }
        
        /// <summary>
        /// Convert day of year to absolute tick for the display year
        /// </summary>
        public int GetTickForDay(int dayOfYear)
        {
            return GenDate.TicksPerYear * DisplayYear + dayOfYear * GenDate.TicksPerDay;
        }
        
        /// <summary>
        /// Set the calendar to display the current year and select today
        /// </summary>
        public void GoToToday()
        {
            DisplayYear = GenLocalDate.Year(Find.CurrentMap);
            SelectedDay = GenLocalDate.DayOfYear(Find.CurrentMap);
            UpdateReminderCache(forceUpdate: true);
        }
    }
}