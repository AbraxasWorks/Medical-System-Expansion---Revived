using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OrenoMSE.PartInstallation
{
    // This class gets patched into Defs/RecipeDef[@Name="SurgeryInstallBodyPartNaturalBase"]/workerClass

    internal class Recipe_InstallNaturalBodyPartWithChildren : Recipe_InstallNaturalBodyPart
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn ( Pawn pawn, RecipeDef recipe )
        {
            // don't add natural parts to prosthetics
            return from part in base.GetPartsToApplyOn( pawn, recipe )
                   where part.parent == null || pawn.health.hediffSet.HasDirectlyAddedPartFor( part.parent )
                   select part;
        }

        public override void ApplyOnPawn ( Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill )
        {
            // START VANILLA CODE (couldn't know if the surgery was successfull)

            if ( billDoer != null )
            {
                if ( base.CheckSurgeryFail( billDoer, pawn, ingredients, part, bill ) )
                {
                    return;
                }
                TaleRecorder.RecordTale( TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                } );
                //MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts( pawn, part, billDoer.Position, billDoer.Map );
            }

            // END VANILLA CODE

            RecursiveInstallation( pawn, part, ingredients, billDoer.Position, billDoer.Map );
        }

        internal static void RecursiveInstallation ( Pawn pawn, BodyPartRecord part, List<Thing> ingredients, IntVec3 pos, Map map )
        {
            MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts( pawn, part, pos, map );

            List<BodyPartRecord> directChildren = new List<BodyPartRecord>( part.GetDirectChildParts() );

            //Log.Message( "Starting recursive installation from " + part.Label );

            // iterate over non null CompIncludedChildParts
            foreach ( CompIncludedChildParts compChildParts in
                from x in ingredients
                where x is ThingWithComps  // out of every thingwithcomps ingredient
                let comp = (x as ThingWithComps).GetComp<CompIncludedChildParts>() // take the comp
                where comp != null && comp.childPartsIncluded != null
                select comp )
            {
                // iterate over included child things
                foreach ( Thing childThing in compChildParts.childPartsIncluded )
                {
                    bool hasFoundARecipe = false;

                    // iterate over recipes
                    foreach ( RecipeDef anyrec in DefDatabase<RecipeDef>.AllDefs )
                    {
                        // each recipe that includes it
                        if ( anyrec.IsSurgery && anyrec.IsIngredient( childThing.def ) && anyrec.Worker is Recipe_InstallNaturalBodyPartWithChildren )
                        {
                            BodyPartRecord validBP =
                                MedicalRecipesUtility.GetFixedPartsToApplyOn( anyrec, pawn, // out of all the possible places to install this on the pawn
                                        delegate ( BodyPartRecord bp )
                                        {
                                            return directChildren.Contains( bp );  // choose between children of the current part
                                        } )
                                    .FirstOrFallback(); // take the first

                            if ( validBP != null ) // it actually found something
                            {
                                // apply the recipe
                                RecursiveInstallation( pawn, validBP, new List<Thing> { childThing }, pos, map );

                                directChildren.Remove( validBP );
                                hasFoundARecipe = true;

                                break; // only need the first recipe
                            }
                        }
                    }
                    if ( !hasFoundARecipe )
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
                break; // after the first ingredient with children stop (it's the part that has just been installed before recursion)
            }
        }
    }
}