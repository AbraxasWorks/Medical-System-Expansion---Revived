using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace MSE2.HarmonyPatches
{
    // makes it so parts with implants are considered clean

    [HarmonyPatch( typeof( MedicalRecipesUtility ) )]
    [HarmonyPatch( "IsClean" )]
    public static class IsClean_Patch
    {
        [HarmonyPostfix]
        public static void PostFix ( ref bool __result, Pawn pawn, BodyPartRecord part )
        {
            // pawn is alive and the part doesn't have any diff that is not an implant
            __result =
                __result ||
                !pawn.Dead && !(from x in pawn.health.hediffSet.hediffs
                                where x.Part == part
                                where !(x is Hediff_Implant) || x is Hediff_AddedPart || x is Hediff_ModuleAbstract
                                select x).Any();
        }
    }
}