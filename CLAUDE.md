# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

:warning: IMPORTANT! Always read and use the GOLDEN-RULES.md file, these rules are to be applied ALWAYS

## Project Overview

This is a RimWorld mod project for an advanced reminder and task management system. The mod integrates deeply with RimWorld's game systems to provide context-aware reminders, quest tracking, and intelligent scheduling.

## Development Commands

### Build & Development
```bash
# Build and deploy the mod (recommended for development)
make build

# Debug build with symbols for debugging
make debug

# Build locally without deployment to RimWorld
make build-local

# Clean build artifacts and deployed mod
make clean

# Restore NuGet packages
make restore

# Force redeploy (clean + build)
make redeploy

# Check RimWorld installation path
make check-rimworld

# Show all available commands
make help

# Test the mod build process
dotnet build "Source/AdvancedReminders/AdvancedReminders.csproj" --configuration Release
```

### Testing Commands
```bash
# Manual testing workflow:
# 1. Run 'make build' to build and deploy
# 2. Launch RimWorld and enable mod in mod manager  
# 3. Enable dev mode in RimWorld options
# 4. In-game hotkeys:
#    - Alt+R: Create new reminder
#    - Shift+R: Open reminders tab
#    - Ctrl+T: Create test reminder (dev mode only)

# The mod automatically deploys to:
# C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\AdvancedReminders\
```

## Available Source Code

### RimWorld Game Source
**IMPORTANT**: We have complete access to RimWorld's decompiled source code in the `_GameSource/` folder. This contains:

- **Complete Assembly-CSharp source**: All RimWorld core classes, methods, and systems
- **Game mechanics implementation**: How time, quests, notifications, UI, and all systems work
- **Existing mod integration patterns**: How other systems integrate with the game
- **UI/UX patterns**: How RimWorld implements its interface widgets and windows
- **Data structures and APIs**: Complete understanding of available classes and methods

### How to Use Game Source
When implementing features:

1. **Research existing implementations**: Before creating new functionality, check `_GameSource/` for similar existing code
2. **Follow established patterns**: Use the same coding patterns, naming conventions, and architectural approaches found in the game source
3. **Understand integration points**: Study how the game implements similar features (alerts, time management, quest system)
4. **Reference correct APIs**: Use the actual method signatures and class structures from the source
5. **Maintain compatibility**: Ensure our code follows the same patterns to avoid conflicts

### Key Source Directories to Reference
- `_GameSource/RimWorld/`: Core game systems (quests, time, alerts, UI)
- `_GameSource/Verse/`: Base framework (save/load, XML, utilities)
- `_GameSource/UnityEngine/`: Unity integration patterns
- `_GameSource/`: Root contains main game loop and system integrations

### Installation Paths
- **RimWorld Installation**: `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\`
- **Game Source Code**: `_GameSource/` (complete Assembly-CSharp decompiled source)
- **Required Dependencies**:
  - HugsLib: `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\HugsLib\`
  - Harmony: `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\HarmonyMod\Current\`

### Build Process
The build system automatically:
1. Compiles the mod against RimWorld and dependency assemblies
2. Deploys the mod to `RimWorld/Mods/AdvancedReminders/`
3. Copies all required mod files (About.xml, Languages, Defs, etc.)
4. Makes the mod immediately available in RimWorld's mod manager

### Testing
```bash
# Run unit tests (once implemented)
dotnet test

# Manual testing:
# 1. Run 'make build' to build and deploy
# 2. Launch RimWorld and enable mod in mod manager
# 3. Enable dev mode in RimWorld options
# 4. Test functionality in-game with dev tools
```

## Project Structure & File Organization

Understanding the codebase structure is critical for efficient development and avoiding code duplication. This project follows Clean Architecture principles with a modern widget-based UI system.

### Root Directory Structure
```
rimworld-reminders/
├── About/                      # Mod metadata and assets
├── Assemblies/                 # Compiled mod DLL (auto-generated)
├── Defs/                      # RimWorld definition files
├── Languages/                 # Localization files
├── Source/AdvancedReminders/  # Main source code
├── _GameSource/               # RimWorld decompiled source (reference only)
├── CLAUDE.md                  # This file - development guidance
├── GOLDEN-RULES.md           # Core development principles
├── TECH-GUIDELINES.md        # Technical architecture guidelines
├── V2-PLAN-OF-ATTACK.md      # V2 architecture transformation plan
└── Makefile                  # Build automation
```

### Source Code Organization (`Source/AdvancedReminders/`)

#### **Core Layer** (`Core/`)
*Domain models and interfaces - no external dependencies*

```
Core/
├── Enums/
│   ├── ActionType.cs          # Action types: Notification, Sound, Message, PauseGame
│   ├── SeverityLevel.cs       # Reminder severity levels: Low, Medium, High, Critical, Urgent
│   ├── TriggerType.cs         # Trigger types: Time, Quest, Resource, Event
│   └── QuestPhase.cs          # Quest phases: Preparation, Active, NearDeadline, Completed, Failed
├── Interfaces/
│   ├── IReminderAction.cs     # Contract for reminder actions
│   ├── IReminderManager.cs    # Contract for reminder management
│   └── ITrigger.cs            # Contract for reminder triggers
└── Models/
    ├── Reminder.cs            # Core reminder entity with save/load
    ├── ReminderAction.cs      # Abstract base for all actions
    └── Trigger.cs             # Abstract base for all triggers
```

#### **Domain Layer** (`Domain/`)
*Business logic implementations*

```
Domain/
├── Actions/
│   └── NotificationAction.cs  # Handles RimWorld letter notifications with severity-based styling
└── Triggers/
    ├── TimeTrigger.cs         # Time-based reminder triggers (absolute/relative)
    └── QuestDeadlineTrigger.cs # Quest deadline tracking triggers
```

#### **Application Layer** (`Application/`)
*Application services and orchestration*

```
Application/
└── Services/
    └── ReminderManager.cs     # Core reminder management service (singleton pattern)
```

#### **Infrastructure Layer** (`Infrastructure/`)
*External concerns and RimWorld integration*

```
Infrastructure/
├── Input/
│   ├── HotkeyHandler.cs       # Hotkey processing (Alt+R for create, Ctrl+T for test)
│   └── KeyBindingDefOf.cs     # Key binding definitions
├── Integration/
│   └── HarmonyPatches/
│       └── Quest_Accept_Patch.cs # Patches quest acceptance for auto-cleanup
├── Localization/
│   ├── LocalizationKeys.cs    # Centralized localization key definitions
│   └── LocalizationHelper.cs  # Translation utilities and formatting
├── Patches/
│   └── MainTabWindow_Quests_Patch.cs # Adds "Set Reminder" button to quest UI
├── Persistence/
│   └── RemindersGameComponent.cs # Save/load integration with RimWorld
└── Settings/
    └── AdvancedRemindersSettings.cs # Mod settings and configuration
```

#### **Presentation Layer** (`Presentation/`)
*UI implementation following V2 widget architecture*

```
Presentation/
├── Components/
│   └── CalendarComponent.cs   # Calendar display component (legacy - being modernized)
└── UI/                       # Modern V2 widget-based UI system
    ├── Core/                 # Widget foundation system
    │   ├── IWidget.cs        # Core widget interface (GetMaxHeight, Draw, IsVisible)
    │   ├── WidgetBase.cs     # Base widget with object pooling and helpers
    │   ├── CachedWidget.cs   # Performance-optimized widget with dirty tracking
    │   └── ResponsiveDialog.cs # Content-aware dialog base class
    ├── Layout/               # Responsive layout system
    │   ├── LayoutContainer.cs # Widget composition container (vertical/horizontal)
    │   └── LayoutEngine.cs   # Dynamic grid calculations and sizing utilities
    ├── Theme/
    │   └── ReminderTheme.cs  # Centralized styling, colors, and UI constants
    ├── State/
    │   └── ReminderState.cs  # Centralized state management with reactive updates
    ├── ViewModels/
    │   └── ReminderViewModel.cs # UI logic separation from data models
    ├── Components/           # Reusable UI widgets
    │   ├── ActionButtonGroupWidget.cs # Button groups for reminder actions
    │   ├── FormWidgets.cs    # Form input widgets (TimeInput, QuestSelector, etc.)
    │   └── ModernCalendarWidget.cs # Modern responsive calendar widget
    ├── MainTab/
    │   └── ModernMainTabWindow.cs # Main tab implementation using widget composition
    └── Dialogs/
        └── ModernCreateReminderDialog.cs # Create reminder dialog using widget system
```

### **Key File Locations for Common Tasks**

#### **Adding New Reminder Types**
- **Trigger**: Add to `Domain/Triggers/` implementing `Trigger` base class
- **Enum**: Update `TriggerType.cs` in `Core/Enums/`
- **UI**: Update form widgets in `Presentation/UI/Components/FormWidgets.cs`

#### **Adding New Actions**
- **Action**: Add to `Domain/Actions/` implementing `ReminderAction` base class  
- **Enum**: Update `ActionType.cs` in `Core/Enums/`
- **Localization**: Update keys in `Infrastructure/Localization/LocalizationKeys.cs`

#### **UI Components & Widgets**
- **New widgets**: Add to `Presentation/UI/Components/`
- **Layout utilities**: Extend `Presentation/UI/Layout/LayoutEngine.cs`
- **Styling**: Update `Presentation/UI/Theme/ReminderTheme.cs`
- **Main UI**: Modify `Presentation/UI/MainTab/ModernMainTabWindow.cs`

#### **Game Integration**
- **Harmony patches**: Add to `Infrastructure/Integration/HarmonyPatches/`
- **Game hooks**: Add to `Infrastructure/Patches/`
- **Save/load**: Modify `Infrastructure/Persistence/RemindersGameComponent.cs`

#### **Localization & Settings**
- **Text keys**: `Infrastructure/Localization/LocalizationKeys.cs`
- **Translation files**: `Languages/English/Keyed/AdvancedReminders.xml`
- **Mod settings**: `Infrastructure/Settings/AdvancedRemindersSettings.cs`

### **RimWorld Integration Files**
```
About/
├── About.xml              # Mod metadata, dependencies, load order
├── ModIcon.png           # Mod list icon (32x32+)
└── Preview.png           # Steam Workshop preview (640x360)

Defs/
├── KeyBindingCategoryDefs/
│   └── KeyBindings.xml   # Hotkey definitions for RimWorld
├── MainTabDefs/
│   └── MainTabDefs.xml   # Main tab registration (uses ModernMainTabWindow)
└── SoundDefs/
    └── ReminderSounds.xml # Sound effect definitions

Languages/English/Keyed/
└── AdvancedReminders.xml # English localization strings
```

### **Development Guidelines by Location**

#### **Before Adding New Code:**
1. **Check existing files** in the relevant layer first
2. **Follow naming conventions** established in each directory
3. **Use appropriate base classes** (`Trigger`, `ReminderAction`, `WidgetBase`)
4. **Update enums** when adding new types
5. **Add localization keys** for all user-facing text

#### **Widget Development (V2 Architecture):**
- All UI components must implement `IWidget` interface
- Use `WidgetBase` for standard functionality
- Leverage `ReminderTheme` for consistent styling
- Follow responsive design principles (no hardcoded dimensions)
- Use `ReminderState` for data access, never direct manager calls

#### **Integration Points:**
- **Harmony patches** for modifying game behavior
- **GameComponent** for save/load and lifecycle management
- **MainTabWindow** for primary UI integration
- **KeyBindingDef** for hotkey registration

This structure ensures clean separation of concerns, easy testing, and maintainable code following modern architectural patterns.

## Architecture & Key Concepts

### Project Structure (Clean Architecture)
```
Source/AdvancedReminders/
├── Application/           # Application services and orchestration
│   ├── Commands/         # Command handlers (CQRS pattern)
│   ├── Queries/          # Query handlers
│   └── Services/         # Core application services (ReminderManager)
├── Core/                 # Domain models and interfaces (no dependencies)
│   ├── Enums/           # Shared enumerations (TriggerType, SeverityLevel)
│   ├── Interfaces/      # Core abstractions (ITrigger, IReminderAction)
│   └── Models/          # Domain entities (Reminder, Trigger, ReminderAction)
├── Domain/              # Domain logic and implementations
│   ├── Actions/         # Action implementations (NotificationAction)
│   ├── Triggers/        # Trigger implementations (TimeTrigger, QuestDeadlineTrigger)
│   ├── QuestAnalyzers/  # Quest analysis logic
│   └── Templates/       # Reminder templates
├── Infrastructure/      # External concerns and integrations
│   ├── Input/           # Key bindings and hotkey handling
│   ├── Integration/     # RimWorld game integration
│   ├── Localization/    # Translation and localization
│   ├── Patches/         # Harmony patches for game integration
│   ├── Persistence/     # Save/load system (RemindersGameComponent)
│   └── Settings/        # Mod settings and configuration
└── Presentation/        # UI layer
    ├── Components/      # Reusable UI components
    ├── MainTab/         # Main tab window implementation
    └── Windows/         # Dialog windows (CreateReminder, EditReminder)
```

### RimWorld Time System
- **Year**: 60 days (4 quadrums of 15 days each)
- **Quadrums**: Aprimay, Jugust, Septober, Decembary
- **Ticks**: Core time unit (60,000 ticks = 1 day)
- **Real-time**: 1 day = 16:40 real minutes
- **Processing**: Reminders checked every 60 ticks (configurable)

### V2 Architecture (RimHUD-Inspired) - CURRENT TARGET

#### Widget-Based UI System
- **IWidget Interface**: All UI components implement standardized widget pattern with `GetMaxHeight()` and `Draw()`
- **Content-Aware Layout**: Dynamic sizing based on content, no hardcoded dimensions
- **Component Composition**: Complex UI built from small, reusable widgets (ReminderItemWidget, CalendarWidget)
- **Hierarchical Layout**: LayoutContainer manages child widget positioning automatically
- **Responsive Design**: UI adapts to screen size and available space

#### Centralized State Management  
- **ReminderState**: Single source of truth for all reminder data with reactive updates
- **Model-View Separation**: ReminderModel (data) + ReminderViewModel (UI logic) + Widget (rendering)
- **Smart Caching**: Expensive calculations cached with intelligent invalidation via events
- **Event-Driven Updates**: State changes automatically propagate through UI system

#### Performance Optimization Patterns
- **Early Exit Strategies**: Skip processing when components aren't visible or data unchanged
- **Minimal Allocations**: Object pooling, StringBuilder reuse, cached widget instances
- **Conditional Rendering**: Only draw widgets that actually need updates
- **Tick-Aware Processing**: Efficient integration with RimWorld's 60-tick update cycle

### Core Components (V2 Target)
- **ReminderState**: Centralized state management with reactive updates
- **Widget System**: Reusable UI components (ReminderItemWidget, SeverityIndicatorWidget, CalendarWidget)
- **ReminderTheme**: Consistent styling and color management across all components
- **ResponsiveDialog**: Content-aware dialog sizing replacing fixed dimensions
- **LayoutEngine**: Dynamic grid and stacking calculations for responsive design

### Domain Model (Enhanced)
- **ReminderModel**: Pure data representation without UI concerns
- **ReminderViewModel**: UI-specific computed properties (StatusText, StatusColor, IsUrgent)
- **IWidget**: Standardized interface for all UI components with lifecycle management
- **LayoutContainer**: Hierarchical widget composition with automatic positioning
- **CachedWidget**: Performance-optimized base class with dirty tracking

### Integration Points
- **TickManager**: Harmony patch on DoSingleTick for time-based processing
- **GameComponent**: Handles input processing and world-specific lifecycle
- **MainTabWindow**: Custom main tab for reminder management UI
- **Scribe System**: Full save/load integration with version handling

## RimWorld Modding Standards

### Dependencies
- **HugsLib**: Required for settings and mod lifecycle
- **Harmony**: Required for patching game methods
- **Target Framework**: .NET 4.7.2 (RimWorld standard)

### Key Integration Points
- `TickManager`: For time-based processing
- `QuestManager`: For quest detection
- `LetterStack`: For notifications
- `MainTabWindow`: For UI integration

### Localization
- All user-facing text uses translation keys
- Primary languages: EN, ES, DE, FR, RU, ZH, JA
- Keys follow pattern: `AdvRem_FeatureName`

## Development Guidelines

### When Adding Features
1. Check existing patterns in similar mods
2. Use RimWorld's built-in UI widgets
3. Follow established naming conventions
4. All text must use localization keys
5. Test with multiple game speeds

### Performance Considerations
- Minimize per-tick processing
- Cache expensive calculations
- Use lazy initialization
- Profile with 100+ active reminders

### Save Game Compatibility
- Use `Scribe_*` methods for persistence
- Version your save data
- Handle missing/corrupt data gracefully
- Test save/load extensively

## Current Implementation Status

### Phase 1 Complete (Ready for Alpha Release)
✅ **Core Features Working:**
- Time-based reminders with hour/day precision
- Professional reminder creation dialog (Y hotkey)
- Main tab window with sorting, filtering, statistics
- Full save/load persistence via RemindersGameComponent
- Severity-matched letter notifications with pause game option
- Comprehensive localization system (English complete)
- Performance-optimized tick processing (60-tick intervals)
- Color-coded UI with opacity for completed reminders

✅ **Technical Implementation:**
- Clean architecture with proper layer separation
- Harmony patches integrated safely
- HugsLib integration for settings and lifecycle
- Full Scribe integration for save compatibility
- Error handling and validation throughout

### Phase 2 Planned (Quest Integration & Advanced Features)
🔄 **In Development:**
- Edit reminder functionality
- Quest deadline detection and auto-reminders
- Advanced trigger types (event, resource, pawn-based)
- Reminder templates system
- Calendar/timeline view

## Common Development Tasks

### Adding a New Trigger Type
1. Create class in `Domain/Triggers/` implementing `ITrigger`
2. Add trigger evaluation logic with proper tick handling
3. Update `TriggerType` enum in `Core/Enums/`
4. Add trigger selection UI in `Dialog_CreateReminder`
5. Add localization keys in `Languages/English/Keyed/`
6. Test with save/load to ensure Scribe compatibility

### Adding a New Reminder Action
1. Create class in `Domain/Actions/` implementing `IReminderAction`
2. Implement `Execute()` method with proper error handling
3. Add to `ActionType` enum in `Core/Enums/`
4. Update action configuration UI
5. Add localization keys and test execution

### Modifying the UI
1. UI files located in `Presentation/` directory
2. Follow RimWorld's UI widget patterns (see `_GameSource/RimWorld/`)
3. Use `LocalizationKeys` class for all user-facing text
4. Test with different screen resolutions and UI scaling
5. Maintain color coding consistency (severity levels)

### Adding Harmony Patches
1. Create patch class in `Infrastructure/Integration/HarmonyPatches/`
2. Follow naming pattern: `[TargetClass]_[TargetMethod]_Patch.cs`
3. Use HugsLib's patch attribution system
4. Test thoroughly - patches can break save compatibility
5. Check `_GameSource/` for correct method signatures

## Testing Checklist

### Core Functionality Testing
- [x] Basic reminder creation via Y hotkey ✅
- [x] Time-based triggers at different game speeds ✅
- [x] Notification display with correct severity colors ✅
- [x] Pause game functionality for critical reminders ✅
- [x] Save/load persistence across game sessions ✅
- [x] Main tab window display and sorting ✅
- [x] Delete and "Clear Completed" functionality ✅
- [ ] Edit reminder functionality (Phase 2)
- [ ] Quest detection and auto-reminders (Phase 2)

### Performance Testing
- [x] Tested up to 50+ reminders without performance impact ✅
- [ ] Load testing with 100+ reminders (Phase 2)
- [x] Tick processing optimization (60-tick intervals) ✅
- [x] Memory usage during extended gameplay ✅

### Compatibility Testing
- [x] Works with base RimWorld 1.5 and 1.6 ✅
- [x] HugsLib integration working correctly ✅
- [ ] Quest mod compatibility testing (Phase 2)
- [ ] UI scaling and resolution testing
- [ ] Mod load order compatibility

### Localization Testing
- [x] All English text using localization keys ✅
- [x] No hardcoded strings in UI ✅
- [ ] Translation framework ready for other languages
- [x] Localization helper utilities working ✅

## Known Issues & Limitations

### Current Limitations
- Only time-based triggers implemented (Phase 1 scope)
- No in-place editing of reminders (delete/recreate required)
- Quest integration not yet implemented
- Single language support (English only)
- No recurring/repeating reminder patterns

### Fixed Issues
- ✅ Date calculation off-by-one bug resolved
- ✅ Notification color mismatch fixed
- ✅ Save/load GameComponent namespace issues resolved
- ✅ OnGUI null reference exceptions eliminated
- ✅ Sound loading errors fixed for RimWorld 1.6
- ✅ Key binding conflicts resolved