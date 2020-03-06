using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrenoMSE.HarmonyPatches
{
    public class PawnHelthTracker_RestorePartRecursiveInt
    {
        [HarmonyPatch( typeof( Pawn_HealthTracker ) )]
        [HarmonyPatch( "RestorePartRecursiveInt" )]
        internal class RestoreCleanWithoutRecursion
        {
            [HarmonyPrefix]
            [HarmonyPriority( Priority.Low )]
            public static bool RemoveRecursion ( Pawn_HealthTracker __instance, BodyPartRecord part, Hediff diffException = null )
			{
				// copied code from vanilla
				
				List<Hediff> hediffs = __instance.hediffSet.hediffs;
				for ( int i = hediffs.Count - 1; i >= 0; i-- )
				{
					Hediff hediff = hediffs[i];
					if ( hediff.Part == part && hediff != diffException )
					{
						Hediff hediff2 = hediffs[i];
						hediffs.RemoveAt( i );
						hediff2.PostRemoved();
					}
				}
				//for ( int j = 0; j < part.parts.Count; j++ )                    // Removed recursion
				//{
				//	  this.RestorePartRecursiveInt( part.parts[j], diffException );
				//}

				// don't execute normal method
				return false;
            }
        }
    }
}
