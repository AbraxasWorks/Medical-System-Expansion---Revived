using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace MSE2
{
    [StaticConstructorOnStartup]
    public static class MedicalSystemExpansion
    {
        static MedicalSystemExpansion ()
        {
            Harmony harmony = new Harmony( "MSE2" );

            harmony.PatchAll( Assembly.GetExecutingAssembly() );

            IncludedPartsUtilities.CacheAllStandardParents();

            IgnoreSubPartsUtilities.IgnoreAllNonCompedSubparts();

            IncludedPartsUtilities.FixHediffPriceOffset();
        }
    }
}