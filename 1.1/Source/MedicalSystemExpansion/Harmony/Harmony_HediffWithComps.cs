using HarmonyLib;
using Verse;

namespace OrenoMSE.HarmonyPatches
{
    public class Harmony_HediffWithComps
    {
        [HarmonyPatch(typeof(HediffWithComps))]
        [HarmonyPatch("PostAdd")]
        internal class HediffWithComps_PostAdd
        {
            //[HarmonyPostfix]
            //[HarmonyPriority(Priority.First)]
            //private static void PartSystem(HediffWithComps __instance, DamageInfo? dinfo)
            //{
            //    HediffComp_PartModule partModule = __instance.TryGetComp<HediffComp_PartModule>();
            //    if (partModule != null)
            //    {
            //        partModule.SpecialPostAdd(dinfo);
            //    }
            //}
        }
    }
}
