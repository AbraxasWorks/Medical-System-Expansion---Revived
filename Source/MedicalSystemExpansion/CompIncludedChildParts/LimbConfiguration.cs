using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MSE2
{
    public class LimbConfiguration
    {
        protected HashSet<BodyPartRecord> allRecords = new HashSet<BodyPartRecord>();

        protected LimbConfiguration ()
        {
            allLimbDefs.Add( this );
        }

        protected LimbConfiguration ( BodyPartRecord bodyPartRecord ) : this()
        {
            this.TryAddRecord( bodyPartRecord );

            foreach ( var item in
                from body in DefDatabase<BodyDef>.AllDefs
                from bpr in body.AllParts
                where bpr.def == bodyPartRecord.def
                where !recordToLimb.ContainsKey( bpr )
                select bpr )
            {
                this.TryAddRecord( item );
            }

            id = this.CountSimilar();

            //Log.Message( "limb config created with " + this.allRecords.Count + " compatible records: " + string.Join( ", ", this.allRecords.Select( bpr => bpr.Label + "(" + bpr.body.defName + ")" ) ) );
        }

        protected bool HasCompatibleStructure ( BodyPartRecord bodyPartRecord )
        {
            return allRecords.EnumerableNullOrEmpty() ||
            bodyPartRecord.HasSameStructure( allRecords.First() );
        }

        protected void TryAddRecord ( BodyPartRecord recordToAdd )
        {
            if ( this.HasCompatibleStructure( recordToAdd ) )
            {
                if ( this.allRecords.Add( recordToAdd ) )
                {
                    recordToLimb.Add( recordToAdd, this );
                }
            }
        }

        public BodyPartDef PartDef => this.allRecords.FirstOrDefault()?.def;

        public IEnumerable<BodyDef> Bodies => this.allRecords.Select( r => r.body ).Distinct();

        public bool Contains ( BodyPartRecord bodyPartRecord )
        {
            return allRecords.Contains( bodyPartRecord );
        }

        private int id = -1;

        public int CountSimilar ()
        {
            return allLimbDefs.Where( l => l.PartDef == this.PartDef ).Count() - 1;
        }

        //TODO maybe cache the string joins on these 2
        public string UniqueName
        {
            get
            {
                return string.Join( "", this.Bodies ) + "_" + this.PartDef.defName + "_" + id;
            }
        }

        public string Label
        {
            get
            {
                return string.Join( ", ", this.Bodies );
            }
        }

        public BodyPartRecord RecordExample
        {
            get => this.allRecords.FirstOrDefault();
        }

        public IEnumerable<BodyPartDef> AllSegments
        {
            get
            {
                BodyPartRecord example = this.allRecords.First();
                return example.AllChildParts().Prepend( example ).Select( r => r.def );
            }
        }

        public IEnumerable<LimbConfiguration> ChildLimbs
        {
            get
            {
                if ( this.allRecords.EnumerableNullOrEmpty() )
                {
                    Log.Error( "Tried to get Child limbs of incomplete limb configuration" );
                    return Enumerable.Empty<LimbConfiguration>();
                }
                else
                {
                    return from bpr in this.RecordExample.parts
                           select GenerateOrGetLimbConfigForBodyPartRecord( bpr );
                }
            }
        }

        protected static Dictionary<BodyPartRecord, LimbConfiguration> recordToLimb = new Dictionary<BodyPartRecord, LimbConfiguration>();
        protected static List<LimbConfiguration> allLimbDefs = new List<LimbConfiguration>();

        public static LimbConfiguration GenerateOrGetLimbConfigForBodyPartRecord ( BodyPartRecord bodyPartRecord )
        {
            if ( recordToLimb.TryGetValue( bodyPartRecord, out LimbConfiguration outVal ) )
            {
                return outVal;
            }
            else
            {
                return new LimbConfiguration( bodyPartRecord );
            }
        }

        public static IEnumerable<LimbConfiguration> LimbConfigsMatchingBodyAndPart ( BodyDef body, BodyPartDef partDef )
        {
            return from bpr in body.AllParts
                   where bpr.def == partDef
                   let lc = GenerateOrGetLimbConfigForBodyPartRecord( bpr )
                   group lc by lc into g
                   select g.Key;
        }

        static LimbConfiguration ()
        {
            if ( DefDatabase<BodyDef>.AllDefs.EnumerableNullOrEmpty() )
            {
                Log.Error( "[MSE2] Tried to use limbs before the BodyDef database was loaded." );
                return;
            }
        }
    }
}