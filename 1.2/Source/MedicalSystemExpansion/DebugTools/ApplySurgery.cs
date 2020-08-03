using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MSE2.DebugTools
{
    public static class ApplySurgery
    {
        [DebugAction( "Pawns", "Apply surgery...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap )]
        private static void Apply ()
        {
            Find.WindowStack.Add( new Dialog_DebugOptionListLister( Options_ApplySurgery() ) );
        }

        private static List<DebugMenuOption> Options_ApplySurgery ()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach ( RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs.Where( r => r.IsSurgery ) )
            {
                list.Add( new DebugMenuOption( recipe.LabelCap, DebugMenuOptionMode.Tool, delegate ()
                {
                    Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt( UI.MouseCell() ).OfType<Pawn>().FirstOrDefault<Pawn>();
                    if ( pawn != null )
                    {
                        Find.WindowStack.Add( new Dialog_DebugOptionListLister( Options_ApplySurgery_BodyParts( pawn, recipe ) ) );
                    }
                } ) );
            }
            return list;
        }

        private static List<DebugMenuOption> Options_ApplySurgery_BodyParts ( Pawn pawn, RecipeDef recipe )
        {
            if ( pawn == null )
            {
                throw new ArgumentNullException( "p" );
            }
            List<DebugMenuOption> list = new List<DebugMenuOption>();

            if ( recipe.AllRecipeUsers.Select( ru => ru.race.body ).Contains( pawn.RaceProps.body ) )
            {
                foreach ( BodyPartRecord bpr in recipe.Worker.GetPartsToApplyOn( pawn, recipe ) )
                {
                    list.Add( new DebugMenuOption( bpr.Label, DebugMenuOptionMode.Action, delegate ()
                    {
                        recipe.Worker.ApplyOnPawn( pawn, bpr, null, null, null );
                    } ) );
                }
            }

            return list;
        }
    }
}