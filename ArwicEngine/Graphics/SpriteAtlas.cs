using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArwicEngine.Graphics
{
    public class SpriteAtlas
    {
        public Sprite Atlas { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int ItemDim { get; private set; }

        public SpriteAtlas(ContentManager cm, string path, int iconDim)
        {
            Atlas = new Sprite(cm, path);
            ItemDim = iconDim;
            Width = Atlas.Width / ItemDim;
            Height = Atlas.Height / ItemDim;
        }

        public void Draw(SpriteBatch sb, int index, Rectangle dest, Rectangle? source = null, Color? color = null)
        {
            if (source == null) source = new Rectangle(0, 0, ItemDim, ItemDim);

            if (color == null) color = Color.White;
            int x = 0;
            int y = 0;
            int i = index;
            if (i < Width)
            {
                x = i;
            }
            else
            {
                int di = i;
                y = -1;
                while (di >= 0)
                {
                    di -= Width;
                    y++;
                }
                x = i - Width * y;
            }
            Rectangle src = new Rectangle(source.Value.X + x * ItemDim, source.Value.Y + y * ItemDim, source.Value.Width, source.Value.Height);
            Atlas.Draw(sb, dest, src, color);
        }

        public void DrawNineCut(SpriteBatch sb, int index, Rectangle dest, Rectangle? source = null, Color? color = null, int? destEdge = null, int? sourceEdge = null)
        {
            if (source == null) source = new Rectangle(0, 0, ItemDim, ItemDim);
            if (destEdge == null) destEdge = ItemDim;
            if (sourceEdge == null) sourceEdge = ItemDim / 3;

            if (color == null) color = Color.White;
            int x = 0;
            int y = 0;
            int i = index;
            if (i < Width)
            {
                x = i;
            }
            else
            {
                int di = i;
                y = -1;
                while (di >= 0)
                {
                    di -= Width;
                    y++;
                }
                x = i - Width * y;
            }
            Rectangle src = new Rectangle(source.Value.X + x * ItemDim, source.Value.Y + y * ItemDim, source.Value.Width, source.Value.Height);
            Atlas.DrawNineCut(sb, dest, src, color, destEdge, sourceEdge);
        }
    }
}
