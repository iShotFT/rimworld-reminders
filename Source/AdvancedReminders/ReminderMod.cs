using HugsLib;
using HugsLib.Settings;
using UnityEngine;
using Verse;

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
            // Initialize systems after defs are loaded
        }

        public override void WorldLoaded()
        {
            Logger.Message("Advanced Reminders world loaded");
            // Initialize world-specific systems
        }

        public override void MapLoaded(Map map)
        {
            Logger.Message($"Advanced Reminders map loaded: {map}");
            // Initialize map-specific systems
        }

        public override void SettingsChanged()
        {
            Logger.Message("Advanced Reminders settings changed");
            // Handle settings changes
        }

        public override void OnGUI()
        {
            // Handle any immediate mode GUI
        }
    }
}