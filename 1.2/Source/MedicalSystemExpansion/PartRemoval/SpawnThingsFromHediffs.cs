using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MSE2.HarmonyPatches
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
                    if ( map != null ) GenSpawn.Spawn( item, pos, map );
                }

                return false;
            }
        }

        /// <summary>
        /// Generates a list of all Things that can be dropped from a part and its subparts
        /// </summary>
        /// <param name="pawn">Pawn from which to check hediffs</param>
        /// <param name="part">From where to look for hediffs</param>
        /// <param name="pos">Position where to drop natural subparts</param>
        /// <param name="map">Map where to drop natural subparts</param>
        /// <returns>All Things that hediffs from part and childparts can drop, with subparts inserted into the correct parent</returns>
        public static IEnumerable<Thing> MakeThingsFromHediffs ( Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map )
        {
            // stop if the part is missing
            if ( !pawn.health.hediffSet.GetNotMissingParts().Contains( part ) )
            {
                yield break;
            }

            /// Things that can be made from all subPart hediffs
            List<Thing> subThings = new List<Thing>();

            foreach ( BodyPartRecord subPart in part.GetDirectChildParts() ) // for each subpart
            {
                if ( !MedicalRecipesUtility.IsClean( pawn, part ) ) // If parent is not clean
                {
                    MedicalRecipesUtility.SpawnNaturalPartIfClean( pawn, subPart, pos, map ); // try to make natural parts out of children
                }

                // add each thing coming from the child hediffs
                subThings.AddRange( MakeThingsFromHediffs( pawn, subPart, pos, map ) );
            }

            // for every thing makeable from hediffs on this part: add subparts if possible then return it
            List<Thing> items = new List<Thing>();
            foreach ( Hediff hediff in from x in pawn.health.hediffSet.hediffs
                                       where x.Part == part
                                       select x ) // for every hediff on the part
            {
                if ( hediff.def.spawnThingOnRemoved != null ) // if it spawns an item
                {
                    Thing item = ThingMaker.MakeThing( hediff.def.spawnThingOnRemoved );

                    if ( item is ThingWithComps itemWithComps ) // compose if possible
                    {
                        AddSubparts( itemWithComps, subThings );
                    }

                    items.Add( item );
                }
            }

            // merge siblings
            for ( int i = items.Count - 1; i >= 0; i-- )
            {
                if ( items[i] is ThingWithComps )
                    AddSubparts( items[i] as ThingWithComps, items, false );
            }

            // return all
            foreach ( Thing item in items.Concat( subThings ) )
            {
                yield return item;
            }

            yield break;
        }

        /// <summary>
        /// From the list of available Things, add the compatible subparts to the item
        /// </summary>
        /// <param name="item">The item to add the subparts to</param>
        /// <param name="available">The available things to add</param>
        /// <param name="reset">Should it reset the list of subparts in the item</param>
        public static void AddSubparts ( ThingWithComps item, List<Thing> available, bool reset = true )
        {
            CompIncludedChildParts comp = item.TryGetComp<CompIncludedChildParts>();

            if ( comp != null )
            {
                if ( reset )
                {
                    comp.IncludedParts.Clear();
                }

                foreach ( ThingDef compatible in comp.StandardParts )
                {
                    // TODO can prob simplify it
                    Thing match = available.Find( x => x.def == compatible );

                    if ( match != null )
                    {
                        available.Remove( match );
                        comp.AddPart( match );
                    }
                }
            }
        }
    }
}