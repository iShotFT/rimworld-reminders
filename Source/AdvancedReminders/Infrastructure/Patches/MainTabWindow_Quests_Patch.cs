using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using AdvancedReminders.Presentation.UI.Dialogs;
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
                    // Open the modern reminder creation dialog with pre-selected quest
                    var dialog = new ModernCreateReminderDialog(selectedQuest);
                    
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