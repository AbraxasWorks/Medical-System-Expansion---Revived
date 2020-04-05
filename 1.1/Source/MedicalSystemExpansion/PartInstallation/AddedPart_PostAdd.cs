using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrenoMSE.PartInstallation
{

    [HarmonyPatch( typeof( Hediff_AddedPart ) )]
    [HarmonyPatch( "PostAdd" )]
    class AddedPart_PostAdd
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


            // insert after restore part

            int indexOfRestorePart = instructionList.FindIndex( i => i.Calls( typeof( Pawn_HealthTracker ).GetMethod( nameof( Pawn_HealthTracker.RestorePart ) ) ) ) + 1;
            instructionList.InsertRange( indexOfRestorePart, baseCall );

            // return

            return instructionList;

        }
    }
}
