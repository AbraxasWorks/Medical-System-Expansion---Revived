using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace OrenoMSE
{
    public static class IncludedPartsUtilities
    {
        /// <summary>
        /// For each of the hediffs in the game, adds it to its children's standard parents cache (as mapped by CompProperties_CompIncludedChildParts of the thingdef)
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
            var comp = parent.spawnThingOnRemoved?.GetCompProperties<CompProperties_CompIncludedChildParts>(); // comp on the corresponding ThingDef
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
    }
}