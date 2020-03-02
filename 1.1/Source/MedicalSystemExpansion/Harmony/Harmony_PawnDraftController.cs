using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrenoMSE.HarmonyPatches
{
    public class Harmony_PawnDraftController
    {
        [HarmonyPatch(typeof(Pawn_DraftController))]
        [HarmonyPatch("GetGizmos")]
        internal class PawnDraftController_GetGizmos
        {
            [HarmonyPostfix]
            public static void HediffGizmos(ref IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
            {
                List<Gizmo> gizmos = new List<Gizmo>(__result);
                foreach ( Hediff hediff in __instance.pawn.health.hediffSet.hediffs )
                {
                    HediffComp_HediffGizmo hediffGizmo = hediff.TryGetComp<HediffComp_HediffGizmo>();
                    if (hediffGizmo != null)
                    {
                        foreach ( Gizmo h in hediffGizmo.CompGetGizmos() )
                        {
                            gizmos.Add(h);
                        }
                    }
                }
                __result = gizmos;
            }
        }
    }
}
