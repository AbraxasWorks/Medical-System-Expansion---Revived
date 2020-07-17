using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2
{
    public class CompProperties_IncludedChildParts : CompProperties
    {
        public CompProperties_IncludedChildParts ()
        {
            this.compClass = typeof( CompIncludedChildParts );
        }

        public override IEnumerable<string> ConfigErrors ( ThingDef parentDef )
        {
            foreach ( var entry in base.ConfigErrors( parentDef ) )
                yield return entry;

            // warning for empy comp
            if ( standardChildren.NullOrEmpty() )
            {
                yield return "[MSE] CompIncludedChildParts on " + parentDef.defName + " has no children";
            }
        }

        public List<ThingDef> standardChildren = new List<ThingDef>();

        public List<ThingDef> alwaysInclude;
    }
}