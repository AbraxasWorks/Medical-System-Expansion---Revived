using Verse;

namespace OrenoMSE
{
    public class HediffComp_PartSystemSelfRepair : HediffComp
    {
        public HediffCompProperties_PartSystemSelfRepair Props
        {
            get
            {
                return (HediffCompProperties_PartSystemSelfRepair)this.props;
            }
        }

        public bool HasInjury
        {
            get
            {
                if (MSE_VanillaExtender.PartHasInjury(base.Pawn, base.parent.Part, true))
                {
                    return true;
                }
                return false;
            }
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (this.HasInjury)
                {
                    return this.Props.repairLabel;
                }
                return null;
            }
        }

        public override TextureAndColor CompStateIcon
        {
            get
            {
                if (this.HasInjury)
                {
                    return MedicalSystemExpansion.IconPartSystemDamaged;
                }
                return MedicalSystemExpansion.IconPartSystem;
            }
        }
    }
}
