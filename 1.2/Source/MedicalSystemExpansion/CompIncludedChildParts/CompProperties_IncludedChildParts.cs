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

            installationDestinations = IncludedPartsUtilities.InstallationDestinations( parentDef ).ToList();
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

        public bool EverInstallableOn ( BodyPartRecord bodyPartRecord )
        {
            return installationDestinations.Contains( (bodyPartRecord.body, bodyPartRecord.def) );
        }

        public IEnumerable<(ThingDef, BodyPartRecord)> StandardPartsForBodyPartRecord ( BodyPartRecord bodyPartRecord )
        {
            if ( !this.EverInstallableOn( bodyPartRecord ) )
            {
                Log.Error( "[MSE2] Tried to get standard parts of " + parentDef.defName + " for an incompatible part record (" + bodyPartRecord + ")" );
                yield break;
            }

            List<BodyPartDef> ignoredParts = new List<BodyPartDef>(
                DefDatabase<HediffDef>.AllDefs.First( h => h.spawnThingOnRemoved == this.parentDef ).GetModExtension<IgnoreSubParts>()?.ignoredSubParts
                ?? Enumerable.Empty<BodyPartDef>() );

            foreach ( var bpr in bodyPartRecord.GetDirectChildParts().Where( p => !ignoredParts.Contains( p.def ) ) )
            {
                var thingDef = standardChildren.Where( td => IncludedPartsUtilities.InstallationDestinations( td ).Contains( (bpr.body, bpr.def) ) ).FirstOrDefault();
                if ( thingDef != null )
                {
                    yield return (thingDef, bpr);
                }
                else
                {
                    Log.Error( "[MSE2] Could not find a standard child of " + parentDef.defName + " compatible with body part record " + bpr );
                }
            }
        }

        public IEnumerable<ThingDef> AllPartsForBodyPartRecord ( BodyPartRecord bodyPartRecord )
        {
            foreach ( (ThingDef thingDef, BodyPartRecord part) in this.StandardPartsForBodyPartRecord( bodyPartRecord ) )
            {
                yield return thingDef;

                var comp = thingDef.GetCompProperties<CompProperties_IncludedChildParts>();

                if ( comp != null )
                {
                    foreach ( var item in comp.AllPartsForBodyPartRecord( part ) )
                    {
                        yield return item;
                    }
                }
            }
        }

        public ThingDef parentDef;

        public List<(BodyDef, BodyPartDef)> installationDestinations;

        public List<ThingDef> standardChildren = new List<ThingDef>();

        public List<ThingDef> alwaysInclude;
    }
}