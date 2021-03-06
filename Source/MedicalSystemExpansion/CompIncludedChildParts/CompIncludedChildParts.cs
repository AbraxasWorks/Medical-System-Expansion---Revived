﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MSE2
{
    public partial class CompIncludedChildParts : ThingComp
    {
        public override void PostSpawnSetup ( bool respawningAfterLoad )
        {
            base.PostSpawnSetup( respawningAfterLoad );

            // Create the needed command gizmos
            this.command_AddExistingSubpart = new Command_AddExistingSubpart( this );
            this.command_SplitOffSubpart = new Command_SplitOffSubpart( this );
        }

        public CompProperties_IncludedChildParts Props
        {
            get
            {
                return this.props as CompProperties_IncludedChildParts;
            }
        }

        private List<Thing> childPartsIncluded = new List<Thing>();

        /// <summary>
        /// The list constaining the direct child parts of this part
        /// </summary>
        public List<Thing> IncludedParts
        {
            get => this.childPartsIncluded;
            set => this.childPartsIncluded = value;
        }

        public List<ThingDef> StandardParts
        {
            get => this.Props?.standardChildren;
        }

        // Creation / Deletion

        public override void PostPostMake ()
        {
            base.PostPostMake();

            // (if you want it incomplete replace the list after creating the thing)
            this.InitializeIncludedParts();
        }

        /// <summary>
        /// Initialize IncludedParts to standard children
        /// </summary>
        protected void InitializeIncludedParts ()
        {
            if ( this.StandardParts != null )
            {
                foreach ( ThingDef sChild in this.StandardParts )
                {
                    this.IncludedParts.Add( ThingMaker.MakeThing( sChild ) );
                }
            }
        }

        public override void PostDestroy ( DestroyMode mode, Map previousMap )
        {
            base.PostDestroy( mode, previousMap );

            // destroy included child items (idk if it does anything as they aren't spawned)
            if ( this.IncludedParts != null )
            {
                foreach ( Thing childPart in this.IncludedParts )
                {
                    childPart.Destroy( DestroyMode.Vanish );
                }
            }
        }

        // Stats display

        protected String cachedInspectString = null;

        public override string CompInspectStringExtra ()
        {
            if ( this.IncludedParts != null )
            {
                if ( this.cachedInspectString == null )
                {
                    this.cachedInspectString = "Includes "
                        + this.IncludedParts.Count + (this.IncludedParts.Count != 1 ? " direct subparts" : " direct subpart");

                    if ( this.MissingParts.Any() )
                    {
                        this.cachedInspectString += " (incomplete)";
                    }
                    else if ( this.AllMissingParts.Any() )
                    {
                        this.cachedInspectString += " (sub-part incomplete)";
                    }

                    this.cachedInspectString += ".";
                }

                return this.cachedInspectString;
            }
            return null;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats ()
        {
            // hyperlink lists
            var includedPartLinks = new List<Dialog_InfoCard.Hyperlink>(
                from x in this.IncludedParts
                select new Dialog_InfoCard.Hyperlink( x ) );

            var missingPartLinks = new List<Dialog_InfoCard.Hyperlink>(
                from x in this.MissingParts
                select new Dialog_InfoCard.Hyperlink( x ) );

            // always return the included parts entry
            yield return new StatDrawEntry(
                StatCategoryDefOf.Basics,
                "Included subparts:", // Translate
                includedPartLinks.Count.ToString(),
                "When implanted it will also install theese:",
                2500,
                null,
                includedPartLinks,
                false );

            // if some parts are missing also return missing parts entry
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

            yield break;
        }

        // gizmos for merging and splitting

        private Command_AddExistingSubpart command_AddExistingSubpart;
        private Command_SplitOffSubpart command_SplitOffSubpart;

        public override IEnumerable<Gizmo> CompGetGizmosExtra ()
        {
            yield return this.command_AddExistingSubpart;
            yield return this.command_SplitOffSubpart;

            foreach ( var g in base.CompGetGizmosExtra() ) yield return g;

            yield break;
        }

        public void AddPart ( Thing part )
        {
            if ( !this.StandardParts.Contains( part.def ) )
            {
                Log.Error( part.Label + " is not a valid subpart for " + this.parent.Label );
                return;
            }

            this.IncludedParts.Add( part );
            this.DirtyCache();

            if ( part.Spawned )
            {
                part.DeSpawn();
            }
        }

        public void RemoveAndSpawnPart ( Thing part, IntVec3 position = default, Map map = null )
        {
            if ( !this.IncludedParts.Contains( part ) )
            {
                Log.Error( "Tried to remove " + part.Label + " from " + this.parent.Label + " while it wasn't actually included." );
                return;
            }

            // if a map is not provided, take it from this parent
            if ( map == null )
            {
                map = this.parent.Map;
                position = this.parent.Position;
            }

            this.IncludedParts.Remove( part );
            this.DirtyCache();

            GenPlace.TryPlaceThing( part, position, map, ThingPlaceMode.Near );
        }

        // Save / Load

        public override void PostExposeData ()
        {
            base.PostExposeData();
            Scribe_Collections.Look( ref this.childPartsIncluded, "childPartsIncluded", LookMode.Deep );

            if ( this.IncludedParts == null )
            {
                this.IncludedParts = new List<Thing>();
                InitializeIncludedParts();
            }
        }

        // Missing Parts

        private List<ThingDef> cachedMissingParts;

        public IEnumerable<ThingDef> MissingParts
        {
            get
            {
                if ( this.cachedMissingParts == null )
                {
                    this.UpdateMissingParts();
                }

                return this.cachedMissingParts;
            }
        }

        protected void UpdateMissingParts ()
        {
            if ( this.cachedMissingParts == null )
            {
                this.cachedMissingParts = new List<ThingDef>();
            }
            else
            {
                this.cachedMissingParts.Clear();
            }

            if ( !this.StandardParts.NullOrEmpty() )
            {
                List<ThingDef> defsIncluded = new List<ThingDef>( this.IncludedParts.Select( t => t.def ) );

                foreach ( ThingDef expectedDef in this.StandardParts )
                {
                    if ( defsIncluded.Contains( expectedDef ) )
                    {
                        defsIncluded.Remove( expectedDef );
                    }
                    else
                    {
                        this.cachedMissingParts.Add( expectedDef );
                    }
                }
            }
        }

        // Missing parts value $$$

        private float cachedMissingValue = -1f;

        public float MissingValue
        {
            get
            {
                if ( this.cachedMissingValue == -1f )
                {
                    this.UpdateMissingValue();
                }

                return this.cachedMissingValue;
            }
        }

        protected float UpdateMissingValue ()
        {
            this.cachedMissingValue = 0f;

            // add 80% of the value of missing parts
            foreach ( var missingPart in this.MissingParts )
            {
                this.cachedMissingValue += missingPart.BaseMarketValue * 0.8f;
            }

            // add the missing value of all subparts
            foreach ( var subPart in this.IncludedParts )
            {
                var sComp = subPart.TryGetComp<CompIncludedChildParts>();

                if ( sComp != null )
                {
                    this.cachedMissingValue += sComp.MissingValue;
                }
            }

            // can't have more than 80% of the normal value as missing value
            return Mathf.Clamp( this.cachedMissingValue, 0f, this.parent.def.BaseMarketValue * 0.8f );
        }

        /// <summary>
        /// Resets the cache for MissingParts, MissingValue and inspectString
        /// </summary>
        public void DirtyCache ()
        {
            this.cachedMissingParts = null;
            this.cachedMissingValue = -1f;
            this.cachedInspectString = null;
        }

        /// <summary>
        /// Calls DirtyCache for all the ancestors of recursionEnd (excluded) up to the calling comp
        /// </summary>
        /// <returns>True if it actually found recursionEnd</returns>
        public bool DirtyCacheDeep ( CompIncludedChildParts recursionEnd )
        {
            if ( this == recursionEnd )
            {
                // base case (no DirtyCache!)
                return true;
            }
            foreach ( var comp in from i in this.IncludedParts
                                  let comp = i.TryGetComp<CompIncludedChildParts>()
                                  where comp != null
                                  select comp )
            {
                // Depth-First-Search, call DirtyCache descending the call stack
                if ( comp.DirtyCacheDeep( recursionEnd ) )
                {
                    this.DirtyCache();
                    return true;
                }
            }
            return false;
        }

        // From children

        /// <summary>
        /// Recursively searched for MissingParts in all of the sub-parts
        /// </summary>
        public IEnumerable<(ThingDef, CompIncludedChildParts)> AllMissingParts
        {
            get => Enumerable.Concat(

                // the missing parts of this part
                this.MissingParts.Select( p => (p, this) ),

                // the missing parts of the children with CompIncludedChildParts
                from i in this.IncludedParts
                let comp = i.TryGetComp<CompIncludedChildParts>()
                where comp != null
                from couple in comp.AllMissingParts
                select couple );
        }

        /// <summary>
        /// Recursively searches for IncludedParts in all of the sub-parts
        /// </summary>
        public IEnumerable<(Thing, CompIncludedChildParts)> AllIncludedParts
        {
            get => Enumerable.Concat(

                // the sub-parts included in this part
                this.IncludedParts.Select( p => (p, this) ),

                // the sub-parts of the children with CompIncludedChildParts
                from i in this.IncludedParts
                let comp = i.TryGetComp<CompIncludedChildParts>()
                where comp != null
                from couple in comp.AllIncludedParts
                select couple );
        }
    }
}