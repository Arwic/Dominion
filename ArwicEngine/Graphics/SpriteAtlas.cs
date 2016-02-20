// Dominion - Copyright (C) Timothy Ings
// SpriteAtlas.cs
// This file contains classes that define a 2d sprite atlas

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ArwicEngine.Graphics
{
    public class SpriteAtlas
    {
        /// <summary>
        /// Gets or sets the sprite used as a source
        /// </summary>
        public Sprite Source { get; private set; }

        /// <summary>
        /// Gets or sets the width of each sub sprite on the atlas
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets or sets the height of each sub sprite on the atlas
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets or sets the dimension of each sub sprite on the atlas
        /// </summary>
        public int ItemDim { get; private set; }

        /// <summary>
        /// Creates a new sprite atlas
        /// </summary>
        /// <param name="cm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="iconDim"></param>
        public SpriteAtlas(ContentManager cm, string sourcePath, int iconDim)
        {
            Source = new Sprite(cm, sourcePath);
            ItemDim = iconDim;
            Width = Source.Width / ItemDim;
            Height = Source.Height / ItemDim;
        }

        /// <summary>
        /// Draws the sub sprite with the given index from the atlas
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="index"></param>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        public void Draw(SpriteBatch sb, int index, Rectangle dest, Rectangle? source = null, Color? color = null)
        {
            if (source == null) source = new Rectangle(0, 0, ItemDim, ItemDim);

            if (color == null) color = Color.White;
            int x = 0;
            int y = 0;
            int i = index;
            // calculate the x and y coords on the source sprite
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
            Source.Draw(sb, dest, src, color);
        }

        /// <summary>
        /// Draws the sub sprite with the given index from the atlas using the nine cut method
        /// This is mostly useful for gui elements like forms and buttons
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="index"></param>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="destEdge"></param>
        /// <param name="sourceEdge"></param>
        public void DrawNineCut(SpriteBatch sb, int index, Rectangle dest, Rectangle? source = null, Color? color = null, int? destEdge = null, int? sourceEdge = null)
        {
            if (source == null) source = new Rectangle(0, 0, ItemDim, ItemDim);
            if (destEdge == null) destEdge = ItemDim;
            if (sourceEdge == null) sourceEdge = ItemDim / 3;

            if (color == null) color = Color.White;
            int x = 0;
            int y = 0;
            int i = index;
            // calculate the x and y coords on the source sprite
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
            Source.DrawNineCut(sb, dest, src, color, destEdge, sourceEdge);
        }
    }
}
