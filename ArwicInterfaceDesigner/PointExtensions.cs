using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArwicInterfaceDesigner
{
    public static class PointExtensions
    {
        public static Microsoft.Xna.Framework.Point ToXNAPoint(this Point winPoint)
        {
            return new Microsoft.Xna.Framework.Point(winPoint.X, winPoint.Y);
        }
    }

}
