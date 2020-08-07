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

                this.defaultLabel = "CommandAddExistingSubpart_Label".Translate();
                this.defaultDesc = "CommandAddExistingSubpart_Description".Translate();
            }

            public override bool Visible
            {
                get
                {
                    return comp.AllMissingParts.Any();
                }
            }

            public override void ProcessInput ( Event ev )
            {
                base.ProcessInput( ev );

                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach ( (Thing thingCandidate, CompIncludedChildParts compDestination) in this.PossibleThings )
                {
                    options.Add( new FloatMenuOption(
                        // name
                        compDestination != this.comp ?  // if added to other subpart specify it
                            "CommandAddExistingSubpart_AddTo".Translate( thingCandidate.Label.CapitalizeFirst(), compDestination.parent.Label ).ToString()
                            : thingCandidate.Label.CapitalizeFirst(),
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
                if ( options.Any() )
                {
                    Find.WindowStack.Add( new FloatMenu( options ) );
                }
                else
                {
                    Messages.Message( "CommandAddExistingSubpart_CouldNotFindPart".Translate(), MessageTypeDefOf.RejectInput );
                }
            }

            /// <summary>
            /// Returns all things on the map that could be added to this part, and the comp that can accept them
            /// </summary>
            private IEnumerable<(Thing, CompIncludedChildParts)> PossibleThings
            {
                get =>
                    from t in this.comp.parent.Map.listerThings.AllThings
                    from u in this.comp.AllMissingParts.Distinct()
                    where u.Item1 == t.def
                    select (t, u.Item3);
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