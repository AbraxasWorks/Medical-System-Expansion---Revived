using Verse;

namespace OrenoMSE
{
    public class Hediff_SurgerySupport : HediffWithComps
    {
        public override string LabelBase
        {
            get
            {
                return MSE_VanillaExtender.PrettyLabel(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
            {
                Log.Error("Hediff_SurgerySupport has null part after loading.", false);
                this.pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
        }
    }
}
