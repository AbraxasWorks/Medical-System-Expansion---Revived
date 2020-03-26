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
	[HarmonyPatch( "CalculateLimbEfficiency" )]
	internal class CalculateLimbEfficiency_Patch
	{

		// replace
		// should prob implement a transpiler

		[HarmonyPrefix]
		[HarmonyPriority( Priority.Last )]
		public static bool Replace ( ref float __result, HediffSet diffSet, BodyPartTagDef limbCoreTag, BodyPartTagDef limbSegmentTag, BodyPartTagDef limbDigitTag, float appendageWeight, out float functionalPercentage, List<PawnCapacityUtility.CapacityImpactor> impactors )
		{
			BodyDef body = diffSet.pawn.RaceProps.body;
			float totLimbEff = 0f;
			int totLimbs = 0;
			int functionalLimbs = 0;

			Func<BodyPartRecord, float> cachedCPE = null;

			foreach ( BodyPartRecord limbCore in body.GetPartsWithTag( limbCoreTag ) )
			{
				Log.Message( "limb efficiency of " + diffSet.pawn.Name + ", " + limbCore.customLabel );
				
				// part efficiency lambda

				Func<BodyPartRecord, float> CalculatePartEfficiency;
				if ( (CalculatePartEfficiency = cachedCPE) == null )
				{
					CalculatePartEfficiency = (cachedCPE = ( BodyPartRecord part ) => PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord( diffSet, part, impactors ));
				}

				// segment calculations

				float limbEff = PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord( diffSet, limbCore, impactors );
				float minSegmentEff = Mathf.Min( limbEff, 1 );
				int functionalLimbSegments = limbEff > 0f ? 1 : 0;
				int totLimbSegments = 1;

				Log.Message( limbCore.customLabel + " " + limbEff );

				foreach ( BodyPartRecord limbSegment in limbCore.GetConnectedParts( limbSegmentTag ) )
				{
					float segmentEff = PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord( diffSet, limbSegment, impactors );
					limbEff += segmentEff;
					minSegmentEff = Mathf.Min(segmentEff, minSegmentEff);

					Log.Message( limbSegment.customLabel + " " + segmentEff );

					if ( segmentEff > 0f ) functionalLimbSegments++; // part works
					totLimbSegments++;
				}

				if ( totLimbSegments > 0 && functionalLimbSegments == totLimbSegments ) // all parts are working 
				{
					limbEff /= totLimbSegments; // average of segments and core
					limbEff = Mathf.Lerp( limbEff, limbEff * minSegmentEff, 0.5f );
				}
				else
				{
					limbEff = 0; // missing segments
				}

				Log.Message( "parts: " + functionalLimbSegments + "/" + totLimbSegments + "; eff: " + limbEff );


				// digit calculations

				if ( limbCore.HasChildParts( limbDigitTag ) )
				{

					IEnumerable<BodyPartRecord> childParts = limbCore.GetChildParts( limbDigitTag );
					
					limbEff = Mathf.Lerp( limbEff, Mathf.Sqrt( limbEff * childParts.Average( CalculatePartEfficiency ) ), appendageWeight );

				}

				Log.Message( "with fingers: " + limbEff );

				totLimbEff += limbEff;
				totLimbs++;
				if ( limbEff > 0f )
				{
					functionalLimbs++;
				}
			}

			if ( totLimbs == 0 )
			{
				functionalPercentage = 0f;
				__result = 0f;
			}

			functionalPercentage = (float)functionalLimbs / (float)totLimbs;
			__result = totLimbEff / (float)totLimbs;

			return false;
		}
	}
}
