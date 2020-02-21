using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OrenoMSE
{
    public class Recipe_RemoveBodyPartSystem : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
            using (IEnumerator<BodyPartRecord> enumerator = parts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BodyPartRecord part = enumerator.Current;
                    var check1 = pawn.health.hediffSet.hediffs.Any((Hediff d) => d.def.HasComp(typeof(HediffComp_PartSystem)) && d.Part == part);
                    var check2 = pawn.health.hediffSet.hediffs.Any((Hediff d) => d.def.HasComp(typeof(HediffComp_PartModule)) && d.Part == part);
                    if (check1 || check2)
                    {
                        yield return part;
                    }
                }
            }
            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            bool flag1 = pawn.health.hediffSet.hediffs.Any((Hediff d) => d.def.HasComp(typeof(HediffComp_PartModule)) && d.Part == part);
            bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);

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
                MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
                MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
            }

            if (flag1)
            {
                pawn.health.RestorePart(part, null, true);
                HediffWithComps hediff_BodyPartModule = (HediffWithComps)HediffMaker.MakeHediff(MSE_HediffDefOf.MSE_BodyPartModule, pawn, null);
                hediff_BodyPartModule.Part = part;
                pawn.health.hediffSet.AddDirect(hediff_BodyPartModule, null, null);
            }
            else
            {
                DamageDef surgicalCut = DamageDefOf.SurgicalCut;
                float amount = 99999f;
                float armorPenetration = 999f;
                pawn.TakeDamage(new DamageInfo(surgicalCut, amount, armorPenetration, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
            }

            if (flag2 && pawn.Faction != null && billDoer != null && billDoer.Faction != null)
            {
                Faction faction1 = pawn.Faction;
                Faction faction2 = billDoer.Faction;
                int goodwillChange = -20;
                string reason = "GoodwillChangedReason_RemovedBodyPart".Translate(part.LabelShort);
                GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(pawn);
                faction1.TryAffectGoodwillWith(faction2, goodwillChange, true, true, reason, lookTarget);
            }
        }

        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            var check1 = pawn.health.hediffSet.hediffs.Any((Hediff d) => d.def.HasComp(typeof(HediffComp_PartSystem)) && d.Part == part);
            if (check1)
            {               
                return "RemovePartSystem".Translate();
            }

            var check2 = pawn.health.hediffSet.hediffs.Any((Hediff d) => d.def.HasComp(typeof(HediffComp_PartModule)) && d.Part == part);
            if (check2)
            {
                return "RemovePartModule".Translate();
            }

            return this.recipe.label;
        }
    }
}
