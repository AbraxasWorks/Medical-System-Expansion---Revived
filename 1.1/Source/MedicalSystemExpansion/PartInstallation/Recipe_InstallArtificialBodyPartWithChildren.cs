using System.Collections.Generic;
using System.Linq;
using OrenoMSE.EfficiencyCalculationPatches;
using RimWorld;
using Verse;

namespace OrenoMSE.PartInstallation
{
	// This class gets patched into Defs/RecipeDef[@Name="SurgeryInstallBodyPartArtificialBase"]/workerClass

	class Recipe_InstallArtificialBodyPartWithChildren : Recipe_InstallArtificialBodyPart
    {
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn ( Pawn pawn, RecipeDef recipe )
		{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn( recipe, pawn, delegate ( BodyPartRecord record )
			{
				IEnumerable<Hediff> alreadyPresent = from x in pawn.health.hediffSet.hediffs
													 where x.Part == record
													 where x.def == recipe.addsHediff
													 select x;
				return // hediff not already present
					!alreadyPresent.Any()
					// has something to attach to
					&& (record.parent == null || pawn.health.hediffSet.GetNotMissingParts( BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null ).Contains( record.parent ))
					// is compatible with parent
					&& !recipe.HasRestrictionsForPart( record, pawn.health.hediffSet )
					// part shouldn't be ignored
					&& !pawn.health.hediffSet.PartShouldBeIgnored( record );
			} );
		}

		public override void ApplyOnPawn ( Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill )
		{
			// START VANILLA CODE (couldn't know if the surgery was successfull)
			
			bool partIsClean = MedicalRecipesUtility.IsClean( pawn, part );
			bool isViolation = !PawnGenerator.IsBeingGenerated( pawn ) && this.IsViolationOnPawn( pawn, part, Faction.OfPlayer );
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
				MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts( pawn, part, billDoer.Position, billDoer.Map );
				if ( partIsClean && isViolation && part.def.spawnThingOnRemoved != null )
				{
					ThoughtUtility.GiveThoughtsForPawnOrganHarvested( pawn );
				}
				if ( isViolation )
				{
					base.ReportViolation( pawn, billDoer, pawn.FactionOrExtraHomeFaction, -70, "GoodwillChangedReason_NeedlesslyInstalledWorseBodyPart".Translate( this.recipe.addsHediff.label ) );
				}
			}
			else if ( pawn.Map != null )
			{
				MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts( pawn, part, pawn.Position, pawn.Map );
			}
			else
			{
				pawn.health.RestorePart( part, null, true );
			}
			pawn.health.AddHediff( this.recipe.addsHediff, part, null, null );

			// END VANILLA CODE




			List<BodyPartRecord> directChildren = new List<BodyPartRecord>( part.GetDirectChildParts() );
			
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
					bool hasFoundARec = false;

					// iterate over recipes
					foreach ( RecipeDef anyrec in DefDatabase<RecipeDef>.AllDefs )
					{ 
						// each recipe that includes it
						if ( anyrec.IsSurgery && anyrec.IsIngredient( childThing.def ) && anyrec.Worker is Recipe_InstallArtificialBodyPartWithChildren recursiveRecipe )
						{ 
							// recursiveRecipe is the RecipeWorker

							BodyPartRecord validBP =
								MedicalRecipesUtility.GetFixedPartsToApplyOn( anyrec, pawn, // out of all the possible places to install this on the pawn
										delegate ( BodyPartRecord bp )
										{ return directChildren.Contains( bp ); } ) // choose between children of the current part
									.FirstOrFallback(); // take the first

							if ( validBP != null ) // it actually found something
							{
								// apply the recipe 
								recursiveRecipe.ApplyOnPawn( pawn, validBP, null, new List<Thing> { childThing }, null );

								directChildren.Remove( validBP );
								hasFoundARec = true;

								break; // only need the first recipe
							}
						}
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
						
						//Log.Error( "[MSE] Couldn't install " + childThing.Label );
						//childThing.Position = pawn.Position;
						//childThing.SpawnSetup( pawn.Map, false );
					}
				}
				break; // after the first ingredient with children stop (it's the part that has just been installed before recursion)
			}
		}
	}
}
