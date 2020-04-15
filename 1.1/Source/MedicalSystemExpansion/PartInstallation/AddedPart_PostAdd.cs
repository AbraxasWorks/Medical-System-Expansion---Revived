using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace MSE2.HarmonyPatches
{
    [HarmonyPatch( typeof( Hediff_AddedPart ) )]
    [HarmonyPatch( "PostAdd" )]
    internal class AddedPart_PostAdd
    {
        // swap calls to base.postAdd and restorepart
        // this allows to add hediffs in comppostpostadd

        public static IEnumerable<CodeInstruction> Transpiler ( IEnumerable<CodeInstruction> instructions )
        {
            List<CodeInstruction> instructionList = new List<CodeInstruction>( instructions );

            // extract base call

            int baseCallLength = instructionList.FindIndex( i => i.Calls( typeof( Hediff_Implant ).GetMethod( nameof( Hediff_Implant.PostAdd ) ) ) ) + 1;
            List<CodeInstruction> baseCall = instructionList.GetRange( 0, baseCallLength );
            instructionList.RemoveRange( 0, baseCallLength );

            // insert it before return statement

            int indexOfReturn = instructionList.FindIndex( i => i.opcode == OpCodes.Ret );
            instructionList.InsertRange( indexOfReturn, baseCall );

            // return new list

            return instructionList;
        }
    }
}