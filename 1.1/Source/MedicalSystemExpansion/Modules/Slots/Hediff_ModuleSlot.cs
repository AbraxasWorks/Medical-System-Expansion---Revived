using Verse;

namespace MSE2
{
    public class Hediff_ModuleSlot : Hediff_ModuleAbstract
    {
        public override string LabelBase
        {
            get
            {
                return base.LabelBase + (AvailableSlots != 1 ? (" (" + AvailableSlots + " available)") : "");
            }
        }

        public override void PostAdd ( DamageInfo? dinfo )
        {
            base.PostAdd( dinfo );

            base.ModuleHolder.moduleSlot = this;
        }

        public int AvailableSlots
        {
            get
            {
                return ModuleHolder.Props.maxModules - ModuleHolder.currentModules;
            }
        }
    }
}