using RimWorld;
using Verse;

namespace MSE2
{
    [DefOf]
    public static class MSE_HediffDefOf
    {
        static MSE_HediffDefOf ()
        {
            DefOfHelper.EnsureInitializedInCtor( typeof( MSE_HediffDefOf ) );
        }

        public static HediffDef MSE_ModuleSlot;
    }
}