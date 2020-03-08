using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace OrenoMSE
{

    class CompProperties_CompIncludedChildParts : CompProperties
    {
        public CompProperties_CompIncludedChildParts ()
        {
            this.compClass = typeof( CompIncludedChildParts );
        }


        public override void ResolveReferences ( ThingDef parentDef )
        {
            base.ResolveReferences( parentDef );

            // autogeneration
            if ( autogenerate )
            {
                // should probably be improved
                try
                {
                    string name = parentDef.defName;

                    // i use a temporary list so if the try fails no null things are added to standardChildren
                    List<ThingDef> temp = new List<ThingDef>();

                    if ( name.Contains( "Arm" ) )
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
