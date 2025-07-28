using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using AdvancedReminders.Presentation.Windows;
using AdvancedReminders.Core.Enums;

namespace AdvancedReminders.Infrastructure.Patches
{
    /// <summary>
    /// Harmony patch to add "Set Reminder" button to vanilla quest display
    /// for unaccepted quests with deadlines
    /// </summary>
    [HarmonyPatch(typeof(MainTabWindow_Quests))]
    public static class MainTabWindow_Quests_Patch
    {
        /// <summary>
        /// Patch the DoAcceptButton method to add our "Set Reminder" button
        /// </summary>
        [HarmonyPatch("DoAcceptButton")]
        [HarmonyPostfix]
        public static void DoAcceptButton_Postfix(MainTabWindow_Quests __instance, Rect innerRect, ref float curY)
        {
            try
            {
                // Get the selected quest using reflection
                var selectedField = AccessTools.Field(typeof(MainTabWindow_Quests), "selected");
                var selectedQuest = (Quest)selectedField.GetValue(__instance);
                
                if (selectedQuest == null)
                    return;
                
                // Only show for unaccepted quests with deadlines
                if (selectedQuest.State != QuestState.NotYetAccepted || selectedQuest.TicksUntilExpiry <= 0)
                    return;
                
                // Add some spacing
                curY += 10f;
                
                // Create "Set Reminder" button
                var buttonRect = new Rect(innerRect.x, curY, 150f, 35f);
                
                // Use default button styling
                if (Widgets.ButtonText(buttonRect, "Set Reminder"))
                {
                    // Open reminder creation dialog with quest pre-selected
                    var dialog = new Dialog_CreateReminder();
                    
                    // Pre-configure for quest reminder
                    var formComponent = AccessTools.Field(typeof(Dialog_CreateReminder), "formComponent").GetValue(dialog);
                    if (formComponent != null)
                    {
                        // Set reminder type to Quest
                        AccessTools.Property(formComponent.GetType(), "ReminderType").SetValue(formComponent, ReminderType.Quest);
                        
                        // Set selected quest
                        AccessTools.Property(formComponent.GetType(), "SelectedQuest").SetValue(formComponent, selectedQuest);
                        
                        // Set default title and description
                        AccessTools.Property(formComponent.GetType(), "ReminderTitle").SetValue(formComponent, $"Quest Deadline: {selectedQuest.name}");
                        AccessTools.Property(formComponent.GetType(), "ReminderDescription").SetValue(formComponent, $"Remember to accept quest '{selectedQuest.name}' before it expires!");
                        
                        // Set default timing - 1 day before expiration
                        var hoursUntilExpiry = selectedQuest.TicksUntilExpiry / (float)GenDate.TicksPerHour;
                        var defaultHours = Mathf.Max(1, Mathf.Min(24, Mathf.RoundToInt(hoursUntilExpiry * 0.1f))); // 10% of remaining time, capped between 1-24 hours
                        AccessTools.Property(formComponent.GetType(), "QuestHoursBeforeExpiry").SetValue(formComponent, defaultHours);
                    }
                    
                    Find.WindowStack.Add(dialog);
                }
                
                // Update curY to account for our button
                curY += 40f; // Button height + spacing
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Error in MainTabWindow_Quests patch: {ex}");
            }
        }
    }
}