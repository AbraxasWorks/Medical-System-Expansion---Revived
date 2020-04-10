using System.Collections.Generic;
using System.Linq;

using Verse;

namespace OrenoMSE.PartInstallation
{
    public class InstallationRestrictions : DefModExtension
    {
        private readonly List<HediffDef> whitelist;

        public bool CompatibleWithPart ( BodyPartRecord part, HediffSet hediffSet )
        {
            return this.whitelist == null
                || (from h in hediffSet.hediffs
                    where h.Part == part
                    where this.whitelist.Contains( h.def )
                    select h).Any();
        }

        public override IEnumerable<string> ConfigErrors ()
        {
            foreach ( var e in base.ConfigErrors() ) yield return e;

            if ( this.whitelist == null )
                yield return "[MSE] all InstallationRestrictions are null";

            yield break;
        }
    }
}