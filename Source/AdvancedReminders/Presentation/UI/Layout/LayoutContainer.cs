using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdvancedReminders.Presentation.UI.Core;

namespace AdvancedReminders.Presentation.UI.Layout
{
    /// <summary>
    /// Layout types for container arrangement
    /// </summary>
    public enum LayoutType
    {
        Vertical,
        Horizontal,
        Grid
    }

    /// <summary>
    /// Container widget that manages the layout of child widgets.
    /// Provides vertical, horizontal, and grid layout capabilities.
    /// </summary>
    public class LayoutContainer : WidgetBase
    {
        private readonly LayoutType _layoutType;
        private readonly List<IWidget> _children;
        private float _spacing;

        public LayoutContainer(LayoutType layoutType = LayoutType.Vertical)
        {
            _layoutType = layoutType;
            _children = new List<IWidget>();
            _spacing = 6f; // Default spacing
        }

        /// <summary>
        /// Spacing between child widgets
        /// </summary>
        public float Spacing
        {
            get => _spacing;
            set => _spacing = value;
        }

        /// <summary>
        /// Child widgets in this container
        /// </summary>
        public List<IWidget> Children => _children;

        /// <summary>
        /// Creates a horizontal layout container
        /// </summary>
        public static LayoutContainer Horizontal(params IWidget[] children)
        {
            var container = new LayoutContainer(LayoutType.Horizontal);
            foreach (var child in children)
            {
                if (child != null)
                    container.Children.Add(child);
            }
            return container;
        }

        /// <summary>
        /// Creates a vertical layout container
        /// </summary>
        public static LayoutContainer Vertical(params IWidget[] children)
        {
            var container = new LayoutContainer(LayoutType.Vertical);
            foreach (var child in children)
            {
                if (child != null)
                    container.Children.Add(child);
            }
            return container;
        }

        public override float GetMaxHeight(float availableWidth)
        {
            if (!_children.Any()) return 0f;

            switch (_layoutType)
            {
                case LayoutType.Vertical:
                    return CalculateVerticalHeight(availableWidth);
                
                case LayoutType.Horizontal:
                    return CalculateHorizontalHeight(availableWidth);
                
                case LayoutType.Grid:
                    return CalculateGridHeight(availableWidth);
                
                default:
                    return 0f;
            }
        }

        protected override bool DrawInternal(Rect rect)
        {
            if (!_children.Any()) return true;

            switch (_layoutType)
            {
                case LayoutType.Vertical:
                    return DrawVertical(rect);
                
                case LayoutType.Horizontal:
                    return DrawHorizontal(rect);
                
                case LayoutType.Grid:
                    return DrawGrid(rect);
                
                default:
                    return false;
            }
        }

        #region Layout Calculations

        private float CalculateVerticalHeight(float availableWidth)
        {
            float totalHeight = 0f;
            foreach (var child in _children.Where(c => c.IsVisible))
            {
                var childHeight = child.GetMaxHeight(availableWidth);
                if (childHeight < 0f) return -1f; // Child wants all available height
                totalHeight += childHeight;
            }
            
            // Add spacing between children
            var visibleChildren = _children.Count(c => c.IsVisible);
            if (visibleChildren > 1)
                totalHeight += _spacing * (visibleChildren - 1);
            
            return totalHeight;
        }

        private float CalculateHorizontalHeight(float availableWidth)
        {
            var visibleChildren = _children.Where(c => c.IsVisible).ToList();
            if (!visibleChildren.Any()) return 0f;

            // For horizontal layout, height is the maximum height of any child
            var childWidth = (availableWidth - _spacing * (visibleChildren.Count - 1)) / visibleChildren.Count;
            return visibleChildren.Max(c => c.GetMaxHeight(childWidth));
        }

        private float CalculateGridHeight(float availableWidth)
        {
            // Simple grid implementation - can be enhanced later
            var visibleChildren = _children.Where(c => c.IsVisible).ToList();
            if (!visibleChildren.Any()) return 0f;

            var itemsPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / 200f)); // Assume 200px per item
            var rows = Mathf.CeilToInt((float)visibleChildren.Count / itemsPerRow);
            var itemWidth = (availableWidth - _spacing * (itemsPerRow - 1)) / itemsPerRow;
            var itemHeight = visibleChildren.Max(c => c.GetMaxHeight(itemWidth));
            
            return itemHeight * rows + _spacing * (rows - 1);
        }

        #endregion

        #region Drawing Methods

        private bool DrawVertical(Rect rect)
        {
            float currentY = rect.y;
            
            foreach (var child in _children.Where(c => c.IsVisible))
            {
                var childHeight = child.GetMaxHeight(rect.width);
                if (childHeight < 0f) childHeight = rect.height - (currentY - rect.y);
                
                var childRect = new Rect(rect.x, currentY, rect.width, childHeight);
                child.Draw(childRect);
                
                currentY += childHeight + _spacing;
                
                // Stop if we've run out of space
                if (currentY >= rect.yMax) break;
            }
            
            return true;
        }

        private bool DrawHorizontal(Rect rect)
        {
            var visibleChildren = _children.Where(c => c.IsVisible).ToList();
            if (!visibleChildren.Any()) return true;

            var childWidth = (rect.width - _spacing * (visibleChildren.Count - 1)) / visibleChildren.Count;
            float currentX = rect.x;
            
            foreach (var child in visibleChildren)
            {
                var childRect = new Rect(currentX, rect.y, childWidth, rect.height);
                child.Draw(childRect);
                
                currentX += childWidth + _spacing;
            }
            
            return true;
        }

        private bool DrawGrid(Rect rect)
        {
            var visibleChildren = _children.Where(c => c.IsVisible).ToList();
            if (!visibleChildren.Any()) return true;

            var itemsPerRow = Mathf.Max(1, Mathf.FloorToInt(rect.width / 200f));
            var itemWidth = (rect.width - _spacing * (itemsPerRow - 1)) / itemsPerRow;
            var itemHeight = visibleChildren.Max(c => c.GetMaxHeight(itemWidth));
            
            int index = 0;
            foreach (var child in visibleChildren)
            {
                int row = index / itemsPerRow;
                int col = index % itemsPerRow;
                
                var x = rect.x + col * (itemWidth + _spacing);
                var y = rect.y + row * (itemHeight + _spacing);
                
                var childRect = new Rect(x, y, itemWidth, itemHeight);
                child.Draw(childRect);
                
                index++;
            }
            
            return true;
        }

        #endregion
    }
}