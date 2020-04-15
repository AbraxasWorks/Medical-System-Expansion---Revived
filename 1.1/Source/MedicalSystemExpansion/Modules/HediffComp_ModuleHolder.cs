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

        // overriding comppostpostadd doesn't seem to work properly
        public void UpdateSlotHediff ()
        {
            // has slots available
            if ( this.Props.maxModules - currentModules > 0 )
            {
                if ( moduleSlot == null )
                {
                    //Log.Message( "Adding slot " + this.parent.pawn.Name + " " + this.parent.Part.Label );

                    // create the slot

                    moduleSlot = (Hediff_ModuleSlot)HediffMaker.MakeHediff( MSE_HediffDefOf.MSE_ModuleSlot, this.parent.pawn, this.parent.Part );

                    this.parent.pawn.health.AddHediff( moduleSlot );
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

            //Log.Message( "Added module to " + this.parent.Label );

            if ( currentModules > this.Props.maxModules )
            {
                Log.Error( "[MSE] Added too many modules to part " + this.parent.Label );
            }

            UpdateSlotHediff();
        }

        public void Notify_ModuleRemoved ()
        {
            currentModules--;

            //Log.Message( "Removed module from " + this.parent.Label );

            UpdateSlotHediff();
        }

        public override void CompPostPostRemoved ()
        {
            base.CompPostPostRemoved();
        }

        public override void CompExposeData ()
        {
            base.CompExposeData();

            Scribe_Values.Look( ref currentModules, "currentModules" );

            Scribe_References.Look( ref moduleSlot, "moduleSlot" );
        }

        public int currentModules;

        private Hediff_ModuleSlot moduleSlot;
    }
}