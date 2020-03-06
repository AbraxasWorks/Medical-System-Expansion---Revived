using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrenoMSE
{
	class Recipe_InstallNaturalBodyPartWithChildren : Recipe_InstallNaturalBodyPart
	{

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

		public void RecursiveInstallation ( Pawn pawn, BodyPartRecord part, List<Thing> ingredients, IntVec3 pos, Map map )
		{
			MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts( pawn, part, pos, map );


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
								if ( anyrec.IsSurgery && anyrec.IsIngredient( childThing.def ) && anyrec.Worker is Recipe_InstallNaturalBodyPartWithChildren recursiveRecipe )
								{ // try to get the RecipeWorker
								  //Log.Message( "Candidate surgery: " + anyrec.defName );
								  //int i = 0;
									BodyPartRecord validBP =
										MedicalRecipesUtility.GetFixedPartsToApplyOn( anyrec, pawn, // out of all the possible places to install this on the pawn
												delegate ( BodyPartRecord bp )
												{ return partAndDirectChildren.Contains( bp ); } ) // choose between children of the current part
											.FirstOrFallback(); // take the first

									if ( validBP != null ) // it actually found something
									{
										//Log.Message( "Found a surgery: " + anyrec.defName );
										recursiveRecipe.RecursiveInstallation( pawn, validBP, new List<Thing> { childThing }, pos, map );
										partAndDirectChildren.Remove( validBP );
										hasFoundARec = true;
										break;
									}
								}
							}
							if ( !hasFoundARec )
							{
								Log.Error( "[MSE] Couldn't install " + childThing.Label );
								childThing.Position = pawn.Position;
								childThing.SpawnSetup( pawn.Map, false );
							}
						}
						break; // after the first ingredient with children stop (it's the part that has just been installed before recursion)
					}
				}
			}
		}




		//private static void RestoreSinglePart ( Pawn_HealthTracker Pawn_HT, BodyPartRecord part, Hediff diffException = null )
		//{
		//	List<Hediff> hediffs = Pawn_HT.hediffSet.hediffs;
		//	for ( int i = hediffs.Count - 1; i >= 0; i-- )
		//	{
		//		Hediff hediff = hediffs[i];
		//		if ( hediff.Part == part && hediff != diffException )
		//		{
		//			Hediff hediff2 = hediffs[i];
		//			hediffs.RemoveAt( i );
		//			hediff2.PostRemoved();
		//		}
		//	}
		//}

		// Edited functions originally from Verse.Pawn_HealthTracker

		//public static void RestorePart ( Pawn_HealthTracker Pawn_HT, BodyPartRecord part, Hediff diffException = null, bool checkStateChange = true )
		//{
		//	if ( part == null )
		//	{
		//		Log.Error( "Tried to restore null body part.", false );
		//		return;
		//	}
		//	RestorePartRecursiveInt( Pawn_HT, part, diffException );
		//	Pawn_HT.hediffSet.DirtyCache();
		//	if ( checkStateChange )
		//	{
		//		Pawn_HT.CheckForStateChange( null, null );
		//	}
		//}

		//private static void RestorePartRecursiveInt ( Pawn_HealthTracker Pawn_HT, BodyPartRecord part, Hediff diffException = null )
		//{
		//	RestoreSinglePart( Pawn_HT, part, diffException );

		//	for ( int j = 0; j < part.parts.Count; j++ )
		//	{
		//		RestorePartRecursiveInt( Pawn_HT, part.parts[j], diffException );
		//	}
		//}


	}
}
