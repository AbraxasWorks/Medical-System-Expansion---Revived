using RimWorld;
using Verse;

namespace OrenoMSE
{
    public class HediffComp_PartSystem : HediffComp
    {
        public HediffCompProperties_PartSystem Props
        {
            get
            {
                return (HediffCompProperties_PartSystem)this.props;
            }
        }

        public void SpecialPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            this.parent.pawn.health.RestorePart(this.parent.Part, this.parent, false);
            for (int i = 0; i < this.parent.Part.parts.Count; i++)
            {
                if (this.parent.Part.Index == 1)
                {
                    Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.parent.pawn, null);
                    hediff_MissingPart.IsFresh = true;
                    hediff_MissingPart.lastInjury = HediffDefOf.SurgicalCut;
                    hediff_MissingPart.Part = this.parent.Part.parts[i];
                    this.parent.pawn.health.hediffSet.AddDirect(hediff_MissingPart, null, null);
                }
                else if (this.Props.generateModule)
                {
                    HediffWithComps hediff_BodyPartModule = (HediffWithComps)HediffMaker.MakeHediff(MSE_HediffDefOf.MSE_BodyPartModule, this.parent.pawn, null);
                    hediff_BodyPartModule.Part = this.parent.Part.parts[i];
                    this.parent.pawn.health.hediffSet.AddDirect(hediff_BodyPartModule, null, null);
                }
            }

            MSE_VanillaExtender.HediffApplyHediffs(this.parent, this.parent.pawn, this.parent.Part);
        }
    }
}
