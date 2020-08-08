using System.Linq;

using Verse;

namespace MSE2
{
    public abstract class Hediff_ModuleAbstract : Hediff_Implant
    {
        public override void PostAdd ( DamageInfo? dinfo )
        {
            base.PostAdd( dinfo );

            // find a module holder comp in the same bodypart
            this.moduleHolderComp =
                (from c in this.pawn.health.hediffSet.GetAllComps()
                 where c is HediffComp_ModuleHolder
                 where c.parent.Part == this.Part
                 let mh = (HediffComp_ModuleHolder)c
                 where mh.currentModules < mh.Props.maxModules
                 select mh)
                .First();

            this.moduleHolderDiff = this.moduleHolderComp.parent; // store hediff as you can't save the comp
        }

        public HediffComp_ModuleHolder ModuleHolder
        {
            get
            {
                // try to cache it
                if ( moduleHolderComp == null )
                {
                    moduleHolderComp = moduleHolderDiff?.TryGetComp<HediffComp_ModuleHolder>();
                    // if still null call error
                    if ( moduleHolderComp == null )
                    {
                        Log.Error( "[MSE] Null ModuleHolder for module " + this.Label );
                    }
                }

                return moduleHolderComp;
            }
        }

        public override void ExposeData ()
        {
            base.ExposeData();

            Scribe_References.Look( ref moduleHolderDiff, "moduleHolder" ); // need to save the diff as the comp is not referenceable for some reason

            if ( Scribe.mode == LoadSaveMode.PostLoadInit && this.moduleHolderDiff == null )
            {
                Log.Error( "[MSE2] " + this.Label + " has null holder after loading, removing.", false );
                this.pawn.health.hediffSet.hediffs.Remove( this );
                return;
            }
        }

        private HediffWithComps moduleHolderDiff = null;
        private HediffComp_ModuleHolder moduleHolderComp = null;
    }
}