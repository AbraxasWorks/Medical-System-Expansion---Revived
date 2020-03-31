using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OrenoMSE.Modules
{
    public class HediffCompProperties_ModuleHolder : HediffCompProperties
    {
        
        public HediffCompProperties_ModuleHolder()
        {
            this.compClass = typeof( HediffComp_ModuleHolder );
        }


        public override IEnumerable<string> ConfigErrors ( HediffDef parentDef )
        {
            IEnumerable<string> baseCE = base.ConfigErrors( parentDef );
            if (this.maxModules < 0)
            {
                baseCE.Concat( "Comp has negative module slots" );
            }

            return baseCE;
        }




        public int maxModules = 1;

    }
}
