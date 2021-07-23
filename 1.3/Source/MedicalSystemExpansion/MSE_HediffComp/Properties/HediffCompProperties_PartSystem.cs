using Verse;

namespace OrenoMSE
{
    public class HediffCompProperties_PartSystem : HediffCompProperties
    {
        public HediffCompProperties_PartSystem()
        {
            this.compClass = typeof(HediffComp_PartSystem);
        }

        public bool generateModule = true;
    }
}
