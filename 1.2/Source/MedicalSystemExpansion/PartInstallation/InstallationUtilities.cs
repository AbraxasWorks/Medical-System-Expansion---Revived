using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2
{
    public static class InstallationUtilities
    {
        public static bool HasRestrictionsForPart ( this RecipeDef recipe, BodyPartRecord part, HediffSet hediffSet )
        {
            var modExt = recipe.GetModExtension<InstallationRestrictions>();

            return modExt != null && part.parent != null && !modExt.CompatibleWithPart( part.parent, hediffSet );
        }

        /// <summary>
        /// Looks for a recipe and a compatible bodypart that can be applied to the pawn
        /// </summary>
        /// <returns>If it was successfull or not</returns>
        public static bool TryGetRecipeAndPart ( Pawn pawn, ThingDef thingDef, Predicate<BodyPartRecord> predicate, out RecipeDef recipeDef, out BodyPartRecord bodyPartRecord )
        {
            (recipeDef, bodyPartRecord) =
                (from r in DefDatabase<RecipeDef>.AllDefs
                 where r.IsSurgery
                 where r.IsIngredient( thingDef )
                 where r.Worker is Recipe_Surgery
                 from p in r.Worker.GetPartsToApplyOn( pawn, r ) // out of all the possible places to install this on the pawn
                 where predicate == null || predicate.Invoke( p ) // choose using the predicate
                 select (r, p))
                 .FirstOrDefault();

            return recipeDef != null && bodyPartRecord != null;
        }

        /// <summary>
        /// Recursively installs the included parts, or drops them when possible
        /// </summary>
        /// <param name="compChildParts">Where to get the parts to install</param>
        public static void RecursiveInstallation ( this CompIncludedChildParts compChildParts, Pawn pawn, BodyPartRecord part )
        {
            List<BodyPartRecord> partsToConsider = new List<BodyPartRecord>( part.GetDirectChildParts().Append( part ) );

            // iterate over included child things
            foreach ( Thing childThing in compChildParts.IncludedParts )
            {
                if ( TryGetRecipeAndPart( pawn, childThing.def, partsToConsider.Contains, out RecipeDef recipe, out BodyPartRecord bpr ) )
                {
                    // apply the recipe
                    recipe.Worker.ApplyOnPawn( pawn, bpr, null, new List<Thing> { childThing }, null );

                    partsToConsider.Remove( bpr );
                }
                else
                {
                    if ( pawn.Map != null && pawn.Position != null )
                    {
                        childThing.ForceSetStateToUnspawned();
                        childThing.stackCount = 1;
                        GenPlace.TryPlaceThing( childThing, pawn.Position, pawn.Map, ThingPlaceMode.Near );
                    }
                    else
                    {
                        childThing.Destroy();
                    }

                    Log.Warning( "[MSE] Couldn't install " + childThing.Label + " on " + part.Label + " of " + pawn.Name );
                }
            }
        }

        /// <summary>
        /// Called when applying a recipe without ingredients, adds all the standardChildren
        /// </summary>
        /// <param name="compProp">Where to get the standard children from</param>
        /// <param name="pawn">What pawn to add the diffs to</param>
        /// <param name="part">The parent part of the parts to consider</param>
        public static void RecursiveDefInstallation ( this CompProperties_IncludedChildParts compProp, Pawn pawn, BodyPartRecord part )
        {
            List<BodyPartRecord> partsToConsider = new List<BodyPartRecord>( part.GetDirectChildParts().Append( part ) );

            // iterate over included child things
            foreach ( (ThingDef childThingDef, _) in compProp.StandardPartsForLimb( LimbConfiguration.GenerateOrGetLimbConfigForBodyPartRecord( part ) ) )
            {
                if ( TryGetRecipeAndPart( pawn, childThingDef, partsToConsider.Contains, out RecipeDef recipe, out BodyPartRecord bpr ) )
                {
                    // apply the recipe (with no ingredients)
                    recipe.Worker.ApplyOnPawn( pawn, bpr, null, null, null );

                    partsToConsider.Remove( bpr );
                }
            }
        }
    }
}