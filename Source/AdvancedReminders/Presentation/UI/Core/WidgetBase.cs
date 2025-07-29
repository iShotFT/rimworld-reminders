using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AdvancedReminders.Presentation.UI.Core
{
    /// <summary>
    /// Base widget class providing common functionality for all widgets.
    /// Implements RimHUD-inspired patterns for performance optimization and consistency.
    /// </summary>
    public abstract class WidgetBase : IWidget
    {
        // Object pooling for performance optimization (RimHUD pattern)
        private static readonly Stack<StringBuilder> StringBuilderPool = new Stack<StringBuilder>();
        private static readonly object PoolLock = new object();

        /// <summary>
        /// Calculates the maximum height needed for this widget given available width.
        /// Must be implemented by derived classes for responsive design.
        /// </summary>
        public abstract float GetMaxHeight(float availableWidth);

        /// <summary>
        /// Draws the widget within the given rectangle.
        /// Includes common early exit patterns for performance optimization.
        /// </summary>
        public virtual bool Draw(Rect rect)
        {
            // Early exit patterns following RimHUD performance optimization
            if (!IsVisible) return false;
            if (rect.height < 1f) return false;
            if (rect.width < 1f) return false;

            return DrawInternal(rect);
        }

        /// <summary>
        /// Internal draw method that derived classes must implement.
        /// Called only after performance checks pass.
        /// </summary>
        protected abstract bool DrawInternal(Rect rect);

        /// <summary>
        /// Whether this widget should be visible and processed.
        /// Default implementation always returns true.
        /// </summary>
        public virtual bool IsVisible => true;

        /// <summary>
        /// Gets a StringBuilder from the object pool for performance optimization.
        /// Caller must return it using ReturnStringBuilder().
        /// </summary>
        protected static StringBuilder BorrowStringBuilder()
        {
            lock (PoolLock)
            {
                if (StringBuilderPool.Count > 0)
                {
                    var sb = StringBuilderPool.Pop();
                    sb.Clear();
                    return sb;
                }
                return new StringBuilder();
            }
        }

        /// <summary>
        /// Returns a StringBuilder to the object pool for reuse.
        /// Must be called after using BorrowStringBuilder().
        /// </summary>
        protected static void ReturnStringBuilder(StringBuilder sb)
        {
            if (sb == null) return;
            
            lock (PoolLock)
            {
                if (StringBuilderPool.Count < 10) // Limit pool size
                {
                    StringBuilderPool.Push(sb);
                }
            }
        }

        /// <summary>
        /// Helper method for drawing text with consistent styling
        /// </summary>
        protected static void DrawText(Rect rect, string text, GameFont font = GameFont.Small, 
            TextAnchor anchor = TextAnchor.MiddleLeft, Color? color = null)
        {
            var oldFont = Text.Font;
            var oldAnchor = Text.Anchor;
            var oldColor = GUI.color;

            Text.Font = font;
            Text.Anchor = anchor;
            if (color.HasValue) GUI.color = color.Value;

            Verse.Widgets.Label(rect, text);

            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
            GUI.color = oldColor;
        }

        /// <summary>
        /// Helper method for drawing buttons with consistent styling
        /// </summary>
        protected static bool DrawStyledButton(Rect rect, string label, bool enabled = true)
        {
            var oldColor = GUI.color;
            if (!enabled) GUI.color = Color.gray;

            bool result = enabled && Verse.Widgets.ButtonText(rect, label);

            GUI.color = oldColor;
            return result;
        }
    }
}