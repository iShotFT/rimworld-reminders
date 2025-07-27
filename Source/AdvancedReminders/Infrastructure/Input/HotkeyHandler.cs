using UnityEngine;
using Verse;
using RimWorld;
using AdvancedReminders.Presentation.Windows;

namespace AdvancedReminders.Infrastructure.Input
{
    public static class HotkeyHandler
    {
        public static void ProcessHotkeys()
        {
            // Check for create reminder hotkey
            if (KeyBindingDefOf.AdvRem_CreateReminder.JustPressed)
            {
                OnCreateReminderPressed();
            }
            
            // Check for test reminder hotkey (dev mode only)
            if (Prefs.DevMode && KeyBindingDefOf.AdvRem_CreateTestReminder.JustPressed)
            {
                OnCreateTestReminderPressed();
            }
        }
        
        private static void OnCreateReminderPressed()
        {
            try
            {
                // For now, just create a simple test reminder
                // Later this will open the creation dialog
                var dialog = new Dialog_CreateReminder();
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