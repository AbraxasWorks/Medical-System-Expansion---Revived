using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection.Emit;

namespace OrenoMSE.PawnGeneration
{
    [HarmonyPatch(typeof( PawnTechHediffsGenerator ) )]
    [HarmonyPatch( "InstallPart" )]
    public static class InstallPart_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler ( IEnumerable<CodeInstruction> instructions )//, List<Thing> ___emptyIngredientsList )
        {
            foreach ( var instr in instructions )
            {
                if ( instr.opcode == OpCodes.Ldsfld && instr.OperandIs( typeof(PawnTechHediffsGenerator).GetField("emptyIngredientsList", BindingFlags.NonPublic | BindingFlags.Static ) ) )
                {
                    yield return new CodeInstruction( OpCodes.Ldarg_1 );
                    yield return new CodeInstruction( OpCodes.Call, typeof( InstallPart_Patch ).GetMethod(nameof(SingletonFromDef)) );
                }
                else
                    yield return instr;
            }
            yield break;
        }


        public static List<Thing> SingletonFromDef ( ThingDef partDef )
        {
            //Log.Message( "Generated pawn with " + partDef.defName );

            var compProp = partDef.GetCompProperties<CompProperties_CompIncludedChildParts>();
            if ( compProp != null )
            {
                return new List<Thing> { ThingMaker.MakeThing( partDef ) };
            }

            return new List<Thing>();
        }


        //    public static void Postfix ( Pawn pawn, ThingDef partDef )
        //{
        //    var compProp = partDef.GetCompProperties<CompProperties_CompIncludedChildParts>();
        //    if ( compProp != null )
        //    {


        //        foreach ( var childpart in compProp.standardChildren )
        //        {
        //            if ( Rand.Chance(0.9f) )
        //            {

        //            }
        //        }
        //    }
        //}

        //public static void RecursiveInstall ( Pawn pawn, ThingDef partDef, ThingDef childPart )
        //{
        //    IEnumerable<RecipeDef> source = from x in DefDatabase<RecipeDef>.AllDefs
        //                                    where x.IsIngredient( partDef ) && pawn.def.AllRecipes.Contains( x )
        //                                    select x;
        //    if ( source.Any<RecipeDef>() )
        //    {
        //        RecipeDef recipeDef = source.RandomElement<RecipeDef>();
        //        if ( recipeDef.Worker.GetPartsToApplyOn( pawn, recipeDef ).Any<BodyPartRecord>() )
        //        {
        //            recipeDef.Worker.ApplyOnPawn( pawn, recipeDef.Worker.GetPartsToApplyOn( pawn, recipeDef ).RandomElement<BodyPartRecord>(), null, PawnTechHediffsGenerator.emptyIngredientsList, null );
        //        }
        //    }
        //}

    }
}
