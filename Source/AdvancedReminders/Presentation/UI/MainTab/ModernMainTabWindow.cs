using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Presentation.UI.Core;
using AdvancedReminders.Presentation.UI.Layout;
using AdvancedReminders.Presentation.UI.State;
using AdvancedReminders.Presentation.UI.Theme;
using AdvancedReminders.Presentation.UI.ViewModels;
using AdvancedReminders.Presentation.UI.Components;
using AdvancedReminders.Presentation.UI.Dialogs;
using AdvancedReminders.Core.Models;

namespace AdvancedReminders.Presentation.UI.MainTab
{
    /// <summary>
    /// Modern widget-based MainTabWindow replacing the legacy 646-line implementation.
    /// Uses RimHUD-inspired architecture with responsive design and centralized state management.
    /// </summary>
    public class ModernMainTabWindow : MainTabWindow
    {
        private IWidget _rootWidget;
        private bool _showCompleted = true;
        private SortMode _currentSort = SortMode.TriggerTime;

        public override Vector2 RequestedTabSize => new Vector2(1200f, 600f);

        /// <summary>
        /// Initialize the modern widget-based UI
        /// </summary>
        public override void PreOpen()
        {
            base.PreOpen();
            CreateRootWidget();
            
            // Subscribe to state changes for reactive updates
            ReminderState.OnRemindersChanged += OnRemindersChanged;
            ReminderState.OnStatisticsChanged += OnStatisticsChanged;
        }

        /// <summary>
        /// Clean up event subscriptions
        /// </summary>
        public override void PreClose()
        {
            base.PreClose();
            
            // Unsubscribe from state changes
            ReminderState.OnRemindersChanged -= OnRemindersChanged;
            ReminderState.OnStatisticsChanged -= OnStatisticsChanged;
        }

        /// <summary>
        /// Main drawing method using widget system
        /// </summary>
        public override void DoWindowContents(Rect inRect)
        {
            // Ensure we have a root widget
            if (_rootWidget == null)
                CreateRootWidget();

            // Draw the widget hierarchy
            _rootWidget?.Draw(inRect);
        }

        /// <summary>
        /// Creates the root widget hierarchy
        /// </summary>
        private void CreateRootWidget()
        {
            _rootWidget = new LayoutContainer(LayoutType.Vertical)
            {
                Spacing = ReminderTheme.StandardSpacing,
                Children =
                {
                    CreateHeader(),
                    CreateMainContent()
                }
            };
        }

        /// <summary>
        /// Creates the header widget with title, stats, and controls
        /// </summary>
        private IWidget CreateHeader()
        {
            return new HeaderWidget(_showCompleted, _currentSort, OnShowCompletedChanged, OnSortChanged);
        }

        /// <summary>
        /// Creates the main content area
        /// </summary>
        private IWidget CreateMainContent()
        {
            return LayoutContainer.Horizontal(
                CreateRemindersList(),
                CreateSidebar()
            );
        }

        /// <summary>
        /// Creates the reminders list widget
        /// </summary>
        private IWidget CreateRemindersList()
        {
            return new ReminderListWidget(_showCompleted, _currentSort);
        }

        /// <summary>
        /// Creates the sidebar with calendar and actions
        /// </summary>
        private IWidget CreateSidebar()
        {
            return new SidebarWidget();
        }

        #region Event Handlers

        private void OnRemindersChanged()
        {
            // Refresh the UI when reminders change
            CreateRootWidget();
        }

        private void OnStatisticsChanged()
        {
            // Refresh statistics displays
            CreateRootWidget();
        }

        private void OnShowCompletedChanged(bool showCompleted)
        {
            _showCompleted = showCompleted;
            CreateRootWidget();
        }

        private void OnSortChanged(SortMode sortMode)
        {
            _currentSort = sortMode;
            CreateRootWidget();
        }

        #endregion
    }

    /// <summary>
    /// Header widget for the main tab with title, statistics, and controls
    /// </summary>
    public class HeaderWidget : WidgetBase
    {
        private readonly bool _showCompleted;
        private readonly SortMode _currentSort;
        private readonly System.Action<bool> _onShowCompletedChanged;
        private readonly System.Action<SortMode> _onSortChanged;

        public HeaderWidget(bool showCompleted, SortMode currentSort,
            System.Action<bool> onShowCompletedChanged, System.Action<SortMode> onSortChanged)
        {
            _showCompleted = showCompleted;
            _currentSort = currentSort;
            _onShowCompletedChanged = onShowCompletedChanged;
            _onSortChanged = onSortChanged;
        }

        public override float GetMaxHeight(float availableWidth) => 120f;

        protected override bool DrawInternal(Rect rect)
        {
            ReminderTheme.DrawPanel(rect);
            var innerRect = rect.Padded();

            // Split into sections with more space for buttons
            var (titleSection, controlsSection) = innerRect.SplitVertical(30f);
            var (statsSection, buttonsSection) = controlsSection.SplitVertical(25f); // Give adequate height to buttons

            DrawTitle(titleSection);
            DrawStatistics(statsSection);
            DrawControls(buttonsSection);

            return true;
        }

        private void DrawTitle(Rect rect)
        {
            ReminderTheme.DrawStyledText(rect, "Advanced Reminders", ReminderTheme.TextStyle.Header);
        }

        private void DrawStatistics(Rect rect)
        {
            var stats = ReminderState.Statistics;
            var text = $"Active: {stats.ActiveCount} | Urgent: {stats.UrgentCount} | Total: {stats.TotalCount}";
            ReminderTheme.DrawStyledText(rect, text, ReminderTheme.TextStyle.Body);
        }

        private void DrawControls(Rect rect)
        {
            // Give more space to action buttons - 50% for controls, 50% for buttons
            var (controlsSection, buttonSection) = rect.SplitHorizontal(rect.width * 0.5f);
            
            // Split controls section between sort and toggle
            var (sortSection, toggleSection) = controlsSection.SplitHorizontal(controlsSection.width * 0.7f);

            DrawSortControls(sortSection);
            DrawToggleControls(toggleSection);
            DrawActionButtons(buttonSection);
        }

        private void DrawSortControls(Rect rect)
        {
            var labelWidth = 40f;
            var (labelRect, buttonsRect) = rect.SplitHorizontal(labelWidth);

            ReminderTheme.DrawStyledText(labelRect, "Sort:", ReminderTheme.TextStyle.Body);

            var sortOptions = new[]
            {
                ("Time", SortMode.TriggerTime),
                ("Created", SortMode.Created),
                ("Severity", SortMode.Severity),
                ("Status", SortMode.Status),
                ("Title", SortMode.Title)
            };

            var buttonRects = LayoutEngine.StackHorizontal(buttonsRect, 2f, 
                sortOptions.Select(s => 60f).ToArray());

            for (int i = 0; i < sortOptions.Length && i < buttonRects.Length; i++)
            {
                var (label, mode) = sortOptions[i];
                var isSelected = _currentSort == mode;

                var oldColor = GUI.color;
                GUI.color = isSelected ? Color.yellow : Color.white;

                if (ReminderTheme.DrawStyledButton(buttonRects[i], label))
                {
                    _onSortChanged?.Invoke(mode);
                }

                GUI.color = oldColor;
            }
        }

        private void DrawToggleControls(Rect rect)
        {
            var showCompleted = _showCompleted;
            Text.Font = GameFont.Small;
            Verse.Widgets.CheckboxLabeled(rect, "Show completed", ref showCompleted);

            if (showCompleted != _showCompleted)
            {
                _onShowCompletedChanged?.Invoke(showCompleted);
            }
        }

        private void DrawActionButtons(Rect rect)
        {
            // Use better spacing for action buttons
            var buttonRects = rect.AsButtonGroup(2, 8f, true);

            if (ReminderTheme.DrawStyledButton(buttonRects[0], "Clear Completed", true, 
                "Remove all completed reminders"))
            {
                ReminderState.ClearCompletedReminders();
            }

            if (ReminderTheme.DrawStyledButton(buttonRects[1], "Create New", true, 
                "Create a new reminder"))
            {
                var dialog = new ModernCreateReminderDialog();
                Find.WindowStack.Add(dialog);
            }
        }

        private (Rect left, Rect center, Rect right) SplitHorizontalThree(Rect rect)
        {
            var thirdWidth = rect.width / 3f;
            var left = new Rect(rect.x, rect.y, thirdWidth, rect.height);
            var center = new Rect(rect.x + thirdWidth, rect.y, thirdWidth, rect.height);
            var right = new Rect(rect.x + thirdWidth * 2, rect.y, thirdWidth, rect.height);
            return (left, center, right);
        }
    }

    /// <summary>
    /// List widget for displaying reminders with modern architecture
    /// </summary>
    public class ReminderListWidget : WidgetBase
    {
        private readonly bool _showCompleted;
        private readonly SortMode _sortMode;
        private Vector2 _scrollPosition = Vector2.zero;

        public ReminderListWidget(bool showCompleted, SortMode sortMode)
        {
            _showCompleted = showCompleted;
            _sortMode = sortMode;
        }

        public override float GetMaxHeight(float availableWidth) => -1f; // Take all available

        protected override bool DrawInternal(Rect rect)
        {
            var reminders = ReminderState.GetFilteredReminders(_showCompleted, _sortMode).ToList();

            if (!reminders.Any())
            {
                DrawEmptyState(rect);
                return true;
            }

            // Calculate total height needed
            var itemHeights = reminders.Select(r => CalculateItemHeight(r, rect.width)).ToList();
            var totalHeight = itemHeights.Sum() + (reminders.Count * ReminderTheme.SmallSpacing);

            var viewRect = new Rect(0f, 0f, rect.width - 16f, totalHeight);

            Verse.Widgets.BeginScrollView(rect, ref _scrollPosition, viewRect);

            float currentY = 0f;
            for (int i = 0; i < reminders.Count; i++)
            {
                var itemRect = new Rect(0f, currentY, viewRect.width, itemHeights[i]);
                var viewModel = reminders[i].ToViewModel();
                
                var itemWidget = new ModernReminderItemWidget(viewModel, i);
                itemWidget.Draw(itemRect);

                currentY += itemHeights[i] + ReminderTheme.SmallSpacing;
            }

            Verse.Widgets.EndScrollView();
            return true;
        }

        private void DrawEmptyState(Rect rect)
        {
            var messageRect = new Rect(rect.x, rect.y + rect.height * 0.3f, rect.width, 100f);
            
            ReminderTheme.DrawStyledText(messageRect, "No active reminders", ReminderTheme.TextStyle.Header, 
                anchor: TextAnchor.MiddleCenter);

            var instructionRect = new Rect(rect.x, messageRect.yMax + 20f, rect.width, 50f);
            ReminderTheme.DrawStyledText(instructionRect, 
                "Click 'Create New' or press Alt+R to get started!", ReminderTheme.TextStyle.Body,
                anchor: TextAnchor.MiddleCenter);
        }

        private float CalculateItemHeight(Reminder reminder, float width)
        {
            // Use a simplified calculation for now
            // In a full implementation, this would use the actual widget's GetMaxHeight
            return 80f;
        }
    }

    /// <summary>
    /// Modern reminder item widget using the new architecture
    /// </summary>
    public class ModernReminderItemWidget : CachedReminderWidget
    {
        private readonly int _index;

        public ModernReminderItemWidget(ReminderViewModel viewModel, int index) : base(viewModel)
        {
            _index = index;
        }

        public override float GetMaxHeight(float availableWidth) => 80f;

        protected override bool DrawReminder(Rect rect, ReminderViewModel viewModel)
        {
            var bgColor = ReminderTheme.GetAlternatingBackgroundColor(_index);
            bgColor.a *= viewModel.DisplayOpacity;

            ReminderTheme.DrawPanel(rect, bgColor);

            var innerRect = rect.Padded(8f);
            var (contentRect, buttonRect) = innerRect.SplitHorizontal(innerRect.width - 140f);

            DrawReminderContent(contentRect, viewModel);
            DrawReminderButtons(buttonRect, viewModel);

            return true;
        }

        private void DrawReminderContent(Rect rect, ReminderViewModel viewModel)
        {
            float currentY = rect.y;
            
            // Title
            var titleRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(titleRect, viewModel.DisplayTitle, ReminderTheme.TextStyle.Body, 
                viewModel.SeverityColor);
            currentY += Text.LineHeight + 2f;

            // Description
            var descRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(descRect, viewModel.DisplayDescription, ReminderTheme.TextStyle.Secondary);
            currentY += Text.LineHeight + 2f;

            // Status
            var statusRect = new Rect(rect.x, currentY, rect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(statusRect, viewModel.StatusText, ReminderTheme.TextStyle.Secondary, 
                viewModel.StatusColor);
        }

        private void DrawReminderButtons(Rect rect, ReminderViewModel viewModel)
        {
            var buttonRects = LayoutEngine.StackVertical(rect, 4f, 
                viewModel.HasQuest ? new[] { 1f, 1f, 1f } : new[] { 1f, 1f });

            int buttonIndex = 0;

            if (viewModel.HasQuest)
            {
                if (ReminderTheme.DrawStyledButton(buttonRects[buttonIndex++], "Quest", 
                    viewModel.CanEdit, "View associated quest"))
                {
                    // Handle quest button click
                }
            }

            if (ReminderTheme.DrawStyledButton(buttonRects[buttonIndex++], "Edit", 
                viewModel.CanEdit, "Edit this reminder"))
            {
                var dialog = new ModernCreateReminderDialog(); // Will create ModernEditReminderDialog later
                Find.WindowStack.Add(dialog);
            }

            if (ReminderTheme.DrawStyledButton(buttonRects[buttonIndex], "Delete", 
                viewModel.CanDelete, "Delete this reminder"))
            {
                ReminderState.RemoveReminder(viewModel.Id);
            }
        }
    }

    /// <summary>
    /// Sidebar widget with modern calendar and additional features
    /// </summary>
    public class SidebarWidget : WidgetBase
    {
        private readonly ModernCalendarWidget _calendarWidget;

        public SidebarWidget()
        {
            _calendarWidget = new ModernCalendarWidget();
            _calendarWidget.OnDaySelected += OnDaySelected;
        }

        public override float GetMaxHeight(float availableWidth) => -1f; // Take all available

        protected override bool DrawInternal(Rect rect)
        {
            ReminderTheme.DrawPanel(rect);
            var innerRect = rect.Padded();
            
            // Wrap calendar in scrollable widget
            var scrollableCalendar = new ScrollableWidget(_calendarWidget);
            scrollableCalendar.Draw(innerRect);

            return true;
        }

        private void OnDaySelected(int year, int dayOfYear)
        {
            // Handle day selection - could filter reminders by date
            Messages.Message($"Selected day {dayOfYear + 1} of year {year}", MessageTypeDefOf.NeutralEvent);
        }
    }
}