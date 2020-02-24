using Verse;

namespace OrenoMSE
{
    public class Hediff_AddedPartModule : HediffWithComps
    {
        public override bool ShouldRemove
        {
            get
            {
                return false;
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            if (base.Part == null)
            {
                Log.Error("Part module is null. It should be set before PostAdd for " + this.def + ".", false);
                return;
            }

            this.pawn.health.RestorePart(base.Part, this, false);
            for (int i = 0; i < base.Part.parts.Count; i++)
            {
                Hediff_BodyPartModule hediff_BodyPartModule = (Hediff_BodyPartModule)HediffMaker.MakeHediff(MSE_HediffDefOf.MSE_BodyPartModule, this.pawn, null);
                hediff_BodyPartModule.Part = base.Part.parts[i];
                this.pawn.health.hediffSet.AddDirect(hediff_BodyPartModule, null, null);
            }

            MSE_VanillaExtender.HediffApplyHediffs(this, this.pawn, base.Part);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
            {
                Log.Error("Hediff_AddedPartModule has null part after loading.", false);
                this.pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
        }
    }
}
