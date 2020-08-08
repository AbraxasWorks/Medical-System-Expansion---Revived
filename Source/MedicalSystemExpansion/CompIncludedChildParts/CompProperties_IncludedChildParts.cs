using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using Verse;

namespace MSE2
{
    public class CompProperties_IncludedChildParts : CompProperties
    {
        public CompProperties_IncludedChildParts ()
        {
            this.compClass = typeof( CompIncludedChildParts );
        }

        public override void ResolveReferences ( ThingDef parentDef )
        {
            base.ResolveReferences( parentDef );

            this.parentDef = parentDef;

            installationDestinations = IncludedPartsUtilities.CachedInstallationDestinations( parentDef ).ToList();
        }

        public override IEnumerable<string> ConfigErrors ( ThingDef parentDef )
        {
            foreach ( var entry in base.ConfigErrors( parentDef ) )
                yield return entry;

            if ( this.parentDef != parentDef )
            {
                yield return "ParentDefs do not match (should never happen wtf, did you manually call this function or ResolveReferences?)";
            }

            // warning for never installable
            if ( installationDestinations.NullOrEmpty() )
            {
                yield return parentDef.defName + " will never be installable anywhere";
            }

            // warning for empy comp
            if ( standardChildren.NullOrEmpty() )
            {
                yield return "CompIncludedChildParts on " + parentDef.defName + " has no children";
            }
        }

        public bool EverInstallableOn ( LimbConfiguration limb )
        {
            return installationDestinations.Contains( limb );
        }

        public IEnumerable<(ThingDef, LimbConfiguration)> StandardPartsForLimb ( LimbConfiguration limb )
        {
            if ( !this.EverInstallableOn( limb ) )
            {
                Log.Error( "[MSE2] Tried to get standard parts of " + parentDef.defName + " for an incompatible part record (" + limb + ")" );
                yield break;
            }

            List<BodyPartDef> ignoredParts = new List<BodyPartDef>(
                DefDatabase<HediffDef>.AllDefs.First( h => h.spawnThingOnRemoved == this.parentDef ).GetModExtension<IgnoreSubParts>()?.ignoredSubParts
                ?? Enumerable.Empty<BodyPartDef>() );

            foreach ( var lc in limb.ChildLimbs.Where( p => !ignoredParts.Contains( p.PartDef ) ) )
            {
                var thingDef = standardChildren.Where( td => IncludedPartsUtilities.CachedInstallationDestinations( td ).Contains( lc ) ).FirstOrDefault();
                if ( thingDef != null )
                {
                    yield return (thingDef, lc);
                }
                else
                {
                    Log.Error( "[MSE2] Could not find a standard child of " + parentDef.defName + " compatible with body part record " + lc );
                }
            }
        }

        public IEnumerable<ThingDef> AllPartsForLimb ( LimbConfiguration limb )
        {
            foreach ( (ThingDef thingDef, LimbConfiguration childLimb) in this.StandardPartsForLimb( limb ) )
            {
                yield return thingDef;

                var comp = thingDef.GetCompProperties<CompProperties_IncludedChildParts>();

                if ( comp != null )
                {
                    foreach ( var item in comp.AllPartsForLimb( childLimb ) )
                    {
                        yield return item;
                    }
                }
            }
        }

        public ThingDef parentDef;

        public List<LimbConfiguration> installationDestinations;

        public List<ThingDef> standardChildren = new List<ThingDef>();

        public List<ThingDef> alwaysInclude;
    }
}