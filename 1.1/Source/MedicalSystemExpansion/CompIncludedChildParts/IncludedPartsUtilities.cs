using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MSE2
{
    public static class IncludedPartsUtilities
    {
        /// <summary>
        /// For each of the hediffs in the game, adds it to its children's standard parents cache (as mapped by CompProperties_IncludedChildParts of the thingdef)
        /// </summary>
        public static void CacheAllStandardParents ()
        {
            foreach ( var def in DefDatabase<HediffDef>.AllDefs )
            {
                def.CacheParentOfChildren();
            }
        }

        private static void CacheParentOfChildren ( this HediffDef parent )
        {
            var comp = parent.spawnThingOnRemoved?.GetCompProperties<CompProperties_IncludedChildParts>(); // comp on the corresponding ThingDef
            if ( comp != null && !comp.standardChildren.NullOrEmpty() ) // if it has standard children
            {
                foreach ( var def in from d in DefDatabase<HediffDef>.AllDefs
                                     where d.spawnThingOnRemoved != null
                                     where comp.standardChildren.Contains( d.spawnThingOnRemoved )
                                     select d )
                {
                    def.AddStandardParent( parent ); // add it to the hediffdefs corresponding to the standard children
                }
            }
        }

        private static void AddStandardParent ( this HediffDef hediffDef, HediffDef parent )
        {
            if ( !hediffDef.HasModExtension<MSE_cachedStandardParents>() )
            {
                if ( hediffDef.modExtensions == null ) hediffDef.modExtensions = new List<DefModExtension>();

                hediffDef.modExtensions.Add( new MSE_cachedStandardParents() );
            }

            hediffDef.GetModExtension<MSE_cachedStandardParents>().Add( parent );
        }

        /// <summary>
        /// Checks if the parent part of the hediff has a hediff that is a standard parent of the given one
        /// </summary>
        public static bool IsParentStandard ( this Hediff hediff )
        {
            var modExt = hediff.def.GetModExtension<MSE_cachedStandardParents>();

            return modExt != null && hediff.Part != null && hediff.Part.parent != null
                && hediff.pawn.health.hediffSet.hediffs.Any( h => h.Part == hediff.Part.parent && modExt.Contains( h.def ) );
        }

        // hediff price offset

        public static void FixHediffPriceOffset ()
        {
            foreach ( (HediffDef hediffDef, ThingDef thingDef, CompProperties_IncludedChildParts comp) in
                from hd in DefDatabase<HediffDef>.AllDefs
                let td = hd.spawnThingOnRemoved
                where td != null
                let cpcicp = td.GetCompProperties<CompProperties_IncludedChildParts>()
                where cpcicp != null
                select (hd, td, cpcicp) )
            {
                float childValue = comp.standardChildren.Select( c => c.BaseMarketValue ).Aggregate( ( a, b ) => a + b );

                childValue = Mathf.Min( childValue, thingDef.BaseMarketValue * 0.9f );

                hediffDef.priceOffset += thingDef.BaseMarketValue - childValue;

                //Log.Message( "Reduced value of " + hediffDef.defName + " by " + childValue + ". New value: " + hediffDef.priceOffset );
            }
        }

        public static IEnumerable<RecipeDef> SurgeryToInstall ( ThingDef thing )
        {
            return DefDatabase<RecipeDef>.AllDefs.Where( d => d.IsSurgery && d.fixedIngredientFilter.Allows( thing ) );
        }

        private static Dictionary<ThingDef, List<(BodyDef, BodyPartDef)>> cachedInstallationDestinations = new Dictionary<ThingDef, List<(BodyDef, BodyPartDef)>>();

        public static IEnumerable<(BodyDef, BodyPartDef)> InstallationDestinations ( ThingDef parentDef )
        {
            if ( cachedInstallationDestinations.TryGetValue( parentDef, out List<(BodyDef, BodyPartDef)> val ) )
            {
                return val;
            }
            else
            {
                List<(BodyDef, BodyPartDef)> newVal =
                    (from s in SurgeryToInstall( parentDef )
                     from u in s.AllRecipeUsers
                     let b = u.race.body
                     from bpd in s.appliedOnFixedBodyParts
                     where b.AllParts.Any( bpr => bpr.def == bpd )
                     select (b, bpd))
                    .ToList();

                cachedInstallationDestinations.Add( parentDef, newVal );
                return newVal;
            }
        }

        public static bool InstallationCompatibility ( IEnumerable<Thing> thingDefs, IEnumerable<BodyPartRecord> bodyPartRecords )
        {
            Log.Message( "compat: " + thingDefs.Count() + " " + bodyPartRecords.Count() );

            foreach ( var bpr in bodyPartRecords )
            {
                foreach ( var thing in thingDefs )
                {
                    CompIncludedChildParts comp = thing.TryGetComp<CompIncludedChildParts>();
                    if ( thing.TryGetComp<CompIncludedChildParts>()?.CompatibleParts.Contains( bpr ) ?? // subparts are compatible
                        InstallationDestinations( thing.def ).Any( b_bpd => bpr.body == b_bpd.Item1 && bpr.def == b_bpd.Item2 ) // has no subparts and is compatible
                        && InstallationCompatibility( thingDefs.Except( thing ), bodyPartRecords.Except( bpr ) ) ) // all other things check out
                    {
                        return true;
                    }
                }
            }
            return !bodyPartRecords.Any();
        }

        public static bool EverInstallableOn ( ThingDef thingDef, BodyPartRecord bodyPartRecord )
        {
            return InstallationDestinations( thingDef ).Any( bd_bpd => bd_bpd.Item1 == bodyPartRecord.body && bd_bpd.Item2 == bodyPartRecord.def );
        }
    }
}