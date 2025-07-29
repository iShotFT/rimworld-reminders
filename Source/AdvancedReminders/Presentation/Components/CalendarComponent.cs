using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Application.Services;

namespace AdvancedReminders.Presentation.Components
{
    /// <summary>
    /// Compact 3-year calendar component inspired by vanilla RimWorld scheduling UI
    /// </summary>
    public class CalendarComponent
    {
        // Calendar state
        public int CenterYear { get; set; } // The middle year in our 3-year view
        public int SelectedYear { get; set; } = -1;
        public int SelectedDay { get; set; } = -1; // Day of year (0-59), -1 = none selected
        
        // Events
        public Action<int, int> OnDaySelected { get; set; } // Called when a day is clicked (year, day of year)
        public Action<int, int> OnDayDoubleClicked { get; set; } // Called when a day is double-clicked
        
        // UI Constants (compact design inspired by vanilla scheduling)
        private const float CELL_SIZE = 16f; // Smaller square cells
        private const float CELL_PADDING = 1f;
        private const float YEAR_HEADER_HEIGHT = 20f;
        private const float YEAR_SPACING = 12f;
        private const float NAV_BUTTON_SIZE = 24f;
        private const float GRID_MARGIN = 10f;
        
        // Colors (vanilla-inspired with past day opacity)
        private static readonly Color CELL_NORMAL = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color CELL_HOVER = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color CELL_SELECTED = new Color(0.4f, 0.4f, 0.6f, 1f);
        private static readonly Color CELL_TODAY = new Color(0.2f, 0.4f, 0.2f, 1f);
        private static readonly Color CELL_REMINDER = new Color(0.6f, 0.4f, 0.2f, 1f);
        private static readonly Color CELL_PAST = new Color(0.15f, 0.15f, 0.15f, 0.6f); // Past with opacity
        
        // Quadrum season indicators (subtle colors, no text)
        private static readonly Color[] QuadrumIndicators = {
            new Color(0.3f, 0.6f, 0.3f, 0.3f), // Spring green
            new Color(0.6f, 0.6f, 0.2f, 0.3f), // Summer yellow
            new Color(0.6f, 0.4f, 0.2f, 0.3f), // Fall orange  
            new Color(0.4f, 0.5f, 0.6f, 0.3f)  // Winter blue
        };
        
        // Reminder data cache (by year and day)
        private Dictionary<int, Dictionary<int, List<Reminder>>> remindersByYearAndDay = new Dictionary<int, Dictionary<int, List<Reminder>>>();
        private int lastReminderCacheUpdate = -1;
        
        // Double-click detection
        private int lastClickedYear = -1;
        private int lastClickedDay = -1;
        private float lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.5f;
        
        public CalendarComponent()
        {
            CenterYear = GenLocalDate.Year(Find.CurrentMap);
        }
        
        /// <summary>
        /// Calculate the total size needed for the 3-year calendar
        /// </summary>
        public Vector2 GetCalendarSize()
        {
            var gridWidth = 15 * (CELL_SIZE + CELL_PADDING) - CELL_PADDING;
            var gridHeight = 4 * (CELL_SIZE + CELL_PADDING) - CELL_PADDING;
            
            var width = gridWidth + (GRID_MARGIN * 2);
            var height = NAV_BUTTON_SIZE + 10f + // Navigation
                         3 * (YEAR_HEADER_HEIGHT + gridHeight + YEAR_SPACING) - YEAR_SPACING + GRID_MARGIN; // 3 years
            
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
        /// Draw the compact 3-year calendar
        /// </summary>
        public void DrawCalendar(Rect rect)
        {
            UpdateReminderCache();
            
            var currentTick = Find.TickManager.TicksGame;
            var currentYear = GenLocalDate.Year(Find.CurrentMap);
            var currentDayOfYear = GenLocalDate.DayOfYear(Find.CurrentMap);
            
            // Navigation bar
            var navRect = new Rect(rect.x, rect.y, rect.width, NAV_BUTTON_SIZE);
            DrawNavigationBar(navRect);
            
            // Calendar content area
            var contentRect = new Rect(rect.x, rect.y + NAV_BUTTON_SIZE + 10f, 
                                     rect.width, rect.height - NAV_BUTTON_SIZE - 10f);
            
            // Draw 3 years (previous, current, next)
            var gridWidth = 15 * (CELL_SIZE + CELL_PADDING) - CELL_PADDING;
            var gridHeight = 4 * (CELL_SIZE + CELL_PADDING) - CELL_PADDING;
            
            for (int yearOffset = -1; yearOffset <= 1; yearOffset++)
            {
                var year = CenterYear + yearOffset;
                var yearIndex = yearOffset + 1; // 0, 1, 2
                
                var yearRect = new Rect(
                    contentRect.x + GRID_MARGIN,
                    contentRect.y + yearIndex * (YEAR_HEADER_HEIGHT + gridHeight + YEAR_SPACING),
                    gridWidth,
                    YEAR_HEADER_HEIGHT + gridHeight
                );
                
                DrawYearSection(yearRect, year, currentYear, currentDayOfYear);
            }
        }
        
        /// <summary>
        /// Draw navigation bar with year controls
        /// </summary>
        private void DrawNavigationBar(Rect rect)
        {
            var prevRect = new Rect(rect.x, rect.y, NAV_BUTTON_SIZE, NAV_BUTTON_SIZE);
            var nextRect = new Rect(rect.xMax - NAV_BUTTON_SIZE, rect.y, NAV_BUTTON_SIZE, NAV_BUTTON_SIZE);
            var titleRect = new Rect(prevRect.xMax + 5f, rect.y, nextRect.x - prevRect.xMax - 10f, NAV_BUTTON_SIZE);
            
            // Previous year button
            if (Widgets.ButtonText(prevRect, "◀"))
            {
                CenterYear--;
                SelectedYear = -1;
                SelectedDay = -1;
                UpdateReminderCache(forceUpdate: true);
            }
            
            // Next year button  
            if (Widgets.ButtonText(nextRect, "▶"))
            {
                CenterYear++;
                SelectedYear = -1;
                SelectedDay = -1;
                UpdateReminderCache(forceUpdate: true);
            }
            
            // Year range display
            var yearRangeText = $"{CenterYear - 1} - {CenterYear + 1}";
            var currentYear = GenLocalDate.Year(Find.CurrentMap);
            if (currentYear >= CenterYear - 1 && currentYear <= CenterYear + 1)
                yearRangeText += " (Current)";
                
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(titleRect, yearRangeText);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        
        /// <summary>
        /// Draw a single year section with header and grid
        /// </summary>
        private void DrawYearSection(Rect rect, int year, int currentYear, int currentDayOfYear)
        {
            // Year header
            var headerRect = new Rect(rect.x, rect.y, rect.width, YEAR_HEADER_HEIGHT);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            
            // Highlight current year
            if (year == currentYear)
            {
                GUI.color = new Color(0.4f, 0.6f, 0.4f);
                GUI.DrawTexture(headerRect, BaseContent.WhiteTex);
                GUI.color = Color.white;
            }
            
            Widgets.Label(headerRect, $"Year {year}");
            Text.Anchor = TextAnchor.UpperLeft;
            
            // Grid area
            var gridRect = new Rect(rect.x, rect.y + YEAR_HEADER_HEIGHT, rect.width, 
                                  4 * (CELL_SIZE + CELL_PADDING) - CELL_PADDING);
            
            DrawYearGrid(gridRect, year, currentYear, currentDayOfYear);
        }
        
        /// <summary>
        /// Draw the day grid for a specific year
        /// </summary>
        private void DrawYearGrid(Rect rect, int year, int currentYear, int currentDayOfYear)
        {
            for (int quadrum = 0; quadrum < 4; quadrum++)
            {
                // Subtle quadrum background indicator
                var quadrumRect = new Rect(
                    rect.x,
                    rect.y + quadrum * (CELL_SIZE + CELL_PADDING),
                    rect.width,
                    CELL_SIZE
                );
                
                GUI.color = QuadrumIndicators[quadrum];
                GUI.DrawTexture(quadrumRect, BaseContent.WhiteTex);
                GUI.color = Color.white;
                
                for (int dayInQuadrum = 0; dayInQuadrum < 15; dayInQuadrum++)
                {
                    var dayOfYear = quadrum * 15 + dayInQuadrum;
                    var cellRect = new Rect(
                        rect.x + dayInQuadrum * (CELL_SIZE + CELL_PADDING),
                        rect.y + quadrum * (CELL_SIZE + CELL_PADDING),
                        CELL_SIZE,
                        CELL_SIZE
                    );
                    
                    DrawDayCell(cellRect, year, dayOfYear, currentYear, currentDayOfYear);
                }
            }
        }
        
        /// <summary>
        /// Draw an individual day cell
        /// </summary>
        private void DrawDayCell(Rect cellRect, int year, int dayOfYear, int currentYear, int currentDayOfYear)
        {
            var isToday = (year == currentYear && dayOfYear == currentDayOfYear);
            var isSelected = (year == SelectedYear && dayOfYear == SelectedDay);
            var hasReminders = HasRemindersForDay(year, dayOfYear);
            var isHovered = Mouse.IsOver(cellRect);
            var isPast = (year < currentYear) || (year == currentYear && dayOfYear < currentDayOfYear);
            
            // Determine cell background color
            Color backgroundColor = CELL_NORMAL;
            if (isPast)
                backgroundColor = CELL_PAST;
            else if (isSelected)
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
            GUI.color = isPast ? new Color(0.5f, 0.5f, 0.5f, 0.6f) : Color.gray;
            Widgets.DrawBox(cellRect, 1);
            
            // Draw special border for today
            if (isToday)
            {
                GUI.color = Color.yellow;
                Widgets.DrawBox(cellRect, 2);
            }
            
            // Draw day number
            GUI.color = isPast ? new Color(1f, 1f, 1f, 0.6f) : Color.white;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            var dayNumber = (dayOfYear + 1).ToString();
            Widgets.Label(cellRect, dayNumber);
            Text.Anchor = TextAnchor.UpperLeft;
            
            // Handle mouse interaction
            if (Widgets.ButtonInvisible(cellRect))
            {
                HandleDayClick(year, dayOfYear);
            }
            
            // Tooltip for reminders
            if (hasReminders && Mouse.IsOver(cellRect))
            {
                var reminders = GetRemindersForDay(year, dayOfYear);
                var tooltipText = $"Reminders on {dayOfYear + 1}, {year}:\n" + 
                                 string.Join("\n", reminders.Select(r => $"• {r.Title}"));
                TooltipHandler.TipRegion(cellRect, tooltipText);
            }
            
            GUI.color = Color.white;
        }
        
        /// <summary>
        /// Handle day cell clicks with double-click detection
        /// </summary>
        private void HandleDayClick(int year, int dayOfYear)
        {
            var currentTime = Time.realtimeSinceStartup;
            
            // Check for double-click
            if (lastClickedYear == year && lastClickedDay == dayOfYear && (currentTime - lastClickTime) < DOUBLE_CLICK_TIME)
            {
                OnDayDoubleClicked?.Invoke(year, dayOfYear);
                lastClickedYear = -1; // Reset to prevent triple-click
                lastClickedDay = -1;
            }
            else
            {
                SelectedYear = year;
                SelectedDay = dayOfYear;
                OnDaySelected?.Invoke(year, dayOfYear);
                lastClickedYear = year;
                lastClickedDay = dayOfYear;
                lastClickTime = currentTime;
            }
        }
        
        /// <summary>
        /// Update the reminder cache for all visible years
        /// </summary>
        private void UpdateReminderCache(bool forceUpdate = false)
        {
            var currentTick = Find.TickManager.TicksGame;
            
            // Only update cache periodically to avoid performance issues
            if (!forceUpdate && Math.Abs(currentTick - lastReminderCacheUpdate) < 2500) // Update every hour
                return;
                
            lastReminderCacheUpdate = currentTick;
            remindersByYearAndDay.Clear();
            
            if (ReminderManager.Instance == null)
                return;
                
            // Get all active reminders
            var activeReminders = ReminderManager.Instance.AllReminders
                .Where(r => r.IsActive && r.Trigger != null)
                .ToList();
            
            // Group reminders by year and day for the 3-year range
            for (int yearOffset = -1; yearOffset <= 1; yearOffset++)
            {
                var year = CenterYear + yearOffset;
                foreach (var reminder in activeReminders)
                {
                    var reminderDays = GetReminderDaysInYear(reminder, year);
                    foreach (var day in reminderDays)
                    {
                        if (!remindersByYearAndDay.ContainsKey(year))
                            remindersByYearAndDay[year] = new Dictionary<int, List<Reminder>>();
                        if (!remindersByYearAndDay[year].ContainsKey(day))
                            remindersByYearAndDay[year][day] = new List<Reminder>();
                        remindersByYearAndDay[year][day].Add(reminder);
                    }
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
        /// Check if a specific day has reminders
        /// </summary>
        private bool HasRemindersForDay(int year, int dayOfYear)
        {
            return remindersByYearAndDay.ContainsKey(year) && 
                   remindersByYearAndDay[year].ContainsKey(dayOfYear) && 
                   remindersByYearAndDay[year][dayOfYear].Any();
        }
        
        /// <summary>
        /// Get reminders for a specific day
        /// </summary>
        public List<Reminder> GetRemindersForDay(int year, int dayOfYear)
        {
            if (remindersByYearAndDay.ContainsKey(year) && remindersByYearAndDay[year].ContainsKey(dayOfYear))
                return remindersByYearAndDay[year][dayOfYear];
            return new List<Reminder>();
        }
        
        /// <summary>
        /// Convert day of year to absolute tick for the specified year
        /// </summary>
        public int GetTickForDay(int year, int dayOfYear)
        {
            return GenDate.TicksPerYear * year + dayOfYear * GenDate.TicksPerDay;
        }
        
        /// <summary>
        /// Set the calendar to display the current year and select today
        /// </summary>
        public void GoToToday()
        {
            var currentYear = GenLocalDate.Year(Find.CurrentMap);
            CenterYear = currentYear;
            SelectedYear = currentYear;
            SelectedDay = GenLocalDate.DayOfYear(Find.CurrentMap);
            UpdateReminderCache(forceUpdate: true);
        }
    }
}