using Verse;

namespace MSE2
{
    public class HediffComp_ModuleHolder : HediffComp
    {
        public HediffCompProperties_ModuleHolder Props
        {
            get
            {
                return (HediffCompProperties_ModuleHolder)this.props;
            }
        }

        public override void CompPostPostAdd ( DamageInfo? dinfo )
        {
            base.CompPostPostAdd( dinfo );

            this.currentModules = 0;

            UpdateSlotHediff();
        }

        public void UpdateSlotHediff ()
        {
            // has slots available
            if ( this.Props.maxModules - currentModules > 0 )
            {
                if ( moduleSlot == null )
                {
                    // create the slot
                    this.parent.pawn.health.AddHediff( MSE_HediffDefOf.MSE_ModuleSlot, this.parent.Part );
                }
            }
            // else remove eventual slot
            else if ( moduleSlot != null )
            {
                parent.pawn.health.RemoveHediff( moduleSlot );
                moduleSlot = null;
            }
        }

        public void Notify_ModuleAdded ()
        {
            currentModules++;

            if ( currentModules > this.Props.maxModules )
            {
                Log.Error( "[MSE2] Added too many modules to part " + this.parent.Label );
            }

            UpdateSlotHediff();
        }

        public void Notify_ModuleRemoved ()
        {
            currentModules--;

            UpdateSlotHediff();
        }

        public override void CompPostPostRemoved ()
        {
            base.CompPostPostRemoved();

            if ( moduleSlot != null )
            {
                parent.pawn.health.RemoveHediff( moduleSlot );
                moduleSlot = null;
            }
        }

        public override void CompExposeData ()
        {
            base.CompExposeData();

            Scribe_Values.Look( ref currentModules, "currentModules" );

            Scribe_References.Look( ref moduleSlot, "moduleSlot" );
        }

        public int currentModules;

        public Hediff_ModuleSlot moduleSlot;
    }
}