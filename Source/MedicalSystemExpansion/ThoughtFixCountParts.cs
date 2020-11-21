using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Verse;
using RimWorld;
using System.Reflection.Emit;

namespace MSE2
{
    [HarmonyPatch( typeof( HediffUtility ) )]
    [HarmonyPatch( nameof( HediffUtility.CountAddedAndImplantedParts ) )]
    internal static class ThoughtFixCountParts
    {
        // to fix thoughts and shit, only count limbs once
        // makes the function into this

        /*
        public static int CountAddedAndImplantedParts(this HediffSet hs)
		{
            int num = 0;
            List<Hediff> hediffs = hs.hediffs;
            for ( int i = 0; i < hediffs.Count; i++ )
            {
                if ( hediffs[i].def.countsAsAddedPartOrImplant && !hs.AncestorHasDirectlyAddedParts( hediffs[i].Part ) )
                {
                    num++;
                }
            }
            return num;
        }
        */

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler ( IEnumerable<CodeInstruction> instructions )
        {
            var instrList = instructions.ToList();

            var brfalseIndex = instrList.FindIndex( i => i.opcode == OpCodes.Brfalse_S );
            if ( brfalseIndex < 0 ) throw new ApplicationException( "Could not find branch false operation" );
            var brfalse = instrList[brfalseIndex];

            instrList.InsertRange( brfalseIndex + 1, InstructionsToInsert( brfalse.operand ) );

            return instrList;
        }

        private static IEnumerable<CodeInstruction> InstructionsToInsert ( object branchTarget )
        {
            yield return new CodeInstruction( OpCodes.Ldarg_0 );
            yield return new CodeInstruction( OpCodes.Ldloc_1 );
            yield return new CodeInstruction( OpCodes.Ldloc_2 );

            var listGetter = typeof( List<Hediff> ).GetMethod( "get_Item" );
            if ( listGetter == null ) throw new ApplicationException( "Could not find list getter" );
            yield return new CodeInstruction( OpCodes.Callvirt, listGetter );

            var partGetter = typeof( Hediff ).GetMethod( "get_Part" );
            if ( partGetter == null ) throw new ApplicationException( "Could not find hediff part getter" );
            yield return new CodeInstruction( OpCodes.Callvirt, partGetter );

            var ancHasPart = typeof( HediffSet ).GetMethod( nameof( HediffSet.AncestorHasDirectlyAddedParts ) );
            if ( ancHasPart == null ) throw new ApplicationException( "Could not find HediffSet.AncestorHasDirectlyAddedParts" );
            yield return new CodeInstruction( OpCodes.Callvirt, ancHasPart );

            yield return new CodeInstruction( OpCodes.Brtrue_S, branchTarget );
        }
    }
}