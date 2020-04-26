using System;
using System.Collections.Generic;
using System.Linq;

using Verse;

namespace MSE2
{
    internal static class IgnoreSubPartsUtilities
    {
        public static bool PartShouldBeIgnored ( this HediffSet set, BodyPartRecord bodyPart )
        {
            if ( bodyPart != null && bodyPart.parent != null )
            {
                var modExt = set.GetHediffs<Hediff_AddedPart>()
                    .FirstOrDefault( h => h.Part == bodyPart.parent )? // added part on parent bodypartrecord
                    .def.GetModExtension<IgnoreSubParts>();

                return
                    (modExt != null && modExt.ignoredSubParts.Contains( bodyPart.def ))
                    || set.PartShouldBeIgnored( bodyPart.parent );
            }
            return false;
        }

        public static IEnumerable<BodyPartRecord> AllChildParts ( this BodyPartRecord bodyPart )
        {
            foreach ( var p in bodyPart.parts )
            {
                yield return p;
                foreach ( var p2 in p.AllChildParts() )
                    yield return p2;
            }
            yield break;
        }

        private static List<BodyPartDef> AllChildPartDefs ( this BodyPartDef bodyPartDef )
        {
            List<BodyPartDef> list = new List<BodyPartDef>();

            foreach ( var bodyDef in DefDatabase<BodyDef>.AllDefs )
            {
                foreach ( var partRecord in bodyDef.AllParts )
                {
                    if ( partRecord.def == bodyPartDef )
                    {
                        list.AddRange( 
                            partRecord.AllChildParts()
                            .Select( r => r.def )
                            .Where( d => !list.Contains( d ) ) );
                    }
                }
            }

            return list;
        }

        public static void IgnoreAllNonCompedSubparts ()
        {
            bool missingAny = false;

            foreach ( RecipeDef recipeDef in
                from r in DefDatabase<RecipeDef>.AllDefs
                where r.IsSurgery
                where
                    // has ingredients
                    r.fixedIngredientFilter?.AllowedThingDefs != null
                    // no ingredient has a CompProperties_IncludedChildParts
                    && !r.fixedIngredientFilter.AllowedThingDefs
                        .Select( t => t.GetCompProperties<CompProperties_IncludedChildParts>() )
                        .Any( c => c != null )
                where
                    // adds a hediff
                    r.addsHediff?.hediffClass != null
                    // of type addedpart
                    && typeof( Hediff_AddedPart ).IsAssignableFrom( r.addsHediff.hediffClass )
                    // that does not already ignore parts
                    && !r.addsHediff.HasModExtension<IgnoreSubParts>()
                select r )
            {
                var modExt = new IgnoreSubParts();

                // add all the subparts this prosthesis could have
                foreach ( BodyPartDef partDef in recipeDef.appliedOnFixedBodyParts ?? Enumerable.Empty<BodyPartDef>() )
                {
                    if ( modExt.ignoredSubParts == null )
                        modExt.ignoredSubParts = new List<BodyPartDef>();

                    modExt.ignoredSubParts.AddRange( partDef.AllChildPartDefs() );
                }

                // found any
                if ( !modExt.ignoredSubParts.NullOrEmpty() )
                {
                    Log.Message( "[MSE2] Part " + recipeDef.addsHediff.label + " has no standard subparts. Automatically ignoring: " + string.Join( ", ", modExt.ignoredSubParts.Select( p => p.label ) ) );

                    if ( recipeDef.addsHediff.modExtensions == null )
                        recipeDef.addsHediff.modExtensions = new List<DefModExtension>();

                    // add the modextension
                    recipeDef.addsHediff.modExtensions.Add( modExt );

                    missingAny = true;
                }
            }

            if ( missingAny )
            {
                Log.Warning( "[MSE2] Some prostheses that have not been patched were detected. They will default to vanilla behaviour." );
            }
        }
    }
}