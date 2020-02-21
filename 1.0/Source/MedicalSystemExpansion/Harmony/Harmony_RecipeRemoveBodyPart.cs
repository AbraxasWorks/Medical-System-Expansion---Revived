using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace OrenoMSE.Harmony
{
    public class Harmony_RecipeRemoveBodyPart
    {
        [HarmonyPatch]
        internal class RecipeRemoveBodyPart_GetPartsToApplyOn
        {
            static MethodBase TargetMethod()
            {
                var type = "RimWorld.Recipe_RemoveBodyPart".ToType();
                return type.MethodNamed("GetPartsToApplyOn");
            }

            [HarmonyPostfix]
            [HarmonyPriority(Priority.First)]
            private static void ExcludePartSystem(ref IEnumerable<BodyPartRecord> __result, Pawn pawn)
            {
                /*
                // Somehow modified original result with convert it to list then modified it resulted
                // in weird result. It's not properly modified. Then this is the temporary solution.
                */
                __result = GetPartsToApplyOn(pawn);
            }

            public static IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn)
            {
                IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
                using (IEnumerator<BodyPartRecord> enumerator = parts.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        BodyPartRecord part = enumerator.Current;
                        var check1 = pawn.health.hediffSet.hediffs.Any((Hediff d) => !d.def.HasComp(typeof(HediffComp_PartSystem)) && d.Part == part);
                        var check2 = pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_SurgerySupport) && d.Part == part);
                        var check3 = pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury || d is Hediff_SurgerySupport) && d.def.isBad && d.Visible && d.Part == part);
                        if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part) && check1 && check2)
                        {
                            if (!pawn.health.hediffSet.AncestorHasDirectlyAddedParts(part))
                            {
                                yield return part;
                            }
                        }
                        else if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part) && !pawn.health.hediffSet.AncestorHasDirectlyAddedParts(part))
                        {
                            yield return part;
                        }
                        else if (part != pawn.RaceProps.body.corePart && part.def.canSuggestAmputation && check3)
                        {
                            yield return part;
                        }
                    }
                }
                yield break;
            }
        }
    }
}
