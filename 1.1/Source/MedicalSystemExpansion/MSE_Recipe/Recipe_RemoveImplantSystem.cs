using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrenoMSE
{
    public class Recipe_RemoveImplantSystem : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
            using (IEnumerator<BodyPartRecord> enumerator = parts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BodyPartRecord part = enumerator.Current;
                    if (pawn.health.hediffSet.hediffs.Any((Hediff d) => d is Hediff_Implant && d.Part == part && d.def == recipe.removesHediff && d.Visible))
                    {
                        yield return part;
                    }
                }
            }
            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
                {
                    string text;
                    if (!this.recipe.successfullyRemovedHediffMessage.NullOrEmpty())
                    {
                        text = string.Format(this.recipe.successfullyRemovedHediffMessage, billDoer.LabelShort, pawn.LabelShort);
                    }
                    else
                    {
                        text = "MessageSuccessfullyRemovedImplant".Translate(billDoer.LabelShort, pawn.LabelShort, this.recipe.removesHediff.label.Named("HEDIFF"), billDoer.Named("SURGEON"), pawn.Named("PATIENT"));
                    }
                    Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent, true);
                }
            }
            Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == this.recipe.removesHediff && x is Hediff_Implant && x.Part == part && x.Visible);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}
