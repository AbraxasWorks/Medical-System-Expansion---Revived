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
        public static bool PartShouldBeIgnored ( this HediffSet set, BodyPartRecord bodyPart )
        {
            if ( bodyPart.parent != null )
            {
                var modExt = set.GetHediffs<Hediff_AddedPart>()
                    .FirstOrDefault( ( Hediff_AddedPart h ) => h.Part == bodyPart.parent )? // added part on parent bodypartrecord
                    .def.GetModExtension<IgnoreSubParts>();

                //bool res = 
                return
                    ( modExt != null && modExt.ignoredSubParts.Contains( bodyPart.def ) )
                    || set.PartShouldBeIgnored( bodyPart.parent );

                //if ( res )
                //    Log.Message( "Part to ignore " + bodyPart.Label + " of " + set.pawn.Name );

                //return res;
            }
            return false;
        }
    }

}
