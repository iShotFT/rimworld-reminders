using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedReminders.Core.Interfaces;
using AdvancedReminders.Core.Models;
using Verse;

namespace AdvancedReminders.Application.Services
{
    public class ReminderManager : IReminderManager
    {
        private static ReminderManager _instance;
        public static ReminderManager Instance => _instance;
        
        private readonly List<Reminder> _reminders;
        private int _nextId;
        
        public IReadOnlyList<Reminder> ActiveReminders => 
            _reminders.Where(r => r.IsActive).ToList().AsReadOnly();
            
        public IReadOnlyList<Reminder> AllReminders => 
            _reminders.Where(r => !string.IsNullOrWhiteSpace(r.Title)).ToList().AsReadOnly();
        
        private ReminderManager()
        {
            _reminders = new List<Reminder>();
            _nextId = 1;
        }
        
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new ReminderManager();
                Log.Message("[AdvancedReminders] ReminderManager initialized");
            }
        }
        
        public void OnWorldLoaded()
        {
            // World-specific initialization
            Log.Message("[AdvancedReminders] ReminderManager world loaded");
            
            // Load reminders from GameComponent
            LoadRemindersFromGameComponent();
        }
        
        private void LoadRemindersFromGameComponent()
        {
            if (Current.Game != null)
            {
                var gameComponent = Current.Game.GetComponent<Reminders.RemindersGameComponent>();
                if (gameComponent != null && gameComponent.ReminderQueue != null)
                {
                    int loadedCount = 0;
                    int skippedCount = 0;
                    
                    foreach (var reminder in gameComponent.ReminderQueue)
                    {
                        // Skip empty/invalid reminders
                        if (string.IsNullOrWhiteSpace(reminder.Title))
                        {
                            skippedCount++;
                            Log.Warning($"[AdvancedReminders] Skipped loading empty reminder with ID: {reminder.Id}");
                            continue;
                        }
                        
                        _reminders.Add(reminder);
                        if (reminder.Id >= _nextId)
                        {
                            _nextId = reminder.Id + 1;
                        }
                        loadedCount++;
                    }
                    
                    Log.Message($"[AdvancedReminders] Loaded {loadedCount} reminders from save" + 
                               (skippedCount > 0 ? $", skipped {skippedCount} empty reminders" : ""));
                }
            }
        }
        
        public void SaveRemindersToGameComponent()
        {
            if (Current.Game != null)
            {
                var gameComponent = Current.Game.GetComponent<Reminders.RemindersGameComponent>();
                if (gameComponent != null)
                {
                    gameComponent.ReminderQueue.Clear();
                    gameComponent.ReminderQueue.AddRange(_reminders);
                    Log.Message($"[AdvancedReminders] Saved {_reminders.Count} reminders to GameComponent");
                }
            }
        }
        
        public void AddReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                Log.Warning("[AdvancedReminders] Attempted to add null reminder");
                return;
            }
            
            // Validate reminder has required data
            if (string.IsNullOrWhiteSpace(reminder.Title))
            {
                Log.Warning("[AdvancedReminders] Attempted to add reminder with empty title - ignoring");
                return;
            }
            
            reminder.Id = _nextId++;
            _reminders.Add(reminder);
            
            Log.Message($"[AdvancedReminders] Added reminder: {reminder.Title} (ID: {reminder.Id})");
        }
        
        public void UpdateReminder(Reminder reminder)
        {
            if (reminder == null)
            {
                Log.Warning("[AdvancedReminders] Attempted to update null reminder");
                return;
            }
            
            var existingReminder = _reminders.FirstOrDefault(r => r.Id == reminder.Id);
            if (existingReminder == null)
            {
                Log.Warning($"[AdvancedReminders] Reminder with ID {reminder.Id} not found for update");
                return;
            }
            
            // Replace the reminder in the list
            var index = _reminders.IndexOf(existingReminder);
            _reminders[index] = reminder;
            
            Log.Message($"[AdvancedReminders] Updated reminder: {reminder.Title} (ID: {reminder.Id})");
        }
        
        public void RemoveReminder(int reminderId)
        {
            var reminder = _reminders.FirstOrDefault(r => r.Id == reminderId);
            if (reminder == null)
            {
                Log.Warning($"[AdvancedReminders] Reminder with ID {reminderId} not found for removal");
                return;
            }
            
            _reminders.Remove(reminder);
            Log.Message($"[AdvancedReminders] Removed reminder: {reminder.Title} (ID: {reminderId})");
        }
        
        public Reminder GetReminder(int reminderId)
        {
            return _reminders.FirstOrDefault(r => r.Id == reminderId);
        }
        
        public void ProcessTriggers()
        {
            var activeReminders = _reminders.Where(r => r.IsActive).ToList();
            
            foreach (var reminder in activeReminders)
            {
                try
                {
                    if (reminder.ShouldTrigger())
                    {
                        Log.Message($"[AdvancedReminders] Triggering reminder: {reminder.Title}");
                        reminder.TriggerActions();
                        
                        // If the reminder is not repeating, it will be marked as inactive
                        if (!reminder.IsActive)
                        {
                            Log.Message($"[AdvancedReminders] Reminder completed: {reminder.Title}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[AdvancedReminders] Error processing reminder {reminder.Title}: {ex}");
                }
            }
        }
        
        public List<T> GetRemindersByType<T>() where T : Reminder
        {
            return _reminders.OfType<T>().ToList();
        }
        
        public void ClearAllReminders()
        {
            var count = _reminders.Count;
            _reminders.Clear();
            Log.Message($"[AdvancedReminders] Cleared {count} reminders");
        }
        
        public void ClearCompletedReminders()
        {
            var completedReminders = _reminders.Where(r => !r.IsActive).ToList();
            foreach (var reminder in completedReminders)
            {
                _reminders.Remove(reminder);
            }
            
            Log.Message($"[AdvancedReminders] Cleared {completedReminders.Count} completed reminders");
        }
    }
}