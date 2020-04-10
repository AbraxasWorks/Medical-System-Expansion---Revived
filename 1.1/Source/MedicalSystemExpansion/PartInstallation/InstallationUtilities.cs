using Verse;

namespace OrenoMSE.PartInstallation
{
    internal static class InstallationUtilities
    {
        public static bool HasRestrictionsForPart ( this RecipeDef recipe, BodyPartRecord part, HediffSet hediffSet )
        {
            var modExt = recipe.GetModExtension<InstallationRestrictions>();

            return modExt != null && part.parent != null && !modExt.CompatibleWithPart( part.parent, hediffSet );
        }
    }
}