using HarmonyLib;
using Verse;

namespace MSE2.HarmonyPatches
{
    public class Hediff_AddDirect
    {
        [HarmonyPatch( typeof( HediffSet ) )]
        [HarmonyPatch( "AddDirect" )]
        internal class AddDirect
        {
            // Added error for when trying to add hediff to part that should be ignored

            [HarmonyPrefix]
            public static bool ErrorOnIgnoredPart ( HediffSet __instance, Hediff hediff )
            {
                if ( !(hediff is Hediff_MissingPart) && __instance.PartShouldBeIgnored( hediff.Part ) )
                {
                    Log.Error( "Tried to add health diff to part that should be ignored. Canceling.", false );
                    return false;
                }

                return true;
            }
        }
    }
}