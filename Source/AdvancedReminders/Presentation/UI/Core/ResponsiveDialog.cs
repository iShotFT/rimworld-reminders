using UnityEngine;
using Verse;
using AdvancedReminders.Presentation.UI.Layout;
using AdvancedReminders.Presentation.UI.Theme;

namespace AdvancedReminders.Presentation.UI.Core
{
    /// <summary>
    /// Base class for responsive dialogs that automatically size based on content.
    /// Replaces fixed-size dialogs with content-aware, responsive design following RimHUD patterns.
    /// </summary>
    public abstract class ResponsiveDialog : Window
    {
        // Default sizing constraints
        protected virtual float MinWidth => 400f;
        protected virtual float MaxWidth => 1200f;
        protected virtual float MinHeight => 300f;
        protected virtual float MaxHeight => 800f;
        
        // Padding and margins
        protected virtual float ContentPadding => 18f;
        protected virtual float TitleBarHeight => 35f;
        
        // Responsive sizing flags
        protected virtual bool AutoSizeWidth => true;
        protected virtual bool AutoSizeHeight => true;
        protected virtual bool CenterOnScreen => true;

        // Cached content widget for performance
        private IWidget _cachedContent;
        private bool _contentDirty = true;

        /// <summary>
        /// Creates the content widget for this dialog.
        /// This method is called to build the dialog's UI structure.
        /// </summary>
        protected abstract IWidget CreateContent();

        /// <summary>
        /// Gets the dialog title text. Override to provide custom titles.
        /// </summary>
        protected virtual string GetDialogTitle() => "Dialog";

        /// <summary>
        /// Indicates whether the dialog should be closeable with escape or close button.
        /// </summary>
        protected virtual bool IsCloseable => true;

        /// <summary>
        /// Override to provide responsive initial size based on content
        /// </summary>
        public override Vector2 InitialSize
        {
            get
            {
                var content = GetContent();
                if (content == null)
                    return new Vector2(MinWidth, MinHeight);

                return LayoutEngine.CalculateDialogSize(content, MinWidth, MaxWidth, MinHeight, MaxHeight);
            }
        }

        /// <summary>
        /// Centers the dialog on screen if enabled
        /// </summary>
        public override void PostOpen()
        {
            base.PostOpen();
            
            if (CenterOnScreen)
            {
                var screenCenter = new Vector2(UnityEngine.Screen.width / 2f, UnityEngine.Screen.height / 2f);
                var dialogSize = InitialSize;
                windowRect.x = screenCenter.x - (dialogSize.x / 2f);
                windowRect.y = screenCenter.y - (dialogSize.y / 2f);
            }
        }

        /// <summary>
        /// Main drawing method - handles responsive layout and content rendering
        /// </summary>
        public override void DoWindowContents(Rect inRect)
        {
            // Handle close button if closeable
            if (IsCloseable && Verse.Widgets.ButtonText(new Rect(inRect.xMax - 120f, 0f, 120f, 35f), "Close".Translate()))
            {
                Close();
                return;
            }

            // Draw title if provided
            var title = GetDialogTitle();
            if (!string.IsNullOrEmpty(title))
            {
                var titleRect = new Rect(0f, 0f, inRect.width - (IsCloseable ? 125f : 0f), TitleBarHeight);
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

        /// <summary>
        /// Handle keyboard input for responsive dialogs
        /// </summary>
        public override void OnAcceptKeyPressed()
        {
            if (HandleAcceptKey())
            {
                Event.current.Use();
            }
            else
            {
                base.OnAcceptKeyPressed();
            }
        }

        public override void OnCancelKeyPressed()
        {
            if (HandleCancelKey())
            {
                Event.current.Use();
            }
            else
            {
                base.OnCancelKeyPressed();
            }
        }

        /// <summary>
        /// Gets the cached content widget, creating it if necessary
        /// </summary>
        protected IWidget GetContent()
        {
            if (_cachedContent == null || _contentDirty)
            {
                _cachedContent = CreateContent();
                _contentDirty = false;
            }
            return _cachedContent;
        }

        /// <summary>
        /// Marks the content as dirty, forcing recreation on next access
        /// </summary>
        protected void InvalidateContent()
        {
            _contentDirty = true;
        }

        /// <summary>
        /// Override to handle Accept key (Enter) presses. Return true if handled.
        /// </summary>
        protected virtual bool HandleAcceptKey()
        {
            return false;
        }

        /// <summary>
        /// Override to handle Cancel key (Escape) presses. Return true if handled.
        /// </summary>
        protected virtual bool HandleCancelKey()
        {
            if (IsCloseable)
            {
                Close();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a simple content dialog with just a widget
        /// </summary>
        public static void ShowDialog(IWidget content, string title = "Dialog", 
            float minWidth = 400f, float maxWidth = 1200f, 
            float minHeight = 300f, float maxHeight = 800f)
        {
            var dialog = new SimpleResponsiveDialog(content, title, minWidth, maxWidth, minHeight, maxHeight);
            Find.WindowStack.Add(dialog);
        }

        /// <summary>
        /// Simple implementation of ResponsiveDialog for quick dialogs
        /// </summary>
        private class SimpleResponsiveDialog : ResponsiveDialog
        {
            private readonly IWidget _content;
            private readonly string _title;
            
            protected override float MinWidth { get; }
            protected override float MaxWidth { get; }
            protected override float MinHeight { get; }
            protected override float MaxHeight { get; }

            public SimpleResponsiveDialog(IWidget content, string title, 
                float minWidth, float maxWidth, float minHeight, float maxHeight)
            {
                _content = content;
                _title = title;
                MinWidth = minWidth;
                MaxWidth = maxWidth;
                MinHeight = minHeight;
                MaxHeight = maxHeight;
            }

            protected override IWidget CreateContent() => _content;
            protected override string GetDialogTitle() => _title;
        }
    }

    /// <summary>
    /// Form-specific responsive dialog with common form functionality
    /// </summary>
    public abstract class ResponsiveFormDialog : ResponsiveDialog
    {
        // Form-specific styling
        protected override float ContentPadding => 24f;
        protected virtual float FormSpacing => 8f;
        protected virtual float ButtonHeight => 35f;

        /// <summary>
        /// Creates the form content. Override to build your form.
        /// </summary>
        protected abstract IWidget CreateFormContent();

        /// <summary>
        /// Creates form action buttons (OK, Cancel, etc.)
        /// </summary>
        protected virtual IWidget CreateFormButtons()
        {
            return LayoutContainer.Horizontal(
                new ButtonWidget("OK", HandleOK, "Accept changes and close dialog"),
                new ButtonWidget("Cancel", HandleCancel, "Cancel changes and close dialog")
            );
        }

        /// <summary>
        /// Creates the complete form layout with content and buttons
        /// </summary>
        protected override IWidget CreateContent()
        {
            var formContent = CreateFormContent();
            var formButtons = CreateFormButtons();

            if (formButtons == null)
                return formContent;

            return new ResponsiveFormLayout(formContent, formButtons);
        }

        /// <summary>
        /// Handles OK button click. Override to provide custom OK logic.
        /// </summary>
        protected virtual void HandleOK()
        {
            if (ValidateForm())
            {
                ApplyChanges();
                Close();
            }
        }

        /// <summary>
        /// Handles Cancel button click.
        /// </summary>
        protected virtual void HandleCancel()
        {
            Close();
        }

        /// <summary>
        /// Override to validate form data before accepting. Return false to prevent closing.
        /// </summary>
        protected virtual bool ValidateForm()
        {
            return true;
        }

        /// <summary>
        /// Override to apply form changes when OK is clicked.
        /// </summary>
        protected virtual void ApplyChanges()
        {
            // Override in derived classes
        }

        protected override bool HandleAcceptKey()
        {
            HandleOK();
            return true;
        }

        protected override bool HandleCancelKey()
        {
            HandleCancel();
            return true;
        }
    }

    /// <summary>
    /// Simple button widget for dialog actions
    /// </summary>
    public class ButtonWidget : WidgetBase
    {
        private readonly string _text;
        private readonly System.Action _onClick;
        private readonly string _tooltip;

        public ButtonWidget(string text, System.Action onClick = null, string tooltip = null)
        {
            _text = text;
            _onClick = onClick;
            _tooltip = tooltip;
        }

        public override float GetMaxHeight(float availableWidth) => ReminderTheme.ButtonHeight;

        protected override bool DrawInternal(Rect rect)
        {
            if (ReminderTheme.DrawStyledButton(rect, _text, true, _tooltip))
            {
                _onClick?.Invoke();
            }
            return true;
        }
    }

    /// <summary>
    /// Layout widget that gives form content maximum space and places buttons at bottom
    /// </summary>
    public class ResponsiveFormLayout : WidgetBase
    {
        private readonly IWidget _formContent;
        private readonly IWidget _formButtons;
        private const float ButtonHeight = 35f;
        private const float ButtonMargin = 10f;

        public ResponsiveFormLayout(IWidget formContent, IWidget formButtons)
        {
            _formContent = formContent;
            _formButtons = formButtons;
        }

        public override float GetMaxHeight(float availableWidth) => -1f; // Take all available space

        protected override bool DrawInternal(Rect rect)
        {
            // Calculate button area height
            var buttonAreaHeight = _formButtons != null ? ButtonHeight + ButtonMargin : 0f;
            
            // Split rect: form content gets most space, buttons get fixed height at bottom
            var contentRect = new Rect(rect.x, rect.y, rect.width, rect.height - buttonAreaHeight);
            var buttonRect = new Rect(rect.x, rect.yMax - buttonAreaHeight, rect.width, buttonAreaHeight);

            // Draw form content in the content area
            if (_formContent != null && _formContent.IsVisible)
            {
                _formContent.Draw(contentRect);
            }

            // Draw buttons at the bottom
            if (_formButtons != null && _formButtons.IsVisible)
            {
                var buttonInnerRect = new Rect(buttonRect.x, buttonRect.y + ButtonMargin / 2f, 
                    buttonRect.width, ButtonHeight);
                _formButtons.Draw(buttonInnerRect);
            }

            return true;
        }
    }
}