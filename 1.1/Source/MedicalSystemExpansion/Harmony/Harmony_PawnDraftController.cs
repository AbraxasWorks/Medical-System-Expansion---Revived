using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;

namespace OrenoMSE.Harmony
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
                List<Hediff> hediffs = __instance.pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    HediffComp_HediffGizmo hediffGizmo = hediffs[i].TryGetComp<HediffComp_HediffGizmo>();
                    if (hediffGizmo != null)
                    {
                        foreach (Gizmo h in hediffGizmo.CompGetGizmos())
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
