using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2.HarmonyPatches
{
    public class MedicalRecipesUtility_SpawnNaturalPartIfClean_Patch
    {
        [HarmonyPatch( typeof( MedicalRecipesUtility ) )]
        [HarmonyPatch( "SpawnNaturalPartIfClean" )]
        internal class SpawnNaturalPartIfClean
        {
            [HarmonyPostfix]
            public static void PostFix ( ref Thing __result, Pawn pawn, BodyPartRecord part )
            {
                // if it created a Thing check the children for Things to add
                if ( __result != null && __result is ThingWithComps resWithComps )
                {
                    AddSubparts( ref resWithComps, pawn, part );
                }
            }
        }

        /// <summary>
        /// Looks at the part children and adds the corresponding Thing to the item
        /// </summary>
        /// <param name="item">Where the item parts are to be added</param>
        /// <param name="pawn">The pawn from which to check if the parts are ok</param>
        /// <param name="part">The parent part</param>
        public static void AddSubparts ( ref ThingWithComps item, Pawn pawn, BodyPartRecord part )
        {
            List<Thing> childThings = new List<Thing>();

            // populates childThings
            foreach ( BodyPartRecord subPart in from x in part.GetDirectChildParts()
                                                where pawn.health.hediffSet.GetNotMissingParts().Contains( x ) // subpart is missing: skip it
                                                select x )
            {
                Thing childThing = MakeNaturalPartIfClean( pawn, subPart );

                if ( childThing != null )
                {
                    childThings.Add( childThing );
                }
            }

            // if it found childthings add them as comp to the item
            if ( childThings.Count > 0 )
            {
                CompIncludedChildParts comp = item.TryGetComp<CompIncludedChildParts>();

                if ( comp == null ) // if it doesn't have the comp, make it
                {
                    comp = new CompIncludedChildParts();
                    comp.parent = item;
                    comp.props = new CompProperties_IncludedChildParts();
                    comp.PostSpawnSetup( false );
                    item.AllComps.Add( comp );
                }

                comp.IncludedParts = childThings; // replace child part list
            }
        }

        /// <summary>
        /// If the part is clean it returns it as item
        /// </summary>
        /// <param name="pawn">The pawn from which to check if the parts are ok</param>
        /// <param name="part">The part to drop as item</param>
        /// <returns>The Thing corresponding to the part if it is clean, with eventual subparts</returns>
        public static Thing MakeNaturalPartIfClean ( Pawn pawn, BodyPartRecord part )
        {
            if ( MedicalRecipesUtility.IsCleanAndDroppable( pawn, part ) )
            {
                Thing item = ThingMaker.MakeThing( part.def.spawnThingOnRemoved );

                if ( item is ThingWithComps itemWithComps )
                {
                    AddSubparts( ref itemWithComps, pawn, part );
                }

                return item;
            }
            else
            {
                return null;
            }
        }
    }
}