using System.Collections.Generic;
using Harmony;
using Verse;

namespace OrenoMSE.Harmony
{
    public class Harmony_PawnGenerator
    {
        [HarmonyPatch(typeof(PawnGenerator))]
        [HarmonyPatch("GenerateInitialHediffs")]
        internal class PawnGenerator_GenerateInitialHediffs
        {
            private static void Postfix(Pawn pawn)
            {
                List<Hediff> hediffs = pawn.health.hediffSet?.hediffs;
                if (hediffs != null)
                {
                    for (int i = 0; i < hediffs.Count; i++)
                    {
                        if (hediffs[i].def.HasModExtension<MSE_SpawnInitialSeverity>())
                        {
                            MSE_SpawnInitialSeverity spawnInitialSeverity = hediffs[i].def.GetModExtension<MSE_SpawnInitialSeverity>();
                            if (spawnInitialSeverity != null)
                            {
                                hediffs[i].Severity = spawnInitialSeverity.initialSeverity;
                            }
                        }
                    }
                }
            }
        }
    }
}
