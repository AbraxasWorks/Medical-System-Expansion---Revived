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
        [HarmonyPatch( "CalculateImmediatePartEfficiencyAndRecord" )]
        internal class Patch
        {
            // short circuit to return 0 efficency if the part is missing

            [HarmonyPrefix]
            public static bool PreFix ( ref float __result, HediffSet diffSet, BodyPartRecord part, List<PawnCapacityUtility.CapacityImpactor> impactors )
            {
                // if the part is missing
                if ( !diffSet.GetNotMissingParts().Contains( part ) )
                {
                    __result = 0f; // it has 0 efficiency

                    // if possible add it to the list of things affecting the stat
                    if ( impactors != null )
                    {
                        if ( part.parent == null || diffSet.GetNotMissingParts().Contains( part.parent ) )
                        {
                            var imp = new PawnCapacityUtility.CapacityImpactorBodyPartHealth { bodyPart = part };

                            impactors.Add( imp );
                        }
                    }

                    return false;
                }
                return true;
            }

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