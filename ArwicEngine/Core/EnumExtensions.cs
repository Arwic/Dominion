// Dominion - Copyright (C) Timothy Ings
// EnumExtensions.cs
// This file defines extension methods for enums

using System;

namespace ArwicEngine.Core
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the name of the given enum
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetName(this Enum e)
        {
            return Enum.GetName(e.GetType(), e);
        }
    }
}