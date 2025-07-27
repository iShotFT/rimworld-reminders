# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

:warning: IMPORTANT! Always read and use the GOLDEN-RULES.md file, these rules are to be applied ALWAYS

## Project Overview

This is a RimWorld mod project for an advanced reminder and task management system. The mod integrates deeply with RimWorld's game systems to provide context-aware reminders, quest tracking, and intelligent scheduling.

## Development Commands

### Build & Development
```bash
# Build the mod
make build

# Clean build artifacts
make clean

# Debug build
make debug

# Run RimWorld with development mode
# Set dev mode in RimWorld options, then use in-game dev tools

# Package mod for release
# Built DLL is automatically placed in Assemblies folder
# Mod is automatically deployed to RimWorld/Mods directory
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

## Architecture & Key Concepts

### RimWorld Time System
- **Year**: 60 days (4 quadrums of 15 days each)
- **Quadrums**: Aprimay, Jugust, Septober, Decembary
- **Ticks**: Core time unit (60,000 ticks = 1 day)
- **Real-time**: 1 day = 16:40 real minutes

### Core Components
- **ReminderManager**: Central service managing all reminders
- **QuestTracker**: Monitors quests and creates automatic reminders
- **TriggerProcessor**: Evaluates trigger conditions
- **Harmony Patches**: Integrate with RimWorld systems

### Domain Model
- **Reminder**: Core entity with title, triggers, actions, severity
- **Trigger**: Conditions that fire reminders (time/event/resource-based)
- **Action**: What happens when triggered (notification/pause/camera)
- **QuestReminder**: Specialized reminder for quest management

### Technical Patterns
- **CQRS**: Commands for state changes, queries for data retrieval
- **Event-Driven**: Decoupled components via events
- **Repository Pattern**: Data access abstraction
- **Domain-Driven Design**: Clear separation of concerns

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

## Common Tasks

### Adding a New Trigger Type
1. Create class inheriting from `ITrigger`
2. Implement evaluation logic
3. Add to trigger factory
4. Create UI selector component
5. Add localization keys

### Adding a New Reminder Action
1. Create class implementing `IReminderAction`
2. Implement execution logic
3. Register in action system
4. Add configuration UI
5. Test with various scenarios

### Quest Integration
- Hook `QuestManager.Add` for new quests
- Analyze quest type and requirements
- Create appropriate reminder templates
- Monitor quest state changes

## Testing Checklist
- [ ] Basic reminder creation/editing/deletion
- [ ] Time-based triggers across game speeds
- [ ] Quest detection and auto-reminders
- [ ] Save/load persistence
- [ ] Performance with many reminders
- [ ] Localization completeness
- [ ] Mod compatibility (especially quest mods)