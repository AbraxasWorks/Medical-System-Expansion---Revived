using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using OrenoMSE.EfficiencyCalculationPatches;
using RimWorld;
using Verse;

namespace OrenoMSE.PartInstallation
{
    public class Hediff_AddDirect
    {

        [HarmonyPatch( typeof( HediffSet ) )]
        [HarmonyPatch( "AddDirect" )]
        internal class AddDirect
        {

            // Added error for when trying to add hediff to part that should be ignored

            [HarmonyPrefix]
            public static bool ErrorOnIgnoredPart ( HediffSet __instance, Hediff hediff )
            {
                if ( !( hediff is Hediff_MissingPart ) && __instance.PartShouldBeIgnored( hediff.Part ) )
                {
                    Log.Error( "Tried to add health diff to part that should be ignored. Canceling.", false );
                    return false;
                }

                return true;
            }


            // adding custom exit condition for when trying to remove a part that is missing
            // it might be better not to do this at all as it can mask underlying problems
            // also probably better to implement a transpiler


            [HarmonyPrefix]
            [HarmonyPriority( Priority.Low )]
            public static bool ReplaceWithExitCondition ( HediffSet __instance, Hediff hediff, DamageInfo? dinfo = null, DamageWorker.DamageResult damageResult = null )
            {
                if ( hediff.def == null )
                {
                    Log.Error( "Tried to add health diff with null def. Canceling.", false );
                    return false;
                }
                if ( hediff.Part != null && !__instance.GetNotMissingParts( BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null ).Contains( hediff.Part ) )
                {
                    // start custom code
                    
                    if ( hediff is Hediff_MissingPart )
                    {
                        return false;
                    }
                    
                    // end custom code

                    Log.Error( "Tried to add health diff to missing part " + hediff.Part, false );
                    return false;
                }
                hediff.ageTicks = 0;
                hediff.pawn = __instance.pawn;
                bool flag = false;
                for ( int i = 0; i < __instance.hediffs.Count; i++ )
                {
                    if ( __instance.hediffs[i].TryMergeWith( hediff ) )
                    {
                        flag = true;
                    }
                }
                if ( !flag )
                {
                    __instance.hediffs.Add( hediff );
                    hediff.PostAdd( dinfo );
                    if ( __instance.pawn.needs != null && __instance.pawn.needs.mood != null )
                    {
                        __instance.pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                    }
                }
                bool flag2 = hediff is Hediff_MissingPart;
                if ( !(hediff is Hediff_MissingPart) && hediff.Part != null && hediff.Part != __instance.pawn.RaceProps.body.corePart && __instance.GetPartHealth( hediff.Part ) == 0f && hediff.Part != __instance.pawn.RaceProps.body.corePart )
                {
                    bool flag3 = __instance.HasDirectlyAddedPartFor( hediff.Part );
                    Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff( HediffDefOf.MissingBodyPart, __instance.pawn, null );
                    hediff_MissingPart.IsFresh = !flag3;
                    hediff_MissingPart.lastInjury = hediff.def;
                    __instance.pawn.health.AddHediff( hediff_MissingPart, hediff.Part, dinfo, null );
                    if ( damageResult != null )
                    {
                        damageResult.AddHediff( hediff_MissingPart );
                    }
                    if ( flag3 )
                    {
                        if ( dinfo != null )
                        {
                            hediff_MissingPart.lastInjury = HealthUtility.GetHediffDefFromDamage( dinfo.Value.Def, __instance.pawn, hediff.Part );
                        }
                        else
                        {
                            hediff_MissingPart.lastInjury = null;
                        }
                    }
                    flag2 = true;
                }
                __instance.DirtyCache();
                if ( flag2 && __instance.pawn.apparel != null )
                {
                    __instance.pawn.apparel.Notify_LostBodyPart();
                }
                if ( hediff.def.causesNeed != null && !__instance.pawn.Dead )
                {
                    __instance.pawn.needs.AddOrRemoveNeedsAsAppropriate();
                }



                return false;
            }
        }
    }
}
