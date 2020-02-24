using System.Collections.Generic;
using Harmony;
using Verse;

namespace OrenoMSE.Harmony
{
    public class Harmony_HediffUtility
    {
        [HarmonyPatch(typeof(HediffUtility))]
        [HarmonyPatch("IsPermanent")]
        internal class HediffUtility_IsPermanent
        {
            [HarmonyPostfix]
            public static void DontHealInjury(ref bool __result, Hediff hd)
            {
                List<Hediff> hediffs = hd.pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    if (hediffs[i].Part == hd.Part && hediffs[i].def.HasModExtension<MSE_DontHealInjury>())
                    {
                        MSE_DontHealInjury dontHealInjury = hediffs[i].def.GetModExtension<MSE_DontHealInjury>();
                        if (dontHealInjury != null && dontHealInjury.notHealInjury)
                        {
                            __result = true;
                            return;
                        }
                    }
                }
            }
        }
    }
}
