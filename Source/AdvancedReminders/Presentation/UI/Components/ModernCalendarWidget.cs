using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Presentation.UI.Core;
using AdvancedReminders.Presentation.UI.Theme;
using AdvancedReminders.Presentation.UI.State;
using AdvancedReminders.Core.Models;

namespace AdvancedReminders.Presentation.UI.Components
{
    /// <summary>
    /// Modern responsive calendar widget that adapts to available space.
    /// Replaces hardcoded CELL_SIZE with dynamic sizing based on available width.
    /// </summary>
    public class ModernCalendarWidget : WidgetBase
    {
        #region Fields

        private int _centerYear;
        private int _selectedYear = -1;
        private int _selectedDay = -1;
        private Vector2 _scrollPosition = Vector2.zero;

        // Events
        public Action<int, int> OnDaySelected { get; set; }
        public Action<int, int> OnDayDoubleClicked { get; set; }

        // Cached reminder data
        private Dictionary<int, List<Reminder>> _remindersByYear = new Dictionary<int, List<Reminder>>();
        private int _lastCacheUpdate = -1;

        #endregion

        public ModernCalendarWidget()
        {
            _centerYear = GenDate.Year(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(0).x);
            RefreshReminderCache();
        }

        #region Properties

        public int CenterYear 
        { 
            get => _centerYear; 
            set 
            { 
                _centerYear = value;
                RefreshReminderCache();
            } 
        }

        public int SelectedYear => _selectedYear;
        public int SelectedDay => _selectedDay;

        #endregion

        public override float GetMaxHeight(float availableWidth)
        {
            // Calculate responsive dimensions based on available width
            var dimensions = CalculateResponsiveDimensions(availableWidth);
            
            // Header height + 3 years of calendar + navigation + margins
            return dimensions.HeaderHeight + 
                   (dimensions.YearHeight * 3) + 
                   (dimensions.YearSpacing * 2) + 
                   dimensions.NavigationHeight + 
                   (dimensions.Margin * 2);
        }

        protected override bool DrawInternal(Rect rect)
        {
            // Update cache if needed
            if (_lastCacheUpdate != Find.TickManager.TicksGame / 60000) // Update daily
            {
                RefreshReminderCache();
            }

            var dimensions = CalculateResponsiveDimensions(rect.width);
            var innerRect = rect.ContractedBy(dimensions.Margin);
            var currentY = innerRect.y;

            // Navigation header
            var navRect = new Rect(innerRect.x, currentY, innerRect.width, dimensions.NavigationHeight);
            DrawNavigationHeader(navRect, dimensions);
            currentY += dimensions.NavigationHeight + dimensions.YearSpacing;

            // Draw 3 years (previous, current, next)
            for (int i = -1; i <= 1; i++)
            {
                var year = _centerYear + i;
                var yearRect = new Rect(innerRect.x, currentY, innerRect.width, dimensions.YearHeight);
                DrawYearCalendar(yearRect, year, dimensions);
                currentY += dimensions.YearHeight + dimensions.YearSpacing;
            }

            return true;
        }

        #region Responsive Calculations

        private CalendarDimensions CalculateResponsiveDimensions(float availableWidth)
        {
            // Calculate cell size based on available width
            // 15 cells per row (days in quadrum) with some padding
            var margin = ReminderTheme.StandardPadding;
            var usableWidth = availableWidth - (margin * 2);
            var cellSize = Mathf.Max(12f, Mathf.Min(24f, usableWidth / 18f)); // 15 cells + padding
            var cellPadding = Mathf.Max(1f, cellSize * 0.08f);
            
            return new CalendarDimensions
            {
                CellSize = cellSize,
                CellPadding = cellPadding,
                HeaderHeight = ReminderTheme.HeaderHeight,
                YearHeight = ReminderTheme.HeaderHeight + (cellSize + cellPadding) * 4 + cellPadding * 3, // 4 quadrums
                YearSpacing = ReminderTheme.StandardSpacing,
                NavigationHeight = ReminderTheme.ButtonHeight,
                Margin = margin
            };
        }

        #endregion

        #region Drawing Methods

        private void DrawNavigationHeader(Rect rect, CalendarDimensions dimensions)
        {
            var buttonWidth = dimensions.NavigationHeight;
            var (leftButtonRect, centerRect, rightButtonRect) = SplitThreeWays(rect, buttonWidth, buttonWidth);

            // Previous year button
            if (ReminderTheme.DrawStyledButton(leftButtonRect, "◀", true, "Previous year"))
            {
                _centerYear--;
                RefreshReminderCache();
            }

            // Current year display
            var yearText = $"Year {_centerYear}";
            ReminderTheme.DrawStyledText(centerRect, yearText, ReminderTheme.TextStyle.Header, 
                anchor: TextAnchor.MiddleCenter);

            // Next year button
            if (ReminderTheme.DrawStyledButton(rightButtonRect, "▶", true, "Next year"))
            {
                _centerYear++;
                RefreshReminderCache();
            }
        }

        private void DrawYearCalendar(Rect rect, int year, CalendarDimensions dimensions)
        {
            var currentY = rect.y;
            
            // Get actual current year from game
            var actualCurrentYear = GenDate.Year(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(0).x);

            // Year header
            var headerRect = new Rect(rect.x, currentY, rect.width, dimensions.HeaderHeight);
            var headerText = year == actualCurrentYear ? $"Year {year} (Current)" : $"Year {year}";
            var headerColor = year == actualCurrentYear ? ReminderTheme.AccentBlue : ReminderTheme.TextPrimary;
            ReminderTheme.DrawStyledText(headerRect, headerText, ReminderTheme.TextStyle.Body, headerColor);
            currentY += dimensions.HeaderHeight;

            // Draw quadrums (seasons)
            var quadrumWidth = rect.width / 4f;
            for (int quadrum = 0; quadrum < 4; quadrum++)
            {
                var quadrumRect = new Rect(rect.x + quadrum * quadrumWidth, currentY, 
                    quadrumWidth, dimensions.YearHeight - dimensions.HeaderHeight);
                DrawQuadrum(quadrumRect, year, quadrum, dimensions);
            }
        }

        private void DrawQuadrum(Rect rect, int year, int quadrum, CalendarDimensions dimensions)
        {
            ReminderTheme.DrawPanel(rect, ReminderTheme.WithAlpha(GetQuadrumColor(quadrum), 0.1f));
            
            var innerRect = rect.ContractedBy(dimensions.CellPadding);
            var cellsPerRow = 3; // 3x5 grid for 15 days
            var cellsPerCol = 5;
            
            var cellWidth = (innerRect.width - dimensions.CellPadding * (cellsPerRow - 1)) / cellsPerRow;
            var cellHeight = (innerRect.height - dimensions.CellPadding * (cellsPerCol - 1)) / cellsPerCol;

            for (int day = 0; day < 15; day++) // 15 days per quadrum
            {
                var row = day / cellsPerRow;
                var col = day % cellsPerRow;
                
                var cellX = innerRect.x + col * (cellWidth + dimensions.CellPadding);
                var cellY = innerRect.y + row * (cellHeight + dimensions.CellPadding);
                var cellRect = new Rect(cellX, cellY, cellWidth, cellHeight);
                
                var dayOfYear = quadrum * 15 + day;
                DrawDayCell(cellRect, year, dayOfYear, dimensions);
            }
        }

        private void DrawDayCell(Rect rect, int year, int dayOfYear, CalendarDimensions dimensions)
        {
            var currentYear = GenDate.Year(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(0).x);
            var currentDay = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(0).x);
            
            // Determine cell state
            var isToday = (year == currentYear && dayOfYear == currentDay);
            var isPast = (year < currentYear || (year == currentYear && dayOfYear < currentDay));
            var isSelected = (year == _selectedYear && dayOfYear == _selectedDay);
            var hasReminder = HasReminderOnDay(year, dayOfYear);
            var isHovered = Mouse.IsOver(rect);

            // Get base quadron color  
            var quadrum = dayOfYear / 15; // 0-3 for the four quadrums
            var quadronColor = GetQuadrumColor(quadrum);
            
            // Choose cell color with quadron differentiation
            Color cellColor;
            if (isSelected)
                cellColor = ReminderTheme.AccentBlue;
            else if (isToday)
                cellColor = ReminderTheme.AccentGreen;
            else if (hasReminder)
                cellColor = ReminderTheme.GetSeverityColor(AdvancedReminders.Core.Enums.SeverityLevel.Medium);
            else if (isHovered)
                cellColor = ReminderTheme.BackgroundHover;
            else
                cellColor = quadronColor; // Use quadron-specific color

            if (isPast)
                cellColor = ReminderTheme.WithAlpha(cellColor, 0.5f);

            // Draw cell
            ReminderTheme.DrawPanel(rect, cellColor);

            // Draw day number
            var dayNumber = (dayOfYear % 15) + 1;
            ReminderTheme.DrawStyledText(rect, dayNumber.ToString(), ReminderTheme.TextStyle.Secondary,
                anchor: TextAnchor.MiddleCenter);

            // Handle click
            if (Verse.Widgets.ButtonInvisible(rect))
            {
                _selectedYear = year;
                _selectedDay = dayOfYear;
                OnDaySelected?.Invoke(year, dayOfYear);
            }

            // Tooltip
            if (Mouse.IsOver(rect))
            {
                var tooltipText = $"Day {dayOfYear + 1}, Year {year}";
                if (hasReminder)
                    tooltipText += "\nHas reminders";
                TooltipHandler.TipRegion(rect, tooltipText);
            }
        }

        #endregion

        #region Helper Methods

        private Color GetQuadrumColor(int quadrum)
        {
            // Much more visible quadron colors for better UX differentiation
            return quadrum switch
            {
                0 => new Color(0.2f, 0.4f, 0.2f, 0.6f), // Aprimay - Spring green (deeper green)
                1 => new Color(0.4f, 0.4f, 0.1f, 0.6f), // Jugust - Summer yellow-green (more contrast)
                2 => new Color(0.4f, 0.2f, 0.1f, 0.6f), // Septober - Autumn orange-brown (warmer)
                3 => new Color(0.15f, 0.2f, 0.35f, 0.6f), // Decembary - Winter blue-gray (cooler)
                _ => ReminderTheme.BackgroundSecondary
            };
        }

        private (Rect left, Rect center, Rect right) SplitThreeWays(Rect rect, float leftWidth, float rightWidth)
        {
            var left = new Rect(rect.x, rect.y, leftWidth, rect.height);
            var right = new Rect(rect.xMax - rightWidth, rect.y, rightWidth, rect.height);
            var center = new Rect(rect.x + leftWidth, rect.y, rect.width - leftWidth - rightWidth, rect.height);
            return (left, center, right);
        }

        private bool HasReminderOnDay(int year, int dayOfYear)
        {
            if (!_remindersByYear.ContainsKey(year))
                return false;

            return _remindersByYear[year].Any(r => GetReminderDayOfYear(r) == dayOfYear);
        }

        private int GetReminderDayOfYear(Reminder reminder)
        {
            // Simple implementation - can be enhanced for different trigger types
            if (reminder.Trigger is Domain.Triggers.TimeTrigger timeTrigger)
            {
                return GenDate.DayOfYear(timeTrigger.TargetTick, Find.WorldGrid.LongLatOf(0).x);
            }
            return -1;
        }

        private void RefreshReminderCache()
        {
            _remindersByYear.Clear();
            _lastCacheUpdate = Find.TickManager.TicksGame / 60000;

            var reminders = ReminderState.AllReminders;
            foreach (var reminder in reminders)
            {
                if (reminder.Trigger is Domain.Triggers.TimeTrigger timeTrigger)
                {
                    var year = GenDate.Year(timeTrigger.TargetTick, Find.WorldGrid.LongLatOf(0).x);
                    if (!_remindersByYear.ContainsKey(year))
                        _remindersByYear[year] = new List<Reminder>();
                    _remindersByYear[year].Add(reminder);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Responsive calendar dimensions calculated based on available space
    /// </summary>
    public struct CalendarDimensions
    {
        public float CellSize;
        public float CellPadding;
        public float HeaderHeight;
        public float YearHeight;
        public float YearSpacing;
        public float NavigationHeight;
        public float Margin;
    }
}