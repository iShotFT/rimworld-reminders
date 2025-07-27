using HugsLib;
using HugsLib.Settings;
using UnityEngine;
using Verse;
using AdvancedReminders.Application.Services;
using AdvancedReminders.Infrastructure.Settings;
using AdvancedReminders.Infrastructure.Input;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Domain.Triggers;
using AdvancedReminders.Domain.Actions;

namespace AdvancedReminders
{
    public class ReminderMod : ModBase
    {
        public static ReminderMod Instance { get; private set; }
        
        public override string ModIdentifier => "AdvancedReminders";

        public override void Initialize()
        {
            Instance = this;
            Logger.Message("Advanced Reminders mod initializing...");
        }

        public override void DefsLoaded()
        {
            Logger.Message("Advanced Reminders defs loaded");
            try
            {
                // Initialize settings
                ReminderSettings.Initialize(Settings);
                Logger.Message("ReminderSettings initialized");
                
                // Initialize core services
                ReminderManager.Initialize();
                Logger.Message("ReminderManager initialized");
            }
            catch (System.Exception e)
            {
                Logger.Error($"Failed to initialize systems: {e}");
            }
        }

        public override void WorldLoaded()
        {
            Logger.Message("Advanced Reminders world loaded");
            try
            {
                // Initialize world-specific components
                ReminderManager.Instance?.OnWorldLoaded();
                Logger.Message("World-specific reminder systems initialized");
                
                // Create a test reminder for testing (remove this in production)
                if (Prefs.DevMode && ReminderSettings.EnableDebugLogging?.Value == true)
                {
                    CreateTestReminder();
                }
            }
            catch (System.Exception e)
            {
                Logger.Error($"Failed to initialize world systems: {e}");
            }
        }

        public override void MapLoaded(Map map)
        {
            Logger.Message($"Advanced Reminders map loaded: {map}");
            // Map-specific initialization if needed
        }

        public override void SettingsChanged()
        {
            Logger.Message("Advanced Reminders settings changed");
            // Handle settings changes if needed
        }

        public override void OnGUI()
        {
            // OnGUI is now only used for GUI-specific operations
            // Hotkey processing moved to GameComponent.GameComponentUpdate()
        }
        
        public static void CreateTestReminder()
        {
            try
            {
                var reminder = new Reminder("Test Reminder", "This is a test reminder that should trigger in 30 seconds", SeverityLevel.Medium);
                
                // Create a time trigger for testing - 5 seconds from now
                var timeTrigger = new TimeTrigger();
                timeTrigger.TargetTick = Find.TickManager.TicksGame + 300; // 5 seconds at normal speed
                timeTrigger.IsRelativeTime = false;
                
                // Create a notification action
                var notificationAction = new NotificationAction(pauseGame: true);
                
                reminder.Trigger = timeTrigger;
                reminder.Actions.Add(notificationAction);
                
                ReminderManager.Instance?.AddReminder(reminder);
                
                Log.Message("[AdvancedReminders] Test reminder created successfully!");
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Failed to create test reminder: {ex}");
            }
        }
    }
}