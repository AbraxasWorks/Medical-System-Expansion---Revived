//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using Verse;

//namespace OrenoMSE.HarmonyPatches
//{
//    public class Harmony_HediffUtility
//    {
//        [HarmonyPatch(typeof(HediffUtility))]
//        [HarmonyPatch("IsPermanent")]
//        internal class HediffUtility_IsPermanent
//        {
//            [HarmonyPostfix]
//            public static void DontHealInjury(ref bool __result, Hediff hd)
//            {

//                // makes it so injuries on hediff_addedpart are permanent (they also cannot bleed)

//                __result = __result ||
//                    (from h in hd.pawn.health.hediffSet.hediffs
//                     where h.Part == hd.Part
//                     where h is Hediff_AddedPart
//                     select h).Any();
                
//            }
//        }
//    }
//}
