using Verse;

namespace OrenoMSE
{
    public class Hediff_BodyPartModule : HediffWithComps
    {
        public override string LabelBase
        {
            get
            {
                return MSE_VanillaExtender.PrettyLabel(this);
            }
        }

        public override bool ShouldRemove
        {
            get
            {
                return false;
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
    }
}
