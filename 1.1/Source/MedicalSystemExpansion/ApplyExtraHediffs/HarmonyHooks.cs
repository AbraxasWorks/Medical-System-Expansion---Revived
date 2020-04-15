//using HarmonyLib;
//using RimWorld;
//using Verse;

//namespace MSE2.ApplyExtraHediffs
//{
//    internal static class HarmonyHooks
//    {
//        [HarmonyPatch( typeof( Recipe_InstallNaturalBodyPart ) )]
//        [HarmonyPatch( "ApplyOnPawn" )]
//        internal class RecipeInstallNaturalBodyPart_ApplyOnPawn
//        {
//            [HarmonyPostfix]
//            [HarmonyPriority( Priority.LowerThanNormal )]
//            private static void AdditionalHediff ( Recipe_InstallNaturalBodyPart __instance, Pawn pawn, BodyPartRecord part )
//            {
//                Utilities.RecipeApplyExtraHediffs( __instance, pawn, part );
//            }
//        }
//    }
//}