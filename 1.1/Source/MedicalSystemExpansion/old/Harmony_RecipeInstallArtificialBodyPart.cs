namespace OrenoMSE.HarmonyPatches
{
    //public class Harmony_RecipeInstallArtificialBodyPart
    //{
    //    [HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart))]
    //    [HarmonyPatch("GetPartsToApplyOn")]
    //    internal class RecipeInstallArtificialBodyPart_GetPartsToApplyOn
    //    {
    //        [HarmonyPostfix]
    //        private static void ExcludePartSystem(ref IEnumerable<BodyPartRecord> __result, Pawn pawn, RecipeDef recipe)
    //        {
    //            List<BodyPartRecord> newRes = new List<BodyPartRecord>();

    //            foreach ( BodyPartRecord bodypart in __result )
    //            {
    //                var check1 = pawn.health.hediffSet.hediffs.Any((Hediff d) => (d is Hediff_BodyPartModule) && d.Part == bodypart );
    //                var check2 = pawn.health.hediffSet.hediffs.Any((Hediff d) => d.def.HasComp(typeof(HediffComp_PartModule)) && d.Part == bodypart );
    //                if ( !(check1 || check2) )
    //                {
    //                    newRes.Add( bodypart );
    //                }
    //            }
    //            __result = newRes.AsEnumerable();
    //        }
    //    }
    //}
}