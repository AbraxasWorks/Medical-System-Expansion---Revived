using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrenoMSE.HarmonyPatches
{
    public class Pawn_HealthTracker_RestorePartRecursiveInt_Patch
    {
        [HarmonyPatch( typeof( Pawn_HealthTracker ) )]
        [HarmonyPatch( "RestorePartRecursiveInt" )]
        internal class RemoveRecursiveCall
        {
			static MethodInfo TargetMethodInfo ()
			{
				var type = "Verse.Pawn_HealthTracker".ToType();
				return type.MethodNamed( "RestorePartRecursiveInt" );
			}

			// transpiler that removes the recursive call
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler ( IEnumerable<CodeInstruction> instructions )
			{
				foreach ( CodeInstruction instruction in instructions )
				{
					if ( instruction.Calls( TargetMethodInfo() ) )
					{
						yield return new CodeInstruction( OpCodes.Pop );
						yield return new CodeInstruction( OpCodes.Pop );
						yield return new CodeInstruction( OpCodes.Pop );
					}
					else
					{
						yield return instruction;
					}
				}
				yield break;
			}

			/*
			 * Vanilla equivalent:
			 * 
				
				List<Hediff> hediffs = __instance.hediffSet.hediffs;
				for ( int i = hediffs.Count - 1; i >= 0; i-- )
				{
					Hediff hediff = hediffs[i];
					if ( hediff.Part == part && hediff != diffException )
					{
						Hediff hediff2 = hediffs[i];
						hediffs.RemoveAt( i );
						hediff2.PostRemoved();
					}
				}
				for ( int j = 0; j < part.parts.Count; j++ )                    
				{
					  this.RestorePartRecursiveInt( part.parts[j], diffException ); // <----- transpiler removes this call
				}

			 * 
			 */
		}
	}
}
