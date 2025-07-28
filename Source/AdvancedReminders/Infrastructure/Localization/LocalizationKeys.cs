using Verse;

namespace AdvancedReminders.Infrastructure.Localization
{
    public static class LocalizationKeys
    {
        // Main Interface
        public const string ModName = "AdvRem_ModName";
        public const string ModDescription = "AdvRem_ModDescription";
        public const string MainTabTitle = "AdvRem_MainTabTitle";
        public const string MainTabDescription = "AdvRem_MainTabDescription";
        
        // Reminder Management
        public const string CreateReminderTitle = "AdvRem_CreateReminderTitle";
        public const string EditReminderTitle = "AdvRem_EditReminderTitle";
        public const string ReminderTitle = "AdvRem_ReminderTitle";
        public const string ReminderDescription = "AdvRem_ReminderDescription";
        public const string ReminderSeverity = "AdvRem_ReminderSeverity";
        public const string ReminderTrigger = "AdvRem_ReminderTrigger";
        public const string ReminderActions = "AdvRem_ReminderActions";
        public const string IsRepeating = "AdvRem_IsRepeating";
        public const string IsActive = "AdvRem_IsActive";
        
        // Severity Levels
        public const string SeverityLow = "AdvRem_SeverityLow";
        public const string SeverityMedium = "AdvRem_SeverityMedium";
        public const string SeverityHigh = "AdvRem_SeverityHigh";
        public const string SeverityCritical = "AdvRem_SeverityCritical";
        public const string SeverityUrgent = "AdvRem_SeverityUrgent";
        
        // Trigger Types
        public const string TriggerTypeTime = "AdvRem_TriggerTypeTime";
        public const string TriggerTypeEvent = "AdvRem_TriggerTypeEvent";
        public const string TriggerTypeResource = "AdvRem_TriggerTypeResource";
        public const string TriggerTypePawn = "AdvRem_TriggerTypePawn";
        public const string TriggerTypeQuest = "AdvRem_TriggerTypeQuest";
        public const string TriggerTypeCalendar = "AdvRem_TriggerTypeCalendar";
        public const string TriggerTypeCondition = "AdvRem_TriggerTypeCondition";
        
        // Time Trigger
        public const string TimeTriggerInDays = "AdvRem_TimeTriggerInDays";
        public const string TimeTriggerOnDate = "AdvRem_TimeTriggerOnDate";
        public const string DaysFromNow = "AdvRem_DaysFromNow";
        public const string SpecificDate = "AdvRem_SpecificDate";
        public const string TimeRemaining = "AdvRem_TimeRemaining";
        public const string TimeRemainingNow = "AdvRem_TimeRemainingNow";
        
        // Action Types
        public const string ActionTypeNotification = "AdvRem_ActionTypeNotification";
        public const string ActionTypePause = "AdvRem_ActionTypePause";
        
        // Main Tab Window
        public const string RemindersTabTitle = "AdvRem_RemindersTabTitle";
        public const string CreateNewReminder = "AdvRem_CreateNewReminder";
        public const string NoActiveReminders = "AdvRem_NoActiveReminders";
        public const string TriggerType = "AdvRem_TriggerType";
        public const string NoTriggerSet = "AdvRem_NoTriggerSet";
        
        // Notification Action
        public const string ShowNotification = "AdvRem_ShowNotification";
        public const string ShowNotificationAndPause = "AdvRem_ShowNotificationAndPause";
        public const string PauseGame = "AdvRem_PauseGame";
        public const string CustomTitle = "AdvRem_CustomTitle";
        public const string CustomText = "AdvRem_CustomText";
        
        // Quest Integration
        public const string QuestDeadlineWarning = "AdvRem_QuestDeadlineWarning";
        public const string QuestPreparation = "AdvRem_QuestPreparation";
        public const string QuestReminder = "AdvRem_QuestReminder";
        public const string QuestPhasePreparation = "AdvRem_QuestPhasePreparation";
        public const string QuestPhaseActive = "AdvRem_QuestPhaseActive";
        public const string QuestPhaseCompletion = "AdvRem_QuestPhaseCompletion";
        public const string QuestPhaseExpired = "AdvRem_QuestPhaseExpired";
        
        // Time System
        public const string QuadrumAprimay = "AdvRem_QuadrumAprimay";
        public const string QuadrumJugust = "AdvRem_QuadrumJugust";
        public const string QuadrumSeptober = "AdvRem_QuadrumSeptober";
        public const string QuadrumDecembary = "AdvRem_QuadrumDecembary";
        public const string Day = "AdvRem_Day";
        public const string Days = "AdvRem_Days";
        public const string Hour = "AdvRem_Hour";
        public const string Hours = "AdvRem_Hours";
        public const string Year = "AdvRem_Year";
        public const string Years = "AdvRem_Years";
        
        // UI Elements
        public const string ButtonCreate = "AdvRem_ButtonCreate";
        public const string ButtonEdit = "AdvRem_ButtonEdit";
        public const string ButtonDelete = "AdvRem_ButtonDelete";
        public const string ButtonCancel = "AdvRem_ButtonCancel";
        public const string ButtonSave = "AdvRem_ButtonSave";
        public const string ButtonClose = "AdvRem_ButtonClose";
        public const string ButtonAdd = "AdvRem_ButtonAdd";
        public const string ButtonRemove = "AdvRem_ButtonRemove";
        
        // Reminder List
        public const string NoReminders = "AdvRem_NoReminders";
        public const string ActiveReminders = "AdvRem_ActiveReminders";
        public const string CompletedReminders = "AdvRem_CompletedReminders";
        public const string ReminderCount = "AdvRem_ReminderCount";
        public const string ShowCompleted = "AdvRem_ShowCompleted";
        public const string HideCompleted = "AdvRem_HideCompleted";
        
        // Status Messages
        public const string ReminderCreated = "AdvRem_ReminderCreated";
        public const string ReminderUpdated = "AdvRem_ReminderUpdated";
        public const string ReminderDeleted = "AdvRem_ReminderDeleted";
        public const string ReminderTriggered = "AdvRem_ReminderTriggered";
        public const string ReminderCompleted = "AdvRem_ReminderCompleted";
        
        // Error Messages
        public const string ErrorTitleRequired = "AdvRem_ErrorTitleRequired";
        public const string ErrorInvalidDays = "AdvRem_ErrorInvalidDays";
        public const string ErrorNoTrigger = "AdvRem_ErrorNoTrigger";
        public const string ErrorNoActions = "AdvRem_ErrorNoActions";
        public const string ErrorFailedToCreate = "AdvRem_ErrorFailedToCreate";
        public const string ErrorFailedToUpdate = "AdvRem_ErrorFailedToUpdate";
        public const string ErrorFailedToDelete = "AdvRem_ErrorFailedToDelete";
        
        // Settings
        public const string SettingsTitle = "AdvRem_SettingsTitle";
        public const string EnableAutoProcessing = "AdvRem_EnableAutoProcessing";
        public const string EnableAutoProcessingDesc = "AdvRem_EnableAutoProcessingDesc";
        public const string EnableDebugLogging = "AdvRem_EnableDebugLogging";
        public const string EnableDebugLoggingDesc = "AdvRem_EnableDebugLoggingDesc";
        public const string CreateReminderHotkey = "AdvRem_CreateReminderHotkey";
        public const string CreateReminderHotkeyDesc = "AdvRem_CreateReminderHotkeyDesc";
        public const string ProcessingInterval = "AdvRem_ProcessingInterval";
        public const string ProcessingIntervalDesc = "AdvRem_ProcessingIntervalDesc";
        
        // Hotkeys
        public const string CreateReminderHotkeyLabel = "AdvRem_CreateReminderHotkeyLabel";
        public const string CreateReminderHotkeyDesc2 = "AdvRem_CreateReminderHotkeyDesc";
        
        // Quest Reminder UI
        public const string ReminderType = "AdvRem_ReminderType";
        public const string ReminderTypeTime = "AdvRem_ReminderTypeTime";
        public const string ReminderTypeQuest = "AdvRem_ReminderTypeQuest";
        public const string SelectQuest = "AdvRem_SelectQuest";
        public const string SelectQuestPrompt = "AdvRem_SelectQuestPrompt";
        public const string RemindMe = "AdvRem_RemindMe";
        public const string BeforeQuestExpires = "AdvRem_BeforeQuestExpires";
        public const string NoQuestsWithDeadlines = "AdvRem_NoQuestsWithDeadlines";
        public const string QuestExpiresIn = "AdvRem_QuestExpiresIn";
        public const string PleaseSelectQuest = "AdvRem_PleaseSelectQuest";
        public const string HoursBeforeExpiryRequired = "AdvRem_HoursBeforeExpiryRequired";
        public const string ReminderDetails = "AdvRem_ReminderDetails";
        public const string Timing = "AdvRem_Timing";
        public const string FromNow = "AdvRem_FromNow";
        public const string Priority = "AdvRem_Priority";
        public const string Options = "AdvRem_Options";
    }
}