using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OrenoMSE
{
    class Recipe_InstallArtificialBodyPartWithChildren : Recipe_InstallArtificialBodyPart
    {
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn ( Pawn pawn, RecipeDef recipe )
		{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn( recipe, pawn, delegate ( BodyPartRecord record )
			{
				IEnumerable<Hediff> source = from x in pawn.health.hediffSet.hediffs
											 where x.Part == record
											 select x;
				return 
					(source.Count<Hediff>() != 1 || source.First<Hediff>().def != recipe.addsHediff) 
					&& (record.parent == null || pawn.health.hediffSet.GetNotMissingParts( BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null ).Contains( record.parent ))
					/*&& (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts( record ) || pawn.health.hediffSet.HasDirectlyAddedPartFor( record ))*/;
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

			List<BodyPartRecord> partAndDirectChildren = new List<BodyPartRecord>( part.GetDirectChildParts() );
			//partAndDirectChildren.Add( part );

			//Log.Message( "Starting recursive installation from " + part.Label );
			foreach ( Thing ingredient in ingredients )
			{
				if ( ingredient is ThingWithComps bodyPartIngredient ) 
				{ // if it has comps
					//Log.Message( "Considering ingredient: " + bodyPartIngredient.Label );

					CompIncludedChildParts compChildParts = bodyPartIngredient.GetComp<CompIncludedChildParts>();
					if ( compChildParts != null )
					{ // has child items

						//Log.Message( "With child parts: " + compChildParts.childPartsIncluded.ToString() );

						foreach ( Thing childThing in compChildParts.childPartsIncluded )
						{ // for each child thing
							//Log.Message( "Trying to install child " + childThing.Label );
							bool hasFoundARec = false;
							foreach ( RecipeDef anyrec in DefDatabase<RecipeDef>.AllDefs )
							{ // each recipe that includes it
								if ( anyrec.IsSurgery && anyrec.IsIngredient( childThing.def ) && anyrec.Worker is Recipe_InstallArtificialBodyPartWithChildren recursiveRecipe )
								{ // try to get the RecipeWorker
									//Log.Message( "Candidate surgery: " + anyrec.defName );
									//int i = 0;
									BodyPartRecord validBP =
										MedicalRecipesUtility.GetFixedPartsToApplyOn( anyrec, pawn, // out of all the possible places to install this on the pawn
												delegate ( BodyPartRecord bp ) 
												{ return partAndDirectChildren.Contains( bp ); } ) // choose between children of the current part
											.FirstOrFallback();	// take the first

									if ( validBP != null ) // it actually found something
									{
										//Log.Message( "Found a surgery: " + anyrec.defName );
										recursiveRecipe.ApplyOnPawn( pawn, validBP, null, new List<Thing> { childThing }, null );
										partAndDirectChildren.Remove( validBP );
										hasFoundARec = true;
										break;
									}
								}
							}
							if (!hasFoundARec)
							{
								Log.Error( "[MSE] Couldn't install " + childThing.Label );
								childThing.Position = pawn.Position;
								childThing.SpawnSetup(pawn.Map, false);
							}
						}
						break; // after the first ingredient with children stop (it's the part that has just been installed before recursion)
					}
				}
			}
		}
	}
}
