using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrenoMSE.MSE_ThingClass
{
    class MSE_ThingBodyPart : ThingWithComps // New ThingClass of BodyPartBase
    {
        public override void PostMake ()
        {
            base.PostMake();

            Log.Message( "Test thing class replacing" );


            if ( this.def.defName == "BionicArm" )
            {
                this.childPartsIncluded.Add( (MSE_ThingBodyPart) ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed( "BionicHand" ) ));
                this.childPartsIncluded.Add( (MSE_ThingBodyPart)ThingMaker.MakeThing( DefDatabase<ThingDef>.GetNamed( "BionicHumerus" ) ) );
                this.childPartsIncluded.Add( (MSE_ThingBodyPart)ThingMaker.MakeThing( DefDatabase<ThingDef>.GetNamed( "BionicRadius" ) ) );
            }
            if ( this.def.defName == "BionicHand" )
            {
                this.childPartsIncluded.Add( (MSE_ThingBodyPart)ThingMaker.MakeThing( DefDatabase<ThingDef>.GetNamed( "BionicFinger" ) ) );
                this.childPartsIncluded.Add( (MSE_ThingBodyPart)ThingMaker.MakeThing( DefDatabase<ThingDef>.GetNamed( "BionicFinger" ) ) );
                this.childPartsIncluded.Add( (MSE_ThingBodyPart)ThingMaker.MakeThing( DefDatabase<ThingDef>.GetNamed( "BionicFinger" ) ) );
                this.childPartsIncluded.Add( (MSE_ThingBodyPart)ThingMaker.MakeThing( DefDatabase<ThingDef>.GetNamed( "BionicFinger" ) ) );
                this.childPartsIncluded.Add( (MSE_ThingBodyPart)ThingMaker.MakeThing( DefDatabase<ThingDef>.GetNamed( "BionicFinger" ) ) );
            }

        }


        public List<MSE_ThingBodyPart> childPartsIncluded = new List<MSE_ThingBodyPart>();
    }
}
