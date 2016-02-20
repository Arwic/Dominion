// Dominion - Copyright (C) Timothy Ings
// ColorExtensions.cs
// This file defines classes that extend color related classes

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArwicEngine.Core
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns the RichText format of an xna color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToRichFormat(this Color color)
        {
            return $"{color.R},{color.G},{color.B}";
        }
    }
}
