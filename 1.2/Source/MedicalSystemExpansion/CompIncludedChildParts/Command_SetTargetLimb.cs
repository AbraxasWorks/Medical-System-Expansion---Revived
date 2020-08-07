﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MSE2
{
    public partial class CompIncludedChildParts
    {
        private class Command_SetTargetLimb : Command
        {
            public CompIncludedChildParts comp;

            public Command_SetTargetLimb ( CompIncludedChildParts comp )
            {
                this.comp = comp;

                // use same icon as thing it belongs to
                this.icon = comp.parent.def.uiIcon;
                this.iconAngle = comp.parent.def.uiIconAngle;

                this.defaultLabel = "Command_SetTargetLimb_Label".Translate();
                this.defaultDesc = "Command_SetTargetLimb_Description".Translate();
            }

            public override bool Visible => base.Visible;

            public override void ProcessInput ( Event ev )
            {
                base.ProcessInput( ev );
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach ( LimbConfiguration possibleTarget in this.comp.Props.installationDestinations )
                {
                    options.Add( new FloatMenuOption(
                        possibleTarget.Label,
                        () => // click action
                        {
                            this.comp.TargetLimb = possibleTarget;
                        }
                        ) );
                }

                // Option to set to no target
                options.Add( new FloatMenuOption(
                    "None",
                    () => // click action
                    {
                        this.comp.TargetLimb = null;
                    }
                    ) );

                Find.WindowStack.Add( new FloatMenu( options ) );
            }
        }
    }
}