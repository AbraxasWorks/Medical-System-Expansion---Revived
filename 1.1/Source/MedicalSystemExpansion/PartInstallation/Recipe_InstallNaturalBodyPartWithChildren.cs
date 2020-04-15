using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2.PartInstallation
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
                   where comp != null && comp.childPartsIncluded != null
                   select comp )
            {
                compChildParts.RecursiveInstallation( pawn, part );
            }
        }
    }
}