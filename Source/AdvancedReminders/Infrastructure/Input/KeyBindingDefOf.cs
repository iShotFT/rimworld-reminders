using RimWorld;
using Verse;

namespace AdvancedReminders.Infrastructure.Input
{
    /// <summary>
    /// Key binding definitions for the mod.
    /// Currently empty as all hotkeys have been disabled per user feedback.
    /// </summary>
    [DefOf]
    public static class KeyBindingDefOf
    {
        // No hotkeys configured per user feedback - access through main tab menu
        
        static KeyBindingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingDefOf));
        }
    }
}