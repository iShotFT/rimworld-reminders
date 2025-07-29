using UnityEngine;

namespace AdvancedReminders.Presentation.UI.Core
{
    /// <summary>
    /// Performance-optimized base widget with dirty tracking and conditional rendering.
    /// Follows RimHUD patterns for minimal allocations and smart update cycles.
    /// </summary>
    public abstract class CachedWidget : WidgetBase
    {
        #region Fields

        private bool _isDirty = true;
        private float _lastWidth = -1f;
        private float _lastHeight = -1f;
        private int _lastFrame = -1;

        // Data binding fields
        private object _boundData;
        private int _boundDataHash;

        #endregion

        #region Dirty Tracking

        /// <summary>
        /// Whether this widget needs to be redrawn
        /// </summary>
        public bool IsDirty => _isDirty;

        /// <summary>
        /// Marks the widget as dirty, forcing a redraw on next frame
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Marks the widget as clean (up to date)
        /// </summary>
        protected void MarkClean()
        {
            _isDirty = false;
            _lastFrame = Time.frameCount;
        }

        #endregion

        #region Conditional Rendering

        /// <summary>
        /// Override to provide custom dirty checking logic
        /// </summary>
        protected virtual bool ShouldRedraw(Rect rect)
        {
            // Redraw if explicitly marked dirty
            if (_isDirty) return true;

            // Redraw if size changed
            if (!Mathf.Approximately(rect.width, _lastWidth) || 
                !Mathf.Approximately(rect.height, _lastHeight))
                return true;

            // Redraw if data might have changed (override in derived classes)
            if (HasDataChanged()) return true;

            return false;
        }

        /// <summary>
        /// Override to provide custom data change detection
        /// </summary>
        protected virtual bool HasDataChanged()
        {
            // Check bound data by default, override for custom logic
            return _boundData != null && BoundDataChanged();
        }


        #endregion

        #region Abstract Methods

        /// <summary>
        /// Implementation of WidgetBase.DrawInternal - bridges to the caching system
        /// </summary>
        protected override bool DrawInternal(Rect rect)
        {
            // Check if we need to redraw
            bool shouldRedraw = ShouldRedraw(rect);
            
            if (shouldRedraw)
            {
                // Update tracking variables
                _lastWidth = rect.width;
                _lastHeight = rect.height;

                // Perform the actual drawing
                bool result = DrawCached(rect);
                
                // Mark as clean after successful draw
                if (result)
                {
                    MarkClean();
                }
                
                return result;
            }
            else
            {
                // Use cached rendering if available
                return DrawFromCache(rect);
            }
        }

        /// <summary>
        /// Draws the widget and caches the result. Called when redraw is needed.
        /// </summary>
        protected abstract bool DrawCached(Rect rect);

        /// <summary>
        /// Draws from cache when no redraw is needed. Default implementation calls DrawCached.
        /// Override to provide true caching (render to texture, etc.)
        /// </summary>
        protected virtual bool DrawFromCache(Rect rect)
        {
            return DrawCached(rect);
        }

        #endregion

        #region Performance Helpers

        /// <summary>
        /// Whether this widget was drawn in the current frame
        /// </summary>
        protected bool DrawnThisFrame => _lastFrame == Time.frameCount;

        /// <summary>
        /// How many frames since last draw
        /// </summary>
        protected int FramesSinceLastDraw => Time.frameCount - _lastFrame;

        /// <summary>
        /// Whether enough time has passed to consider redrawing
        /// </summary>
        protected bool ShouldUpdateThisFrame(int frameInterval = 1)
        {
            return FramesSinceLastDraw >= frameInterval;
        }

        #endregion

        #region Data Binding Support

        /// <summary>
        /// Binds data to this widget for automatic dirty detection
        /// </summary>
        protected void BindData(object data)
        {
            _boundData = data;
            _boundDataHash = data?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Checks if bound data has changed
        /// </summary>
        protected bool BoundDataChanged()
        {
            if (_boundData == null) return false;
            
            int currentHash = _boundData.GetHashCode();
            if (currentHash != _boundDataHash)
            {
                _boundDataHash = currentHash;
                return true;
            }
            
            return false;
        }


        #endregion

        #region Lifecycle

        /// <summary>
        /// Called when the widget is being destroyed or removed
        /// </summary>
        public virtual void OnDestroy()
        {
            _boundData = null;
        }

        #endregion
    }

    /// <summary>
    /// Generic cached widget with strongly-typed data binding
    /// </summary>
    public abstract class CachedWidget<T> : CachedWidget where T : class
    {
        private T _data;

        /// <summary>
        /// The data bound to this widget
        /// </summary>
        public T Data
        {
            get => _data;
            set
            {
                if (!ReferenceEquals(_data, value))
                {
                    _data = value;
                    BindData(value);
                    MarkDirty();
                }
            }
        }

        /// <summary>
        /// Constructor with initial data
        /// </summary>
        protected CachedWidget(T data = null)
        {
            Data = data;
        }

        /// <summary>
        /// Override to provide data-specific drawing
        /// </summary>
        protected abstract bool DrawWithData(Rect rect, T data);

        /// <summary>
        /// Implementation of DrawCached using strongly-typed data
        /// </summary>
        protected override bool DrawCached(Rect rect)
        {
            return DrawWithData(rect, _data);
        }
    }

    /// <summary>
    /// Cached widget specifically for reminder data with common optimizations
    /// </summary>
    public abstract class CachedReminderWidget : CachedWidget<ViewModels.ReminderViewModel>
    {
        protected CachedReminderWidget(ViewModels.ReminderViewModel viewModel = null) : base(viewModel)
        {
        }

        /// <summary>
        /// Enhanced data change detection for reminder view models
        /// </summary>
        protected override bool HasDataChanged()
        {
            if (Data?.Model == null) return false;

            // Check if the underlying reminder model has changed
            // This could be enhanced with more specific change tracking
            return base.HasDataChanged();
        }

        /// <summary>
        /// Convenience method for drawing with null checks
        /// </summary>
        protected override bool DrawWithData(Rect rect, ViewModels.ReminderViewModel data)
        {
            if (data?.Model == null) return false;
            return DrawReminder(rect, data);
        }

        /// <summary>
        /// Override to draw the reminder with the view model
        /// </summary>
        protected abstract bool DrawReminder(Rect rect, ViewModels.ReminderViewModel viewModel);
    }
}