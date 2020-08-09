using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime;

using UnityEngine;
using Verse;
using HugsLib;
using RimWorld;
using System.Threading;
using UnityEngineInternal;
using System;

namespace MSE2
{
    public class MedicalSystemExpansion : ModBase
    {
        public override void DefsLoaded ()
        {
            base.DefsLoaded();

            IncludedPartsUtilities.CacheAllStandardParents();

            IgnoreSubPartsUtilities.IgnoreAllNonCompedSubparts();

            // add the recipes to craft the prostheses with the various configurations of parts
            foreach ( RecipeDef def in LimbRecipeDefGenerator.ImpliedLimbRecipeDefs() )
            {
                def.ResolveReferences();
                DefGenerator.AddImpliedDef<RecipeDef>( def );
                HugsLib.Utils.InjectedDefHasher.GiveShortHashToDef( def, typeof( RecipeDef ) );
            }

            // duplicate ambiguous installation surgeries
            List<LimbConfiguration> tmplimbsItCanTargetList = new List<LimbConfiguration>();

            foreach ( (ThingDef thingDef, CompProperties_IncludedChildParts comp) in
                DefDatabase<ThingDef>.AllDefs
                .Select( t => (t, t.GetCompProperties<CompProperties_IncludedChildParts>()) )
                .Where( c => c.Item2 != null ) )
            {
                foreach ( RecipeDef surgery in IncludedPartsUtilities.SurgeryToInstall( thingDef ).ToArray() )
                {
                    tmplimbsItCanTargetList.Clear();
                    tmplimbsItCanTargetList.AddRange( comp.installationDestinations.Where( l => surgery.appliedOnFixedBodyParts.Contains( l.PartDef ) && surgery.AllRecipeUsers.Any( ru => l.Bodies.Contains( ru.race.body ) ) ) );

                    Log.Message( surgery.label + " can target(" + tmplimbsItCanTargetList.Count + "): " + string.Join( ", ", tmplimbsItCanTargetList.Select( l => l.UniqueName ) ) );

                    int count = 0;
                    foreach ( var limb in tmplimbsItCanTargetList )
                    {
                        if ( count == 0 )
                        {
                            if ( surgery.modExtensions == null ) surgery.modExtensions = new List<DefModExtension>();

                            surgery.modExtensions.Add( new RestrictTargetLimb() { targetLimb = limb } );
                        }
                        else
                        {
                            RecipeDef surgeryClone = (RecipeDef)typeof( RecipeDef ).GetMethod( "MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance ).Invoke( surgery, new object[0] );

                            surgeryClone.defName = string.Copy( surgery.defName ) + count;

                            surgeryClone.label = string.Copy( surgery.defName ) + " " + count;

                            surgeryClone.modExtensions.Remove( surgery.GetModExtension<RestrictTargetLimb>() );
                            surgeryClone.modExtensions.Add( new RestrictTargetLimb() { targetLimb = limb } );

                            typeof( RecipeDef ).GetField( "workerInt", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( surgeryClone, null );
                            typeof( RecipeDef ).GetField( "workerCounterInt", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( surgeryClone, null );
                            typeof( RecipeDef ).GetField( "ingredientValueGetterInt", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( surgeryClone, null );

                            surgeryClone.shortHash = 0;

                            surgeryClone.ResolveReferences();
                            DefGenerator.AddImpliedDef( surgeryClone );
                            HugsLib.Utils.InjectedDefHasher.GiveShortHashToDef( surgeryClone, typeof( RecipeDef ) );
                        }
                        count++;
                    }

                    if ( count > 1 )
                    {
                        surgery.label += " 0";
                    }
                }
            }
        }

        public override string ModIdentifier => "MSE2";
    }
}