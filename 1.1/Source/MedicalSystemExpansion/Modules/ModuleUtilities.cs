﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using OrenoMSE.Modules.Slots;
using OrenoMSE.Modules.Actual;

namespace OrenoMSE.Modules
{
    public static class ModuleUtilities
    {
        public static Hediff_ModuleAbstract GetModuleToReplace ( Pawn pawn, BodyPartRecord part )
        {
            // the module abstracts in the bodypart
            var set = from h in pawn.health.hediffSet.GetHediffs<Hediff_ModuleAbstract>()
                      where h.Part == part
                      select h;


            // try to extract a slot or just get a module

            return (from h in set
                        where h is Hediff_ModuleSlot
                        select (Hediff_ModuleSlot)h) // from the slots in the set
                        .FirstOrFallback( // get the first or else get the first of the set
                            set.FirstOrDefault());
        }

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
                GenSpawn.Spawn( ThingMaker.MakeThing( module.def.spawnThingOnRemoved ), module.pawn.Position, module.pawn.Map );
            }
            // remove hediff
            module.pawn.health.RemoveHediff( module );
        }

        public static void InstallModule ( Pawn pawn, HediffDef def, BodyPartRecord part )
        {
            Hediff_ModuleAbstract m = GetModuleToReplace( pawn, part );


            // remove old hediff
            if ( m is Hediff_ModuleAdded ma )
                RemoveAndSpawnModule( ma );


            pawn.health.AddHediff( def, part, null, null );
        }
    }
}