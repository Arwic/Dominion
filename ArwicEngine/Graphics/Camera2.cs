using Microsoft.Xna.Framework;

namespace ArwicEngine.Graphics
{
    public class Camera2
    {
        public Vector2 TranslationTarget { get; set; }
        public Vector2 ZoomTarget { get; set; }
        public Vector2 FollowSpeed { get; set; }
        public Vector2 Translation { get; set; }
        public Vector2 Zoom { get; set; }
        public Rectangle ViewLimit { get; set; }
        public Vector2 AutoFollowSpeed { get; set; }

        private Vector2 origin;
        private GraphicsManager graphics;

        public Camera2(GraphicsManager gm)
        {
            graphics = gm;
            Translation = Vector2.Zero;
            Zoom = Vector2.One;
            TranslationTarget = Vector2.Zero;
            FollowSpeed = new Vector2(-1, -1);
            ZoomTarget = new Vector2(1, 1);
            ViewLimit = new Rectangle(-1, -1, -1, -1);
            AutoFollowSpeed = new Vector2(15, 15);
        }

        public Rectangle ConvertScreenToWorld(Rectangle r)
        {
            Vector2 xy = ConvertScreenToWorld(new Vector2(r.X, r.Y));
            Rectangle rect = new Rectangle((int)xy.X, (int)xy.Y, (int)(r.Width / Zoom.X), (int)(r.Height / Zoom.Y));
            return rect;
        }

        public Vector2 ConvertScreenToWorld(Vector2 p)
        {
            Vector2 pos = p;
            pos = new Vector2(pos.X / graphics.Scale, pos.Y / graphics.Scale);
            pos = new Vector2(pos.X / Zoom.X, pos.Y / Zoom.Y);
            pos = new Vector2(pos.X - graphics.Device.Viewport.Width / 2 / Zoom.X, pos.Y - graphics.Device.Viewport.Height / 2 / Zoom.Y);
            pos = new Vector2(pos.X + Translation.X, pos.Y + Translation.Y);
            return pos;
        }

        public Rectangle ConvertWorldToScreen(Rectangle r)
        {
            Vector2 xy = ConvertWorldToScreen(new Vector2(r.X, r.Y));
            Rectangle rect = new Rectangle((int)xy.X, (int)xy.Y, (int)(r.Width * Zoom.X), (int)(r.Height * Zoom.Y));
            return rect;
        }

        public Vector2 ConvertWorldToScreen(Vector2 p)
        {
            Vector2 pos = p;
            pos = new Vector2(pos.X - Translation.X, pos.Y - Translation.Y);
            pos = new Vector2(pos.X + graphics.Device.Viewport.Width / 2 / Zoom.X, pos.Y + graphics.Device.Viewport.Height / 2 / Zoom.Y);
            pos = new Vector2(pos.X * Zoom.X, pos.Y * Zoom.Y);
            return pos;
        }

        public void Update(float delta)
        {
            if (ZoomTarget != Zoom)
            {
                Vector2 sep = ZoomTarget - Zoom;
                float autoZoomMod = 10.0f;
                Vector2 inc = new Vector2(sep.X * delta * autoZoomMod, sep.Y * delta * autoZoomMod);
                if (inc.Length() >= sep.Length())
                    Zoom = ZoomTarget;
                else
                    Zoom += inc;
            }

            origin = new Vector2(graphics.Device.Viewport.Width / 2 / Zoom.X, graphics.Device.Viewport.Height / 2 / Zoom.Y);

            if (TranslationTarget != Translation)
            {
                Vector2 sep = TranslationTarget - Translation;
                if (FollowSpeed.X == -1)
                {
                    float inc = sep.X * delta * AutoFollowSpeed.X;
                    Translation += new Vector2(inc, 0);
                }
                else
                {
                    float inc = FollowSpeed.X * delta;
                    if (inc > sep.X)
                    {
                        inc = sep.X;
                    }
                    Translation += new Vector2(inc, 0);
                }

                if (FollowSpeed.Y == -1)
                {
                    float inc = sep.Y * delta * AutoFollowSpeed.Y;
                    Translation += new Vector2(0, inc);
                }
                else
                {
                    float inc = FollowSpeed.Y * delta;
                    if (inc > sep.Y)
                    {
                        inc = sep.Y;
                    }
                    Translation += new Vector2(0, inc);
                }
            }

            if (ViewLimit.X != -1 && ViewLimit.Y != -1 && ViewLimit.Width != -1 && ViewLimit.Height != -1)
            {
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