using System.Collections.Generic;

using Verse;

namespace MSE2
{
    internal class IgnoreSubParts : DefModExtension
    {
        // DefModExtension to add to hediffs of addedparts when they don't support certain child parts (i.e. peglegs have no foot or bones)

        public List<BodyPartDef> ignoredSubParts;

        public override IEnumerable<string> ConfigErrors ()
        {
            foreach ( var ce in base.ConfigErrors() ) yield return ce;

            if ( this.ignoredSubParts == null )
            {
                yield return "[MSE2] ignoredSubPart is null";
            }

            yield break;
        }
    }
}