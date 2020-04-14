using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OrenoMSE.PartInstallation
{
    public static class InstallationUtilities
    {
        public static bool HasRestrictionsForPart ( this RecipeDef recipe, BodyPartRecord part, HediffSet hediffSet )
        {
            var modExt = recipe.GetModExtension<InstallationRestrictions>();

            return modExt != null && part.parent != null && !modExt.CompatibleWithPart( part.parent, hediffSet );
        }

        public static void RecursiveInstallation ( this CompIncludedChildParts compChildParts, Pawn pawn, BodyPartRecord part )
        {
            List<BodyPartRecord> partsToConsider = new List<BodyPartRecord>( part.GetDirectChildParts().Append( part ) );

            // iterate over included child things
            foreach ( Thing childThing in compChildParts.childPartsIncluded )
            {
                bool hasFoundARec = false;

                // iterate over recipes that include it
                foreach ( (RecipeDef recipe, BodyPartRecord bpr)
                    in from r in DefDatabase<RecipeDef>.AllDefs
                       where r.IsSurgery
                       where r.IsIngredient( childThing.def )
                       where r.Worker is Recipe_Surgery
                       from p in r.Worker.GetPartsToApplyOn( pawn, r ) // out of all the possible places to install this on the pawn
                       where partsToConsider.Contains( p ) // choose between children of the current part
                       select (r, p) )
                {
                    // apply the recipe
                    recipe.Worker.ApplyOnPawn( pawn, bpr, null, new List<Thing> { childThing }, null );

                    partsToConsider.Remove( bpr );

                    hasFoundARec = true;
                    break; // only need the first recipe
                }
                if ( !hasFoundARec )
                {
                    if ( pawn.Map != null && pawn.Position != null )
                    {
                        GenPlace.TryPlaceThing( childThing, pawn.Position, pawn.Map, ThingPlaceMode.Near );
                    }
                    else
                    {
                        childThing.Destroy();
                    }

                    Log.Warning( "[MSE] Couldn't install " + childThing.Label );
                }
            }
        }
    }
}