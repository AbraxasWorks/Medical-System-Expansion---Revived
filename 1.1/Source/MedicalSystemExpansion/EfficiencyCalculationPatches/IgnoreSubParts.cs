using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;

namespace OrenoMSE.EfficiencyCalculationPatches
{
    class IgnoreSubParts : DefModExtension
    {
        // DefModExtension to add to hediffs of addedparts when they don't support certain child parts (i.e. peglegs have no foot or bones)


        public List<BodyPartDef> ignoredSubParts;


        public override IEnumerable<string> ConfigErrors ()
        {
            foreach ( var ce in base.ConfigErrors() ) yield return ce;

            if ( this.ignoredSubParts == null || this.ignoredSubParts.Count == 0 )
            {
                yield return "[MSE] ignoredSubPart is null or empty";
            }

            yield break;
        }
    }

    
    static class Utilities
    {
        public static bool ParentPartIgnores ( this HediffSet set, BodyPartRecord bodyPart )
        {
            if ( bodyPart.parent != null )
            {
                var modExt = set.hediffs
                    .Find( ( Hediff h ) => h is Hediff_AddedPart && h.Part == bodyPart.parent ) // added part on parent bodypartrecord
                    .def.GetModExtension<IgnoreSubParts>();

                return modExt != null && modExt.ignoredSubParts.Contains( bodyPart.def );
            }
            return false;
        }
    }

}
