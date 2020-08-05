using RimWorld;
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

        // Save / Load

        public override void PostExposeData ()
        {
            base.PostExposeData();
            Scribe_Collections.Look( ref this.childPartsIncluded, "childPartsIncluded", LookMode.Deep );

            if ( this.IncludedParts == null )
            {
                this.IncludedParts = new List<Thing>();
                Log.Warning( "[MSE2] Included parts null while serializing or deserializing data." );
                //InitializeIncludedParts();
            }
        }

        /// <summary>
        /// Resets the cache for MissingParts, MissingValue and inspectString
        /// </summary>
        public void DirtyCache ()
        {
            this.cachedCompatibleLimbs = null;
            this.cachedTransformLabelString = null;
            this.cachedInspectString = null;
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

        #region PartHandling

        // Included parts

        private List<Thing> childPartsIncluded = new List<Thing>();

        public List<Thing> IncludedParts
        {
            get => this.childPartsIncluded;
            set
            {
                this.childPartsIncluded = value;
                this.DirtyCache();
            }
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

        // Standard parts

        public List<ThingDef> StandardParts
        {
            get => this.Props?.standardChildren;
        }

        // Compatible limbs

        private List<LimbConfiguration> cachedCompatibleLimbs;

        public IEnumerable<LimbConfiguration> CompatibleLimbs
        {
            get
            {
                if ( this.cachedCompatibleLimbs == null )
                {
                    this.cachedCompatibleLimbs = (from lc in this.Props.installationDestinations
                                                  where this.IsCompatibleWith( lc )
                                                  select lc
                                                  ).ToList();
                }
                return this.cachedCompatibleLimbs;
            }
        }

        private bool IsCompatibleWith ( LimbConfiguration limb )
        {
            return this.Props.EverInstallableOn( limb )
                && IncludedPartsUtilities.InstallationCompatibility( this.childPartsIncluded, limb.ChildLimbs );
        }

        // Creation / Deletion

        public void InitializeForLimb ( LimbConfiguration limb )
        {
            if ( !this.Props.EverInstallableOn( limb ) )
            {
                Log.Error( "[MSE2] Tried to initialize " + this.parent.Label + " for part where it cannot be installed (" + limb.PartDef + ")" );
                return;
            }

            foreach ( (ThingDef childDef, LimbConfiguration bpr) in this.Props.StandardPartsForLimb( limb ) )
            {
                Thing child = ThingMaker.MakeThing( childDef );
                child.TryGetComp<CompIncludedChildParts>()?.InitializeForLimb( bpr );
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

        #endregion PartHandling

        #region StatsDisplay

        // Label

        protected String cachedTransformLabelString = null;

        public override string TransformLabel ( string label )
        {
            if ( this.IncludedParts != null )
            {
                if ( this.cachedTransformLabelString == null )
                {
                    this.cachedTransformLabelString = " (";

                    if ( this.CompatibleLimbs.Any() )
                    {
                        this.cachedTransformLabelString += String.Join( "; ", this.CompatibleLimbs.Select( lc => lc.Label ) );
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

        // Stat entries

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats ()
        {
            // Included parts
            yield return new StatDrawEntry(
                StatCategoryDefOf.Basics,
                "CompIncludedChildParts_StatIncludedParts_Label".Translate(),
                this.IncludedParts.Count.ToString(),
                "CompIncludedChildParts_StatIncludedParts_Description".Translate(),
                2500,
                null,
                this.IncludedParts.Select( p => new Dialog_InfoCard.Hyperlink( p ) ),
                false );
        }

        #endregion StatsDisplay

        #region RecursiveData

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
        public float ValueOfChildParts
        {
            get =>
                this.IncludedParts.Select( p => p.MarketValue ).Aggregate( 0f, ( a, b ) => a + b );
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

        #endregion RecursiveData
    }
}