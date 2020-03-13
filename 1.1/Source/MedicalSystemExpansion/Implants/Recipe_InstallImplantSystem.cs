using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OrenoMSE
{
    public class Recipe_InstallImplantSystem : Recipe_InstallImplant
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
                        if (pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Contains(record))
                        {
                            var implants = pawn.health.hediffSet.hediffs
                                .Where(d => d.Part == record && d is Hediff_Implant && d.Visible);
                            if (implants.Count() == 3 && pawn.health.hediffSet.HasDirectlyAddedPartFor(record))
                            {
                                yield break;
                            }

                            MSE_ImplantSystem implantSystem = recipe.GetModExtension<MSE_ImplantSystem>();
                            if (implantSystem != null && implantSystem.isSpecial)
                            {
                                if (pawn.health.hediffSet.HasDirectlyAddedPartFor(record) && !MSE_VanillaExtender.PartHasAdvancedImplantSystem(pawn, record))
                                {
                                    yield break;
                                }
                            }

                            if (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && x.def == recipe.addsHediff))
                            {
                                yield return record;
                            }
                        }
                    }
                }
            }
            yield break;
        }
    }
}
