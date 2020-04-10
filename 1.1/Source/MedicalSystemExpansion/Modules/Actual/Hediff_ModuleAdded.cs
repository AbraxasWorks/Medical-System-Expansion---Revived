using Verse;

namespace OrenoMSE.Modules.Actual
{
    public class Hediff_ModuleAdded : Hediff_ModuleAbstract
    {
        public override void PostAdd ( DamageInfo? dinfo )
        {
            base.PostAdd( dinfo );

            if ( base.Part == null )
            {
                Log.Error( "Part module is null. It should be set before PostAdd for " + this.def + ".", false );
                return;
            }

            ApplyExtraHediffs.Utilities.HediffApplyExtraHediffs( this, this.pawn, base.Part );

            this.ModuleHolder.Notify_ModuleAdded();
        }

        public override void PostRemoved ()
        {
            base.PostRemoved();

            this.ModuleHolder.Notify_ModuleRemoved();
        }

        public override void ExposeData ()
        {
            base.ExposeData();

            if ( Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null )
            {
                Log.Error( "Hediff_AddedPartModule has null part after loading.", false );
                this.pawn.health.hediffSet.hediffs.Remove( this );
                return;
            }
        }
    }
}