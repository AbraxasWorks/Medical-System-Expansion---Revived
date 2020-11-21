using HarmonyLib;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using Verse;

namespace MSE2.HarmonyPatches
{
    internal class CalculateImmediatePartEfficiencyAndRecord_Patch
    {
        [HarmonyPatch( typeof( PawnCapacityUtility ) )]
        [HarmonyPatch( nameof( PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord ) )]
        internal class Patch
        {
            // don't say it has some efficiency just because it is child of added part

            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler ( IEnumerable<CodeInstruction> instructions )
            {
                var l = new List<CodeInstruction>( instructions );

                return instructions.Skip( l.FindLastIndex( ( CodeInstruction i ) => i.opcode == OpCodes.Ldarg_0 ) ); // skip instructions before last loadarg0
            }

            // -- original function:

            /*
            public static float CalculateImmediatePartEfficiencyAndRecord ( HediffSet diffSet, BodyPartRecord part, List<PawnCapacityUtility.CapacityImpactor> impactors = null )
            {
                if ( diffSet.AncestorHasDirectlyAddedParts( part ) )
                {
                    return 1f;
                }

                // -- transpiler removes the above code

                return PawnCapacityUtility.CalculatePartEfficiency( diffSet, part, false, impactors );
            }
            */
        }
    }
}