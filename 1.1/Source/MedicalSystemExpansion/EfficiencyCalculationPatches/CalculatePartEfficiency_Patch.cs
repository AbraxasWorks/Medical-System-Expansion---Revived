using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using UnityEngine;
using Verse;

namespace OrenoMSE.EfficiencyCalculationPatches
{
	[HarmonyPatch( typeof( PawnCapacityUtility ) )]
	[HarmonyPatch( "CalculatePartEfficiency" )]
	internal class Patch
	{
		[HarmonyPostfix]
		public static void PostFix ( ref float __result, HediffSet diffSet, BodyPartRecord part, bool ignoreAddedParts, List<PawnCapacityUtility.CapacityImpactor> impactors )
		{
			if ( !diffSet.GetNotMissingParts().Contains( part ) )
			{
				__result = 0f;
			}
		}
	}
}
