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
        private CompIncludedChildParts holderComp;
        private CompIncludedChildParts TopHolderComp => holderComp == null ? this : this.holderComp.TopHolderComp;
        private Map Map => this.TopHolderComp.parent.Map;
        private IntVec3 Position => this.TopHolderComp.parent.Position;

        public override void Initialize ( CompProperties props )
        {
            base.Initialize( props );

            // Create the needed command gizmos
            this.command_SetTargetLimb = new Command_SetTargetLimb( this );
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

            // Deep save the included Things
            Scribe_Collections.Look( ref this.childPartsIncluded, "childPartsIncluded", LookMode.Deep );

            // Save an example of record in the limbTarget
            BodyPartRecord limbPartExample = this.TargetLimb?.RecordExample;
            Scribe_BodyParts.Look( ref limbPartExample, "targetLimb" );
            if ( Scribe.mode == LoadSaveMode.LoadingVars )
                this.TargetLimb = LimbConfiguration.GenerateOrGetLimbConfigForBodyPartRecord( limbPartExample );

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
            this.cachedMissingParts = null;
            this.cachedTransformLabelString = null;
            this.cachedInspectString = null;
        }

        // gizmos for merging and splitting

        private Command_SetTargetLimb command_SetTargetLimb;
        private Command_AddExistingSubpart command_AddExistingSubpart;
        private Command_SplitOffSubpart command_SplitOffSubpart;

        public override IEnumerable<Gizmo> CompGetGizmosExtra ()
        {
            yield return this.command_SetTargetLimb;
            yield return this.command_AddExistingSubpart;
            yield return this.command_SplitOffSubpart;

            foreach ( var g in base.CompGetGizmosExtra() ) yield return g;

            yield break;
        }

        #region PartHandling

        private List<Thing> tmpThingList = new List<Thing>();

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

        public IEnumerable<CompIncludedChildParts> IncludedPartComps =>
            this.IncludedParts.Select( p => p.TryGetComp<CompIncludedChildParts>() ).Where( c => c != null );

        public void AddPart ( Thing part )
        {
            if ( !this.StandardParts.Contains( part.def ) )
            {
                Log.Error( part.Label + " is not a valid subpart for " + this.parent.Label );
                return;
            }

            (ThingDef, LimbConfiguration) target = this.MissingParts.FirstOrDefault( p => p.Item1 == part.def );

            this.IncludedParts.Add( part );
            this.DirtyCache();

            CompIncludedChildParts partComp = part.TryGetComp<CompIncludedChildParts>();
            if ( partComp != null )
            {
                partComp.TargetLimb = target.Item2;
                partComp.holderComp = this;
            }

            if ( part.Spawned )
            {
                part.DeSpawn();
            }
        }

        public void RemoveAndSpawnPart ( Thing part, IntVec3 position, Map map )
        {
            if ( !this.IncludedParts.Contains( part ) )
            {
                Log.Error( "Tried to remove " + part.Label + " from " + this.parent.Label + " while it wasn't actually included." );
                return;
            }

            this.IncludedParts.Remove( part );
            this.DirtyCache();

            CompIncludedChildParts partComp = part.TryGetComp<CompIncludedChildParts>();
            if ( partComp != null )
            {
                partComp.holderComp = null;
            }

            GenPlace.TryPlaceThing( part, position, map, ThingPlaceMode.Near );
        }

        public void RemoveAndSpawnPart ( Thing part )
        {
            this.RemoveAndSpawnPart( part, this.Position, this.Map );
        }

        // Target Limb

        private LimbConfiguration targetLimb;

        public LimbConfiguration TargetLimb
        {
            get
            {
                return targetLimb;
            }
            set
            {
                if ( value != null && !this.Props.EverInstallableOn( value ) )
                {
                    Log.Error( string.Format( "Tried to set invalid target limb ({0}) on {1}", value.Label, this.parent.Label ) );
                }

                this.targetLimb = value;
                this.UpdateTargetLimbOrRemoveIncludedParts();

                this.DirtyCache();
            }
        }

        private void UpdateTargetLimbOrRemoveIncludedParts ()
        {
            if ( this.TargetLimb == null )
            {
                foreach ( var comp in this.IncludedPartComps )
                {
                    comp.TargetLimb = null;
                }
            }
            else
            {
                tmpThingList.Clear();
                tmpThingList.AddRange( this.IncludedParts );

                // update compatible parts
                foreach ( (ThingDef thingDef, LimbConfiguration limb) in this.Props.StandardPartsForLimb( this.TargetLimb ) )
                {
                    Thing candidate = tmpThingList.FirstOrDefault( t => t.def == thingDef );
                    if ( candidate != null )
                    {
                        tmpThingList.Remove( candidate );

                        CompIncludedChildParts potentialComp = candidate.TryGetComp<CompIncludedChildParts>();

                        if ( potentialComp != null )
                        {
                            potentialComp.TargetLimb = limb;
                        }
                        else
                        {
                            // this will either never happen, or can be checked in standardpartsforlimb
                            if ( !limb.ChildLimbs.EnumerableNullOrEmpty() )
                            {
                                Log.Error( string.Format( "[MSE2] Included thing {0} has no CompIncludedChildParts, but was assigned {1}, which has {2} childlimbs.", candidate.def.defName, limb.Label, limb.ChildLimbs.Count() ) );
                            }
                        }
                    }
                }

                // remove the others
                foreach ( var thing in tmpThingList )
                {
                    this.RemoveAndSpawnPart( thing );
                }
            }
        }

        // Standard parts

        public List<ThingDef> StandardParts
        {
            get => this.Props?.standardChildren;
        }

        // Missing Parts

        private List<(ThingDef, LimbConfiguration)> cachedMissingParts;

        public IEnumerable<(ThingDef, LimbConfiguration)> MissingParts
        {
            get
            {
                if ( this.TargetLimb == null )
                {
                    return Enumerable.Empty<(ThingDef, LimbConfiguration)>();
                }
                else
                {
                    if ( cachedMissingParts == null )
                    {
                        cachedMissingParts = new List<(ThingDef, LimbConfiguration)>( this.Props.StandardPartsForLimb( this.TargetLimb ) );

                        foreach ( var thing in this.IncludedParts )
                        {
                            var thingComp = thing.TryGetComp<CompIncludedChildParts>();

                            cachedMissingParts.Remove( cachedMissingParts.FirstOrDefault( c =>
                                 thing.def == c.Item1
                                 && (thingComp == null || thingComp.TargetLimb == c.Item2) ) );
                        }
                    }

                    return cachedMissingParts;
                }
            }
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
            this.TargetLimb = limb;

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

        //public override string TransformLabel ( string label )
        //{
        //    if ( this.IncludedParts != null )
        //    {
        //        if ( this.cachedTransformLabelString == null )
        //        {
        //            this.cachedTransformLabelString = " (";

        //            if ( this.CompatibleLimbs.Any() )
        //            {
        //                this.cachedTransformLabelString += String.Join( "; ", this.CompatibleLimbs.Select( lc => lc.Label ) );
        //            }
        //            else
        //            {
        //                this.cachedTransformLabelString += "incomplete";
        //            }

        //            this.cachedTransformLabelString += ")";
        //        }

        //        return label + this.cachedTransformLabelString;
        //    }
        //    return null;
        //}

        // Inspect string

        protected String cachedInspectString = null;

        public override string CompInspectStringExtra ()
        {
            if ( this.IncludedParts != null )
            {
                if ( this.cachedInspectString == null )
                {
                    this.cachedInspectString = "CompIncludedChildParts_InspectString".Translate( this.IncludedParts.Count );

                    if ( this.TargetLimb != null )
                    {
                        this.cachedInspectString += "\nTarget: " + this.targetLimb.Label;
                        if ( this.AllMissingParts.Any() )
                        {
                            this.cachedInspectString += " (" + this.AllMissingParts.Count() + " missing parts)"; // maybe optimize
                        }
                    }
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

        public IEnumerable<(ThingDef, LimbConfiguration, CompIncludedChildParts)> AllMissingParts
        {
            get => Enumerable.Concat(

                // the standard sub-parts included in this part
                this.MissingParts.Select( p => (p.Item1, p.Item2, this) ),

                // the standard sub-parts of the children with CompIncludedChildParts
                from i in this.IncludedParts
                let comp = i.TryGetComp<CompIncludedChildParts>()
                where comp != null
                from triplet in comp.AllMissingParts
                select triplet );
        }

        #endregion RecursiveData
    }
}