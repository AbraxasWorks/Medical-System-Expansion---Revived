using Verse;

namespace MSE2
{
    public class Hediff_ModuleAdded : Hediff_ModuleAbstract
    {
        public override void PostAdd ( DamageInfo? dinfo )
        {
            base.PostAdd( dinfo );

            this.ModuleHolder.Notify_ModuleAdded();
        }

        public override void PostRemoved ()
        {
            base.PostRemoved();

            this.ModuleHolder.Notify_ModuleRemoved();
        }
    }
}