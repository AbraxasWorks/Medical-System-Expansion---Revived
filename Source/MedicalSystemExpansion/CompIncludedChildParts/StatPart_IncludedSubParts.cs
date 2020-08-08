using RimWorld;
using UnityEngine;
using Verse;

namespace MSE2
{
    internal class StatPart_IncludedSubParts : StatPart
    {
        // This stat part impacts market value adding the value of included subparts

        public override string ExplanationPart ( StatRequest req )
        {
            if ( req.HasThing )
            {
                var comp = req.Thing.TryGetComp<CompIncludedChildParts>();

                if ( comp != null && comp.ValueOfChildParts != 0f )
                {
                    return "StatsReport_IncludedSubParts".Translate( comp.ValueOfChildParts.ToStringMoneyOffset() );
                }
            }

            return null;
        }

        public override void TransformValue ( StatRequest req, ref float val )
        {
            if ( req.HasThing )
            {
                var comp = req.Thing.TryGetComp<CompIncludedChildParts>();

                if ( comp != null )
                {
                    val += comp.ValueOfChildParts;
                }
            }
        }
    }
}