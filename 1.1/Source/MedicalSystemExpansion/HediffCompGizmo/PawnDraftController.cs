using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MSE2.HarmonyPatches
{
    public class PawnDraftController
    {
        [HarmonyPatch( typeof( Pawn_DraftController ) )]
        [HarmonyPatch( "GetGizmos" )]
        internal class GetGizmos
        {
            [HarmonyPostfix]
            public static void HediffGizmos ( ref IEnumerable<Gizmo> __result, Pawn_DraftController __instance )
            {
                List<Gizmo> gizmos = new List<Gizmo>( __result );
                foreach ( Hediff hediff in __instance.pawn.health.hediffSet.hediffs )
                {
                    HediffComp_HediffGizmo hediffGizmo = hediff.TryGetComp<HediffComp_HediffGizmo>();
                    if ( hediffGizmo != null )
                    {
                        foreach ( Gizmo h in hediffGizmo.CompGetGizmos() )
                        {
                            gizmos.Add( h );
                        }
                    }
                }
                __result = gizmos;
            }
        }
    }
}