using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2
{
    public class Recipe_RemoveModules : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn ( Pawn pawn, RecipeDef recipe )
        {
            return from m in pawn.health.hediffSet.GetHediffs<Hediff_ModuleAdded>()
                   group m by m.Part into g
                   select g.Key;
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

            foreach ( Hediff_ModuleAdded module in from x in pawn.health.hediffSet.GetHediffs<Hediff_ModuleAdded>()
                                                   where x.Part == part
                                                   select x )
            {
                // spawn thing if possible
                if ( module.def.spawnThingOnRemoved != null && pawn?.Map != null )
                {
                    GenPlace.TryPlaceThing( ThingMaker.MakeThing( module.def.spawnThingOnRemoved ), pawn.Position, pawn.Map, ThingPlaceMode.Near );
                }

                // remove hediff
                pawn.health.RemoveHediff( module );
            }
        }
    }
}