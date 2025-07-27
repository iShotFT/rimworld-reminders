using HugsLib.Settings;
using UnityEngine;
using Verse;
using AdvancedReminders.Infrastructure.Localization;

namespace AdvancedReminders.Infrastructure.Settings
{
    public static class ReminderSettings
    {
        public static SettingHandle<bool> EnableAutoProcessing { get; private set; }
        public static SettingHandle<bool> EnableDebugLogging { get; private set; }
        public static SettingHandle<KeyCode> CreateReminderHotkey { get; private set; }
        public static SettingHandle<int> ProcessingInterval { get; private set; }
        
        public static void Initialize(ModSettingsPack settings)
        {
            
            EnableAutoProcessing = settings.GetHandle<bool>(
                "enableAutoProcessing",
                LocalizationKeys.EnableAutoProcessing.Translate(),
                LocalizationKeys.EnableAutoProcessingDesc.Translate(),
                true);
            
            EnableDebugLogging = settings.GetHandle<bool>(
                "enableDebugLogging", 
                LocalizationKeys.EnableDebugLogging.Translate(),
                LocalizationKeys.EnableDebugLoggingDesc.Translate(),
                false);
            
            CreateReminderHotkey = settings.GetHandle<KeyCode>(
                "createReminderHotkey",
                LocalizationKeys.CreateReminderHotkey.Translate(), 
                LocalizationKeys.CreateReminderHotkeyDesc.Translate(),
                KeyCode.R);
            
            ProcessingInterval = settings.GetHandle<int>(
                "processingInterval",
                LocalizationKeys.ProcessingInterval.Translate(),
                LocalizationKeys.ProcessingIntervalDesc.Translate(),
                60);
            
            // ProcessingInterval.ValueChanged += OnProcessingIntervalChanged;
        }
        
        private static void OnProcessingIntervalChanged(SettingHandle<int> handle)
        {
            // Validate processing interval
            if (handle.Value < 1)
                handle.Value = 1;
            else if (handle.Value > 1000)
                handle.Value = 1000;
        }
    }
}