using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrenoMSE
{
    public class HediffComp_Painstopper : HediffComp_HediffGizmo
    {
        public HediffCompProperties_Painstopper Props
        {
            get
            {
                return (HediffCompProperties_Painstopper)this.props;
            }
        }

        public bool Painstopper
        {
            get
            {
                return this.painstopperActive;
            }
            set
            {
                if (value == painstopperActive)
                {
                    return;
                }
                this.painstopperActive = value;
                if (value)
                {
                    this.parent.Severity = 0.5f;
                }
                else
                {
                    this.parent.Severity = 1.0f;
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            Command_Toggle painstopper = new Command_Toggle
            {
                isActive = (() => this.Painstopper),
                toggleAction = delegate ()
                {
                    this.Painstopper = !this.Painstopper;
                },
                defaultLabel = this.Props.label,
                defaultDesc = this.Props.description.CapitalizeFirst(),
                icon = MSE_VanillaExtender.GetIcon(base.Pawn.GetUniqueLoadID() + "_" + this.parent.GetUniqueLoadID(), this.Props.uiIconPath)
            };
            if (base.Pawn.Faction != Faction.OfPlayer)
            {
                painstopper.Disable("CannotOrderNonControlled".Translate());
            }
            if (base.Pawn.Downed)
            {
                painstopper.Disable("IsIncapped".Translate(base.Pawn.LabelShort, base.Pawn));
            }
            yield return painstopper;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref this.painstopperActive, "painstopperActive", true, false);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            MSE_VanillaExtender.ClearIcon(base.Pawn.GetUniqueLoadID() + "_" + this.parent.GetUniqueLoadID());
        }

        private bool painstopperActive = true;
    }
}
