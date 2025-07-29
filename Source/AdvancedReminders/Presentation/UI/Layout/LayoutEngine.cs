using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using AdvancedReminders.Presentation.UI.Core;

namespace AdvancedReminders.Presentation.UI.Layout
{
    /// <summary>
    /// Grid definition for responsive layout calculations
    /// </summary>
    public class GridDefinition
    {
        public int Columns { get; set; }
        public int Rows { get; set; }
        public float[] ColumnWidths { get; set; } // Negative values = proportional
        public float[] RowHeights { get; set; }   // Negative values = proportional
        public float ColumnSpacing { get; set; } = 4f;
        public float RowSpacing { get; set; } = 4f;

        public GridDefinition(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            ColumnWidths = new float[columns];
            RowHeights = new float[rows];
        }

        /// <summary>
        /// Creates a grid with equal-width columns
        /// </summary>
        public static GridDefinition EqualColumns(int columns, int rows = 1)
        {
            var grid = new GridDefinition(columns, rows);
            for (int i = 0; i < columns; i++)
                grid.ColumnWidths[i] = -1f; // Proportional
            for (int i = 0; i < rows; i++)
                grid.RowHeights[i] = -1f; // Proportional
            return grid;
        }

        /// <summary>
        /// Creates a grid with specific column ratios (e.g., 2:1:3 ratio)
        /// </summary>
        public static GridDefinition ProportionalColumns(params float[] ratios)
        {
            var grid = new GridDefinition(ratios.Length, 1);
            for (int i = 0; i < ratios.Length; i++)
                grid.ColumnWidths[i] = -ratios[i]; // Negative = proportional
            grid.RowHeights[0] = -1f;
            return grid;
        }
    }

    /// <summary>
    /// Advanced layout engine following RimHUD patterns for responsive UI design.
    /// Provides dynamic grid calculations, content-aware sizing, and flexible layout options.
    /// </summary>
    public static class LayoutEngine
    {
        /// <summary>
        /// Calculates a responsive grid layout based on container size and definition
        /// </summary>
        public static Rect[] CalculateGrid(Rect container, GridDefinition definition)
        {
            if (definition == null || definition.Columns <= 0 || definition.Rows <= 0)
                return new Rect[0];

            var totalCells = definition.Columns * definition.Rows;
            var result = new Rect[totalCells];

            // Calculate column positions and widths
            var columnRects = CalculateColumnLayout(container, definition);
            var rowRects = CalculateRowLayout(container, definition);

            // Create grid cells
            int cellIndex = 0;
            for (int row = 0; row < definition.Rows; row++)
            {
                for (int col = 0; col < definition.Columns; col++)
                {
                    result[cellIndex] = new Rect(
                        columnRects[col].x,
                        rowRects[row].y,
                        columnRects[col].width,
                        rowRects[row].height
                    );
                    cellIndex++;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates optimal height for a collection of widgets given available width
        /// </summary>
        public static float CalculateOptimalHeight(IEnumerable<IWidget> widgets, float availableWidth)
        {
            if (widgets == null) return 0f;

            var widgetList = widgets.Where(w => w.IsVisible).ToList();
            if (!widgetList.Any()) return 0f;

            return widgetList.Sum(widget => widget.GetMaxHeight(availableWidth));
        }

        /// <summary>
        /// Creates vertical stack layout with optional proportional weights
        /// </summary>
        public static Rect[] StackVertical(Rect container, float spacing = 4f, params float[] weights)
        {
            if (weights == null || weights.Length == 0)
                return new Rect[0];

            var result = new Rect[weights.Length];
            var totalSpacing = spacing * (weights.Length - 1);
            var availableHeight = container.height - totalSpacing;

            // Calculate total weight and proportional heights
            var totalWeight = weights.Sum(w => w > 0 ? w : 1f);
            var proportionalHeight = availableHeight / totalWeight;

            float currentY = container.y;
            for (int i = 0; i < weights.Length; i++)
            {
                var weight = weights[i] > 0 ? weights[i] : 1f;
                var height = proportionalHeight * weight;

                result[i] = new Rect(container.x, currentY, container.width, height);
                currentY += height + spacing;
            }

            return result;
        }

        /// <summary>
        /// Creates horizontal stack layout with optional proportional weights
        /// </summary>
        public static Rect[] StackHorizontal(Rect container, float spacing = 4f, params float[] weights)
        {
            if (weights == null || weights.Length == 0)
                return new Rect[0];

            var result = new Rect[weights.Length];
            var totalSpacing = spacing * (weights.Length - 1);
            var availableWidth = container.width - totalSpacing;

            // Calculate total weight and proportional widths
            var totalWeight = weights.Sum(w => w > 0 ? w : 1f);
            var proportionalWidth = availableWidth / totalWeight;

            float currentX = container.x;
            for (int i = 0; i < weights.Length; i++)
            {
                var weight = weights[i] > 0 ? weights[i] : 1f;
                var width = proportionalWidth * weight;

                result[i] = new Rect(currentX, container.y, width, container.height);
                currentX += width + spacing;
            }

            return result;
        }

        /// <summary>
        /// Creates a responsive two-column layout (label + content pattern)
        /// </summary>
        public static (Rect labelRect, Rect contentRect) TwoColumnLayout(Rect container, 
            float labelWidth = -1f, float spacing = 6f)
        {
            // Auto-calculate label width if not specified
            if (labelWidth < 0)
                labelWidth = container.width * 0.3f; // 30% for label by default

            var contentWidth = container.width - labelWidth - spacing;
            
            var labelRect = new Rect(container.x, container.y, labelWidth, container.height);
            var contentRect = new Rect(container.x + labelWidth + spacing, container.y, 
                contentWidth, container.height);

            return (labelRect, contentRect);
        }

        /// <summary>
        /// Calculates responsive dialog size based on content and screen constraints
        /// </summary>
        public static Vector2 CalculateDialogSize(IWidget content, float minWidth = 300f, 
            float maxWidth = 1200f, float minHeight = 200f, float maxHeight = 800f)
        {
            if (content == null)
                return new Vector2(minWidth, minHeight);

            // Use screen dimensions as basis
            var screenWidth = UnityEngine.Screen.width * 0.8f; // 80% of screen width max
            var availableWidth = Mathf.Clamp(screenWidth, minWidth, maxWidth);
            
            // Calculate optimal height for content
            var optimalHeight = content.GetMaxHeight(availableWidth);
            var finalHeight = Mathf.Clamp(optimalHeight, minHeight, maxHeight);

            return new Vector2(availableWidth, finalHeight);
        }

        /// <summary>
        /// Creates a content-aware scrollable area layout
        /// </summary>
        public static (Rect viewRect, Rect scrollRect) CalculateScrollLayout(Rect container, 
            float contentHeight, float marginSize = 16f)
        {
            var viewRect = new Rect(0f, 0f, container.width - marginSize, 
                Mathf.Max(contentHeight, container.height));
                
            var scrollRect = container;
            
            return (viewRect, scrollRect);
        }

        #region Private Helper Methods

        /// <summary>
        /// Calculates column layout rectangles
        /// </summary>
        private static Rect[] CalculateColumnLayout(Rect container, GridDefinition definition)
        {
            var columns = new Rect[definition.Columns];
            var totalSpacing = definition.ColumnSpacing * (definition.Columns - 1);
            var availableWidth = container.width - totalSpacing;

            // Calculate total proportional weight and fixed width
            float totalProportionalWeight = 0f;
            float totalFixedWidth = 0f;

            for (int i = 0; i < definition.Columns; i++)
            {
                if (definition.ColumnWidths[i] < 0)
                    totalProportionalWeight += -definition.ColumnWidths[i];
                else
                    totalFixedWidth += definition.ColumnWidths[i];
            }

            var remainingWidth = availableWidth - totalFixedWidth;
            var proportionalUnit = totalProportionalWeight > 0 ? remainingWidth / totalProportionalWeight : 0f;

            // Create column rectangles
            float currentX = container.x;
            for (int i = 0; i < definition.Columns; i++)
            {
                float width;
                if (definition.ColumnWidths[i] < 0)
                    width = proportionalUnit * (-definition.ColumnWidths[i]);
                else
                    width = definition.ColumnWidths[i];

                columns[i] = new Rect(currentX, container.y, width, container.height);
                currentX += width + definition.ColumnSpacing;
            }

            return columns;
        }

        /// <summary>
        /// Calculates row layout rectangles
        /// </summary>
        private static Rect[] CalculateRowLayout(Rect container, GridDefinition definition)
        {
            var rows = new Rect[definition.Rows];
            var totalSpacing = definition.RowSpacing * (definition.Rows - 1);
            var availableHeight = container.height - totalSpacing;

            // Calculate total proportional weight and fixed height
            float totalProportionalWeight = 0f;
            float totalFixedHeight = 0f;

            for (int i = 0; i < definition.Rows; i++)
            {
                if (definition.RowHeights[i] < 0)
                    totalProportionalWeight += -definition.RowHeights[i];
                else
                    totalFixedHeight += definition.RowHeights[i];
            }

            var remainingHeight = availableHeight - totalFixedHeight;
            var proportionalUnit = totalProportionalWeight > 0 ? remainingHeight / totalProportionalWeight : 0f;

            // Create row rectangles
            float currentY = container.y;
            for (int i = 0; i < definition.Rows; i++)
            {
                float height;
                if (definition.RowHeights[i] < 0)
                    height = proportionalUnit * (-definition.RowHeights[i]);
                else
                    height = definition.RowHeights[i];

                rows[i] = new Rect(container.x, currentY, container.width, height);
                currentY += height + definition.RowSpacing;
            }

            return rows;
        }

        #endregion

        #region Extension Methods for Common Patterns

        /// <summary>
        /// Extension method for easy form-style layout (label: input)
        /// </summary>
        public static (Rect labelRect, Rect inputRect) AsFormRow(this Rect rect, 
            float labelWidth = -1f, float spacing = 6f)
        {
            return TwoColumnLayout(rect, labelWidth, spacing);
        }

        /// <summary>
        /// Extension method for button group layouts
        /// </summary>
        public static Rect[] AsButtonGroup(this Rect rect, int buttonCount, 
            float spacing = 4f, bool rightAligned = false)
        {
            var buttonWidth = (rect.width - (spacing * (buttonCount - 1))) / buttonCount;
            var buttons = new Rect[buttonCount];

            float startX = rightAligned ? rect.xMax - (buttonWidth * buttonCount + spacing * (buttonCount - 1)) : rect.x;
            
            for (int i = 0; i < buttonCount; i++)
            {
                buttons[i] = new Rect(startX + (i * (buttonWidth + spacing)), rect.y, buttonWidth, rect.height);
            }

            return buttons;
        }

        #endregion
    }
}