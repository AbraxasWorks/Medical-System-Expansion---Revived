using HarmonyLib;
using Verse;

namespace OrenoMSE.HarmonyPatches
{
    public class Harmony_HediffImplant
    {
        [HarmonyPatch(typeof(Hediff_Implant))]
        [HarmonyPatch("PostAdd")]
        internal class HediffImplant_PostAdd
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.LowerThanNormal)]
            private static void AdditionalHediff(Hediff_Implant __instance)
            {
                MSE_VanillaExtender.HediffApplyExtraHediffs(__instance, __instance.pawn, __instance.Part);
            }
        }
    }
}
