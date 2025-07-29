using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedReminders.Core.Models;
using AdvancedReminders.Core.Enums;
using AdvancedReminders.Application.Services;

namespace AdvancedReminders.Presentation.UI.State
{
    /// <summary>
    /// Centralized state management for reminder data following RimHUD reactive patterns.
    /// Provides single source of truth with event-driven updates and intelligent caching.
    /// </summary>
    public static class ReminderState
    {
        #region Events

        /// <summary>
        /// Fired when any reminder is added, modified, or removed
        /// </summary>
        public static event Action OnRemindersChanged;

        /// <summary>
        /// Fired when a specific reminder is added
        /// </summary>
        public static event Action<Reminder> OnReminderAdded;

        /// <summary>
        /// Fired when a specific reminder is removed
        /// </summary>
        public static event Action<Reminder> OnReminderRemoved;

        /// <summary>
        /// Fired when a specific reminder is updated
        /// </summary>
        public static event Action<Reminder> OnReminderUpdated;

        /// <summary>
        /// Fired when reminder statistics change (counts, urgency levels, etc.)
        /// </summary>
        public static event Action OnStatisticsChanged;

        #endregion

        #region Cache Management

        private static readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private static readonly object _cacheLock = new object();

        /// <summary>
        /// Gets a cached value or creates it using the factory function
        /// </summary>
        public static T GetCached<T>(string key, Func<T> factory)
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(key, out var cached))
                    return (T)cached;

                var value = factory();
                _cache[key] = value;
                return value;
            }
        }

        /// <summary>
        /// Invalidates all cached values, forcing recalculation
        /// </summary>
        public static void InvalidateCache()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
            }
        }

        /// <summary>
        /// Invalidates specific cached values by key pattern
        /// </summary>
        public static void InvalidateCache(string keyPattern)
        {
            lock (_cacheLock)
            {
                var keysToRemove = _cache.Keys.Where(k => k.Contains(keyPattern)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }
        }

        #endregion

        #region Data Access

        /// <summary>
        /// Gets all reminders from the manager (cached)
        /// </summary>
        public static IReadOnlyList<Reminder> AllReminders
        {
            get
            {
                return GetCached("all_reminders", () =>
                {
                    var manager = ReminderManager.Instance;
                    return manager?.AllReminders?.ToList() ?? new List<Reminder>();
                });
            }
        }

        /// <summary>
        /// Gets only active reminders (cached)
        /// </summary>
        public static IReadOnlyList<Reminder> ActiveReminders
        {
            get
            {
                return GetCached("active_reminders", () =>
                    AllReminders.Where(r => r.IsActive).ToList());
            }
        }

        /// <summary>
        /// Gets only completed reminders (cached)
        /// </summary>
        public static IReadOnlyList<Reminder> CompletedReminders
        {
            get
            {
                return GetCached("completed_reminders", () =>
                    AllReminders.Where(r => !r.IsActive).ToList());
            }
        }

        /// <summary>
        /// Gets reminders by severity level (cached)
        /// </summary>
        public static IReadOnlyList<Reminder> GetRemindersBySeverity(SeverityLevel severity)
        {
            return GetCached($"reminders_severity_{severity}", () =>
                AllReminders.Where(r => r.Severity == severity).ToList());
        }

        /// <summary>
        /// Gets urgent reminders (those triggering soon) (cached)
        /// </summary>
        public static IReadOnlyList<Reminder> UrgentReminders
        {
            get
            {
                return GetCached("urgent_reminders", () =>
                    ActiveReminders.Where(IsUrgent).ToList());
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Gets reminder count statistics (cached)
        /// </summary>
        public static ReminderStatistics Statistics
        {
            get
            {
                return GetCached("statistics", () => new ReminderStatistics
                {
                    TotalCount = AllReminders.Count,
                    ActiveCount = ActiveReminders.Count,
                    CompletedCount = CompletedReminders.Count,
                    UrgentCount = UrgentReminders.Count,
                    CriticalCount = GetRemindersBySeverity(SeverityLevel.Critical).Count,
                    HighCount = GetRemindersBySeverity(SeverityLevel.High).Count,
                    MediumCount = GetRemindersBySeverity(SeverityLevel.Medium).Count,
                    LowCount = GetRemindersBySeverity(SeverityLevel.Low).Count
                });
            }
        }

        #endregion

        #region State Modification

        /// <summary>
        /// Adds a new reminder and updates state
        /// </summary>
        public static void AddReminder(Reminder reminder)
        {
            if (reminder == null) return;

            var manager = ReminderManager.Instance;
            if (manager != null)
            {
                manager.AddReminder(reminder);
                RefreshState();
                OnReminderAdded?.Invoke(reminder);
            }
        }

        /// <summary>
        /// Removes a reminder and updates state
        /// </summary>
        public static void RemoveReminder(int reminderId)
        {
            var reminder = AllReminders.FirstOrDefault(r => r.Id == reminderId);
            if (reminder == null) return;

            var manager = ReminderManager.Instance;
            if (manager != null)
            {
                manager.RemoveReminder(reminderId);
                RefreshState();
                OnReminderRemoved?.Invoke(reminder);
            }
        }

        /// <summary>
        /// Updates an existing reminder and refreshes state
        /// </summary>
        public static void UpdateReminder(Reminder reminder)
        {
            if (reminder == null) return;

            // Note: ReminderManager doesn't have an UpdateReminder method yet,
            // so we'll need to remove and re-add for now
            var manager = ReminderManager.Instance;
            if (manager != null)
            {
                manager.RemoveReminder(reminder.Id);
                manager.AddReminder(reminder);
                RefreshState();
                OnReminderUpdated?.Invoke(reminder);
            }
        }

        /// <summary>
        /// Clears all completed reminders
        /// </summary>
        public static void ClearCompletedReminders()
        {
            var manager = ReminderManager.Instance;
            if (manager != null)
            {
                manager.ClearCompletedReminders();
                RefreshState();
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Refreshes the entire state, invalidating all caches and firing events
        /// </summary>
        public static void RefreshState()
        {
            InvalidateCache();
            OnRemindersChanged?.Invoke();
            OnStatisticsChanged?.Invoke();
        }

        /// <summary>
        /// Initializes the state system. Called when the mod loads.
        /// </summary>
        public static void Initialize()
        {
            // Set up any initial state or event subscriptions
            InvalidateCache();
        }

        /// <summary>
        /// Cleans up the state system. Called when the mod unloads.
        /// </summary>
        public static void Cleanup()
        {
            InvalidateCache();
            
            // Clear all event subscriptions
            OnRemindersChanged = null;
            OnReminderAdded = null;
            OnReminderRemoved = null;
            OnReminderUpdated = null;
            OnStatisticsChanged = null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if a reminder is urgent (triggering soon)
        /// </summary>
        private static bool IsUrgent(Reminder reminder)
        {
            if (!reminder.IsActive) return false;

            // For time-based triggers, check if triggering within 24 hours
            if (reminder.Trigger is Domain.Triggers.TimeTrigger timeTrigger)
            {
                var ticksRemaining = timeTrigger.TargetTick - Verse.Find.TickManager.TicksGame;
                return ticksRemaining <= 60000; // Less than 1 day
            }

            // For other trigger types, consider critical and high severity as urgent
            return reminder.Severity >= SeverityLevel.High;
        }

        /// <summary>
        /// Gets filtered and sorted reminders based on display preferences
        /// </summary>
        public static IEnumerable<Reminder> GetFilteredReminders(bool showCompleted = true, 
            SortMode sortMode = SortMode.TriggerTime)
        {
            var reminders = showCompleted ? AllReminders : ActiveReminders;
            
            return sortMode switch
            {
                SortMode.TriggerTime => reminders.OrderBy(GetSortPriority),
                SortMode.Created => reminders.OrderByDescending(r => r.CreatedTick),
                SortMode.Severity => reminders.OrderByDescending(r => (int)r.Severity).ThenBy(GetSortPriority),
                SortMode.Status => reminders.OrderBy(r => r.IsActive ? 0 : 1).ThenBy(GetSortPriority),
                SortMode.Title => reminders.OrderBy(r => r.Title),
                _ => reminders.OrderBy(GetSortPriority)
            };
        }

        /// <summary>
        /// Gets sort priority for time-based sorting
        /// </summary>
        private static long GetSortPriority(Reminder reminder)
        {
            if (reminder.Trigger is Domain.Triggers.TimeTrigger timeTrigger)
                return timeTrigger.TargetTick;
            
            // Non-time triggers sort by creation time
            return reminder.CreatedTick;
        }

        #endregion
    }

    /// <summary>
    /// Statistics about reminder counts and states
    /// </summary>
    public class ReminderStatistics
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int UrgentCount { get; set; }
        public int CriticalCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public int LowCount { get; set; }

        /// <summary>
        /// Gets a human-readable summary of the statistics
        /// </summary>
        public string GetSummary()
        {
            return $"Active: {ActiveCount} | Urgent: {UrgentCount} | Total: {TotalCount}";
        }
    }

    /// <summary>
    /// Sort modes for reminder display
    /// </summary>
    public enum SortMode
    {
        TriggerTime,
        Created,
        Severity,
        Status,
        Title
    }
}