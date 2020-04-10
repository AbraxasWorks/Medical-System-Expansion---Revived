using System.Linq;

using Verse;

namespace OrenoMSE.Modules
{
    public abstract class Hediff_ModuleAbstract : HediffWithComps
    {
        public override void PostAdd ( DamageInfo? dinfo )
        {
            base.PostAdd( dinfo );

            this.moduleHolderDiff =
                (from c in this.pawn.health.hediffSet.GetAllComps()
                 where c is HediffComp_ModuleHolder
                 where c.parent.Part == this.Part
                 let mh = (HediffComp_ModuleHolder)c
                 where mh.currentModules < mh.Props.maxModules
                 select mh.parent)
                .First();

            // find a module holder comp in the same bodypart
            this.moduleHolderComp =
                (from c in this.pawn.health.hediffSet.GetAllComps()
                 where c is HediffComp_ModuleHolder
                 where c.parent.Part == this.Part
                 let mh = (HediffComp_ModuleHolder)c
                 where mh.currentModules < mh.Props.maxModules
                 select mh)
                .First();
        }

        public override bool ShouldRemove
        {
            get
            {
                return false;
            }
        }

        public HediffComp_ModuleHolder ModuleHolder
        {
            get
            {
                // try to cache it
                if ( moduleHolderComp == null )
                {
                    moduleHolderComp = moduleHolderDiff.TryGetComp<HediffComp_ModuleHolder>();
                }

                // if still null call error
                if ( moduleHolderComp == null )
                {
                    Log.Error( "[MSE] Null ModuleHolder for module " + this.Label );
                }

                return moduleHolderComp;
            }
        }

        public override void ExposeData ()
        {
            base.ExposeData();

            Scribe_References.Look( ref moduleHolderDiff, "moduleHolder" );
        }

        private HediffWithComps moduleHolderDiff = null;
        private HediffComp_ModuleHolder moduleHolderComp = null;
    }
}