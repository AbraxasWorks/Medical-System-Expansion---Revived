using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OrenoMSE.Modules.Actual
{
    public class Recipe_InstallModule : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn ( Pawn pawn, RecipeDef recipe )
        {
            return
                // in the parts that the recipe can be applied to and that the pawn has
                from BodyPartRecord bodyPart in
                    from rbpd in recipe.appliedOnFixedBodyParts
                    from abpr in pawn.health.hediffSet.GetNotMissingParts()
                    where rbpd == abpr.def
                    select abpr
                where
                    pawn.health.hediffSet.GetHediffs<Hediff_ModuleAbstract>().Any(
                            delegate ( Hediff_ModuleAbstract hediff_Module )
                            {
                                return hediff_Module.Part == bodyPart;   // in that part there is any module or slot
                            } )
                where
                    !pawn.health.hediffSet.GetHediffs<Hediff_ModuleAbstract>().Any(
                            delegate ( Hediff_ModuleAbstract hediff_Module )
                            {
                                return hediff_Module.def == recipe.addsHediff;   // part doesn't have the module i'm trying to add
                            } )
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
                    base.ReportViolation( pawn, billDoer, pawn.FactionOrExtraHomeFaction, -70, "GoodwillChangedReason_NeedlesslyInstalledWorseBodyPart".Translate( this.recipe.addsHediff.label ) );
                }
            }

            ModuleUtilities.InstallModule( pawn, this.recipe.addsHediff, part );
        }
    }
}