using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MSE2
{
    [StaticConstructorOnStartup]
    internal static class Assets
    {
        public static readonly Texture2D WidgetMinusSign = ContentFinder<Texture2D>.Get( "UI/Widgets/MinusSign", true );

        public static readonly Texture2D WidgetPlusSign = ContentFinder<Texture2D>.Get( "UI/Widgets/PlusSign", true );
    }
}