using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MSE2.HarmonyPatches
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

            Func<BodyPartRecord, float> CalculatePartEfficiency = null;

            foreach ( BodyPartRecord limbCore in body.GetPartsWithTag( limbCoreTag ) )
            {
                //Log.Message( "limb efficiency of " + diffSet.pawn.Name + ", " + limbCore.customLabel );

                // segments

                List<BodyPartRecord> segments = new List<BodyPartRecord>( limbCore.GetConnectedParts( limbSegmentTag ).Prepend( limbCore ) );

                // parts to ignore

                List<BodyPartDef> partsToIgnore =
                    new List<BodyPartDef>(
                        from hediff in diffSet.hediffs
                        where hediff is Hediff_AddedPart
                        where segments.Contains( hediff.Part )
                        let modExt = hediff.def.GetModExtension<IgnoreSubParts>()
                        where modExt != null
                        from p in modExt.ignoredSubParts
                        select p );

                // segment calculations

                float limbEff = 1f;
                int totLimbSegments = 0;

                foreach ( BodyPartRecord limbSegment in from p in segments
                                                        where !partsToIgnore.Contains( p.def )
                                                        select p )
                {
                    float segmentEff = PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord( diffSet, limbSegment, impactors );
                    limbEff *= segmentEff;

                    totLimbSegments++;
                }

                limbEff = Mathf.Pow( limbEff, 2f / totLimbSegments ); // square of the geometric mean of segments

                //Log.Message( "parts: " + totLimbSegments + "; eff: " + limbEff );

                // digit calculations

                if ( limbCore.HasChildParts( limbDigitTag ) )
                {
                    IEnumerable<BodyPartRecord> digits =
                        from p in limbCore.GetChildParts( limbDigitTag )
                        where !partsToIgnore.Contains( p.def )
                        select p;

                    if ( digits.Any() )
                    {
                        // part efficiency lambda

                        if ( CalculatePartEfficiency == null )
                        {
                            CalculatePartEfficiency = ( BodyPartRecord part ) => PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord( diffSet, part, impactors );
                        }

                        limbEff = Mathf.Lerp( limbEff, limbEff * digits.Average( CalculatePartEfficiency ), appendageWeight );
                    }
                }

                limbEff = Mathf.Sqrt( limbEff );

                //Log.Message( "with fingers: " + limbEff ); //

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