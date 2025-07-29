using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Presentation.UI.Core;
using AdvancedReminders.Presentation.UI.Layout;
using AdvancedReminders.Presentation.UI.Theme;
using AdvancedReminders.Presentation.UI.Components;
using AdvancedReminders.Presentation.UI.State;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Domain.Triggers;
using AdvancedReminders.Domain.Actions;
using AdvancedReminders.Application.Services;
using AdvancedReminders.Core.Interfaces;
using Reminders;

namespace AdvancedReminders.Presentation.UI.Dialogs
{
    /// <summary>
    /// Modern responsive dialog for creating reminders using the new widget system.
    /// Replaces the old fixed-size Dialog_CreateReminder with widget-based architecture.
    /// </summary>
    public class ModernCreateReminderDialog : ResponsiveFormDialog
    {
        // Override sizing constraints for better UX
        protected override float MinHeight => 500f; // Increase minimum height
        protected override float MaxHeight => 900f;
        protected override float MinWidth => 600f; // Increase minimum width
        
        // Override to provide custom close button
        #region Fields

        private string _title = "";
        private string _description = "";
        private int _days = 1;
        private int _hours = 0;
        private int _questWarningDays = 0;
        private int _questWarningHours = 24; // Default to 24 hours before quest expires
        private SeverityLevel _severity = SeverityLevel.Medium;
        private Quest _selectedQuest = null;
        private TriggerType _triggerType = TriggerType.Time;

        // Form widgets
        private TextAreaWidget _titleWidget;
        private TextAreaWidget _descriptionWidget;
        private TimeInputWidget _timeWidget;
        private TimeInputWidget _questWarningTimeWidget;
        private SeveritySliderWidget _severityWidget;
        private QuestSelectorWidget _questWidget;
        private DropdownWidget<TriggerType> _triggerTypeWidget;

        #endregion

        public ModernCreateReminderDialog(Quest preSelectedQuest = null)
        {
            // If a quest is pre-selected, configure the dialog for quest reminder
            if (preSelectedQuest != null)
            {
                _selectedQuest = preSelectedQuest;
                _triggerType = TriggerType.Quest;
                _title = $"Quest Deadline: {preSelectedQuest.name}";
                _description = $"Reminder for quest '{preSelectedQuest.name}' expiring soon";
            }
            
            Initialize();
        }

        private void Initialize()
        {
            // Initialize form widgets with more reasonable sizes
            _titleWidget = new TextAreaWidget("Title", _title, 30f, OnTitleChanged); // Smaller title input
            _descriptionWidget = new TextAreaWidget("Description", _description, 60f, OnDescriptionChanged); // Smaller description
            _timeWidget = new TimeInputWidget("Trigger Time", _days, _hours, OnTimeChanged);
            _questWarningTimeWidget = new TimeInputWidget("Warn before quest expires", _questWarningDays, _questWarningHours, OnQuestWarningTimeChanged);
            _severityWidget = new SeveritySliderWidget("Severity", _severity, OnSeverityChanged);
            _questWidget = new QuestSelectorWidget("Associated Quest (Optional)", _selectedQuest, OnQuestChanged);
            
            // Create dropdown for trigger type
            var triggerOptions = new List<(TriggerType, string)>
            {
                (TriggerType.Time, "Time-based"),
                (TriggerType.Quest, "Quest Deadline")
            };
            _triggerTypeWidget = new DropdownWidget<TriggerType>("Trigger Type", triggerOptions, _triggerType, OnTriggerTypeChanged);
        }

        protected override string GetDialogTitle() => "Create New Reminder";
        
        /// <summary>
        /// Override to provide custom close button with 'X' character
        /// </summary>
        public override void DoWindowContents(Rect inRect)
        {
            // Custom 'X' close button instead of big "Close" button
            var closeButtonSize = 24f;
            var closeButtonRect = new Rect(inRect.xMax - closeButtonSize - 5f, 5f, closeButtonSize, closeButtonSize);
            
            if (Verse.Widgets.ButtonText(closeButtonRect, "×"))
            {
                Close();
                return;
            }

            // Draw title
            var title = GetDialogTitle();
            if (!string.IsNullOrEmpty(title))
            {
                var titleRect = new Rect(0f, 0f, inRect.width - 30f, TitleBarHeight);
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleLeft;
                Verse.Widgets.Label(titleRect, title);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                
                // Adjust content rect to account for title
                inRect.y += TitleBarHeight;
                inRect.height -= TitleBarHeight;
            }

            // Create content area with padding
            var contentRect = inRect.ContractedBy(ContentPadding);
            
            // Get and draw content
            var content = GetContent();
            if (content != null && content.IsVisible)
            {
                content.Draw(contentRect);
            }
        }

        protected override IWidget CreateFormContent()
        {
            // Horizontal layout: form on left, preview on right
            return new LayoutContainer(LayoutType.Horizontal)
            {
                Spacing = ReminderTheme.StandardSpacing,
                Children =
                {
                    CreateScrollableFormSection(), // Form with proper height allocation
                    CreatePreviewSection() // Preview on the right
                }
            };
        }
        
        private IWidget CreateScrollableFormSection()
        {
            // Create a container that takes up available height and makes the form scrollable within it
            return new ScrollableWidget(CreateFormSection());
        }

        private IWidget CreateFormSection()
        {
            var container = new LayoutContainer(LayoutType.Vertical)
            {
                Spacing = ReminderTheme.StandardSpacing
            };

            // Add form widgets in order
            container.Children.Add(_titleWidget);
            container.Children.Add(_descriptionWidget);
            container.Children.Add(_triggerTypeWidget);

            // Add conditional widget based on trigger type
            if (_triggerType == TriggerType.Time)
            {
                container.Children.Add(_timeWidget);
            }
            else if (_triggerType == TriggerType.Quest)
            {
                container.Children.Add(_questWidget);
                container.Children.Add(_questWarningTimeWidget);
            }

            container.Children.Add(_severityWidget);

            return container;
        }


        private IWidget CreatePreviewSection()
        {
            return new ReminderPreviewWidget(_title, _description, _severity, _triggerType, _days, _hours, _selectedQuest, _questWarningDays, _questWarningHours);
        }

        protected override IWidget CreateFormButtons()
        {
            var buttons = new ActionButtonGroupWidget(LayoutType.Horizontal, ReminderTheme.StandardSpacing);
            
            buttons.AddStyledButton("Cancel", HandleCancel, ReminderTheme.TextSecondary, true, "Cancel reminder creation");
            buttons.AddStyledButton("Create", HandleOK, ReminderTheme.AccentGreen, ValidateForm(), "Create the reminder");
            
            return buttons;
        }

        #region Event Handlers

        private void OnTitleChanged(string newTitle)
        {
            _title = newTitle;
            RefreshContent(); // Reactive update - triggers form validation and preview refresh
        }

        private void OnDescriptionChanged(string newDescription)
        {
            _description = newDescription;
            RefreshContent(); // Reactive update for description changes
        }

        private void OnTimeChanged(int days, int hours)
        {
            _days = days;
            _hours = hours;
            RefreshContent(); // Reactive update for time changes - affects validation and preview
        }

        private void OnQuestWarningTimeChanged(int days, int hours)
        {
            _questWarningDays = days;
            _questWarningHours = hours;
            RefreshContent(); // Reactive update for quest warning time changes
        }

        private void OnSeverityChanged(SeverityLevel severity)
        {
            _severity = severity;
            RefreshContent(); // Reactive update for severity changes - affects colors and preview
        }

        private void OnQuestChanged(Quest quest)
        {
            _selectedQuest = quest;
            RefreshContent(); // Reactive update for quest selection
        }

        private void OnTriggerTypeChanged(TriggerType triggerType)
        {
            _triggerType = triggerType;
            RefreshContent(); // Reactive update - may change which form widgets are visible
        }

        /// <summary>
        /// Reactive content refresh following V2 architecture patterns.
        /// Triggers validation updates and dynamic form layout changes.
        /// </summary>
        private void RefreshContent()
        {
            // Re-initialize widgets based on current state
            Initialize();
            
            // Invalidate cached content to force rebuild with new conditional widgets
            InvalidateContent();
            
            // Trigger form validation state update
            // This will enable/disable the Create button dynamically
        }


        #endregion

        #region Form Dialog Overrides

        protected override bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(_title)) return false;
            if (_triggerType == TriggerType.Time && _days <= 0 && _hours <= 0) return false;
            if (_triggerType == TriggerType.Quest && _selectedQuest == null) return false;
            return true;
        }

        protected override void ApplyChanges()
        {
            if (!ValidateForm()) return;

            try
            {
                // Create trigger based on type
                ITrigger trigger = _triggerType switch
                {
                    TriggerType.Time => new TimeTrigger(Find.TickManager.TicksGame + (_days * 60000) + (_hours * 2500)),
                    TriggerType.Quest => new QuestDeadlineTrigger(_selectedQuest.id, (_questWarningDays * 24) + _questWarningHours), // Use user-specified warning time
                    _ => new TimeTrigger(Find.TickManager.TicksGame + 60000) // Default to 1 day
                };

                // Create actions (simplified for now)
                var actions = new List<IReminderAction>
                {
                    new NotificationAction(true) // pauseGame parameter
                };

                // Create reminder using existing constructor (title, description, severity)
                var reminder = new Reminder(_title.Trim(), _description?.Trim(), _severity);
                
                // Set the trigger and actions after construction
                reminder.Trigger = trigger;
                reminder.Actions = actions;

                // Add to manager using existing pattern
                if (ReminderManager.Instance != null)
                {
                    ReminderManager.Instance.AddReminder(reminder);
                    ReminderState.RefreshState(); // Reactive state update following V2 architecture
                    Messages.Message($"Reminder '{_title}' created successfully", MessageTypeDefOf.PositiveEvent);
                }

                // Dialog will close automatically after ApplyChanges completes successfully
            }
            catch (Exception ex)
            {
                Log.Error($"[AdvancedReminders] Error creating reminder: {ex}");
                Messages.Message("Failed to create reminder. Check logs for details.", MessageTypeDefOf.NegativeEvent);
            }
        }

        #endregion
    }


    /// <summary>
    /// Widget for previewing the reminder before creation
    /// </summary>
    public class ReminderPreviewWidget : WidgetBase
    {
        private readonly string _title;
        private readonly string _description;
        private readonly SeverityLevel _severity;
        private readonly TriggerType _triggerType;
        private readonly int _days;
        private readonly int _hours;
        private readonly Quest _quest;
        private readonly int _questWarningDays;
        private readonly int _questWarningHours;

        public ReminderPreviewWidget(string title, string description, SeverityLevel severity, 
            TriggerType triggerType, int days, int hours, Quest quest, int questWarningDays = 0, int questWarningHours = 24)
        {
            _title = title;
            _description = description;
            _severity = severity;
            _triggerType = triggerType;
            _days = days;
            _hours = hours;
            _quest = quest;
            _questWarningDays = questWarningDays;
            _questWarningHours = questWarningHours;
        }

        public override float GetMaxHeight(float availableWidth) => 150f;

        protected override bool DrawInternal(Rect rect)
        {
            ReminderTheme.DrawPanel(rect, ReminderTheme.BackgroundSecondary);
            var innerRect = rect.Padded();
            
            var currentY = innerRect.y;
            
            // Preview header
            var headerRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(headerRect, "Preview", ReminderTheme.TextStyle.Header);
            currentY += Text.LineHeight + ReminderTheme.SmallSpacing;
            
            // Title preview
            if (!string.IsNullOrWhiteSpace(_title))
            {
                var titleRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
                var titleColor = ReminderTheme.GetSeverityColor(_severity);
                ReminderTheme.DrawStyledText(titleRect, $"Title: {_title}", ReminderTheme.TextStyle.Body, titleColor);
                currentY += Text.LineHeight + ReminderTheme.TinySpacing;
            }
            
            // Trigger preview
            var triggerText = _triggerType switch
            {
                TriggerType.Time => $"Triggers in: {_days} days, {_hours} hours",
                TriggerType.Quest => $"Quest: {_quest?.name ?? "None selected"}\nWarns {_questWarningDays}d {_questWarningHours}h before expiration",
                _ => "Unknown trigger"
            };
            
            var triggerRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            ReminderTheme.DrawStyledText(triggerRect, triggerText, ReminderTheme.TextStyle.Secondary);
            currentY += Text.LineHeight + ReminderTheme.TinySpacing;
            
            // Severity preview
            var severityRect = new Rect(innerRect.x, currentY, innerRect.width, Text.LineHeight);
            var severityColor = ReminderTheme.GetSeverityColor(_severity);
            ReminderTheme.DrawStyledText(severityRect, $"Severity: {_severity}", ReminderTheme.TextStyle.Secondary, severityColor);
            
            return true;
        }
    }
}