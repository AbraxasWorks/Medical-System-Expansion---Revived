﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;

namespace OrenoMSE
{
    class StatPart_MissingSubParts : StatPart
    {
        
        // This stat part impacts market value subtractin the value of missing subparts


        public override string ExplanationPart ( StatRequest req )
        {
            if ( req.HasThing )
            {
                var comp = req.Thing.TryGetComp<CompIncludedChildParts>();

                if ( comp != null && comp.MissingValue != 0f )
                {
                    return "Some subparts are missing: -$" + comp.MissingValue;
                }
            }

            return null;
        }

        //public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks ( StatRequest req )
        //{
        //    return base.GetInfoCardHyperlinks( req );
        //}

        public override void TransformValue ( StatRequest req, ref float val )
        {
            if ( req.HasThing )
            {
                var comp = req.Thing.TryGetComp<CompIncludedChildParts>();

                if ( comp != null )
                {
                    val -= comp.MissingValue;
                }
            }
        }
    }
}
