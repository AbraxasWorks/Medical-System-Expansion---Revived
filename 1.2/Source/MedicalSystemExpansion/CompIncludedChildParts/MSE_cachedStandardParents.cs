using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MSE2
{
    /// <summary>
    /// Automatically added to hediffs at load time, takes info from the thingcomp
    /// </summary>
    internal class MSE_cachedStandardParents : DefModExtension
    {
        private List<HediffDef> standardParents = new List<HediffDef>();

        public override IEnumerable<string> ConfigErrors ()
        {
            foreach ( var ce in base.ConfigErrors() ) yield return ce;

            if ( standardParents.NullOrEmpty() ) yield return "standardParents null or empty";
        }

        public void Add ( HediffDef parent )
        {
            this.standardParents.AddDistinct( parent );
        }

        public bool Contains ( HediffDef parent )
        {
            return standardParents.Contains( parent );
        }
    }
}