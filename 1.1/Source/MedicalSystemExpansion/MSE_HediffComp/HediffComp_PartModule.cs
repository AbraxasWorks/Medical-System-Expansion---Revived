using Verse;

namespace OrenoMSE
{
    public class HediffComp_PartModule : HediffComp
    {
        public HediffCompProperties_PartModule Props
        {
            get
            {
                return (HediffCompProperties_PartModule)this.props;
            }
        }

        public void SpecialPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            this.parent.pawn.health.RestorePart(this.parent.Part, this.parent, false);
            for (int i = 0; i < this.parent.Part.parts.Count; i++)
            {
                HediffWithComps hediff_BodyPartModule = (HediffWithComps)HediffMaker.MakeHediff(MSE_HediffDefOf.MSE_BodyPartModule, this.parent.pawn, null);
                hediff_BodyPartModule.Part = this.parent.Part.parts[i];
                this.parent.pawn.health.hediffSet.AddDirect(hediff_BodyPartModule, null, null);
            }

            MSE_VanillaExtender.HediffApplyHediffs(this.parent, this.parent.pawn, this.parent.Part);
        }
    }
}
