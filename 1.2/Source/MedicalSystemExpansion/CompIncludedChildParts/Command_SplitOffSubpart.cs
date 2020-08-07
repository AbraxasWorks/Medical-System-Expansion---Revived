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

                // use same icon as thing it belongs to
                this.icon = comp.parent.def.uiIcon;
                this.iconAngle = comp.parent.def.uiIconAngle;

                this.defaultLabel = "CommandSplitOffSubpart_Label".Translate();
                this.defaultDesc = "CommandSplitOffSubpart_Description".Translate();
            }

            public override bool Visible
            {
                get
                {
                    return this.comp.AllIncludedParts.Any();
                }
            }

            public override void ProcessInput ( Event ev )
            {
                base.ProcessInput( ev );

                List<FloatMenuOption> list = new List<FloatMenuOption>();

                foreach ( (Thing lthing, CompIncludedChildParts lcomp) in comp.AllIncludedParts )
                {
                    list.Add( new FloatMenuOption(
                        // name
                        lcomp != this.comp ?  // if added to other subpart specify it
                            "CommandSplitOffSubpart_RemoveFrom".Translate( lthing.Label.CapitalizeFirst(), lcomp.parent.Label ).ToString()
                            : lthing.Label.CapitalizeFirst(),
                        () => // click action
                        {
                            lcomp.RemoveAndSpawnPart( lthing, comp.parent.Position, comp.parent.Map );
                            comp.DirtyCacheDeep( lcomp );
                        },
                        // icon
                        lthing.def ) );
                }

                Find.WindowStack.Add( new FloatMenu( list ) );
            }

            public override GizmoResult GizmoOnGUI ( Vector2 loc, float maxWidth )
            {
                GizmoResult result = base.GizmoOnGUI( loc, maxWidth );

                // add minus sign in the top right of the gizmo texture
                if ( Assets.WidgetMinusSign != null )
                {
                    Rect rect = new Rect( loc.x, loc.y, this.GetWidth( maxWidth ), 75f );
                    Rect position = new Rect( rect.x + rect.width - 24f, rect.y, 24f, 24f );
                    GUI.DrawTexture( position, Assets.WidgetMinusSign );
                }

                return result;
            }
        }
    }
}