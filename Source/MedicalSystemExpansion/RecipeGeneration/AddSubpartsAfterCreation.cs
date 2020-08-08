using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;

namespace MSE2.HarmonyPatches
{
    [HarmonyPatch]
    internal class AddSubpartsAfterCreation
    {
        private static MethodBase TargetMethod ()
        {
            return typeof( GenRecipe ).GetMethod( "PostProcessProduct", BindingFlags.NonPublic | BindingFlags.Static );
        }

        [HarmonyPostfix]
        public static void AddSubparts ( ref Thing __result, RecipeDef recipeDef )
        {
            if ( recipeDef.HasModExtension<LimbProsthesisCreation>() && __result.TryGetComp<CompIncludedChildParts>() != null )
            {
                __result.TryGetComp<CompIncludedChildParts>().InitializeForLimb( recipeDef.GetModExtension<LimbProsthesisCreation>().targetLimb );
            }
        }
    }
}