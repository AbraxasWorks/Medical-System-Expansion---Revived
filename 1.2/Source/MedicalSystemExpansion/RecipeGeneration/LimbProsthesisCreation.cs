using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;

namespace MSE2
{
    internal class LimbProsthesisCreation : DefModExtension
    {
        public LimbConfiguration targetLimb;

        public override IEnumerable<string> ConfigErrors ()
        {
            foreach ( string error in base.ConfigErrors() ) yield return error;

            if ( targetLimb == null )
            {
                yield return "targetLimb is null";
            }
            else if ( targetLimb.RecordExample == null )
            {
                yield return "targetLimb contains no records";
            }
        }
    }
}