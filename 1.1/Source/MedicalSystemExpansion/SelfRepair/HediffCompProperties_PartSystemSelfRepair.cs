using Verse;

namespace OrenoMSE
{
    public class HediffCompProperties_PartSystemSelfRepair : HediffCompProperties
    {
        public HediffCompProperties_PartSystemSelfRepair()
        {
            this.compClass = typeof(HediffComp_PartSystemSelfRepair);
        }

        public string repairLabel;
    }
}
