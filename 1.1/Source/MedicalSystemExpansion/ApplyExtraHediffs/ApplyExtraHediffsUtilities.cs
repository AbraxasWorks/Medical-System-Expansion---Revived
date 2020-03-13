using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;

namespace OrenoMSE.ApplyExtraHediffs
{
    static class Utilities
    {

        public static void RecipeApplyExtraHediffs ( Recipe_Surgery surgery, Pawn pawn, BodyPartRecord bodyPart )
        {
            MSE_AdditionalHediff additionalHediffs = surgery.recipe.GetModExtension<MSE_AdditionalHediff>();
            GeneralApplyExtraHediffs( additionalHediffs, pawn, bodyPart );
        }

        /// <summary>
        /// Applies the extra hediffs specified in the given hediff's MSE_AdditionalHediff DefModExtension
        /// </summary>
        public static void HediffApplyExtraHediffs ( Hediff hediff, Pawn pawn, BodyPartRecord bodyPart )
        {
            MSE_AdditionalHediff additionalHediffs = hediff.def.GetModExtension<MSE_AdditionalHediff>();
            GeneralApplyExtraHediffs( additionalHediffs, pawn, bodyPart );
        }

        public static void GeneralApplyExtraHediffs ( MSE_AdditionalHediff additionalHediffs, Pawn pawn, BodyPartRecord bodyPart )
        {
            if ( additionalHediffs != null && !additionalHediffs.hediffsToAdd.NullOrEmpty() )
            {
                foreach ( HediffDef hediffToAdd in additionalHediffs.hediffsToAdd )
                {
                    pawn.health.AddHediff( hediffToAdd, bodyPart, null, null );
                }
            }
            return;
        }

    }
}
