using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using HarmonyLib;

using EdB.PrepareCarefully;

namespace MSE2
{
    public static class PrepareCarefullyPatches
    {
        public static void ApplyPC ( Harmony harmony )
        {
            harmony.Patch( AddToPawn.MethodInfo(), AddToPawn.PreFix() );
        }

        // Implant.AddToPawn
        private static class AddToPawn
        {
            public static MethodInfo MethodInfo ()
            {
                return typeof( Implant ).GetMethod( nameof( Implant.AddToPawn ) );
            }

            public static HarmonyMethod PreFix ()
            {
                return new HarmonyMethod( typeof( AddToPawn ).GetMethod( nameof( ReplaceFunction ), BindingFlags.NonPublic | BindingFlags.Static ), Priority.Last );
            }

            private static bool ReplaceFunction ( Implant __instance, CustomPawn customPawn, Verse.Pawn pawn )
            {
                if ( __instance.recipe != null && __instance.BodyPartRecord != null )
                {
                    __instance.recipe.Worker.ApplyOnPawn( pawn, __instance.BodyPartRecord, null, null, null );
                    pawn.health.capacities.Clear();
                }
                return false;
            }
        }
    }
}