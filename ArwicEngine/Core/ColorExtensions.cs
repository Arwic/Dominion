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
        public static string ToRichFormat(this Color color)
        {
            return $"{color.R},{color.G},{color.B}";
        }
    }
}
