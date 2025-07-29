using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Presentation.UI.Core;
using AdvancedReminders.Presentation.UI.Theme;
using AdvancedReminders.Presentation.UI.Layout;
using AdvancedReminders.Core.Enums;

namespace AdvancedReminders.Presentation.UI.Components
{
    /// <summary>
    /// Generic dropdown selection widget
    /// </summary>
    public class DropdownWidget<T> : WidgetBase
    {
        private readonly string _label;
        private readonly List<(T value, string display)> _options;
        private readonly Action<T> _onSelectionChanged;
        private T _selectedValue;

        public DropdownWidget(string label, List<(T, string)> options, T selectedValue, Action<T> onSelectionChanged)
        {
            _label = label;
            _options = options ?? new List<(T, string)>();
            _selectedValue = selectedValue;
            _onSelectionChanged = onSelectionChanged;
        }

        public override float GetMaxHeight(float availableWidth) 
        {
            // Vanilla dropdown doesn't expand inline - uses popup menu
            return Text.LineHeight + ReminderTheme.ButtonHeight + ReminderTheme.SmallSpacing;
        }

        protected override bool DrawInternal(Rect rect)
        {
            var currentY = rect.y;
            
            // Label
            var labelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(labelRect, _label, ReminderTheme.TextStyle.Body);
            currentY += Text.LineHeight + ReminderTheme.SmallSpacing;

            // Use vanilla RimWorld dropdown
            var selectedDisplay = _options.FirstOrDefault(opt => EqualityComparer<T>.Default.Equals(opt.value, _selectedValue)).display ?? "None";
            var dropdownRect = new Rect(rect.x, currentY, rect.width, ReminderTheme.ButtonHeight);
            
            // Use vanilla dropdown functionality
            Verse.Widgets.Dropdown(dropdownRect, _selectedValue, 
                (T val) => selectedDisplay,
                (T val) => GenerateMenuFunc(_selectedValue),
                selectedDisplay);

            return true;
        }
        
        private IEnumerable<Verse.Widgets.DropdownMenuElement<string>> GenerateMenuFunc(T selectedValue)
        {
            var list = new List<Verse.Widgets.DropdownMenuElement<string>>();
            foreach (var option in _options)
            {
                list.Add(new Verse.Widgets.DropdownMenuElement<string>
                {
                    option = new FloatMenuOption(option.display, () =>
                    {
                        _selectedValue = option.value;
                        _onSelectionChanged?.Invoke(_selectedValue);
                    }),
                    payload = option.display
                });
            }
            return list;
        }
    }

    /// <summary>
    /// Widget that adds scrollbar support to any child widget
    /// </summary>
    public class ScrollableWidget : WidgetBase
    {
        private readonly IWidget _childWidget;
        private Vector2 _scrollPosition = Vector2.zero;

        public ScrollableWidget(IWidget childWidget)
        {
            _childWidget = childWidget;
        }

        public override float GetMaxHeight(float availableWidth) => -1f; // Take all available space

        protected override bool DrawInternal(Rect rect)
        {
            if (_childWidget == null) return false;

            // Calculate content height
            var contentHeight = _childWidget.GetMaxHeight(rect.width - 16f); // Account for scrollbar
            
            // If child wants all available space (-1), use the container height instead
            if (contentHeight < 0f)
                contentHeight = rect.height;
                
            // For scrolling to work, content must be larger than container
            // If content is smaller than container, we still need scrolling enabled
            // so force content height to be larger
            if (contentHeight <= rect.height)
                contentHeight = rect.height + 100f; // Add buffer for scrolling
            
            var viewRect = new Rect(0f, 0f, rect.width - 16f, contentHeight);

            // Draw scrollable area
            Verse.Widgets.BeginScrollView(rect, ref _scrollPosition, viewRect);
            
            // Draw child widget in the top portion of the view rect to ensure it's visible
            var childDrawRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 
                _childWidget.GetMaxHeight(viewRect.width));
            _childWidget.Draw(childDrawRect);
            
            Verse.Widgets.EndScrollView();

            return true;
        }
    }

    /// <summary>
    /// Widget for time input with hour and day selection
    /// </summary>
    public class TimeInputWidget : WidgetBase
    {
        private int _days;
        private int _hours;
        private readonly Action<int, int> _onTimeChanged;
        private readonly string _label;

        public TimeInputWidget(string label, int initialDays = 1, int initialHours = 0, Action<int, int> onTimeChanged = null)
        {
            _label = label;
            _days = initialDays;
            _hours = initialHours;
            _onTimeChanged = onTimeChanged;
        }

        public int Days => _days;
        public int Hours => _hours;
        public int TotalTicks => _days * 60000 + _hours * 2500;

        public override float GetMaxHeight(float availableWidth) => Text.LineHeight + ReminderTheme.ButtonHeight + ReminderTheme.InputHeight + (ReminderTheme.SmallSpacing * 2);

        protected override bool DrawInternal(Rect rect)
        {
            var currentY = rect.y;
            
            // Label with current time display
            var totalHours = _days * 24 + _hours;
            var timeDisplay = totalHours < 24 ? $"{totalHours}h" : $"{_days}d {_hours}h";
            var labelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(labelRect, $"{_label} ({timeDisplay})", ReminderTheme.TextStyle.Body);
            currentY += Text.LineHeight + ReminderTheme.SmallSpacing;
            
            // Quick preset buttons for common times - horizontal layout
            var buttonRowRect = new Rect(rect.x, currentY, rect.width, ReminderTheme.ButtonHeight);
            var buttonRects = LayoutEngine.StackHorizontal(buttonRowRect, 4f, 
                new[] { 60f, 60f, 60f, 60f }); // Fixed width buttons, no overlapping
            
            var presets = new[] 
            {
                (1, 0, "1h"),
                (0, 6, "6h"),  
                (1, 0, "1d"),
                (0, 7, "7d")
            };
            
            for (int i = 0; i < presets.Length && i < buttonRects.Length; i++)
            {
                var (days, hours, label) = presets[i];
                // Fix the preset values
                if (label == "1d") { days = 1; hours = 0; }
                if (label == "7d") { days = 7; hours = 0; }
                
                var isSelected = (_days == days && _hours == hours);
                var oldColor = GUI.color;
                GUI.color = isSelected ? Color.yellow : Color.white;
                
                if (ReminderTheme.DrawStyledButton(buttonRects[i], label))
                {
                    _days = days;
                    _hours = hours;
                    _onTimeChanged?.Invoke(_days, _hours);
                }
                GUI.color = oldColor;
            }
            currentY += ReminderTheme.ButtonHeight + ReminderTheme.SmallSpacing;
            
            // Custom input field on separate row - much more visible
            var customRowRect = new Rect(rect.x, currentY, rect.width, ReminderTheme.InputHeight);
            var (customLabelRect, customFieldRect) = customRowRect.SplitHorizontal(60f, ReminderTheme.TinySpacing);
            ReminderTheme.DrawStyledText(customLabelRect, "Custom:", ReminderTheme.TextStyle.Body);
            
            var customValue = totalHours.ToString() + "h";
            var newCustomValue = Verse.Widgets.TextField(customFieldRect, customValue);
            
            // Parse custom input (support formats like "5h", "2d", "1d 6h", etc.)
            if (newCustomValue != customValue && TryParseTimeInput(newCustomValue, out int newDays, out int newHours))
            {
                _days = newDays;
                _hours = newHours;
                _onTimeChanged?.Invoke(_days, _hours);
            }
            
            return true;
        }
        
        /// <summary>
        /// Parses time input in various formats: "5h", "2d", "1d 6h", etc.
        /// </summary>
        private bool TryParseTimeInput(string input, out int days, out int hours)
        {
            days = 0;
            hours = 0;
            
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            input = input.Trim().ToLower();
            
            // Simple hours format: "5h"
            if (input.EndsWith("h") && int.TryParse(input.Substring(0, input.Length - 1), out int totalHours))
            {
                days = totalHours / 24;
                hours = totalHours % 24;
                return true;
            }
            
            // Simple days format: "2d"
            if (input.EndsWith("d") && int.TryParse(input.Substring(0, input.Length - 1), out int totalDays))
            {
                days = totalDays;
                hours = 0;
                return true;
            }
            
            // Combined format: "1d 6h" or "1d6h"
            var parts = input.Replace("d", "d ").Replace("h", "h ").Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (part.EndsWith("d") && int.TryParse(part.Substring(0, part.Length - 1), out int d))
                    days += d;
                else if (part.EndsWith("h") && int.TryParse(part.Substring(0, part.Length - 1), out int h))
                    hours += h;
            }
            
            // Normalize hours to days if needed
            if (hours >= 24)
            {
                days += hours / 24;
                hours = hours % 24;
            }
            
            return days > 0 || hours > 0;
        }
    }

    /// <summary>
    /// Widget for quest selection with search and filtering
    /// </summary>
    public class QuestSelectorWidget : WidgetBase
    {
        private Quest _selectedQuest;
        private readonly Action<Quest> _onQuestChanged;
        private readonly string _label;

        public QuestSelectorWidget(string label, Quest initialQuest = null, Action<Quest> onQuestChanged = null)
        {
            _label = label;
            _selectedQuest = initialQuest;
            _onQuestChanged = onQuestChanged;
        }

        public Quest SelectedQuest => _selectedQuest;

        public override float GetMaxHeight(float availableWidth) 
        {
            // Vanilla dropdown doesn't expand inline - uses popup menu
            return Text.LineHeight + ReminderTheme.ButtonHeight + ReminderTheme.SmallSpacing;
        }

        protected override bool DrawInternal(Rect rect)
        {
            var currentY = rect.y;
            
            // Label
            var labelRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(labelRect, _label, ReminderTheme.TextStyle.Body);
            currentY += Text.LineHeight + ReminderTheme.SmallSpacing;
            
            // Use vanilla dropdown for quest selection
            var selectedDisplay = _selectedQuest?.name ?? "Select Quest...";
            var dropdownRect = new Rect(rect.x, currentY, rect.width, ReminderTheme.ButtonHeight);
            
            // Use vanilla dropdown functionality
            Verse.Widgets.Dropdown(dropdownRect, _selectedQuest, 
                (Quest val) => selectedDisplay,
                (Quest val) => GenerateQuestMenuFunc(),
                selectedDisplay);
            
            return true;
        }

        private IEnumerable<Verse.Widgets.DropdownMenuElement<string>> GenerateQuestMenuFunc()
        {
            var list = new List<Verse.Widgets.DropdownMenuElement<string>>();
            var availableQuests = GetAvailableQuests();
            
            if (!availableQuests.Any())
            {
                list.Add(new Verse.Widgets.DropdownMenuElement<string>
                {
                    option = new FloatMenuOption("No quests with deadlines available", null),
                    payload = ""
                });
            }
            else
            {
                foreach (var quest in availableQuests)
                {
                    var expiryText = quest.TicksUntilExpiry > 0 ? 
                        $" (expires in {quest.TicksUntilExpiry.ToStringTicksToPeriod()})" : "";
                    
                    list.Add(new Verse.Widgets.DropdownMenuElement<string>
                    {
                        option = new FloatMenuOption($"{quest.name}{expiryText}", () =>
                        {
                            _selectedQuest = quest;
                            _onQuestChanged?.Invoke(_selectedQuest);
                        }),
                        payload = quest.name
                    });
                }
            }
            
            return list;
        }

        private List<Quest> GetAvailableQuests()
        {
            if (Find.QuestManager?.questsInDisplayOrder == null)
                return new List<Quest>();
            
            // Filter for unaccepted quests with deadlines only
            return Find.QuestManager.questsInDisplayOrder
                .Where(q => q.State == QuestState.NotYetAccepted) // Only unaccepted quests
                .Where(q => q.acceptanceExpireTick != -1) // Only quests with expiration deadlines
                .Where(q => q.TicksUntilExpiry > 0) // Only quests that haven't expired yet
                .ToList();
        }
    }

    /// <summary>
    /// Widget for severity level selection with visual indicators
    /// </summary>
    public class SeveritySliderWidget : WidgetBase
    {
        private SeverityLevel _severity;
        private readonly Action<SeverityLevel> _onSeverityChanged;
        private readonly string _label;

        public SeveritySliderWidget(string label, SeverityLevel initialSeverity = SeverityLevel.Medium, 
            Action<SeverityLevel> onSeverityChanged = null)
        {
            _label = label;
            _severity = initialSeverity;
            _onSeverityChanged = onSeverityChanged;
        }

        public SeverityLevel Severity => _severity;

        public override float GetMaxHeight(float availableWidth) => Text.LineHeight + ReminderTheme.ButtonHeight + ReminderTheme.SmallSpacing;

        protected override bool DrawInternal(Rect rect)
        {
            var (labelRect, dropdownRect) = rect.SplitVertical(Text.LineHeight, ReminderTheme.SmallSpacing);
            
            // Label with current severity
            var labelText = $"{_label}:";
            ReminderTheme.DrawStyledText(labelRect, labelText, ReminderTheme.TextStyle.Body);
            
            // Use vanilla dropdown for severity selection
            var selectedDisplay = GetSeverityDisplayName(_severity);
            
            Verse.Widgets.Dropdown(dropdownRect, _severity, 
                (SeverityLevel val) => selectedDisplay,
                (SeverityLevel val) => GenerateMenuFunc(_severity),
                selectedDisplay);
            
            return true;
        }
        
        private IEnumerable<Verse.Widgets.DropdownMenuElement<string>> GenerateMenuFunc(SeverityLevel selectedValue)
        {
            var list = new List<Verse.Widgets.DropdownMenuElement<string>>();
            var severityValues = Enum.GetValues(typeof(SeverityLevel)).Cast<SeverityLevel>().ToArray();
            
            foreach (var severity in severityValues)
            {
                list.Add(new Verse.Widgets.DropdownMenuElement<string>
                {
                    option = new FloatMenuOption(GetSeverityDisplayName(severity), () =>
                    {
                        _severity = severity;
                        _onSeverityChanged?.Invoke(_severity);
                    }),
                    payload = GetSeverityDisplayName(severity)
                });
            }
            return list;
        }

        private string GetSeverityDisplayName(SeverityLevel severity)
        {
            return severity switch
            {
                SeverityLevel.Low => "Low",
                SeverityLevel.Medium => "Medium", 
                SeverityLevel.High => "High",
                SeverityLevel.Critical => "Critical",
                _ => "Unknown"
            };
        }
    }

    /// <summary>
    /// Widget for multi-line text input with scrolling
    /// </summary>
    public class TextAreaWidget : WidgetBase
    {
        private string _text;
        private readonly Action<string> _onTextChanged;
        private readonly string _label;
        private readonly float _height;
        private Vector2 _scrollPosition = Vector2.zero;

        public TextAreaWidget(string label, string initialText = "", float height = 80f, Action<string> onTextChanged = null)
        {
            _label = label;
            _text = initialText ?? "";
            _height = height;
            _onTextChanged = onTextChanged;
        }

        public string Text => _text;

        public override float GetMaxHeight(float availableWidth) => Verse.Text.LineHeight + _height + ReminderTheme.SmallSpacing;

        protected override bool DrawInternal(Rect rect)
        {
            var (labelRect, textRect) = rect.SplitVertical(Verse.Text.LineHeight, ReminderTheme.SmallSpacing);
            
            // Label
            ReminderTheme.DrawStyledText(labelRect, _label, ReminderTheme.TextStyle.Body);
            
            // Text area
            ReminderTheme.DrawPanel(textRect, ReminderTheme.BackgroundSecondary);
            var innerRect = textRect.ContractedBy(4f);
            
            var newText = Verse.Widgets.TextArea(innerRect, _text);
            if (newText != _text)
            {
                _text = newText;
                _onTextChanged?.Invoke(_text);
            }
            
            return true;
        }
    }
}