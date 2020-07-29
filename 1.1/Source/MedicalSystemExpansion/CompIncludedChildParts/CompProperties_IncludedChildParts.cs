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

            installationDestinations = IncludedPartsUtilities.InstallationDestinations( parentDef ).ToList();
        }

        public override IEnumerable<string> ConfigErrors ( ThingDef parentDef )
        {
            foreach ( var entry in base.ConfigErrors( parentDef ) )
                yield return entry;

            // warning for never installable
            if ( installationDestinations.NullOrEmpty() )
            {
                yield return "[MSE] " + parentDef.defName + " will never be installable anywhere";
            }

            // warning for empy comp
            if ( standardChildren.NullOrEmpty() )
            {
                yield return "[MSE] CompIncludedChildParts on " + parentDef.defName + " has no children";
            }
        }

        public bool EverInstallableOn(BodyPartRecord bodyPartRecord)
        {
            return installationDestinations.Contains( (bodyPartRecord.body, bodyPartRecord.def) );
        }

        public IEnumerable<(ThingDef, BodyPartRecord)> StandardPartsForBodyPartRecord ( BodyPartRecord bodyPartRecord )
        {
            if(!this.EverInstallableOn(bodyPartRecord))
            {
                Log.Error( "[MSE2] Tried to get standard parts for an incompatible part record (" + bodyPartRecord + ")" );
                yield break;
            }

            foreach ( var bpr in bodyPartRecord.GetDirectChildParts() )
            {
                var thingDef = standardChildren.Where( td => IncludedPartsUtilities.InstallationDestinations( td ).Contains( (bpr.body, bpr.def) ) ).FirstOrDefault();
                if ( thingDef != null)
                {
                    yield return (thingDef, bpr);
                }
                else
                {
                    Log.Error( "[MSE2] Could not find a standard child compatible with body part record " + bpr );
                }
            }
        }

        public IEnumerable<ThingDef> AllPartsForBodyPartRecord ( BodyPartRecord bodyPartRecord )
        {
            foreach ( ( ThingDef thingDef, BodyPartRecord part) in this.StandardPartsForBodyPartRecord(bodyPartRecord) )
            {
                yield return thingDef;

                var comp = thingDef.GetCompProperties<CompProperties_IncludedChildParts>();

                if ( comp != null )
                {
                    foreach ( var item in comp.AllPartsForBodyPartRecord(part) )
                    {
                        yield return item;
                    }
                }
            }
        }

        public List<(BodyDef, BodyPartDef)> installationDestinations;

        public List<ThingDef> standardChildren = new List<ThingDef>();

        public List<ThingDef> alwaysInclude;
    }
}