using HarmonyLib;
using AdvancedReminders.Application.Services;
using AdvancedReminders.Infrastructure.Settings;
using Verse;

namespace AdvancedReminders.Infrastructure.Integration.HarmonyPatches
{
    [HarmonyPatch(typeof(TickManager), "DoSingleTick")]
    public static class TickManager_DoSingleTick_Patch
    {
        private static int lastProcessTick = 0;
        
        static void Postfix(TickManager __instance)
        {
            try
            {
                // Only process if we have an active game and reminder manager is initialized
                if (Current.Game == null || ReminderManager.Instance == null)
                    return;
                
                // Check if it's time to process triggers based on the interval setting
                var currentTick = Find.TickManager.TicksGame;
                var interval = ReminderSettings.ProcessingInterval?.Value ?? 60;
                
                if (currentTick - lastProcessTick >= interval)
                {
                    lastProcessTick = currentTick;
                    
                    // Process all reminder triggers
                    ReminderManager.Instance.ProcessTriggers();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Error in TickManager patch: {ex}");
            }
        }
    }
}