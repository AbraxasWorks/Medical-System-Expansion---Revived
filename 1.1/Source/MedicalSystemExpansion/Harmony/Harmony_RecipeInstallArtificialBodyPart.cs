using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace OrenoMSE.Harmony
{
    public class Harmony_RecipeInstallArtificialBodyPart
    {
        [HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart))]
        [HarmonyPatch("GetPartsToApplyOn")]
        internal class RecipeInstallArtificialBodyPart_GetPartsToApplyOn
        {
            [HarmonyPostfix]
            private static void ExcludePartSystem(ref IEnumerable<BodyPartRecord> __result, Pawn pawn, RecipeDef recipe)
            {
                List<BodyPartRecord> bpList = __result.ToList();
                for (int i = 0; i < bpList.Count; i++)
                {
                    BodyPartRecord record = bpList[i];
                    var check1 = pawn.health.hediffSet.hediffs.Any((Hediff d) => (d is Hediff_BodyPartModule) && d.Part == record);
                    var check2 = pawn.health.hediffSet.hediffs.Any((Hediff d) => d.def.HasComp(typeof(HediffComp_PartModule)) && d.Part == record);
                    if (check1 || check2)
                    {
                        bpList.Remove(record);
                    }
                }
                __result = bpList.AsEnumerable();
            }
        }
    }
}
