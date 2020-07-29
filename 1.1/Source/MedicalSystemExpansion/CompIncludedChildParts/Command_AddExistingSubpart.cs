using RimWorld;
using RimWorld.Planet;
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

                // use same icon as thing it belongs to
                this.icon = comp.parent.def.uiIcon;
                this.iconAngle = comp.parent.def.uiIconAngle;

                this.defaultLabel = "Add sub-part";
                this.defaultDesc = "Chose a part to add to this one's sub-parts.";
            }

            public override bool Visible
            {
                get
                {
                    return this.comp.AllMissingParts.Any();
                }
            }

            public override void ProcessInput ( Event ev )
            {
                base.ProcessInput( ev );

                bool none = true;

                List<FloatMenuOption> list = new List<FloatMenuOption>();

                foreach ( (Thing thingCandidate, CompIncludedChildParts compDestination) in this.PossibleThings )
                {
                    none = false;
                    list.Add( new FloatMenuOption(
                        // name
                        thingCandidate.Label.CapitalizeFirst() + (compDestination != this.comp ? " (to " + compDestination.parent.Label + ")" : ""),
                        () => // click action
                        {
                            compDestination.AddPart( thingCandidate );
                            this.comp.DirtyCacheDeep( compDestination );
                        },
                        // icon
                        thingCandidate.def,
                        MenuOptionPriority.DisabledOption,
                        () => // mouse over action
                        {
                            if ( Current.ProgramState == ProgramState.Playing )
                            {
                                // draw arrow pointing to item
                                TargetHighlighter.Highlight( new GlobalTargetInfo( thingCandidate ) );
                            }
                        }
                    ) );
                }

                // only draw the menu if there are things it can add
                if ( none )
                {
                    Messages.Message( "Could not find a compatible part to add.", MessageTypeDefOf.RejectInput );
                }
                else
                {
                    Find.WindowStack.Add( new FloatMenu( list ) );
                }
            }

            /// <summary>
            /// Returns all things on the map that could be added to this part, and the comp that can accept them
            /// </summary>
            private IEnumerable<(Thing, CompIncludedChildParts)> PossibleThings
            {
                get =>
                    from t in this.comp.parent.Map.listerThings.AllThings
                    from u in this.comp.AllStandardParts.Distinct()
                    where u.Item1 == t.def
                    select (t, u.Item2);
            }

            public override GizmoResult GizmoOnGUI ( Vector2 loc, float maxWidth )
            {
                GizmoResult result = base.GizmoOnGUI( loc, maxWidth );

                // add plus sign in the top right of the gizmo texture
                if ( Assets.WidgetPlusSign != null )
                {
                    Rect rect = new Rect( loc.x, loc.y, this.GetWidth( maxWidth ), 75f );
                    Rect position = new Rect( rect.x + rect.width - 24f, rect.y, 24f, 24f );
                    GUI.DrawTexture( position, Assets.WidgetPlusSign );
                }

                return result;
            }
        }
    }
}