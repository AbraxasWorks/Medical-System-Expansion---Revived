using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;
using RimWorld;

namespace MSE2.HarmonyPatches
{
    [HarmonyPatch( typeof( DefGenerator ) )]
    [HarmonyPatch( "GenerateImpliedDefs_PreResolve" )]
    internal class DefGenerator_ImpliedLimbProstheses
    {
        [HarmonyPostfix]
        public static void ConcatLimbCrafting ()
        {
            foreach ( RecipeDef def in LimbRecipeDefGenerator.ImpliedLimbRecipeDefs() )
            {
                DefGenerator.AddImpliedDef<RecipeDef>( def );
            }
            DefGenerator.AddImpliedDef<RecipeDef>( new RecipeDef() { defName = "aaaaaaaaa" } );
        }
    }

    //[HarmonyPatch( typeof( RecipeDefGenerator ) )]
    //[HarmonyPatch( "ImpliedRecipeDefs" )]
    //internal class Test0
    //{
    //    [HarmonyPrefix]
    //    public static bool Pre ()
    //    {
    //        Log.Message( "ululu0" );
    //        return true;
    //    }

    //    [HarmonyPostfix]
    //    public static void Post ()
    //    {
    //        Log.Message( "ululu1" );
    //    }
    //}
}