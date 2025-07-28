using HarmonyLib;
using RimWorld;
using Verse;
using AdvancedReminders.Application.Services;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Domain.Triggers;
using System.Linq;

namespace AdvancedReminders.Infrastructure.Integration.HarmonyPatches
{
    [HarmonyPatch(typeof(Quest), "Accept")]
    public static class Quest_Accept_Patch
    {
        public static void Postfix(Quest __instance)
        {
            if (__instance == null || ReminderManager.Instance == null)
                return;
                
            try
            {
                // Mark any quest reminders for this quest as completed
                var questReminders = ReminderManager.Instance.AllReminders
                    .Where(r => r.IsActive && r.Trigger is QuestDeadlineTrigger questTrigger && questTrigger.QuestId == __instance.id)
                    .ToList();
                    
                foreach (var reminder in questReminders)
                {
                    reminder.IsActive = false;
                    reminder.LastTriggeredTick = Find.TickManager.TicksGame;
                    
                    Messages.Message($"Quest reminder completed: {reminder.Title}", MessageTypeDefOf.PositiveEvent);
                }
                
                if (questReminders.Any())
                {
                    Log.Message($"[AdvancedReminders] Marked {questReminders.Count} quest reminders as completed for quest: {__instance.name}");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AdvancedReminders] Error in Quest_Accept_Patch: {ex}");
            }
        }
    }
}