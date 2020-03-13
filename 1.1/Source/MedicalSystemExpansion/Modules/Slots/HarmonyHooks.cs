using HarmonyLib;
using Verse;
using RimWorld;


namespace OrenoMSE.Modules.Slots
{
    static class HarmonyHooks
    {

        // adding the slots with these patches because CompPostPostAdd doesn't seem to work properly


        [HarmonyPatch( typeof( Hediff_AddedPart ) )]
        [HarmonyPatch( "PostAdd" )]
        internal class HediffAddedPart_PostAdd
        {
            [HarmonyPostfix]
            private static void AdditionalHediff ( Hediff_AddedPart __instance )
            {

                if ( __instance is HediffWithComps hc )
                {
                    var comp = hc.TryGetComp<HediffComp_ModuleHolder>();
                    if ( comp != null )
                    {
                        comp.UpdateSlotHediff();
                    }
                }
            }
        }


        [HarmonyPatch( typeof( Hediff_Implant ) )]
        [HarmonyPatch( "PostAdd" )]
        internal class HediffImplant_PostAdd
        {
            [HarmonyPostfix]
            private static void AdditionalHediff ( Hediff_Implant __instance )
            {

                if ( __instance is HediffWithComps hc )
                {
                    var comp = hc.TryGetComp<HediffComp_ModuleHolder>();
                    if ( comp != null )
                    {
                        comp.UpdateSlotHediff();
                    }
                }
            }
        }



    }
}
