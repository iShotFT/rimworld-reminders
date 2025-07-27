using System.Collections.Generic;
using AdvancedReminders.Core.Enums;
using RimWorld;
using Verse;

namespace AdvancedReminders.Core.Models
{
    public class QuestReminder : Reminder
    {
        public int QuestId { get; set; }
        public QuestPhase Phase { get; set; }
        public List<string> RequirementChecklist { get; set; }
        public float PreparationProgress { get; set; }
        
        public QuestReminder() : base()
        {
            RequirementChecklist = new List<string>();
            Phase = QuestPhase.Preparation;
            PreparationProgress = 0f;
        }
        
        public QuestReminder(Quest quest, QuestPhase phase = QuestPhase.Preparation) : this()
        {
            if (quest != null)
            {
                QuestId = quest.id;
                Title = $"Quest: {quest.name}";
                Description = $"Quest deadline reminder for: {quest.name}";
                Phase = phase;
                Severity = SeverityLevel.High;
            }
        }
        
        public Quest GetAssociatedQuest()
        {
            if (QuestId == 0)
                return null;
                
            return Find.QuestManager.QuestsListForReading.Find(q => q.id == QuestId);
        }
        
        public void UpdatePreparationProgress()
        {
            if (RequirementChecklist == null || RequirementChecklist.Count == 0)
            {
                PreparationProgress = 1f;
                return;
            }
            
            // This would be implemented with actual requirement checking logic
            // For now, just a placeholder
            PreparationProgress = 0.5f;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            
            int questId = QuestId;
            QuestPhase phase = Phase;
            float preparationProgress = PreparationProgress;
            
            Scribe_Values.Look(ref questId, "questId", 0);
            Scribe_Values.Look(ref phase, "phase", QuestPhase.Preparation);
            Scribe_Values.Look(ref preparationProgress, "preparationProgress", 0f);
            List<string> requirementChecklist = RequirementChecklist;
            Scribe_Collections.Look(ref requirementChecklist, "requirementChecklist", LookMode.Value);
            RequirementChecklist = requirementChecklist;
            
            QuestId = questId;
            Phase = phase;
            PreparationProgress = preparationProgress;
        }
    }
}