using UnityEngine;
using Verse;
using AdvancedReminders.Core.Enums;

namespace AdvancedReminders.Presentation.UI.Theme
{
    /// <summary>
    /// Centralized theme system providing consistent styling across all reminder UI components.
    /// Follows RimHUD patterns for professional, maintainable styling.
    /// </summary>
    public static class ReminderTheme
    {
        #region Color Palette

        // Severity Colors
        public static readonly Color SeverityLow = new Color(0.4f, 0.8f, 0.4f);      // Green
        public static readonly Color SeverityMedium = new Color(0.8f, 0.8f, 0.4f);   // Yellow
        public static readonly Color SeverityHigh = new Color(0.8f, 0.6f, 0.4f);     // Orange
        public static readonly Color SeverityCritical = new Color(0.8f, 0.4f, 0.4f); // Red

        // Status Colors
        public static readonly Color StatusActive = Color.white;
        public static readonly Color StatusCompleted = Color.gray;
        public static readonly Color StatusOverdue = Color.red;
        public static readonly Color StatusVeryUrgent = new Color(1f, 0.4f, 0.4f);
        public static readonly Color StatusUrgent = new Color(1f, 0.8f, 0.4f);

        // UI Colors
        public static readonly Color BackgroundPrimary = new Color(0.1f, 0.1f, 0.1f);
        public static readonly Color BackgroundSecondary = new Color(0.15f, 0.15f, 0.15f);
        public static readonly Color BackgroundHover = new Color(0.2f, 0.2f, 0.2f);
        public static readonly Color BorderColor = new Color(0.4f, 0.4f, 0.4f);
        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = new Color(0.8f, 0.8f, 0.8f);
        public static readonly Color TextMuted = new Color(0.6f, 0.6f, 0.6f);

        // Accent Colors
        public static readonly Color AccentBlue = new Color(0.3f, 0.6f, 0.9f);
        public static readonly Color AccentGreen = new Color(0.3f, 0.8f, 0.3f);
        public static readonly Color AccentRed = new Color(0.9f, 0.3f, 0.3f);

        #endregion

        #region Layout Constants

        // Spacing
        public static readonly float TinySpacing = 2f;
        public static readonly float SmallSpacing = 4f;
        public static readonly float StandardSpacing = 6f;
        public static readonly float LargeSpacing = 12f;
        public static readonly float HugeSpacing = 24f;

        // Padding
        public static readonly float TinyPadding = 3f;
        public static readonly float SmallPadding = 6f;
        public static readonly float StandardPadding = 12f;
        public static readonly float LargePadding = 18f;
        public static readonly float HugePadding = 24f;

        // Heights
        public static readonly float ButtonHeight = 28f;
        public static readonly float SmallButtonHeight = 24f;
        public static readonly float TallButtonHeight = 35f;
        public static readonly float InputHeight = 26f;
        public static readonly float HeaderHeight = 35f;

        // Widths
        public static readonly float MinDialogWidth = 400f;
        public static readonly float MaxDialogWidth = 1200f;
        public static readonly float DefaultButtonWidth = 80f;
        public static readonly float WideButtonWidth = 120f;

        // Radius
        public static readonly float CornerRadius = 3f;
        public static readonly float SmallCornerRadius = 2f;

        // Opacity
        public static readonly float DisabledOpacity = 0.5f;
        public static readonly float HoverOpacity = 0.8f;

        #endregion

        #region Text Styles

        /// <summary>
        /// Creates a styled text style for headers
        /// </summary>
        public static GUIStyle HeaderTextStyle
        {
            get
            {
                var style = new GUIStyle(Text.fontStyles[(int)GameFont.Medium]);
                style.normal.textColor = TextPrimary;
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            }
        }

        /// <summary>
        /// Creates a styled text style for body text
        /// </summary>
        public static GUIStyle BodyTextStyle
        {
            get
            {
                var style = new GUIStyle(Text.fontStyles[(int)GameFont.Small]);
                style.normal.textColor = TextPrimary;
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            }
        }

        /// <summary>
        /// Creates a styled text style for secondary text
        /// </summary>
        public static GUIStyle SecondaryTextStyle
        {
            get
            {
                var style = new GUIStyle(Text.fontStyles[(int)GameFont.Tiny]);
                style.normal.textColor = TextSecondary;
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            }
        }

        /// <summary>
        /// Creates a styled text style for muted text
        /// </summary>
        public static GUIStyle MutedTextStyle
        {
            get
            {
                var style = new GUIStyle(Text.fontStyles[(int)GameFont.Tiny]);
                style.normal.textColor = TextMuted;
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            }
        }

        #endregion

        #region Color Helpers

        /// <summary>
        /// Gets the appropriate color for a severity level
        /// </summary>
        public static Color GetSeverityColor(SeverityLevel severity)
        {
            return severity switch
            {
                SeverityLevel.Low => SeverityLow,
                SeverityLevel.Medium => SeverityMedium,
                SeverityLevel.High => SeverityHigh,
                SeverityLevel.Critical => SeverityCritical,
                _ => TextPrimary
            };
        }

        /// <summary>
        /// Gets color based on urgency level
        /// </summary>
        public static Color GetUrgencyColor(bool isOverdue, bool isVeryUrgent, bool isUrgent)
        {
            if (isOverdue) return StatusOverdue;
            if (isVeryUrgent) return StatusVeryUrgent;
            if (isUrgent) return StatusUrgent;
            return StatusActive;
        }

        /// <summary>
        /// Gets alternating background color for list items
        /// </summary>
        public static Color GetAlternatingBackgroundColor(int index, bool isSelected = false, bool isHover = false)
        {
            var baseColor = index % 2 == 0 ? BackgroundPrimary : BackgroundSecondary;
            
            if (isSelected)
                return Color.Lerp(baseColor, AccentBlue, 0.3f);
            
            if (isHover)
                return Color.Lerp(baseColor, BackgroundHover, 0.5f);
                
            return baseColor;
        }

        /// <summary>
        /// Creates a color with modified alpha
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Creates a lighter version of a color
        /// </summary>
        public static Color Lighter(this Color color, float amount = 0.2f)
        {
            return Color.Lerp(color, Color.white, amount);
        }

        /// <summary>
        /// Creates a darker version of a color
        /// </summary>
        public static Color Darker(this Color color, float amount = 0.2f)
        {
            return Color.Lerp(color, Color.black, amount);
        }

        #endregion

        #region Drawing Helpers

        /// <summary>
        /// Draws a styled background panel
        /// </summary>
        public static void DrawPanel(Rect rect, Color? backgroundColor = null, bool drawBorder = true)
        {
            var bgColor = backgroundColor ?? BackgroundPrimary;
            Verse.Widgets.DrawBoxSolid(rect, bgColor);
            
            if (drawBorder)
            {
                var oldColor = GUI.color;
                GUI.color = BorderColor;
                Verse.Widgets.DrawBox(rect);
                GUI.color = oldColor;
            }
        }

        /// <summary>
        /// Draws a styled button with theme colors
        /// </summary>
        public static bool DrawStyledButton(Rect rect, string text, bool enabled = true, string tooltip = null)
        {
            var oldColor = GUI.color;
            GUI.color = enabled ? Color.white : Color.white.WithAlpha(DisabledOpacity);
            
            bool clicked = enabled && Verse.Widgets.ButtonText(rect, text);
            
            if (!string.IsNullOrEmpty(tooltip) && Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            
            GUI.color = oldColor;
            return clicked;
        }

        /// <summary>
        /// Draws styled text with automatic color and font
        /// </summary>
        public static void DrawStyledText(Rect rect, string text, TextStyle style = TextStyle.Body, 
            Color? color = null, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var oldColor = GUI.color;
            var oldAnchor = Text.Anchor;
            var oldFont = Text.Font;

            GUI.color = color ?? GetTextColor(style);
            Text.Anchor = anchor;
            Text.Font = GetGameFont(style);

            Verse.Widgets.Label(rect, text);

            GUI.color = oldColor;
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        /// <summary>
        /// Draws a progress bar with theme styling
        /// </summary>
        public static void DrawProgressBar(Rect rect, float fillPercent, Color? fillColor = null, 
            Color? backgroundColor = null)
        {
            var bgColor = backgroundColor ?? BackgroundSecondary;
            var fgColor = fillColor ?? AccentBlue;
            
            // Background
            Verse.Widgets.DrawBoxSolid(rect, bgColor);
            
            // Fill
            if (fillPercent > 0f)
            {
                var fillRect = new Rect(rect.x, rect.y, rect.width * fillPercent, rect.height);
                Verse.Widgets.DrawBoxSolid(fillRect, fgColor);
            }
            
            // Border
            var oldColor = GUI.color;
            GUI.color = BorderColor;
            Verse.Widgets.DrawBox(rect);
            GUI.color = oldColor;
        }

        #endregion

        #region Enums and Helpers

        public enum TextStyle
        {
            Header,
            Body,
            Secondary,
            Muted
        }

        private static Color GetTextColor(TextStyle style)
        {
            return style switch
            {
                TextStyle.Header => TextPrimary,
                TextStyle.Body => TextPrimary,
                TextStyle.Secondary => TextSecondary,
                TextStyle.Muted => TextMuted,
                _ => TextPrimary
            };
        }

        private static GameFont GetGameFont(TextStyle style)
        {
            return style switch
            {
                TextStyle.Header => GameFont.Medium,
                TextStyle.Body => GameFont.Small,
                TextStyle.Secondary => GameFont.Tiny,
                TextStyle.Muted => GameFont.Tiny,
                _ => GameFont.Small
            };
        }

        #endregion

        #region Layout Helpers

        /// <summary>
        /// Creates a padded rect with standard padding
        /// </summary>
        public static Rect Padded(this Rect rect, float padding = -1f)
        {
            var actualPadding = padding >= 0 ? padding : StandardPadding;
            return rect.ContractedBy(actualPadding);
        }

        /// <summary>
        /// Creates a rect with custom padding on each side
        /// </summary>
        public static Rect Padded(this Rect rect, float left, float top, float right, float bottom)
        {
            return new Rect(
                rect.x + left,
                rect.y + top,
                rect.width - left - right,
                rect.height - top - bottom
            );
        }

        /// <summary>
        /// Splits a rect horizontally with spacing
        /// </summary>
        public static (Rect left, Rect right) SplitHorizontal(this Rect rect, float leftWidth, float spacing = -1f)
        {
            var actualSpacing = spacing >= 0 ? spacing : StandardSpacing;
            var left = new Rect(rect.x, rect.y, leftWidth, rect.height);
            var right = new Rect(rect.x + leftWidth + actualSpacing, rect.y, 
                rect.width - leftWidth - actualSpacing, rect.height);
            return (left, right);
        }

        /// <summary>
        /// Splits a rect vertically with spacing
        /// </summary>
        public static (Rect top, Rect bottom) SplitVertical(this Rect rect, float topHeight, float spacing = -1f)
        {
            var actualSpacing = spacing >= 0 ? spacing : StandardSpacing;
            var top = new Rect(rect.x, rect.y, rect.width, topHeight);
            var bottom = new Rect(rect.x, rect.y + topHeight + actualSpacing, 
                rect.width, rect.height - topHeight - actualSpacing);
            return (top, bottom);
        }

        #endregion
    }
}