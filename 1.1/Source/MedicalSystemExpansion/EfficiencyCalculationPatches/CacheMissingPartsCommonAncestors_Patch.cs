﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using OrenoMSE.HarmonyPatches;
using Verse;

namespace OrenoMSE.EfficiencyCalculationPatches
{
	[HarmonyPatch]
	class CacheMissingPartsCommonAncestors_Patch
	{
		static MethodBase TargetMethod ()
		{
			var type = "Verse.HediffSet".ToType();
			return type.MethodNamed( "CacheMissingPartsCommonAncestors" );
		}


		// Patch to show that child parts of prosthetics are missing in the health tab and to hide ignored parts

		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler ( IEnumerable<CodeInstruction> instructions )
		{
			foreach ( CodeInstruction instruction in instructions )
			{
				// substitute call to PartOrAnyAncestorHasDirectlyAddedParts with a call to ParentPartIgnores
				if ( instruction.Calls( typeof( HediffSet ).GetMethod( nameof( HediffSet.PartOrAnyAncestorHasDirectlyAddedParts ) ) ) )
				{
					yield return new CodeInstruction( OpCodes.Callvirt, typeof( Utilities ).GetMethod( nameof( Utilities.ParentPartIgnores ) ) );
				}

				else yield return instruction;
			}
			yield break;
		}


		// equivalent change:

		//[HarmonyPrefix]
		//public static bool PreFix ( HediffSet __instance, ref List<Hediff_MissingPart> ___cachedMissingPartsCommonAncestors, ref Queue<BodyPartRecord> ___missingPartsCommonAncestorsQueue )
		//{
			
		//	if ( ___cachedMissingPartsCommonAncestors == null )
		//	{
		//		___cachedMissingPartsCommonAncestors = new List<Hediff_MissingPart>();
		//	}
		//	else
		//	{
		//		___cachedMissingPartsCommonAncestors.Clear();
		//	}
		//	___missingPartsCommonAncestorsQueue.Clear();
		//	___missingPartsCommonAncestorsQueue.Enqueue( __instance.pawn.def.race.body.corePart );
		//	while ( ___missingPartsCommonAncestorsQueue.Count != 0 )
		//	{
		//		BodyPartRecord node = ___missingPartsCommonAncestorsQueue.Dequeue();

		//		// show even if child of added part
		//		if ( !__instance.ParentPartIgnores( node ) ) //!__instance.PartOrAnyAncestorHasDirectlyAddedParts( node ) )
		//		{
		//			Hediff_MissingPart hediff_MissingPart = (from x in __instance.GetHediffs<Hediff_MissingPart>()
		//														where x.Part == node
		//														select x).FirstOrDefault<Hediff_MissingPart>();
		//			if ( hediff_MissingPart != null )
		//			{
		//				___cachedMissingPartsCommonAncestors.Add( hediff_MissingPart );
		//			}
		//			else
		//			{
		//				for ( int i = 0; i < node.parts.Count; i++ )
		//				{
		//					___missingPartsCommonAncestorsQueue.Enqueue( node.parts[i] );
		//				}
		//			}
		//		}
		//	}



		//	return false;
		//}
	}
}
