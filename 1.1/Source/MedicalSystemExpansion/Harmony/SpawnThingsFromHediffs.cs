using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OrenoMSE.HarmonyPatches
{
    public class MedicalRecipesUtility_SpawnThingsFromHediffs_Patch
    {
        
        [HarmonyPatch( typeof( MedicalRecipesUtility ) )]
        [HarmonyPatch( "SpawnThingsFromHediffs" )]
        internal class SpawnThingsFromHediffs
        {
            [HarmonyPrefix]
            [HarmonyPriority( Priority.Last )]
            public static bool ReplaceWithCustom ( Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map )
            {
                // Spawn every thing that can be made from the heddiffs on this part and childparts
                foreach ( Thing item in MakeThingsFromHediffs( pawn, part, pos, map ) )
                {
                    GenSpawn.Spawn( item, pos, map );
                }

                //Log.Message( "Completed SpawnThingsFromHediffs" );
                return false;
            }
        }


        /// <summary>
        /// Generates a list of all Things that can be dropped from a part and subparts
        /// </summary>
        /// <param name="pawn">Pawn from which to check hediffs</param>
        /// <param name="part">From where to look for hediffs</param>
        /// <param name="pos"></param>
        /// <param name="map"></param>
        /// <returns>All Things that hediffs from part and childparts can drop, with subparts inserted into the correct parent</returns>
        public static IEnumerable<Thing> MakeThingsFromHediffs ( Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map )
        {
            //Log.Message( "MakeThingsFromHediffs - " + part.Label );

            // stop if the part is missing
            if ( !pawn.health.hediffSet.GetNotMissingParts().Contains( part ) )
            {
                yield break;
            }

            /// Things that can be made from all subPart hediffs
            List<Thing> subThings = new List<Thing>();

            foreach ( BodyPartRecord subPart in part.GetDirectChildParts() ) // for each subpart
            {
                if ( !MedicalRecipesUtility.IsClean( pawn, part ) ) // If parent is not clean try to make natural parts out of children
                {
                    MedicalRecipesUtility.SpawnNaturalPartIfClean( pawn, subPart, pos, map );
                }

                // add each thing coming from the child hediffs
                foreach ( Thing subthing in MakeThingsFromHediffs( pawn, subPart, pos, map ) )
                {
                    subThings.Add( subthing );
                }
            }

            // for every thing makeable from hediffs on this part: add subparts if possible then return it
            foreach ( Hediff hediff in from x in pawn.health.hediffSet.hediffs 
                                       where x.Part == part
                                       select x ) // for every hediff on the part
            {
                if ( hediff.def.spawnThingOnRemoved != null ) // if it spawns an item
                {
                    Thing item = ThingMaker.MakeThing( hediff.def.spawnThingOnRemoved );
                    
                    if ( item is ThingWithComps itemWithComps ) // compose if possible
                    {
                        AddSubparts( ref itemWithComps, ref subThings, pawn, part, pos, map );
                    }
                    
                    yield return item;
                }
            }
            
            // return other unclaimed subthings 
            foreach ( Thing item in subThings )
            {
                yield return item;
            }

            yield break;
        }

        public static void AddSubparts ( ref ThingWithComps item, ref List<Thing> available, Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map, bool reset = true )
        {
            //Log.Message( "AddSubparts - " + part.Label );
            CompIncludedChildParts comp = item.TryGetComp<CompIncludedChildParts>();
            
            if ( comp != null )
            {
                if ( reset )
                {
                    comp.childPartsIncluded = new List<Thing>();
                }

                List<Thing> childThings = new List<Thing>();

                foreach ( Thing potential in available )
                {
                    Thing match = available.Find( delegate ( Thing x ) { return comp.MissingParts().Contains( x.def ); } );
                    
                    if ( match != null )
                    {
                        available.Remove( match );
                        comp.childPartsIncluded.Add( match );

                        AddSubparts( ref item, ref available, pawn, part, pos, map, false );
                        return;
                    }
                }
                


            }
        }

    }
}