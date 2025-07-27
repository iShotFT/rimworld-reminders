using RimWorld;
using Verse;

namespace AdvancedReminders.Infrastructure.Input
{
    [DefOf]
    public static class KeyBindingDefOf
    {
        public static KeyBindingDef AdvRem_CreateReminder;
        public static KeyBindingDef AdvRem_OpenRemindersTab;
        public static KeyBindingDef AdvRem_CreateTestReminder;
        
        static KeyBindingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingDefOf));
        }
    }
}