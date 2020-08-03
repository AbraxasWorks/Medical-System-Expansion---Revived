using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MSE2.DebugTools
{
    public static class GlobalProthesisFix
    {

        [DebugAction( "Pawns", "Fix all prostheses in the world", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing )]
        private static void Apply ()
        {
            int countFixedParts = 0;
            int countFixedPawns = 0;

            Log.Message( "Starting global prosthesis fix operation" );

            foreach ( Pawn pawn in Find.WorldPawns.AllPawnsAlive.Where( p => p != null ) )
            {
                int c = RestorePawnProstheses( pawn );
                if ( c > 0 )
                {
                    countFixedPawns++;
                    countFixedParts += c;
                }
            }
            foreach ( Map map in Find.Maps )
            {
                foreach ( Pawn pawn in map.mapPawns.AllPawns.Where( p => p != null ) )
                {
                    int c = RestorePawnProstheses( pawn );
                    if ( c > 0 )
                    {
                        countFixedPawns++;
                        countFixedParts += c;
                    }
                }
            }

            Log.Message( "Global prosthesis fix operation complete: fixed " + countFixedParts + " in " + countFixedPawns + " pawns" );
            Messages.Message( new Message( "Global prosthesis fix operation complete: fixed " + countFixedParts + " in " + countFixedPawns + " pawns.", MessageTypeDefOf.NeutralEvent ), false );
        }

        private static int RestorePawnProstheses ( Pawn pawn )
        {
            int countFixedParts = 0;

            foreach ( Hediff hediff in pawn.health.hediffSet.GetHediffs<Hediff_AddedPart>() )
            {
                if ( hediff.Part.parts.Any( p => !pawn.health.hediffSet.HasDirectlyAddedPartFor( p ) && !pawn.health.hediffSet.PartShouldBeIgnored( p ) ) )
                {
                    RecipeDef recipeDef = DefDatabase<RecipeDef>.AllDefs.Where( r => r.IsSurgery && r.addsHediff == hediff.def ).FirstOrDefault();

                    if ( recipeDef != null )
                    {
                        Log.Message( "Fixing " + hediff.Label + " on " + pawn.Name );

                        var part = hediff.Part;
                        pawn.health.RestorePart( part );

                        recipeDef.Worker.ApplyOnPawn( pawn, part, null, null, null );

                        countFixedParts++;
                    }
                    else
                    {
                        Log.Warning( "Could not find a recipe to fix " + hediff.Label + " on " + pawn.Name );
                    }
                }
            }

            return countFixedParts;
        }
    }
}
