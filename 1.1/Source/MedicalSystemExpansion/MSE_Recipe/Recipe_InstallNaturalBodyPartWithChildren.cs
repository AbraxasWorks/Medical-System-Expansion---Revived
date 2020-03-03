using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrenoMSE.MSE_Recipe
{
    class Recipe_InstallArtificialBodyPartWithChildren : Recipe_InstallArtificialBodyPart
    {
		
		public override void ApplyOnPawn ( Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill )
		{
			// START VANILLA CODE (couldn't know if the surgery was successfull)
			
			bool flag = MedicalRecipesUtility.IsClean( pawn, part );
			bool flag2 = !PawnGenerator.IsBeingGenerated( pawn ) && this.IsViolationOnPawn( pawn, part, Faction.OfPlayer );
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
				if ( flag && flag2 && part.def.spawnThingOnRemoved != null )
				{
					ThoughtUtility.GiveThoughtsForPawnOrganHarvested( pawn );
				}
				if ( flag2 )
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
			partAndDirectChildren.Add( part );


			foreach ( Thing ingredient in ingredients )
			{
				if ( ingredient is MSE_ThingClass.MSE_ThingBodyPart bodyPartIngredient ) 
				{ // if actual thing being installed
					if ( bodyPartIngredient.childPartsIncluded != null )
					{ // has child items
						int fingerTracker = 0;
						foreach ( MSE_ThingClass.MSE_ThingBodyPart childThing in bodyPartIngredient.childPartsIncluded )
						{ // for each child thing
							Log.Message( "Trying to install child " + childThing.Label );
							bool hasFoundARec = false;
							foreach ( RecipeDef anyrec in DefDatabase<RecipeDef>.AllDefs )
							{ // each recipe that includes it
								if ( anyrec.IsSurgery && anyrec.IsIngredient( childThing.def ) && anyrec.Worker is Recipe_InstallArtificialBodyPartWithChildren recursiveRecipe )
								{ // try to get the RecipeWorker
									Log.Message( "Candidate surgery: " + anyrec.defName );
									int i = 0;
									foreach ( BodyPartRecord validBP in MedicalRecipesUtility.GetFixedPartsToApplyOn( anyrec, pawn, delegate ( BodyPartRecord bp ) { return partAndDirectChildren.Contains( bp ); } ) )
									{ // for each valid bodypart
										if ( i < fingerTracker % 5 && (validBP.def.defName == "Finger" || validBP.def.defName == "Toe") )
										{
											i++;
											continue;
										}
										fingerTracker++;
										Log.Message( "Found a surgery: " + anyrec.defName );
										recursiveRecipe.ApplyOnPawn( pawn, validBP, null, new List<Thing> { childThing }, null );
										hasFoundARec = true;
										break;
									}
									if (hasFoundARec)
									{
										break;
									}
								}
							}
							if (!hasFoundARec)
							{
								Log.Message( "Couldn't install " + childThing.Label );
								childThing.Position = pawn.Position;
								childThing.SpawnSetup(pawn.Map, false);
							}
						}
					}
					break;
				}
			}
		}
	}
}
