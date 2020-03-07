using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;

namespace OrenoMSE.EfficiencyCalculationPatches
{
    class CalculateImmediatePartEfficiencyAndRecord_Patch
    {
        [HarmonyPatch( typeof(PawnCapacityUtility) )]
        [HarmonyPatch( "CalculateImmediatePartEfficiencyAndRecord" )]
        internal class Patch
        {
            [HarmonyPrefix]
            [HarmonyPriority( Priority.Last )]
            private static bool PreFix ( ref float __result, HediffSet diffSet, BodyPartRecord part, List<PawnCapacityUtility.CapacityImpactor> impactors )
            {
                //if ( diffSet.AncestorHasDirectlyAddedParts( part ) )
                //{
                //    return 1f;
                //}
                __result = PawnCapacityUtility.CalculatePartEfficiency( diffSet, part, false, impactors );

                return false;
            }
        }
    }
}
