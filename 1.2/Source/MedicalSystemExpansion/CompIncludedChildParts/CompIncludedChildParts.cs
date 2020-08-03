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
        public override void Initialize ( CompProperties props )
        {
            base.Initialize( props );

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
            set
            {
                this.childPartsIncluded = value;
                this.DirtyCache();
            }
        }

        public List<ThingDef> StandardParts
        {
            get => this.Props?.standardChildren;
        }

        //

        private List<BodyPartRecord> cachedCompatibleParts;

        public IEnumerable<BodyPartRecord> CompatibleParts
        {
            get
            {
                if ( this.cachedCompatibleParts == null )
                {
                    this.cachedCompatibleParts = (from b_bpd in this.Props.installationDestinations
                                                  from bpr in b_bpd.Item1.AllParts
                                                  where bpr.def == b_bpd.Item2
                                                  where this.IsCompatibleWith( bpr )
                                                  select bpr
                                                  ).ToList();
                }
                return this.cachedCompatibleParts;
            }
        }

        private bool IsCompatibleWith ( BodyPartRecord bodyPartRecord )
        {
            return this.Props.EverInstallableOn( bodyPartRecord )
                && IncludedPartsUtilities.InstallationCompatibility( this.childPartsIncluded, bodyPartRecord.GetDirectChildParts() );
        }

        // Creation / Deletion

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

        public void InitializeForPart ( BodyPartRecord bodyPartRecord )
        {
            if ( !this.Props.EverInstallableOn( bodyPartRecord ) )
            {
                Log.Error( "[MSE2] Tried to initialize " + this.parent.Label + " for part where it cannot be installed (" + bodyPartRecord + " " + bodyPartRecord.body + ")" );
                return;
            }

            foreach ( (ThingDef childDef, BodyPartRecord bpr) in this.Props.StandardPartsForBodyPartRecord( bodyPartRecord ) )
            {
                Thing child = ThingMaker.MakeThing( childDef );
                child.TryGetComp<CompIncludedChildParts>()?.InitializeForPart( bpr );
                this.AddPart( child );
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

        // Inspect string
        protected String cachedInspectString = null;

        public override string CompInspectStringExtra ()
        {
            if ( this.IncludedParts != null )
            {
                if ( this.cachedInspectString == null )
                {
                    this.cachedInspectString = "CompIncludedChildParts_InspectString".Translate( this.IncludedParts.Count );
                }

                return this.cachedInspectString;
            }
            return null;
        }

        // Label

        protected String cachedTransformLabelString = null;

        public override string TransformLabel ( string label )
        {
            if ( this.IncludedParts != null )
            {
                if ( this.cachedTransformLabelString == null )
                {
                    this.cachedTransformLabelString = " (";

                    if ( this.CompatibleParts.Any() )
                    {
                        this.cachedTransformLabelString += String.Join( ", ", this.CompatibleParts.Select( bpr => bpr.body ).Distinct().Select( b => b.label ) );
                    }
                    else
                    {
                        this.cachedTransformLabelString += "incomplete";
                    }

                    this.cachedTransformLabelString += ")";
                }

                return label + this.cachedTransformLabelString;
            }
            return null;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats ()
        {
            // hyperlink lists
            var includedPartLinks =
                from x in this.IncludedParts
                select new Dialog_InfoCard.Hyperlink( x );

            // always return the included parts entry
            yield return new StatDrawEntry(
                StatCategoryDefOf.Basics,
                "CompIncludedChildParts_StatIncludedParts_Label".Translate(),
                this.IncludedParts.Count.ToString(),
                "CompIncludedChildParts_StatIncludedParts_Description".Translate(),
                2500,
                null,
                includedPartLinks,
                false );

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

        /// <summary>
        /// Resets the cache for MissingParts, MissingValue and inspectString
        /// </summary>
        public void DirtyCache ()
        {
            this.cachedInspectString = null;
            this.cachedTransformLabelString = null;
            this.cachedCompatibleParts = null;
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
        public float ValueOfChildParts =>
            this.IncludedParts.Select( p => p.MarketValue ).Aggregate( 0f, ( a, b ) => a + b );

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

        /// <summary>
        /// Recursively searches for StandardParts in all of the sub-parts
        /// </summary>
        public IEnumerable<(ThingDef, CompIncludedChildParts)> AllStandardParts
        {
            get => Enumerable.Concat(

                // the standard sub-parts included in this part
                this.StandardParts.Select( p => (p, this) ),

                // the standard sub-parts of the children with CompIncludedChildParts
                from i in this.IncludedParts
                let comp = i.TryGetComp<CompIncludedChildParts>()
                where comp != null
                from couple in comp.AllStandardParts
                select couple );
        }
    }
}