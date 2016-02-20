// Dominion - Copyright (C) Timothy Ings
// Camera2.cs
// This file contains classes that define a 2d camera

using Microsoft.Xna.Framework;

namespace ArwicEngine.Graphics
{
    public class Camera2
    {
        /// <summary>
        /// The target of the camera's translation
        /// </summary>
        public Vector2 TranslationTarget { get; set; }
        
        /// <summary>
        /// The target of the camera's zoom
        /// </summary>
        public Vector2 ZoomTarget { get; set; }

        /// <summary>
        /// The speed at which the camera approaches its targets
        /// A value of -1 enters auto follow speed mode
        /// </summary>
        public Vector2 FollowSpeed { get; set; }

        /// <summary>
        /// The current translation of the camera
        /// </summary>
        public Vector2 Translation { get; set; }
        
        /// <summary>
        /// The current zoom of the camera
        /// </summary>
        public Vector2 Zoom { get; set; }

        /// <summary>
        /// A rectangle that the camera's translation must stay within
        /// A rectangle = { -1, -1, -1, -1 } disables the view limit
        /// </summary>
        public Rectangle ViewLimit { get; set; }

        /// <summary>
        /// The speed at which teh camer follows the target when automatic mode is enabled
        /// </summary>
        public Vector2 AutoFollowSpeed { get; set; }

        private Vector2 origin;
        private GraphicsManager graphics;

        /// <summary>
        /// Creates a new camera
        /// </summary>
        /// <param name="gm"></param>
        public Camera2(GraphicsManager gm)
        {
            // setup defaults
            graphics = gm;
            Translation = Vector2.Zero;
            Zoom = Vector2.One;
            TranslationTarget = Vector2.Zero;
            FollowSpeed = new Vector2(-1, -1);
            ZoomTarget = new Vector2(1, 1);
            ViewLimit = new Rectangle(-1, -1, -1, -1);
            AutoFollowSpeed = new Vector2(15, 15);
        }

        /// <summary>
        /// Converts a rectangle from screen space to world space
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public Rectangle ConvertScreenToWorld(Rectangle r)
        {
            Vector2 xy = ConvertScreenToWorld(new Vector2(r.X, r.Y));
            Rectangle rect = new Rectangle((int)xy.X, (int)xy.Y, (int)(r.Width / Zoom.X), (int)(r.Height / Zoom.Y));
            return rect;
        }

        /// <summary>
        /// Converts a vector2 from screen space to world space
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector2 ConvertScreenToWorld(Vector2 p)
        {
            Vector2 pos = p;
            pos = new Vector2(pos.X / graphics.Scale, pos.Y / graphics.Scale);
            pos = new Vector2(pos.X / Zoom.X, pos.Y / Zoom.Y);
            pos = new Vector2(pos.X - graphics.Device.Viewport.Width / 2 / Zoom.X, pos.Y - graphics.Device.Viewport.Height / 2 / Zoom.Y);
            pos = new Vector2(pos.X + Translation.X, pos.Y + Translation.Y);
            return pos;
        }

        /// <summary>
        /// Converts a rectangle from world space to screen space
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public Rectangle ConvertWorldToScreen(Rectangle r)
        {
            Vector2 xy = ConvertWorldToScreen(new Vector2(r.X, r.Y));
            Rectangle rect = new Rectangle((int)xy.X, (int)xy.Y, (int)(r.Width * Zoom.X), (int)(r.Height * Zoom.Y));
            return rect;
        }

        /// <summary>
        /// Covnerts a vector2 from world space to screen space
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector2 ConvertWorldToScreen(Vector2 p)
        {
            Vector2 pos = p;
            pos = new Vector2(pos.X - Translation.X, pos.Y - Translation.Y);
            pos = new Vector2(pos.X + graphics.Device.Viewport.Width / 2 / Zoom.X, pos.Y + graphics.Device.Viewport.Height / 2 / Zoom.Y);
            pos = new Vector2(pos.X * Zoom.X, pos.Y * Zoom.Y);
            return pos;
        }

        /// <summary>
        /// Updates the camera
        /// </summary>
        /// <param name="delta"></param>
        public void Update(float delta)
        {
            // adjust the camera's zoom towards its zoom target, if its not already there
            if (ZoomTarget != Zoom)
            {
                Vector2 sep = ZoomTarget - Zoom;
                float autoZoomMod = 10.0f;
                Vector2 inc = new Vector2(sep.X * delta * autoZoomMod, sep.Y * delta * autoZoomMod);
                // make sure the camera doesn't over shoot and cause bouncing
                if (inc.Length() >= sep.Length())
                    Zoom = ZoomTarget;
                else
                    Zoom += inc;
            }

            // calculate the camera's origin
            origin = new Vector2(graphics.Device.Viewport.Width / 2 / Zoom.X, graphics.Device.Viewport.Height / 2 / Zoom.Y);

            // adjust the camera's translation towards its translation target, if its not already there
            if (TranslationTarget != Translation)
            {
                Vector2 sep = TranslationTarget - Translation;
                if (FollowSpeed.X == -1) // -1 indicates auto follow
                {
                    float inc = sep.X * delta * AutoFollowSpeed.X; // calculate a dynamic based on the sep vector
                    Translation += new Vector2(inc, 0); // move the camera
                }
                else
                {
                    float inc = FollowSpeed.X * delta; // use the defined speed
                    if (inc > sep.X) // make sure the camera doesn't over shoot and cause bouncing
                        inc = sep.X;
                    Translation += new Vector2(inc, 0); // move the camera
                }

                if (FollowSpeed.Y == -1) // -1 indicates auto follow
                {
                    float inc = sep.Y * delta * AutoFollowSpeed.Y; // calculate a dynamic based on the sep vector
                    Translation += new Vector2(0, inc); // move the camera
                }
                else
                {
                    float inc = FollowSpeed.Y * delta; // use the defined speed
                    if (inc > sep.Y) // make sure the camera doesn't over shoot and cause bouncing
                        inc = sep.Y;
                    Translation += new Vector2(0, inc); // move the camera
                }
            }

            /// A view limit of { -1, -1, -1, -1 } disables the view limit
            if (ViewLimit.X != -1 && ViewLimit.Y != -1 && ViewLimit.Width != -1 && ViewLimit.Height != -1)
            {
                // clamp translation and translation target on both axis and both signs
                if (Translation.X - graphics.Device.Viewport.Width / 2 < ViewLimit.X)
                {
                    Translation = new Vector2(ViewLimit.X + graphics.Device.Viewport.Width / 2, Translation.Y);
                    TranslationTarget = new Vector2(ViewLimit.X + graphics.Device.Viewport.Width / 2, TranslationTarget.Y);
                }
                else if (Translation.X + graphics.Device.Viewport.Width / 2 > ViewLimit.Width)
                {
                    Translation = new Vector2(ViewLimit.Width - graphics.Device.Viewport.Width / 2, Translation.Y);
                    TranslationTarget = new Vector2(ViewLimit.Width - graphics.Device.Viewport.Width / 2, TranslationTarget.Y);
                }

                if (Translation.Y - graphics.Device.Viewport.Height / 2 < ViewLimit.Y)
                {
                    Translation = new Vector2(Translation.X, ViewLimit.Y + graphics.Device.Viewport.Height / 2);
                    TranslationTarget = new Vector2(TranslationTarget.X, ViewLimit.Y + graphics.Device.Viewport.Height / 2);
                }
                else if (Translation.Y + graphics.Device.Viewport.Height / 2 > ViewLimit.Height)
                {
                    Translation = new Vector2(Translation.X, ViewLimit.Height - graphics.Device.Viewport.Height / 2);
                    TranslationTarget = new Vector2(TranslationTarget.X, ViewLimit.Height - graphics.Device.Viewport.Height / 2);
                }
            }
        }

        /// <summary>
        /// Returns the foreground transformation matrix to e used with an xna sprite batch
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public Matrix GetForegroundTransformation(float scale)
        {
            return Matrix.Identity *
                    Matrix.CreateTranslation(-Translation.X, -Translation.Y, 0) *
                    Matrix.CreateTranslation(origin.X, origin.Y, 0) *
                    Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1.0f)) *
                    Matrix.CreateScale(new Vector3(scale, scale, 1.0f));
        }
    }
}