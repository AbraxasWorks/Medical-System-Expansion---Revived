using Harmony;
using RimWorld;
using Verse;

namespace OrenoMSE.Harmony
{
    public class Harmony_RecipeInstallNaturalBodyPart
    {
        [HarmonyPatch(typeof(Recipe_InstallNaturalBodyPart))]
        [HarmonyPatch("ApplyOnPawn")]
        internal class RecipeInstallNaturalBodyPart_ApplyOnPawn
        {
            [HarmonyPostfix]
            private static void AdditionalHediff(Recipe_InstallNaturalBodyPart __instance, Pawn pawn, BodyPartRecord part)
            {
                MSE_VanillaExtender.RecipeApplyHediffs(__instance, pawn, part);
            }
        }
    }
}
