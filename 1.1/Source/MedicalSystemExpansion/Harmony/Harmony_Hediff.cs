using Harmony;
using Verse;

namespace OrenoMSE.Harmony
{
    public class Harmony_Hediff
    {
        [HarmonyPatch(typeof(Hediff))]
        [HarmonyPatch("LabelBase", MethodType.Getter)]
        internal class Hediff_LabelBase
        {
            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            private static void HediffPrettyLabel(ref string __result, Hediff __instance)
            {
                if (__instance is Hediff_AddedPart || __instance is Hediff_Implant)
                {
                    __result = MSE_VanillaExtender.PrettyLabel(__instance);
                }
            }
        }
    }
}
