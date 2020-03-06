using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace OrenoMSE
{
    class CompIncludedChildParts : ThingComp
    {
        public override void PostPostMake ()
        {
            base.PostPostMake();
            //Log.Message( "CompPostMake" );

            if ( this.Props.standardChildren != null )
            {
                foreach ( ThingDef sChild in this.Props.standardChildren )
                {
                    //Log.Message( "Adding from def: " + sChild.defName );
                    this.childPartsIncluded.Add( ThingMaker.MakeThing( sChild ) );
                }
            }
        }

        public override void PostDestroy ( DestroyMode mode, Map previousMap )
        {
            base.PostDestroy( mode, previousMap );
            if ( this.childPartsIncluded != null )
            {
                foreach ( ThingWithComps childPart in this.childPartsIncluded )
                {
                    childPart.Destroy( DestroyMode.Vanish );
                }
            }
        }
        public override string CompInspectStringExtra ()
        {
            if ( this.childPartsIncluded != null )
            {
                //StringBuilder stringBuilder = new StringBuilder();
                //for ( int i = 0; i < this.childPartsIncluded.Count; i++ )
                //{
                //    stringBuilder.AppendLine( this.childPartsIncluded[i].Label );
                //}
                //return stringBuilder.ToString().TrimEndNewlines();
                return "Includes " 
                    + this.childPartsIncluded.Count + ( this.childPartsIncluded.Count != 1 ? " subparts" : " subpart" ) 
                    + ( this.MissingParts().Count() > 0 ? " (incomplete)" : "" ) 
                    + ".";
            }
            return null;
        }
        public CompProperties_CompIncludedChildParts Props
        {
            get
            {
                return (CompProperties_CompIncludedChildParts) this.props;
            }
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats ()
        {
            if ( this.childPartsIncluded != null )
            {
                List<Dialog_InfoCard.Hyperlink> partLinks = new List<Dialog_InfoCard.Hyperlink>();
                foreach ( Thing childPart in this.childPartsIncluded )
                {
                    partLinks.Add( new Dialog_InfoCard.Hyperlink( childPart ) );
                }

                List<Dialog_InfoCard.Hyperlink> missingPartLinks = new List<Dialog_InfoCard.Hyperlink>();
                foreach ( ThingDef childPart in this.MissingParts() )
                {
                    missingPartLinks.Add( new Dialog_InfoCard.Hyperlink( childPart ) );
                }

                yield return new StatDrawEntry(
                    StatCategoryDefOf.Basics,
                    "Included subparts:", // Translate
                    partLinks.Count.ToString(),
                    "When implanted it will also install theese:",
                    2500,
                    null,
                    partLinks,
                    false );

                if ( missingPartLinks.Count > 0 )
                {
                    yield return new StatDrawEntry(
                        StatCategoryDefOf.Basics,
                        "Missing subparts:", // Translate
                        missingPartLinks.Count.ToString(),
                        "These parts are missing:",
                        2501,
                        null,
                        missingPartLinks,
                        false );
                }

            }
            yield break;
        }
        public override void PostExposeData ()
        {
            base.PostExposeData();
            Scribe_Collections.Look<Thing>( ref this.childPartsIncluded, "childPartsIncluded", LookMode.Deep );
        }

        public IEnumerable<ThingDef> MissingParts()
        {
            if ( this.props != null )
            {
                List< ThingDef > included = this.childPartsIncluded.ConvertAll((Thing x) => { return x.def; });
            
                foreach ( ThingDef expectedDef in this.Props.standardChildren )
                {
                    if ( included.Contains( expectedDef ) )
                    {
                        included.Remove( expectedDef );
                    }
                    else
                    {
                        yield return expectedDef;
                    }
                }
            }

            yield break;
        }

        public List<Thing> childPartsIncluded = new List<Thing>();
    }

    class CompProperties_CompIncludedChildParts : CompProperties
    {
        public CompProperties_CompIncludedChildParts()
        {
            this.compClass = typeof( CompIncludedChildParts );
        }

        public override void ResolveReferences ( ThingDef parentDef )
        {
            base.ResolveReferences( parentDef );

            // autogen
            if (autogenerate)
            {
                try
                {
                    string name = parentDef.defName;

                    List<ThingDef> temp = new List<ThingDef>();

                    if ( name.Contains("Arm") )
                    {
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Arm", "Hand" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Arm", "Humerus" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Arm", "Radius" ) ) );
                    }
                    if ( name.Contains( "Hand" ) )
                    {
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Hand", "Finger" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Hand", "Finger" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Hand", "Finger" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Hand", "Finger" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Hand", "Finger" ) ) );
                    }

                    if ( name.Contains( "Leg" ) )
                    {
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Leg", "Foot" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Leg", "Femur" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Leg", "Tibia" ) ) );
                    }
                    if ( name.Contains( "Foot" ) )
                    {
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Foot", "Toe" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Foot", "Toe" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Foot", "Toe" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Foot", "Toe" ) ) );
                        temp.Add( DefDatabase<ThingDef>.GetNamedSilentFail( name.Replace( "Foot", "Toe" ) ) );
                    }

                    foreach ( ThingDef def in temp )
                    {
                        if ( def != null )
                        {
                            this.standardChildren.Add( def );
                        }
                    }


                }
                catch { }
            }

        }



        public List<ThingDef> standardChildren = new List<ThingDef>();

        public bool autogenerate = false;
    }

}
