using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrenoMSE.HarmonyPatches
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
                //IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
                foreach ( BodyPartRecord bodyPart in pawn.health.hediffSet.GetNotMissingParts() )
                {
                    // The bodypart does not have a hediff with a part system component
                    var check1 = pawn.health.hediffSet.hediffs.Any((Hediff d) => !d.def.HasComp(typeof(HediffComp_PartSystem)) && d.Part == bodyPart);
                    // The bodypart does not have a SurgerySupport hediff
                    var check2 = pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_SurgerySupport) && d.Part == bodyPart);
                    // The bodypart has a BAD, VISIBLE hediff that is not an injury or surgerysupport
                    var check3 = pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury || d is Hediff_SurgerySupport) && d.def.isBad && d.Visible && d.Part == bodyPart);

                    if (pawn.health.hediffSet.HasDirectlyAddedPartFor(bodyPart) && check1 && check2) // has prosthetic, no partsystem and no surgerysupport
                    {
                        //if (!pawn.health.hediffSet.AncestorHasDirectlyAddedParts(bodyPart)) // is not a child of a prosthetic
                        //{
                        //    yield return bodyPart;
                        //}
                        yield return bodyPart;
                    }
                    else if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, bodyPart)/* && !pawn.health.hediffSet.AncestorHasDirectlyAddedParts(bodyPart)*/) // has no hediffs and is not a child of prosthetic
                    {
                        yield return bodyPart; // clean natural bodypart
                    }
                    else if (bodyPart != pawn.RaceProps.body.corePart && bodyPart.def.canSuggestAmputation && check3) // can amputate due to bad hediff
                    {
                        yield return bodyPart; 
                    }
                }
                yield break;
            }
        }
    }
}
