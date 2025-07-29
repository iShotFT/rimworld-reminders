using UnityEngine;

namespace AdvancedReminders.Presentation.UI.Core
{
    /// <summary>
    /// Core interface for all widgets in the RimHUD-inspired architecture.
    /// Provides the foundation for content-aware, responsive UI components.
    /// </summary>
    public interface IWidget
    {
        /// <summary>
        /// Calculates the maximum height this widget needs given the available width.
        /// This enables responsive design where widgets can adapt to different screen sizes.
        /// </summary>
        /// <param name="availableWidth">The width available for this widget</param>
        /// <returns>The maximum height needed, or -1 if the widget wants all available height</returns>
        float GetMaxHeight(float availableWidth);

        /// <summary>
        /// Draws the widget within the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to draw within</param>
        /// <returns>True if the widget was drawn successfully, false otherwise</returns>
        bool Draw(Rect rect);

        /// <summary>
        /// Whether this widget should be visible and processed.
        /// Widgets can use this to hide themselves based on game state or user preferences.
        /// </summary>
        bool IsVisible { get; }
    }
}