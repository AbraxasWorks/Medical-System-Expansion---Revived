using RimWorld;
using Verse;

namespace OrenoMSE
{
    public class Hediff_AddedPartSystemNoModule : Hediff_AddedPart
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            if (base.Part == null)
            {
                Log.Error("Part is null. It should be set before PostAdd for " + this.def + ".", false);
                return;
            }

            this.pawn.health.RestorePart(base.Part, this, false);
            for (int i = 0; i < base.Part.parts.Count; i++)
            {
                if (base.Part.Index == 1)
                {
                    Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.pawn, null);
                    hediff_MissingPart.IsFresh = true;
                    hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                    hediff_MissingPart.Part = base.Part.parts[i];
                    this.pawn.health.hediffSet.AddDirect(hediff_MissingPart, null, null);
                }
            }

            MSE_VanillaExtender.HediffApplyHediffs(this, this.pawn, base.Part);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
            {
                Log.Error("Hediff_AddedPartSystemNoModule has null part after loading.", false);
                this.pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
        }
    }
}
