using System.Linq;
using Verse;

namespace MSE2
{
    public static class ModuleUtilities
    {
        //public static Hediff_ModuleAbstract GetModuleToReplace ( Pawn pawn, BodyPartRecord part )
        //{
        //    // the module abstracts in the bodypart
        //    var set = from h in pawn.health.hediffSet.GetHediffs<Hediff_ModuleAbstract>()
        //              where h.Part == part
        //              select h;

        //    // try to extract a slot or just get a module

        //    return (from h in set
        //            where h is Hediff_ModuleSlot
        //            select (Hediff_ModuleSlot)h) // from the slots in the set
        //                .FirstOrFallback( // get the first or else get the first of the set
        //                    set.FirstOrDefault() );
        //}

        public static void RemoveAndSpawnModule ( Hediff_ModuleAdded module )
        {
            //// only reduce the available slot count on a slot hediff with more than a slot
            //if ( module is Hediff_ModuleSlot slot && slot.AvailableSlots > 0 )
            //{
            //    return;
            //}

            // spawn thing if possible
            if ( module.def.spawnThingOnRemoved != null )
            {
                GenPlace.TryPlaceThing( ThingMaker.MakeThing( module.def.spawnThingOnRemoved ), module.pawn.Position, module.pawn.Map, ThingPlaceMode.Near );
            }
            // remove hediff
            module.pawn.health.RemoveHediff( module );
        }

        public static void InstallModule ( Pawn pawn, HediffDef def, BodyPartRecord part )
        {
            pawn.health.AddHediff( def, part, null, null );
        }
    }
}