//using Microsoft.Xna.Framework;

//namespace ArwicEngine.Graphics
//{
//    public static class RectangleExt
//    {
//        public static Transform2 ToTransform2(this Rectangle rect)
//        {
//            return new Transform2(rect);
//        }
//    }

//    public class Transform2
//    {
//        public Vector2 Location { get; set; }
//        public Vector2 Size { get; set; }
//        public float Rotation { get; set; }
//        public Vector2 RotationOrigin { get; set; }
//        public Vector2 Scale { get; set; }
//        public Rectangle Bounds => new Rectangle((int)Location.X, (int)Location.Y, (int)Size.X, (int)Size.Y);

//        public Transform2(float x = 0f, float y = 0f, float w = 0f, float h = 0f, float rotation = 0f, Vector2? rotOrigin = null, Vector2? scale = null)
//        {
//            if (rotOrigin == null) rotOrigin = new Vector2(0, 0);
//            if (scale == null) scale = new Vector2(1f, 1f);
//            Build(x, y, w, h, rotation, rotOrigin.Value, scale.Value);
//        }

//        public Transform2(Rectangle rect, float rotation = 0f, Vector2? rotOrigin = null, Vector2? scale = null)
//        {
//            if (rotOrigin == null) rotOrigin = new Vector2(0, 0);
//            if (scale == null) scale = new Vector2(1f, 1f);
//            Build(rect.X, rect.Y, rect.Width, rect.Height, rotation, rotOrigin.Value, scale.Value);
//        }

//        private void Build(float x, float y, float w, float h, float rotation, Vector2 rotOrigin, Vector2 scale)
//        {
//            Location = new Vector2(x, y);
//            Size = new Vector2(w, h);
//            Rotation = rotation;
//            RotationOrigin = rotOrigin;
//            Scale = scale;
//        }

//        public bool Contains(Vector2 point, float scale = 1f, Vector2? origin = null)
//        {
//            if (origin == null) origin = Vector2.Zero;
//            if (new Rectangle((int)((Bounds.X + origin.Value.X) * scale), (int)((Bounds.Y + origin.Value.Y) * scale), (int)(Bounds.Width * scale), (int)(Bounds.Height * scale)).Contains(point))
//                return true;
//            return false;
//        }

//        public bool Contains(Rectangle rect, float scale = 1f, Vector2? origin = null)
//        {
//            if (origin == null) origin = Vector2.Zero;
//            if (new Rectangle((int)((Bounds.X + origin.Value.X) * scale), (int)((Bounds.Y + origin.Value.Y) * scale), (int)(Bounds.Width * scale), (int)(Bounds.Height * scale)).Contains(rect))
//                return true;
//            return false;
//        }

//        public bool Contains(Point point, float scale = 1f, Vector2? origin = null)
//        {
//            if (origin == null) origin = Vector2.Zero;
//            if (new Rectangle((int)((Bounds.X + origin.Value.X) * scale), (int)((Bounds.Y + origin.Value.Y) * scale), (int)(Bounds.Width * scale), (int)(Bounds.Height * scale)).Contains(point))
//                return true;
//            return false;
//        }
//    }
//}
