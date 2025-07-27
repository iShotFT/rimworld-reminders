using System.Collections.Generic;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Application.Services;
using Verse;

namespace Reminders
{
    public class RemindersGameComponent : GameComponent
    {
        public List<Reminder> ReminderQueue = new List<Reminder>();
        public List<Reminder> RemindersOnNextLoad = new List<Reminder>();
        
        public RemindersGameComponent()
        {
        }
        
        public RemindersGameComponent(Game game)
        {
        }
        
        public override void ExposeData()
        {
            // Before saving, get the latest data from ReminderManager
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (ReminderManager.Instance != null)
                {
                    ReminderQueue.Clear();
                    ReminderQueue.AddRange(ReminderManager.Instance.ActiveReminders);
                }
            }
            
            // Save/load the reminder data
            Scribe_Collections.Look(ref ReminderQueue, "ReminderQueue", LookMode.Deep);
            Scribe_Collections.Look(ref RemindersOnNextLoad, "RemindersOnNextLoad", LookMode.Deep);
            
            // After loading, update the ReminderManager
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                // Data will be loaded by ReminderManager.LoadRemindersFromGameComponent() when world loads
            }
        }
        
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Log.Message("[AdvancedReminders] GameComponent FinalizeInit called");
        }
        
        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            
            // Process hotkeys in Update instead of OnGUI to avoid multiple calls per frame
            try
            {
                if (Current.Game != null && Find.WindowStack != null)
                {
                    AdvancedReminders.Infrastructure.Input.HotkeyHandler.ProcessHotkeys();
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"[AdvancedReminders] Error processing hotkeys: {e}");
            }
        }
        
        public override void LoadedGame()
        {
            base.LoadedGame();
            Log.Message("[AdvancedReminders] Game loaded, GameComponent ready");
        }
        
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            Log.Message("[AdvancedReminders] New game started, GameComponent ready");
        }
        
        public void SaveReminders()
        {
            if (ReminderManager.Instance != null)
            {
                ReminderQueue.Clear();
                ReminderQueue.AddRange(ReminderManager.Instance.ActiveReminders);
            }
        }
    }
}