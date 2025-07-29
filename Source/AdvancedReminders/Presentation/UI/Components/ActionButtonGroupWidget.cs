using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdvancedReminders.Presentation.UI.Core;
using AdvancedReminders.Presentation.UI.Layout;
using AdvancedReminders.Presentation.UI.Theme;

namespace AdvancedReminders.Presentation.UI.Components
{
    /// <summary>
    /// Reusable widget for groups of action buttons with consistent styling.
    /// Follows RimHUD patterns for professional button layouts.
    /// </summary>
    public class ActionButtonGroupWidget : WidgetBase
    {
        private readonly List<ActionButton> _buttons;
        private readonly LayoutType _layoutType;
        private readonly float _spacing;

        public ActionButtonGroupWidget(LayoutType layoutType = LayoutType.Horizontal, float spacing = -1f)
        {
            _buttons = new List<ActionButton>();
            _layoutType = layoutType;
            _spacing = spacing >= 0 ? spacing : ReminderTheme.SmallSpacing;
        }

        /// <summary>
        /// Adds a button to the group
        /// </summary>
        public ActionButtonGroupWidget AddButton(string text, Action onClick, bool enabled = true, string tooltip = null)
        {
            _buttons.Add(new ActionButton(text, onClick, enabled, tooltip));
            return this;
        }

        /// <summary>
        /// Adds a styled button with specific color
        /// </summary>
        public ActionButtonGroupWidget AddStyledButton(string text, Action onClick, Color? color = null, bool enabled = true, string tooltip = null)
        {
            _buttons.Add(new ActionButton(text, onClick, enabled, tooltip, color));
            return this;
        }

        /// <summary>
        /// Clears all buttons from the group
        /// </summary>
        public void ClearButtons()
        {
            _buttons.Clear();
        }

        public override float GetMaxHeight(float availableWidth)
        {
            if (_buttons.Count == 0) return 0f;

            switch (_layoutType)
            {
                case LayoutType.Vertical:
                    return _buttons.Count * ReminderTheme.ButtonHeight + 
                           (_buttons.Count - 1) * _spacing;
                
                case LayoutType.Horizontal:
                    return ReminderTheme.ButtonHeight;
                
                default:
                    return ReminderTheme.ButtonHeight;
            }
        }

        protected override bool DrawInternal(Rect rect)
        {
            if (_buttons.Count == 0) return true;

            switch (_layoutType)
            {
                case LayoutType.Vertical:
                    return DrawVertical(rect);
                
                case LayoutType.Horizontal:
                    return DrawHorizontal(rect);
                
                default:
                    return DrawHorizontal(rect);
            }
        }

        private bool DrawVertical(Rect rect)
        {
            var buttonRects = LayoutEngine.StackVertical(rect, _spacing, 
                new float[_buttons.Count].Select(_ => 1f).ToArray());

            for (int i = 0; i < _buttons.Count && i < buttonRects.Length; i++)
            {
                DrawButton(buttonRects[i], _buttons[i]);
            }

            return true;
        }

        private bool DrawHorizontal(Rect rect)
        {
            var buttonWidth = (rect.width - _spacing * (_buttons.Count - 1)) / _buttons.Count;
            float currentX = rect.x;

            foreach (var button in _buttons)
            {
                var buttonRect = new Rect(currentX, rect.y, buttonWidth, rect.height);
                DrawButton(buttonRect, button);
                currentX += buttonWidth + _spacing;
            }

            return true;
        }

        private void DrawButton(Rect rect, ActionButton button)
        {
            var oldColor = GUI.color;
            if (button.Color.HasValue)
            {
                GUI.color = button.Color.Value;
            }

            if (ReminderTheme.DrawStyledButton(rect, button.Text, button.Enabled, button.Tooltip))
            {
                button.OnClick?.Invoke();
            }

            GUI.color = oldColor;
        }

        /// <summary>
        /// Helper method to create standard reminder action buttons
        /// </summary>
        public static ActionButtonGroupWidget CreateReminderActions(Action onEdit, Action onDelete, bool hasQuest = false, Action onViewQuest = null)
        {
            var widget = new ActionButtonGroupWidget(LayoutType.Vertical, ReminderTheme.SmallSpacing);
            
            if (hasQuest && onViewQuest != null)
            {
                widget.AddStyledButton("Quest", onViewQuest, ReminderTheme.AccentBlue, true, "View associated quest");
            }
            
            widget.AddButton("Edit", onEdit, true, "Edit this reminder");
            widget.AddStyledButton("Delete", onDelete, ReminderTheme.AccentRed, true, "Delete this reminder");
            
            return widget;
        }

        /// <summary>
        /// Helper method to create header control buttons
        /// </summary>
        public static ActionButtonGroupWidget CreateHeaderActions(Action onClearCompleted, Action onCreateNew)
        {
            var widget = new ActionButtonGroupWidget(LayoutType.Horizontal, ReminderTheme.SmallSpacing);
            
            widget.AddButton("Clear Completed", onClearCompleted, true, "Remove all completed reminders");
            widget.AddStyledButton("Create New", onCreateNew, ReminderTheme.AccentGreen, true, "Create a new reminder");
            
            return widget;
        }
    }

    /// <summary>
    /// Internal class representing a button in the action group
    /// </summary>
    internal class ActionButton
    {
        public string Text { get; }
        public Action OnClick { get; }
        public bool Enabled { get; }
        public string Tooltip { get; }
        public Color? Color { get; }

        public ActionButton(string text, Action onClick, bool enabled = true, string tooltip = null, Color? color = null)
        {
            Text = text;
            OnClick = onClick;
            Enabled = enabled;
            Tooltip = tooltip;
            Color = color;
        }
    }
}