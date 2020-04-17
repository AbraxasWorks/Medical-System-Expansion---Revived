using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MSE2
{
    public partial class CompIncludedChildParts
    {
        private class Command_SplitOffSubpart : Command
        {
            public CompIncludedChildParts comp;

            public Command_SplitOffSubpart ( CompIncludedChildParts comp )
            {
                this.comp = comp;
                this.icon = comp.parent.def.uiIcon;
                this.iconAngle = comp.parent.def.uiIconAngle;

                this.defaultLabel = "Remove sub-part";
                this.defaultDesc = "Chose a part to remove from this one's subparts.";
            }

            public override bool Visible
            {
                get
                {
                    return this.comp.childPartsIncluded.Count > 0;
                }
            }

            public override void ProcessInput ( Event ev )
            {
                base.ProcessInput( ev );

                List<FloatMenuOption> list = new List<FloatMenuOption>();

                foreach ( Thing thing in comp.childPartsIncluded )
                {
                    list.Add( new FloatMenuOption( thing.Label.CapitalizeFirst(), () => comp.RemoveAndSpawnPart( thing ), thing.def ) );
                }

                Find.WindowStack.Add( new FloatMenu( list ) );
            }

            public override GizmoResult GizmoOnGUI ( Vector2 loc, float maxWidth )
            {
                GizmoResult result = base.GizmoOnGUI( loc, maxWidth );
                if ( MedicalSystemExpansion.WidgetMinusSign != null )
                {
                    Rect rect = new Rect( loc.x, loc.y, this.GetWidth( maxWidth ), 75f );
                    Rect position = new Rect( rect.x + rect.width - 24f, rect.y, 24f, 24f );
                    GUI.DrawTexture( position, MedicalSystemExpansion.WidgetMinusSign );
                }
                return result;
            }
        }
    }
}