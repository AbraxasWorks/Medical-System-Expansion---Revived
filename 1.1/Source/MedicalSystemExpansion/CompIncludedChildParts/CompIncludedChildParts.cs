using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OrenoMSE
{
    class CompIncludedChildParts : ThingComp
    {
        public CompProperties_CompIncludedChildParts Props
        {
            get
            {
                return (CompProperties_CompIncludedChildParts)this.props;
            }
        }

        public List<Thing> childPartsIncluded = new List<Thing>();


        // Creation / Deletion

        public override void PostPostMake ()
        {
            base.PostPostMake();

            // add standard children
            // (if you want it inclomplete replace the list after creating the thing)
            if ( this.Props.standardChildren != null )
            {
                foreach ( ThingDef sChild in this.Props.standardChildren )
                {
                    this.childPartsIncluded.Add( ThingMaker.MakeThing( sChild ) );
                }
            }
        }

        public override void PostDestroy ( DestroyMode mode, Map previousMap )
        {
            base.PostDestroy( mode, previousMap );

            // destroy included child items (idk if it does anything as they aren't spawned)
            if ( this.childPartsIncluded != null )
            {
                foreach ( ThingWithComps childPart in this.childPartsIncluded )
                {
                    childPart.Destroy( DestroyMode.Vanish );
                }
            }
        }


        // Stats display

        public override string CompInspectStringExtra ()
        {
            if ( this.childPartsIncluded != null )
            {
                return "Includes "
                    + this.childPartsIncluded.Count + (this.childPartsIncluded.Count != 1 ? " subparts" : " subpart")
                    + (this.MissingParts.Count() > 0 ? " (incomplete)" : "")
                    + ".";
            }
            return null;
        }


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats ()
        {
            if ( this.childPartsIncluded != null )
            {
                var includedPartLinks = new List<Dialog_InfoCard.Hyperlink>(
                    from x in this.childPartsIncluded
                    select new Dialog_InfoCard.Hyperlink( x ) );

                var missingPartLinks = new List<Dialog_InfoCard.Hyperlink>(
                    from x in this.MissingParts
                    select new Dialog_InfoCard.Hyperlink( x ) );

                yield return new StatDrawEntry(
                    StatCategoryDefOf.Basics,
                    "Included subparts:", // Translate
                    includedPartLinks.Count.ToString(),
                    "When implanted it will also install theese:",
                    2500,
                    null,
                    includedPartLinks,
                    false );

                if ( missingPartLinks.Count > 0 )
                {
                    yield return new StatDrawEntry(
                        StatCategoryDefOf.Basics,
                        "Missing subparts:", // Translate
                        missingPartLinks.Count.ToString(),
                        "These parts are missing:",
                        2500,
                        null,
                        missingPartLinks,
                        false );
                }

            }
            yield break;
        }


        // Save / Load

        public override void PostExposeData ()
        {
            base.PostExposeData();
            Scribe_Collections.Look<Thing>( ref this.childPartsIncluded, "childPartsIncluded", LookMode.Deep );
        }


        // Missing Parts

        private List<ThingDef> missingPartsCache;

        public List<ThingDef> MissingParts
        {
            get
            {
                if (missingPartsCache == null)
                {
                    this.UpdateMissingParts();
                }
                
                return missingPartsCache;
            }
        }

        public void UpdateMissingParts()
        {
            if ( missingPartsCache == null )
            {
                missingPartsCache = new List<ThingDef>();
            }
            else
            {
                missingPartsCache.Clear();
            }


            if ( this.Props != null )
            {
                LinkedList<ThingDef> defsIncluded = new LinkedList<ThingDef>( from x in this.childPartsIncluded select x.def );
            
                foreach ( ThingDef expectedDef in this.Props.standardChildren )
                {
                    if ( defsIncluded.Contains( expectedDef ) )
                    {
                        defsIncluded.Remove( expectedDef );
                    }
                    else
                    {
                        missingPartsCache.Add(expectedDef);
                    }
                }
            }
        }

        // Missing value

        private float missingValueCache = -1f;

        public float MissingValue
        {
            get
            {
                if ( missingValueCache == -1f )
                {
                    this.UpdateMissingValue();
                }

                return missingValueCache;
            }
        }

        public float UpdateMissingValue()
        {
            missingValueCache = 0f;

            foreach ( var missingPart in this.MissingParts )
            {                
                missingValueCache += missingPart.BaseMarketValue * 0.8f;
            }
            
            foreach ( var subPart in this.childPartsIncluded )
            {
                var sComp = subPart.TryGetComp<CompIncludedChildParts>();

                if ( sComp != null )
                {
                    sComp.UpdateMissingValue();
                    missingValueCache += sComp.MissingValue;
                }
            }

            return Mathf.Clamp( missingValueCache, 0f, this.parent.def.BaseMarketValue * 0.8f );
        }

    }

}
