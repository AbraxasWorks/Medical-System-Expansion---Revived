using Verse;

namespace OrenoMSE.Modules.Slots
{
    public class Hediff_ModuleSlot : Hediff_ModuleAbstract
    {
        public override string LabelBase
        {
            get
            {
                return MSE_VanillaExtender.PrettyLabel(this) + ( AvailableSlots != 1 ? ( " (" + AvailableSlots + " available)" ) : "");
            }
        }



        public override void ExposeData()
        {
            base.ExposeData();
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
            {
                Log.Error("Hediff_BodyPartModule has null part after loading.", false);
                this.pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
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
