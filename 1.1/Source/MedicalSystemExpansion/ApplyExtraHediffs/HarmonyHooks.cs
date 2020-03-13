using HarmonyLib;
using Verse;
using RimWorld;


namespace OrenoMSE.ApplyExtraHediffs
{
    static class HarmonyHooks
    {

        [HarmonyPatch( typeof( Hediff_AddedPart ) )]
        [HarmonyPatch( "PostAdd" )]
        internal class HediffAddedPart_PostAdd
        {
            [HarmonyPostfix]
            [HarmonyPriority( Priority.LowerThanNormal )]
            private static void AdditionalHediff ( Hediff_AddedPart __instance )
            {
                Utilities.HediffApplyExtraHediffs( __instance, __instance.pawn, __instance.Part );
            }
        }



        [HarmonyPatch( typeof( Hediff_Implant ) )]
        [HarmonyPatch( "PostAdd" )]
        internal class HediffImplant_PostAdd
        {
            [HarmonyPostfix]
            [HarmonyPriority( Priority.LowerThanNormal )]
            private static void AdditionalHediff ( Hediff_Implant __instance )
            {
                Utilities.HediffApplyExtraHediffs( __instance, __instance.pawn, __instance.Part );
            }
        }


        [HarmonyPatch( typeof( Recipe_InstallNaturalBodyPart ) )]
        [HarmonyPatch( "ApplyOnPawn" )]
        internal class RecipeInstallNaturalBodyPart_ApplyOnPawn
        {
            [HarmonyPostfix]
            [HarmonyPriority( Priority.LowerThanNormal )]
            private static void AdditionalHediff ( Recipe_InstallNaturalBodyPart __instance, Pawn pawn, BodyPartRecord part )
            {
                Utilities.RecipeApplyExtraHediffs( __instance, pawn, part );
            }
        }


    }
}
