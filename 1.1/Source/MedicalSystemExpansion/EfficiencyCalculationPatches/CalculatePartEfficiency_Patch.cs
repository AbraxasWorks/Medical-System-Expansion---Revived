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
		[HarmonyPrefix]
		[HarmonyPriority( Priority.High )]
		public static bool PreFix ( ref float __result, HediffSet diffSet, BodyPartRecord part, bool ignoreAddedParts, List<PawnCapacityUtility.CapacityImpactor> impactors )
		{
			// if the part is missing
			if ( !diffSet.GetNotMissingParts().Contains( part ) )
			{
				__result = 0f; // it has 0 efficiency

				// if possible add it to the list of things affecting the stat 
				if ( impactors != null )
				{
					var imp = new PawnCapacityUtility.CapacityImpactorBodyPartHealth { bodyPart = part };

					impactors.Add( imp );
				}

				return false;
			}
			return true;
		}
	}
}
