using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace MSE2.HarmonyPatches
{
    [HarmonyPatch( typeof( PawnCapacityUtility ) )]
    [HarmonyPatch( "CalculatePartEfficiency" )]
    internal class CalculatePartEfficiency_Patch
    {
        // remove the copying of the efficiency of a parent added part

        [HarmonyTranspiler]
        [HarmonyPriority( Priority.Low )]
        public static IEnumerable<CodeInstruction> Transpiler ( IEnumerable<CodeInstruction> instructions )
        {
            // determine all instructions belonging to the first for

            Label l = (Label)instructions.First( ( CodeInstruction i ) => i.opcode == OpCodes.Br_S ).operand; // target of the first branch in the first for loop

            int firstBranchJump = instructions.FirstIndexOf( ( CodeInstruction i ) => i.labels.Contains( l ) ); // the location of that instruction
            firstBranchJump += 3; // the for ends 3 instructions later

            // skip them
            return instructions.Skip( firstBranchJump );
        }
    }
}