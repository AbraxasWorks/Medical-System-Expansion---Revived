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
	internal class CalculatePartEfficiency_Patch
	{
		
		// short circuit to return 0 efficency if the part is missing
		
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

					if ( part.parent == null || diffSet.GetNotMissingParts().Contains(part.parent) )
					{
						var imp = new PawnCapacityUtility.CapacityImpactorBodyPartHealth { bodyPart = part };

						impactors.Add( imp );
					}
				}

				return false;
			}
			return true;
		}
	}
}
