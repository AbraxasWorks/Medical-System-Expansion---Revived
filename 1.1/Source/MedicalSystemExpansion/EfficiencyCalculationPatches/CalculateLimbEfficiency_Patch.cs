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
				//Log.Message( "limb efficiency of " + diffSet.pawn.Name + ", " + limbCore.customLabel );
				
				// part efficiency lambda

				Func<BodyPartRecord, float> CalculatePartEfficiency;
				if ( (CalculatePartEfficiency = cachedCPE) == null )
				{
					CalculatePartEfficiency = (cachedCPE = ( BodyPartRecord part ) => PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord( diffSet, part, impactors ));
				}

				// segments

				List<BodyPartRecord> segments = new List<BodyPartRecord>( limbCore.GetConnectedParts( limbSegmentTag ).Prepend( limbCore ) );

				// parts to ignore

				List <BodyPartDef> partsToIgnore = 
					new List<BodyPartDef>(
						from hediff in diffSet.hediffs
						where hediff is Hediff_AddedPart
						where segments.Contains(hediff.Part)
						let modExt = hediff.def.GetModExtension<IgnoreSubParts>()
						where modExt != null
						from p in modExt.ignoredSubParts
						select p );

				//Log.Message( "Ignored subparts: " + partsToIgnore.Count );


				// segment calculations

				float limbEff = 0f;
				float minSegmentEff = 1f;
				int functionalLimbSegments = 0;
				int totLimbSegments = 0;


				foreach ( BodyPartRecord limbSegment in from p in segments
														where !partsToIgnore.Contains(p.def)
														select p )
				{
					float segmentEff = PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord( diffSet, limbSegment, impactors );
					limbEff += segmentEff;
					minSegmentEff = Mathf.Min(segmentEff, minSegmentEff);

					//Log.Message( limbSegment.customLabel + " " + segmentEff );

					if ( segmentEff > 0f ) functionalLimbSegments++; // part works
					totLimbSegments++;
				}

				if ( totLimbSegments > 0 && functionalLimbSegments == totLimbSegments ) // all parts are working 
				{
					limbEff /= totLimbSegments; // average of segments and core
					limbEff = Mathf.Lerp( limbEff, minSegmentEff, 0.5f );
				}
				else
				{
					limbEff = 0; // missing segments
				}

				//Log.Message( "parts: " + functionalLimbSegments + "/" + totLimbSegments + "; eff: " + limbEff );


				// digit calculations

				if ( limbCore.HasChildParts( limbDigitTag ) )
				{

					IEnumerable<BodyPartRecord> childParts =
						from p in limbCore.GetChildParts( limbDigitTag )
						where !partsToIgnore.Contains( p.def )
						select p ;

					if ( childParts.Any() )
						limbEff = Mathf.Lerp( limbEff, Mathf.Sqrt( limbEff * childParts.Average( CalculatePartEfficiency ) ), appendageWeight );

				}

				//Log.Message( "with fingers: " + limbEff );

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
