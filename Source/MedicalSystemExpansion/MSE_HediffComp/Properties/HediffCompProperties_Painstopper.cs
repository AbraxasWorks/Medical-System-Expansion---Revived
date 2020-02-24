using Verse;

namespace OrenoMSE
{
    public class HediffCompProperties_Painstopper : HediffCompProperties
    {
        public HediffCompProperties_Painstopper()
        {
            this.compClass = typeof(HediffComp_Painstopper);
        }

        public string label;

        [MustTranslate]
        public string description = "No description.";

        [NoTranslate]
        public string uiIconPath;
    }
}
