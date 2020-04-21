using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2
{
    // This class gets patched into Defs/RecipeDef[@Name="SurgeryInstallBodyPartNaturalBase"]/workerClass

    internal class Recipe_InstallNaturalBodyPartWithChildren : Recipe_InstallNaturalBodyPart
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn ( Pawn pawn, RecipeDef recipe )
        {
            // don't add natural parts to prosthetics

            return
                MedicalRecipesUtility.GetFixedPartsToApplyOn( recipe, pawn,
                    ( BodyPartRecord record ) =>
                    // part has some hediff
                    pawn.health.hediffSet.hediffs.Any( ( Hediff x ) => x.Part == record )
                    // parent is not missing
                    && (record.parent == null || pawn.health.hediffSet.GetNotMissingParts().Contains( record.parent ))
                    // parent is not an added part or is not solid (mainly for mutant arms and legs)
                    && (record.parent == null || !pawn.health.hediffSet.HasDirectlyAddedPartFor( record.parent ) || !record.parent.def.IsSolid( record.parent, pawn.health.hediffSet.hediffs ))
                    // is compatible with parent
                    && !recipe.HasRestrictionsForPart( record, pawn.health.hediffSet )
                    // part shouldn't be ignored
                    && !pawn.health.hediffSet.PartShouldBeIgnored( record ) );
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
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts( pawn, part, billDoer.Position, billDoer.Map );
            }

            // END VANILLA CODE
            else if ( pawn.Map != null )
            {
                MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts( pawn, part, pawn.Position, pawn.Map );
            }
            else
            {
                pawn.health.RestorePart( part, null, true );
            }

            // iterate over non null CompIncludedChildParts in ingredients
            foreach ( CompIncludedChildParts compChildParts
                in from x in ingredients
                   where x is ThingWithComps  // out of every thingwithcomps ingredient
                   let comp = (x as ThingWithComps).GetComp<CompIncludedChildParts>() // take the comp
                   where comp?.IncludedParts != null
                   select comp )
            {
                compChildParts.RecursiveInstallation( pawn, part );
            }
        }
    }
}