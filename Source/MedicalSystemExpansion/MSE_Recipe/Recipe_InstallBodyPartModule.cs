using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OrenoMSE
{
    public class Recipe_InstallBodyPartModule : Recipe_InstallArtificialBodyPart
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++)
            {
                BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
                for (int j = 0; j < bpList.Count; j++)
                {
                    BodyPartRecord record = bpList[j];
                    if (record.def == part)
                    {
                        IEnumerable<Hediff> diffs = from x in pawn.health.hediffSet.hediffs where x.Part == record select x;
                        if (diffs.Count<Hediff>() != 1 || diffs.First<Hediff>().def != recipe.addsHediff)
                        {
                            if (record.parent == null || pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Contains(record.parent))
                            {
                                if (pawn.health.hediffSet.hediffs.Any((Hediff d) => d is Hediff_BodyPartModule && d.Part == record))
                                {
                                    yield return record;
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }
    }
}
