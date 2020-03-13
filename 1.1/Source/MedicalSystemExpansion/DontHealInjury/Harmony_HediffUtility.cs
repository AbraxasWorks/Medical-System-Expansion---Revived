using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace OrenoMSE.HarmonyPatches
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
                foreach ( Hediff hediff in hd.pawn.health.hediffSet.hediffs )
                {
                    if ( hediff.Part == hd.Part && hediff.def.HasModExtension<MSE_DontHealInjury>())
                    {
                        MSE_DontHealInjury dontHealInjury = hediff.def.GetModExtension<MSE_DontHealInjury>();
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
