using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace OrenoMSE.HarmonyPatches
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
                    foreach ( Hediff hediff in hediffs )
                    {
                        MSE_SpawnInitialSeverity spawnInitialSeverity = hediff.def.GetModExtension<MSE_SpawnInitialSeverity>();
                        if (spawnInitialSeverity != null)
                        {
                            hediff.Severity = spawnInitialSeverity.initialSeverity;
                        }
                    }
                }
            }
        }
    }
}
