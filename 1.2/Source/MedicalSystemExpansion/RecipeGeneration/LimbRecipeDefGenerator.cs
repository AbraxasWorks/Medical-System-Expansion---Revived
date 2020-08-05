using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using Verse.Noise;

namespace MSE2
{
    public static class LimbRecipeDefGenerator
    {
        public static IEnumerable<RecipeDef> ImpliedLimbRecipeDefs ()
        {
            return from ThingDef thingDef in DefDatabase<ThingDef>.AllDefs
                   where thingDef.GetCompProperties<CompProperties_IncludedChildParts>() != null
                   where thingDef.costList != null
                   from RecipeDef recipe in LimbCreationDef( thingDef )
                   select recipe;
        }

        public static RecipeDef CraftingRecipeForThing ( ThingDef thingDef )
        {
            string targetName = "Make_" + thingDef.defName;
            return DefDatabase<RecipeDef>.AllDefs.FirstOrDefault( r => r.defName == targetName );
        }

        public static (List<IngredientCount>, float) AllIngredientsWorkForLimb ( ThingDef thingDef, LimbConfiguration limb )
        {
            //Log.Message( "Calculating ingredients of " + thingDef.defName );

            if ( thingDef.costList == null )
            {
                Log.Error( "[MSE2] Tried to create list of ingredients for " + thingDef.label + ", which can not be crafted" );
                return (null, 0);
            }

            float work = 0f;

            // all the actual segments the part will include
            List<ThingDefCountClass> allParts = new List<ThingDefCountClass>();
            allParts.Add( new ThingDefCountClass( thingDef, 1 ) );
            var comp = thingDef.GetCompProperties<CompProperties_IncludedChildParts>();
            if ( comp != null )
            {
                // add the rest grouped by thingdef
                allParts.AddRange( comp.AllPartsForLimb( limb ).GroupBy( t => t ).Select( g => new ThingDefCountClass( g.Key, g.Count() ) ) );
            }

            // the ingredients to make the segments, not grouped by thingdef
            List<ThingDefCountClass> intermediateIngredients = new List<ThingDefCountClass>();
            foreach ( var partCount in allParts )
            {
                work += partCount.thingDef.recipeMaker.workAmount * partCount.count;

                if ( partCount.thingDef.costList != null )
                {
                    foreach ( var intIngredientCount in partCount.thingDef.costList )
                    {
                        // add it's ingredients, adjusted by how many are crafted and rounded up
                        intermediateIngredients.Add( new ThingDefCountClass( intIngredientCount.thingDef, (intIngredientCount.count * partCount.count - 1) / partCount.thingDef.recipeMaker.productCount + 1 ) );
                    }
                }
                else
                {
                    // piece is uncraftable, add it to the list
                    intermediateIngredients.Add( partCount );
                }
            }

            List<IngredientCount> ingredients = new List<IngredientCount>();
            foreach ( ThingDefCountClass thingDefCountClass in from i in intermediateIngredients
                                                               group i by i.thingDef into g
                                                               select new ThingDefCountClass( g.Key, g.Select( tdcc => tdcc.count ).Aggregate( 0, ( a, b ) => a + b ) ) )
            {
                IngredientCount ingredientCount = new IngredientCount();
                ingredientCount.SetBaseCount( (float)(thingDefCountClass.count) );
                ingredientCount.filter.SetAllow( thingDefCountClass.thingDef, true );
                ingredients.Add( ingredientCount );
            }

            return (ingredients, work);
        }

        public static IEnumerable<RecipeDef> LimbCreationDef ( ThingDef def )
        {
            //Log.Message( "creating limb recipes for " + def.defName );

            int tot = 0;

            def.GetCompProperties<CompProperties_IncludedChildParts>().ResolveReferences( def );

            foreach ( LimbConfiguration limb in IncludedPartsUtilities.CachedInstallationDestinations( def ) )
            {
                //Log.Message( "At " + part.defName );

                RecipeMakerProperties recipeMaker = def.recipeMaker;
                RecipeDef recipeDef = new RecipeDef();

                recipeDef.defName = "Make_" + def.defName + "_" + limb.UniqueName;
                recipeDef.label = "Make " + def.label + " for " + limb.Label;
                recipeDef.jobString = "RecipeMakeJobString".Translate( def.label );
                recipeDef.modContentPack = def.modContentPack;
                recipeDef.workSpeedStat = recipeMaker.workSpeedStat;
                recipeDef.efficiencyStat = recipeMaker.efficiencyStat;
                recipeDef.defaultIngredientFilter = recipeMaker.defaultIngredientFilter;
                recipeDef.products.Add( new ThingDefCountClass( def, recipeMaker.productCount ) );
                recipeDef.targetCountAdjustment = recipeMaker.targetCountAdjustment;
                recipeDef.skillRequirements = recipeMaker.skillRequirements.ListFullCopyOrNull<SkillRequirement>();
                recipeDef.workSkill = recipeMaker.workSkill;
                recipeDef.workSkillLearnFactor = recipeMaker.workSkillLearnPerTick;
                recipeDef.requiredGiverWorkType = recipeMaker.requiredGiverWorkType;
                recipeDef.unfinishedThingDef = recipeMaker.unfinishedThingDef;
                recipeDef.recipeUsers = recipeMaker.recipeUsers.ListFullCopyOrNull<ThingDef>();
                recipeDef.effectWorking = recipeMaker.effectWorking;
                recipeDef.soundWorking = recipeMaker.soundWorking;
                recipeDef.researchPrerequisite = recipeMaker.researchPrerequisite;
                recipeDef.researchPrerequisites = recipeMaker.researchPrerequisites;
                recipeDef.factionPrerequisiteTags = recipeMaker.factionPrerequisiteTags;
                string[] items = recipeDef.products.Select( delegate ( ThingDefCountClass p )
                {
                    if ( p.count != 1 )
                    {
                        return p.Label;
                    }
                    return Find.ActiveLanguageWorker.WithIndefiniteArticle( p.thingDef.label, false, false );
                } ).ToArray<string>();
                recipeDef.description = "RecipeMakeDescription".Translate( items.ToCommaList( true ) );
                recipeDef.descriptionHyperlinks = (from p in recipeDef.products
                                                   select new DefHyperlink( p.thingDef )).ToList<DefHyperlink>();

                (recipeDef.ingredients, recipeDef.workAmount) = AllIngredientsWorkForLimb( def, limb );

                if ( recipeDef.modExtensions == null ) recipeDef.modExtensions = new List<DefModExtension>();
                recipeDef.modExtensions.Add( new LimbProsthesisCreation() { targetLimb = limb } );

                yield return recipeDef;

                tot++;
            }

            //Log.Message( "Created " + tot + " recipes for " + def.defName );
        }
    }
}