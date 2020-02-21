using System.Collections.Generic;
using System.Reflection;
using Harmony;
using UnityEngine;
using RimWorld;
using Verse;

namespace OrenoMSE
{
    [StaticConstructorOnStartup]
    public class MedicalSystemExpansion
    {
        static MedicalSystemExpansion()
        {
            var harmony = HarmonyInstance.Create("OrenoMSE");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        
        public static readonly Texture2D IconPartSystem = ContentFinder<Texture2D>.Get("UI/Icons/Medical/IconPartSystem", true);

        public static readonly Texture2D IconPartSystemDamaged = ContentFinder<Texture2D>.Get("UI/Icons/Medical/IconPartSystemDamaged", true);
    }

    public static class MSE_VanillaExtender
    {
        public static bool PartHasInjury(Pawn pawn, BodyPartRecord bodyPart, bool mustBeVisible = false)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_Injury hediff_Injury && hediffs[i].Part == bodyPart && (!mustBeVisible || hediffs[i].Visible))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PartHasAdvancedImplantSystem(Pawn pawn, BodyPartRecord record)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].def.HasModExtension<MSE_ImplantSystem>() && hediffs[i].Part == record)
                {
                    MSE_ImplantSystem implantSystem = hediffs[i].def.GetModExtension<MSE_ImplantSystem>();
                    if (implantSystem.isSpecial)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void RecipeApplyHediffs(Recipe_Surgery surgery, Pawn pawn, BodyPartRecord record)
        {
            if (surgery.recipe.HasModExtension<MSE_AdditionalHediff>())
            {
                MSE_AdditionalHediff additionalHediffs = surgery.recipe.GetModExtension<MSE_AdditionalHediff>();
                if (additionalHediffs != null && !additionalHediffs.hediffsToAdd.NullOrEmpty())
                {
                    List<HediffDef> hediffsToAdd = additionalHediffs.hediffsToAdd;
                    for (int i = 0; i < hediffsToAdd.Count; i++)
                    {
                        pawn.health.AddHediff(hediffsToAdd[i], record, null, null);
                    }
                }
            }
            return;
        }

        public static void HediffApplyHediffs(Hediff hediff, Pawn pawn, BodyPartRecord record)
        {
            if (hediff.def.HasModExtension<MSE_AdditionalHediff>())
            {
                MSE_AdditionalHediff additionalHediffs = hediff.def.GetModExtension<MSE_AdditionalHediff>();
                if (additionalHediffs != null && !additionalHediffs.hediffsToAdd.NullOrEmpty())
                {
                    List<HediffDef> hediffsToAdd = additionalHediffs.hediffsToAdd;
                    for (int i = 0; i < hediffsToAdd.Count; i++)
                    {
                        pawn.health.AddHediff(hediffsToAdd[i], record, null, null);
                    }
                }
            }
            return;
        }

        public static string PrettyLabel(Hediff hediff)
        {
            if (hediff.def.HasModExtension<MSE_HediffPrettyLabel>())
            {
                MSE_HediffPrettyLabel hediffPrettyLabel = hediff.def.GetModExtension<MSE_HediffPrettyLabel>();
                if (hediffPrettyLabel != null && !hediffPrettyLabel.labelPretty.NullOrEmpty())
                {
                    string genderNoun = "unknown";
                    string genderlessNoun = hediffPrettyLabel.genderlessNoun;
                    string maleNoun = hediffPrettyLabel.maleNoun;
                    string femaleNoun = hediffPrettyLabel.femaleNoun;
                    if (genderlessNoun != null && hediff.pawn.gender == Gender.None)
                    {
                        genderNoun = genderlessNoun;
                    }
                    else if (maleNoun != null && hediff.pawn.gender == Gender.Male)
                    {
                        genderNoun = maleNoun;
                    }
                    else if (femaleNoun != null && hediff.pawn.gender == Gender.Female)
                    {
                        genderNoun = femaleNoun;
                    }
                    return string.Format(hediffPrettyLabel.labelPretty, hediff.Part.Label, genderNoun);
                }
            }
            return hediff.def.label;
        }

        public static Texture2D GetIcon(string loadID, string iconPath)
        {
            if (iconsCache.ContainsKey(loadID))
            {
                return iconsCache[loadID];
            }
            else
            {
                Texture2D icon = BaseContent.BadTex;
                if (!iconPath.NullOrEmpty())
                {
                    icon = ContentFinder<Texture2D>.Get(iconPath, true);
                    iconsCache.Add(loadID, icon);
                }
                return icon;
            }
        }

        public static void ClearIcon(string loadID)
        {
            iconsCache.Remove(loadID);
        }

        public static readonly Dictionary<string, Texture2D> iconsCache = new Dictionary<string, Texture2D>();
    }
}
