using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;

namespace MSE2.HarmonyPatches
{
    [HarmonyPatch]
    internal static class VisibleHediffs_patch
    {
        private static MethodBase TargetMethod ()
        {
            return AccessTools.Method( typeof( HealthCardUtility ), "VisibleHediffs" );
        }

        [HarmonyPostfix]
        private static void Postfix ( ref IEnumerable<Hediff> __result, Pawn pawn, bool ___showAllHediffs )
        {
            if ( !___showAllHediffs )
            {
                __result = from h in __result
                           where
                            !(h is Hediff_AddedPart)
                            //|| h.Part.parent == null
                            //|| !pawn.health.hediffSet.HasDirectlyAddedPartFor( h.Part.parent )
                            || !h.IsParentStandard()
                            || pawn.health.hediffSet.hediffs.Where( x => x.Part == h.Part && x.Visible ).Except( h ).Any()
                           select h;
            }
        }
    }
}