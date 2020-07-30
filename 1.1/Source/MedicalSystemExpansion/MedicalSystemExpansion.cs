using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;
using Verse;
using HugsLib;
using RimWorld;

namespace MSE2
{
    //[EarlyInit]
    public class MedicalSystemExpansion : ModBase
    {
        public override void DefsLoaded ()
        {
            base.DefsLoaded();

            IncludedPartsUtilities.CacheAllStandardParents();

            IgnoreSubPartsUtilities.IgnoreAllNonCompedSubparts();

            IncludedPartsUtilities.FixHediffPriceOffset();

            foreach ( RecipeDef def in LimbRecipeDefGenerator.ImpliedLimbRecipeDefs() )
            {
                def.ResolveReferences();
                DefGenerator.AddImpliedDef<RecipeDef>( def );
                HugsLib.Utils.InjectedDefHasher.GiveShortHashToDef( def, typeof( RecipeDef ) );
            }
        }

        public override string ModIdentifier => "MSE2";
    }
}