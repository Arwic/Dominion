// Dominion - Copyright (C) Timothy Ings
// GraphicsHelper.cs
// This file contains graphics related helper methods

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ArwicEngine.Graphics
{
    public enum HexagonOrientation
    {
        Pointy,
        Flat
    }

    public static class GraphicsHelper
    {
        private static Texture2D pixel;
        private static Viewport viewport;

        public static Texture2D PixelTexture { get { return pixel; } }

        public static void Init(GraphicsDevice gd)
        {
            viewport = gd.Viewport;
            pixel = new Texture2D(gd, 1, 1);
            Color[] pixels = { Color.White };
            pixel.SetData(pixels);
        }

        //private static bool InBounds(Camera2 camera, Vector2 vector)
        //{
        //    return true;
        //    Point camOffset = Point.Zero;
        //    Vector2 camScale = Vector2.One;
        //    if (camera != null)
        //    {
        //        camOffset = camera.Transform.Location.ToPoint();
        //        camScale = camera.Transform.Scale;
        //    }
        //    Rectangle bounds = new Rectangle(camOffset.X, camOffset.Y, (int)(viewport.Bounds.Width / camScale.X), (int)(viewport.Bounds.Height / camScale.Y));
        //    if (bounds.Contains(vector))
        //        return true;
        //    return false;
        //}

        //private static bool InBounds(Camera2 camera, Rectangle rect)
        //{
        //    return InBounds(camera, new Vector2(rect.X, rect.Y)) &&
        //        InBounds(camera, new Vector2(rect.X, rect.Y + rect.Height)) &&
        //        InBounds(camera, new Vector2(rect.X + rect.Width, rect.Y + rect.Height)) &&
        //        InBounds(camera, new Vector2(rect.X + rect.Width, rect.Y));
        //}

        /// <summary>
        /// Calculates the number of radians between two vector2s
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float Rotation(Vector2 p1, Vector2 p2)
        {
            float adj = p1.X - p2.X;
            float opp = p1.Y - p2.Y;
            float tan = opp / adj;
            float res = MathHelper.ToDegrees((float)Math.Atan2(opp, adj));
            res = (res - 180) % 360;
            if (res < 0) { res += 360; }
            res = MathHelper.ToRadians(res);
            return res;
        }

        /// <summary>
        /// Draws a line begging at p1 and ending at p2 with a given thickness
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="thickness"></param>
        /// <param name="c"></param>
        public static void DrawLine(SpriteBatch sb, Vector2 p1, Vector2 p2, int thickness, Color c)
        {
            float length = Vector2.Distance(p1, p2);
            length += 1;
            float rotation = Rotation(p1, p2);
            sb.Draw(pixel, new Rectangle((int)p1.X - thickness / 2, (int)p1.Y - thickness / 2, (int)length, thickness), new Rectangle(0, 0, 1, 1), c, rotation, Vector2.Zero, SpriteEffects.None, 0.0f);
        }

        /// <summary>
        /// Draws a hexagon with given paramaters
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="centre"></param>
        /// <param name="size"></param>
        /// <param name="thickness"></param>
        /// <param name="c"></param>
        /// <param name="orientation"></param>
        public static void DrawHexagon(SpriteBatch sb, Vector2 centre, int size, int thickness, Color c, HexagonOrientation orientation = HexagonOrientation.Pointy)
        {
            switch (orientation)
            {
                case HexagonOrientation.Pointy:
                   DrawCircle(sb, centre, size, 6, thickness, c);
                    break;
                case HexagonOrientation.Flat:
                    break;
            }
        }

        /// <summary>
        /// Draws an approximation of a circle
        /// The smoothness defines how many lines make up the circle's circumference
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="centre"></param>
        /// <param name="radius"></param>
        /// <param name="smoothness"></param>
        /// <param name="thickness"></param>
        /// <param name="c"></param>
        public static void DrawCircle(SpriteBatch sb, Vector2 centre, int radius, int smoothness, int thickness, Color c)
        {
            double segmentAngle = 2 * Math.PI / smoothness;
            Vector2 last = centre + new Vector2(0, radius);
            for (int i = 0; i < smoothness + 1; i++)
            {
                Vector2 next = centre + new Vector2((float)Math.Sin(segmentAngle * i) * radius, (float)Math.Cos(segmentAngle * i) * radius);
                DrawLine(sb, last, next, thickness, c);
                last = next;
            }
        }

        /// <summary>
        /// Draws the outline of a rectangle
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="rect"></param>
        /// <param name="thickness"></param>
        /// <param name="c"></param>
        public static void DrawRect(SpriteBatch sb, Rectangle rect, int thickness, Color c)
        {
            DrawLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), thickness, c);
            DrawLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + rect.Height), thickness, c);
            DrawLine(sb, new Vector2(rect.X - thickness, rect.Y + rect.Height + 1), new Vector2(rect.X + rect.Width - thickness, rect.Y + rect.Height + 1), thickness, c);
            DrawLine(sb, new Vector2(rect.X + rect.Width + 1, rect.Y + thickness), new Vector2(rect.X + rect.Width + 1, rect.Y + rect.Height + thickness), thickness, c);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="rect"></param>
        /// <param name="c"></param>
        public static void DrawRectFill(SpriteBatch sb, Rectangle rect, Color c)
        {
            sb.Draw(pixel, rect, null, c);
        }

        /// <summary>
        /// Algorithm that checks if a point is inside a polygon. 
        /// Checks how may times a line originating from a point will cross each side. 
        /// An odd result means it is inside the polygon.
        /// http://astronomy.swin.edu.au/~pbourke/geometry/insidepoly/
        /// </summary>
        /// <param name="polygonVerts"></param>
        /// <param name="testPoint"></param>
        /// <returns></returns>
        public static bool InsidePolygon(Vector2[] polygonVerts, Vector2 testPoint)
        {
            int counter = 0;
            Vector2 p1 = polygonVerts[0];
            for (int i = 1; i <= polygonVerts.Length; i++)
            {
                Vector2 p2 = polygonVerts[i % polygonVerts.Length];
                if (testPoint.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (testPoint.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (testPoint.X <= Math.Max(p1.X, p2.X))
                        {
                            if (p1.Y != p2.Y)
                            {
                                double xinters = (testPoint.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                if (p1.X == p2.X || testPoint.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }
            if (counter % 2 == 0)
                return false;
            return true;
        }
    }
}
