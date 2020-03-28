using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using RimWorld;
using Verse;


namespace OrenoMSE.PartRemoval
{

    // makes it so parts with implants are considered clean

    [HarmonyPatch(typeof(MedicalRecipesUtility))]
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
                                where !(x is Hediff_Implant) || x is Hediff_AddedPart
                                select x).Any();
        }
    }
}
