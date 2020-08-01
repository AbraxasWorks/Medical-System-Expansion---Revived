using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2
{
    public class Recipe_InstallModule : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn ( Pawn pawn, RecipeDef recipe )
        {
            return
                // in the parts that the recipe can be applied to and that the pawn has a slot in
                from BodyPartRecord bodyPart in
                    from rbpd in recipe.appliedOnFixedBodyParts
                    from slot in pawn.health.hediffSet.GetHediffs<Hediff_ModuleSlot>()
                    where rbpd == slot.Part.def
                    select slot.Part
                where !pawn.health.hediffSet.GetHediffs<Hediff_ModuleAdded>().Any( m => m.Part == bodyPart && m.def == recipe.addsHediff )   // part doesn't have the module i'm trying to add
                select bodyPart;
        }

        public override void ApplyOnPawn ( Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill )
        {
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
                if ( isViolation )
                {
                    base.ReportViolation( pawn, billDoer, pawn.FactionOrExtraMiniOrHomeFaction, -70, "GoodwillChangedReason_NeedlesslyInstalledWorseBodyPart".Translate( this.recipe.addsHediff.label ) );
                }
            }

            pawn.health.AddHediff( this.recipe.addsHediff, part, null, null );
        }
    }
}