# V2 Plan of Attack: RimHUD-Inspired Architecture Modernization

## Executive Summary

This plan outlines the complete architectural modernization of the Advanced Reminders mod, transitioning from our current functional but architecturally weak codebase to a professional, maintainable system following RimHUD's proven patterns.

**Goal**: Transform our 2000+ line codebase into a clean, performant, widget-based architecture that exemplifies RimWorld modding best practices.

**Timeline**: 8-10 development sessions
**Priority**: High - Foundation for all future features

---

## Current Architecture Problems

### 🚨 Critical Issues
- **Monolithic Components**: 600+ line UI classes violating single responsibility
- **Tight Coupling**: Components directly depend on each other's internals  
- **Hardcoded Layout**: Fixed dimensions prevent responsive design
- **Scattered State**: No centralized state management or cache invalidation
- **Manual Calculations**: Brittle height/width calculations throughout
- **No Widget System**: Each component reinvents basic UI patterns
- **Performance Issues**: No early exits, memory optimization, or conditional rendering

### 📊 Technical Debt Metrics
- **MainTabWindow_Reminders**: 646 lines (should be ~100)
- **ReminderFormComponent**: 580 lines (should be ~50 with widgets)
- **Dialog Classes**: Duplicate 70% of form logic
- **Layout Code**: 15+ hardcoded dimension constants
- **State Updates**: 8 different manual refresh patterns

---

## Phase 1: Widget Foundation System (Sessions 1-3)

### 1.1 Core Widget Architecture

**Create widget foundation following RimHUD patterns:**

```csharp
// Core widget interface
public interface IWidget
{
    float GetMaxHeight(float availableWidth);
    bool Draw(Rect rect);
    bool IsVisible { get; }
}

// Base widget with common functionality
public abstract class WidgetBase : IWidget
{
    protected static readonly ObjectPool<StringBuilder> StringBuilderPool;
    protected Theme Theme => ReminderTheme.Current;
    
    public abstract float GetMaxHeight(float availableWidth);
    public abstract bool Draw(Rect rect);
    public virtual bool IsVisible => true;
}

// Layout container for composition
public class LayoutContainer : WidgetBase
{
    public List<IWidget> Children { get; } = new();
    public LayoutType Layout { get; set; } = LayoutType.Vertical;
    public float Spacing { get; set; } = 4f;
}
```

**Files to Create:**
- `Presentation/Widgets/Core/IWidget.cs`
- `Presentation/Widgets/Core/WidgetBase.cs`
- `Presentation/Widgets/Layout/LayoutContainer.cs`
- `Presentation/Widgets/Layout/ResponsiveGrid.cs`

### 1.2 Specialized Reminder Widgets

**Extract reusable components:**

```csharp
public class ReminderItemWidget : WidgetBase
{
    private readonly ReminderModel _model;
    private readonly SeverityIndicatorWidget _severityWidget;
    private readonly CountdownWidget _countdownWidget;
    private readonly ActionButtonGroupWidget _actionsWidget;
    
    // Clean, focused responsibility
}

public class SeverityIndicatorWidget : WidgetBase
{
    private readonly SeverityLevel _severity;
    // Reusable severity display with consistent styling
}

public class CountdownWidget : WidgetBase
{
    private readonly int _targetTick;
    // Smart time remaining display with color coding
}
```

**Files to Create:**
- `Presentation/Widgets/Reminders/ReminderItemWidget.cs`
- `Presentation/Widgets/Reminders/SeverityIndicatorWidget.cs` 
- `Presentation/Widgets/Reminders/CountdownWidget.cs`
- `Presentation/Widgets/Reminders/ActionButtonGroupWidget.cs`

### 1.3 Form Input Widgets

**Replace manual form logic:**

```csharp
public class TimeInputWidget : WidgetBase
{
    public int Value { get; set; }
    public TimeUnit Unit { get; set; }
    public event Action<int, TimeUnit> OnValueChanged;
    
    // Handles input validation, unit conversion, visual feedback
}

public class QuestSelectorWidget : WidgetBase
{
    private readonly List<Quest> _availableQuests;
    public Quest SelectedQuest { get; set; }
    
    // Searchable dropdown with quest filtering
}

public class SeveritySliderWidget : WidgetBase
{
    public SeverityLevel Value { get; set; }
    // Visual severity selection with color preview
}
```

**Files to Create:**
- `Presentation/Widgets/Forms/TimeInputWidget.cs`
- `Presentation/Widgets/Forms/QuestSelectorWidget.cs`
- `Presentation/Widgets/Forms/SeveritySliderWidget.cs`

---

## Phase 2: Responsive Layout System (Sessions 4-5)

### 2.1 Dynamic Layout Engine

**Replace hardcoded dimensions:**

```csharp
public static class LayoutEngine
{
    public static Rect[] CalculateGrid(Rect container, GridDefinition definition)
    {
        // Dynamic grid calculation based on content and available space
    }
    
    public static float CalculateOptimalHeight(IEnumerable<IWidget> widgets, float width)
    {
        // Content-aware height calculation
    }
    
    public static Rect[] StackVertical(Rect container, params float[] weights)
    {
        // Responsive vertical stacking
    }
}

public class ResponsivePanel : WidgetBase
{
    public float MinWidth { get; set; } = 300f;
    public float MaxWidth { get; set; } = 1200f;
    public bool AllowHorizontalSplit { get; set; } = true;
    
    // Automatically adapts layout based on available space
}
```

### 2.2 Content-Aware Dialog System

**Replace fixed-size dialogs:**

```csharp
public abstract class ResponsiveDialog : Window
{
    protected abstract IWidget CreateContent();
    
    public override Vector2 InitialSize
    {
        get
        {
            var content = CreateContent();
            var optimalSize = content.GetMaxHeight(UI.screenWidth * 0.8f);
            return new Vector2(
                Mathf.Clamp(optimalSize, MinWidth, MaxWidth),
                Mathf.Clamp(optimalSize, MinHeight, MaxHeight)
            );
        }
    }
}

public class CreateReminderDialog : ResponsiveDialog
{
    protected override IWidget CreateContent()
    {
        return new LayoutContainer
        {
            Children =
            {
                new TitleWidget("Create Reminder"),
                new ReminderFormWidget(_formModel),
                new ButtonGroupWidget(CreateButtons())
            }
        };
    }
}
```

### 2.3 Calendar Modernization

**Apply responsive design to calendar:**

```csharp
public class ResponsiveCalendarWidget : WidgetBase
{
    private readonly CalendarModel _model;
    
    public override float GetMaxHeight(float availableWidth)
    {
        var cellSize = CalculateOptimalCellSize(availableWidth);
        return (cellSize * 4) + (YearHeaderHeight * 3) + NavigationHeight;
    }
    
    private float CalculateOptimalCellSize(float availableWidth)
    {
        var maxCellSize = 24f;
        var minCellSize = 12f;
        var availableForGrid = availableWidth - (Margins * 2);
        var calculatedSize = availableForGrid / 15f; // 15 days per row
        
        return Mathf.Clamp(calculatedSize, minCellSize, maxCellSize);
    }
}
```

---

## Phase 3: Centralized State Management (Sessions 6-7)

### 3.1 Reactive State System

**Replace scattered state updates:**

```csharp
public static class ReminderState
{
    private static readonly List<Reminder> _reminders = new();
    private static readonly Dictionary<string, object> _cache = new();
    
    public static event Action OnRemindersChanged;
    public static event Action<Reminder> OnReminderAdded;
    public static event Action<Reminder> OnReminderRemoved;
    
    public static IReadOnlyList<Reminder> AllReminders => _reminders.AsReadOnly();
    
    public static void AddReminder(Reminder reminder)
    {
        _reminders.Add(reminder);
        InvalidateCache();
        OnReminderAdded?.Invoke(reminder);
        OnRemindersChanged?.Invoke();
    }
    
    public static void InvalidateCache()
    {
        _cache.Clear();
        // Smart cache invalidation
    }
    
    public static T GetCached<T>(string key, Func<T> factory)
    {
        if (!_cache.ContainsKey(key))
            _cache[key] = factory();
        return (T)_cache[key];
    }
}
```

### 3.2 Model-View Architecture

**Separate data from presentation:**

```csharp
public class ReminderModel
{
    public string Id { get; }
    public string Title { get; set; }
    public string Description { get; set; }
    public SeverityLevel Severity { get; set; }
    public ITrigger Trigger { get; set; }
    public List<IReminderAction> Actions { get; }
    
    // Pure data model, no UI concerns
}

public class ReminderViewModel
{
    private readonly ReminderModel _model;
    
    public string DisplayTitle => _model.Title;
    public string StatusText => CalculateStatusText();
    public Color StatusColor => GetStatusColor();
    public bool IsUrgent => CalculateUrgency();
    
    // UI-specific computed properties
}

public class ReminderListViewModel
{
    public IReadOnlyList<ReminderViewModel> FilteredReminders { get; }
    public int ActiveCount { get; }
    public int UpcomingCount { get; }
    public SortMode CurrentSort { get; set; }
    
    // List-level view logic
}
```

### 3.3 Performance Optimizations

**Add RimHUD-style optimizations:**

```csharp
public abstract class CachedWidget : WidgetBase
{
    private float _lastHeight = -1f;
    private float _lastWidth = -1f;
    private bool _isDirty = true;
    
    public override bool Draw(Rect rect)
    {
        // Early exit patterns
        if (!IsVisible) return false;
        if (rect.height < 1f) return false;
        
        // Conditional rendering based on changes
        if (!_isDirty && rect.width == _lastWidth) 
        {
            return DrawCached(rect);
        }
        
        _isDirty = false;
        _lastWidth = rect.width;
        _lastHeight = rect.height;
        
        return DrawInternal(rect);
    }
    
    protected abstract bool DrawInternal(Rect rect);
    protected virtual bool DrawCached(Rect rect) => DrawInternal(rect);
    
    protected void MarkDirty() => _isDirty = true;
}
```

---

## Phase 4: Theme System & Polish (Session 8)

### 4.1 Centralized Theme System

```csharp
public static class ReminderTheme
{
    public static readonly Color SeverityLow = new Color(0.4f, 0.8f, 0.4f);
    public static readonly Color SeverityMedium = new Color(0.8f, 0.8f, 0.4f);
    public static readonly Color SeverityHigh = new Color(0.8f, 0.6f, 0.4f);
    public static readonly Color SeverityCritical = new Color(0.8f, 0.4f, 0.4f);
    
    public static readonly float StandardPadding = 6f;
    public static readonly float SmallPadding = 3f;
    public static readonly float ButtonHeight = 24f;
    
    public static Color GetSeverityColor(SeverityLevel level)
    {
        return level switch
        {
            SeverityLevel.Low => SeverityLow,
            SeverityLevel.Medium => SeverityMedium,
            SeverityLevel.High => SeverityHigh,
            SeverityLevel.Critical => SeverityCritical,
            _ => Color.white
        };
    }
    
    public static GUIStyle GetButtonStyle(bool isEnabled = true)
    {
        // Consistent button styling
    }
}
```

### 4.2 Component Integration

**Replace existing components with widget versions:**

```csharp
public class ModernMainTabWindow : MainTabWindow
{
    private readonly ReminderListViewModel _viewModel;
    private readonly LayoutContainer _rootLayout;
    
    public override void DoWindowContents(Rect inRect)
    {
        _rootLayout.Draw(inRect);
    }
    
    private void InitializeLayout()
    {
        _rootLayout = new LayoutContainer
        {
            Layout = LayoutType.Horizontal,
            Children =
            {
                new ReminderListWidget(_viewModel) { Weight = 0.6f },
                new CalendarWidget(CalendarState.Current) { Weight = 0.4f }
            }
        };
    }
}
```

---

## Migration Strategy

### Session-by-Session Breakdown

**Sessions 1-2: Widget Foundation**
- Create `IWidget` interface and base classes
- Extract `ReminderItemWidget` from existing code
- Create `SeverityIndicatorWidget` and `CountdownWidget`
- Test widget composition basics

**Session 3: Form Widgets**
- Create `TimeInputWidget`, `QuestSelectorWidget`, `SeveritySliderWidget`
- Replace form logic in `ReminderFormComponent`
- Test input validation and events

**Sessions 4-5: Layout System**
- Implement responsive grid system
- Create `ResponsiveDialog` base class
- Modernize calendar with dynamic sizing
- Replace hardcoded dimensions throughout

**Sessions 6-7: State Management**
- Implement `ReminderState` centralized system
- Create Model-View separation
- Add performance optimizations (caching, early exits)
- Migrate existing state management

**Session 8: Integration & Polish**  
- Create centralized theme system
- Replace remaining legacy components
- Performance testing and optimization
- Code cleanup and documentation

### Backwards Compatibility

**During Migration:**
- Keep existing classes alongside new ones
- Gradually migrate UI components one at a time
- Maintain save game compatibility
- Preserve all existing functionality

**Testing Strategy:**
- Unit tests for widget system
- Integration tests for state management
- Manual testing of all UI workflows
- Performance benchmarking

### Risk Mitigation

**Technical Risks:**
- **Widget system complexity** → Start with simple widgets, iterate
- **Performance regressions** → Benchmark existing vs new code
- **State synchronization bugs** → Thorough testing of cache invalidation

**Timeline Risks:**
- **Scope creep** → Focus on architecture first, features later  
- **Integration complexity** → Migrate incrementally, not all at once

---

## Expected Outcomes

### Code Quality Improvements
- **70% reduction** in UI code duplication
- **50% fewer lines** in main UI classes
- **Zero hardcoded dimensions** - all responsive
- **Centralized state management** - no scattered updates

### Performance Gains
- **Early exit patterns** - skip invisible/unchanged widgets
- **Object pooling** - reduce allocations by 60%+
- **Smart caching** - avoid redundant calculations
- **Conditional rendering** - only draw what changed

### Developer Experience
- **Widget reusability** - components work across dialogs
- **Consistent styling** - centralized theme system
- **Easier testing** - isolated, mockable components
- **Clear architecture** - obvious where to add features

### User Experience  
- **Responsive UI** - adapts to different screen sizes
- **Consistent visual language** - professional appearance
- **Better performance** - smoother interactions
- **Enhanced accessibility** - proper focus management

---

## Success Metrics

### Before (Current State)
- MainTabWindow_Reminders: 646 lines
- Total UI code: ~2000 lines
- Hardcoded constants: 15+
- State update locations: 8
- Widget reuse: 0%

### After (Target State)
- MainTabWindow_Reminders: ~100 lines
- Total UI code: ~1200 lines (40% reduction)
- Hardcoded constants: 0
- State update locations: 1 (centralized)
- Widget reuse: 80%+

### Quality Gates
- [ ] All UI components use widget system
- [ ] Zero hardcoded dimensions
- [ ] Single source of truth for state
- [ ] 90%+ test coverage for widgets
- [ ] Performance matches or exceeds current
- [ ] All existing features preserved

This modernization will transform Advanced Reminders from a functional mod into a showcase of professional RimWorld development practices, providing a solid foundation for all future features and improvements.