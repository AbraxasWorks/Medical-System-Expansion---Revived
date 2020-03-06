using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrenoMSE.HarmonyPatches
{
    public class MedicalRecipesUtility_SpawnNaturalPartIfClean
    {
        [HarmonyPatch( typeof( MedicalRecipesUtility ) )]
        [HarmonyPatch( "SpawnNaturalPartIfClean" )]
        internal class SpawnNaturalPartIfClean
        {
            [HarmonyPostfix]
            public static void IncludeChildParts ( ref Thing __result, Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map )
            {
                // if it created a Thing check the children for Things to add 
                if ( __result != null && __result is ThingWithComps resWithComps )
                {
                    AddSubparts( ref resWithComps, pawn, part, pos, map );
                }
            }
        }

        /// <summary>
        /// Looks at the part children and adds the corresponding Thing to the item
        /// </summary>
        /// <param name="item">Where the item parts are to be added</param>
        /// <param name="pawn">The pawn from which to check if the parts are ok</param>
        /// <param name="part">The parent part</param>
        /// <param name="pos">Location where to spawn eventual other items from hediffs</param>
        /// <param name="map">Map where to spawn eventual other items from hediffs</param>
        public static void AddSubparts ( ref ThingWithComps item, Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map )
        {
            List<Thing> childThings = new List<Thing>();

            // populates childThings and drops eventual hediff things
            foreach ( BodyPartRecord subPart in part.GetDirectChildParts() )
            {
                if ( !pawn.health.hediffSet.GetNotMissingParts().Contains( subPart ) )
                {
                    continue;
                }

                Thing childThing = MakeNaturalPartIfClean( pawn, subPart, pos, map );
                //MedicalRecipesUtility.SpawnThingsFromHediffs( pawn, subPart, pos, map );

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
                    item.AllComps.Add( comp );
                }

                comp.childPartsIncluded = childThings; // replace child part list
            }
        }

        /// <summary>
        /// If the part is clean it returns it as item; also drops Things from hediffs on subparts
        /// </summary>
        /// <param name="pawn">The pawn from which to check if the parts are ok</param>
        /// <param name="part">The part to drop as item</param>
        /// <param name="pos">Location where to spawn eventual other items</param>
        /// <param name="map">Map where to spawn eventual other items</param>
        /// <returns>The Thing corresponding to the part if it is clean, with eventual subparts</returns>
        public static Thing MakeNaturalPartIfClean( Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map )
        {
            if ( MedicalRecipesUtility.IsCleanAndDroppable( pawn, part ) )
            {
                Thing item = ThingMaker.MakeThing( part.def.spawnThingOnRemoved );
                
                if ( item is ThingWithComps itemWithComps )
                {
                    AddSubparts( ref itemWithComps, pawn, part, pos, map );
                    return itemWithComps;
                }
                else
                {
                    return item;
                }
            }
            return null;
        }
    }
}