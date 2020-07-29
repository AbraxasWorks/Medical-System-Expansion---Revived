using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MSE2
{
    internal class LimbDef
    {
        private HashSet<BodyPartRecord> allRecords = new HashSet<BodyPartRecord>();

        public BodyPartDef PartDef => allRecords.FirstOrDefault()?.def;

        public IEnumerable<BodyDef> Bodies => allRecords.Select( r => r.body ).Distinct();
    }
}