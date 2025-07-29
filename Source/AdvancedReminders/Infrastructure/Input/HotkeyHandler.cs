using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Presentation.UI.Dialogs;

namespace AdvancedReminders.Infrastructure.Input
{
    public static class HotkeyHandler
    {
        public static void ProcessHotkeys()
        {
            // Hotkeys disabled per user feedback - access through main tab menu
            return;
        }
        
        private static void OnCreateReminderPressed()
        {
            try
            {
                // Open the modern create reminder dialog
                var dialog = new ModernCreateReminderDialog();
                Find.WindowStack.Add(dialog);
                
                Log.Message("[AdvancedReminders] Create reminder hotkey pressed");
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Error handling create reminder hotkey: {ex}");
            }
        }
        
        private static void OnCreateTestReminderPressed()
        {
            try
            {
                ReminderMod.CreateTestReminder();
                Log.Message("[AdvancedReminders] Test reminder hotkey pressed");
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Error handling test reminder hotkey: {ex}");
            }
        }
    }
}