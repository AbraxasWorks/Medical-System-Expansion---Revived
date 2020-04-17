using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MSE2
{
    public partial class CompIncludedChildParts
    {
        private class Command_AddExistingSubpart : Command
        {
            public CompIncludedChildParts comp;

            public Command_AddExistingSubpart ( CompIncludedChildParts comp )
            {
                this.comp = comp;
                this.icon = comp.parent.def.uiIcon;
                this.iconAngle = comp.parent.def.uiIconAngle;

                this.defaultLabel = "Add part";
                this.defaultDesc = "Chose a part to add to this one's subparts.";
            }

            public override bool Visible
            {
                get
                {
                    return this.comp.MissingParts.Count > 0;
                }
            }

            public override void ProcessInput ( Event ev )
            {
                base.ProcessInput( ev );

                List<FloatMenuOption> list = new List<FloatMenuOption>();

                foreach ( Thing thing in PossibleThings )
                {
                    list.Add( new FloatMenuOption( thing.Label.CapitalizeFirst(), () => comp.AddPart( thing ) ) );
                }

                Find.WindowStack.Add( new FloatMenu( list ) );
            }

            private IEnumerable<Thing> PossibleThings
            {
                get
                {
                    return comp.parent.Map.listerThings.AllThings.Where( t => comp.MissingParts.Contains( t.def ) );
                }
            }
        }
    }
}