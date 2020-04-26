using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace MSE2
{
    [StaticConstructorOnStartup]
    public static class MedicalSystemExpansion
    {
        static MedicalSystemExpansion ()
        {
            Harmony harmony = new Harmony( "MSE2" );

            //if ( ModLister.HasActiveModWithName( "EdB Prepare Carefully" ) )
            //    PrepareCarefullyPatches.ApplyPC( harmony );

            harmony.PatchAll( Assembly.GetExecutingAssembly() );

            IncludedPartsUtilities.CacheAllStandardParents();

            IgnoreSubPartsUtilities.IgnoreAllNonCompedSubparts();
            
        }

        public static readonly Texture2D WidgetMinusSign = ContentFinder<Texture2D>.Get( "UI/Widgets/MinusSign", true );

        public static readonly Texture2D WidgetPlusSign = ContentFinder<Texture2D>.Get( "UI/Widgets/PlusSign", true );

        public static readonly Texture2D IconPartSystem = ContentFinder<Texture2D>.Get( "UI/Icons/Medical/IconPartSystem", true );

        public static readonly Texture2D IconPartSystemDamaged = ContentFinder<Texture2D>.Get( "UI/Icons/Medical/IconPartSystemDamaged", true );

        public static bool PartHasInjury ( Pawn pawn, BodyPartRecord bodyPart, bool mustBeVisible = false )
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for ( int i = 0; i < hediffs.Count; i++ )
            {
                if ( hediffs[i] is Hediff_Injury && hediffs[i].Part == bodyPart && (!mustBeVisible || hediffs[i].Visible) )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the Hediff has a DefModExtension of MSE_HediffPrettyLabel create the label using it.
        /// Puts like "artificial" and then the body part label.
        /// </summary>
        /// <param name="hediff"></param>
        /// <returns>A dynamically created label</returns>
        public static string PrettyLabel ( Hediff hediff )
        {
            if ( hediff.def.HasModExtension<MSE_HediffPrettyLabel>() )
            {
                MSE_HediffPrettyLabel hediffPrettyLabel = hediff.def.GetModExtension<MSE_HediffPrettyLabel>();
                if ( hediffPrettyLabel != null && !hediffPrettyLabel.labelPretty.NullOrEmpty() )
                {
                    string genderNoun = "unknown";
                    string genderlessNoun = hediffPrettyLabel.genderlessNoun;
                    string maleNoun = hediffPrettyLabel.maleNoun;
                    string femaleNoun = hediffPrettyLabel.femaleNoun;
                    if ( genderlessNoun != null && hediff.pawn.gender == Gender.None )
                    {
                        genderNoun = genderlessNoun;
                    }
                    else if ( maleNoun != null && hediff.pawn.gender == Gender.Male )
                    {
                        genderNoun = maleNoun;
                    }
                    else if ( femaleNoun != null && hediff.pawn.gender == Gender.Female )
                    {
                        genderNoun = femaleNoun;
                    }
                    return string.Format( hediffPrettyLabel.labelPretty, hediff.Part.Label, genderNoun );
                }
            }
            return hediff.def.label;
        }

        public static Texture2D GetIcon ( string loadID, string iconPath )
        {
            if ( iconsCache.ContainsKey( loadID ) )
            {
                return iconsCache[loadID];
            }
            else
            {
                Texture2D icon = BaseContent.BadTex;
                if ( !iconPath.NullOrEmpty() )
                {
                    icon = ContentFinder<Texture2D>.Get( iconPath, true );
                    iconsCache.Add( loadID, icon );
                }
                return icon;
            }
        }

        public static void ClearIcon ( string loadID )
        {
            iconsCache.Remove( loadID );
        }

        public static readonly Dictionary<string, Texture2D> iconsCache = new Dictionary<string, Texture2D>();
    }
}