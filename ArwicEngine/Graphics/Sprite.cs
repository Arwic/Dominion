// Dominion - Copyright (C) Timothy Ings
// Sprite.cs
// This file contains classes that define a 2d sprite

using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ArwicEngine.Graphics
{
    public class Sprite
    {
        private enum SpriteType
        {
            Atlas,
            Texture,
        }

        public const string DEFAULT_TEXTURE_PATH = "Graphics/Util/Default";

        /// <summary>
        /// Gets the texture used to render the sprite, will return the first texture of a layered texture sprites
        /// Valid only for texture and layered texture sprites
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets the path of the texture's file on disk
        /// Valid only for texture sprites
        /// </summary>
        public string Path { get; }
        
        /// <summary>
        /// Gets the atlas used to render the sprite
        /// Valid only for atlas sprites
        /// </summary>
        public SpriteAtlas Atlas { get; }

        /// <summary>
        /// Gets or sets the index that will be used to get the texture
        /// Valid only for atlas sprites
        /// </summary>
        public string AtlasKey { get; private set; }

        /// <summary>
        /// Gets the width of the sprite
        /// Returns a value based on the first layer of a layered texture sprite
        /// </summary>
        public int Width
        {
            get
            {
                if (spriteType == SpriteType.Atlas)
                    return Atlas.SpriteDefinitions[AtlasKey].Width;
                return Texture.Width;
            }
        }

        /// <summary>
        /// Gets the height of the sprite
        /// Returns a value based on the first layer of a layered texture sprite
        /// </summary>
        public int Height
        {
            get
            {
                if (spriteType == SpriteType.Atlas)
                    return Atlas.SpriteDefinitions[AtlasKey].Height;
                return Texture.Height;
            }
        }

        private SpriteType spriteType;

        /// <summary>
        /// Creates a new sprite in texture mode
        /// </summary>
        /// <param name="cm">content manager</param>
        /// <param name="path">path to files</param>
        public Sprite(string path)
        {
            Path = path;
            try { Texture = Engine.Instance.Content.Load<Texture2D>(path); }
            catch (Exception)
            {
                try { Texture = Engine.Instance.Content.Load<Texture2D>(DEFAULT_TEXTURE_PATH); }
                catch (Exception e) { throw new Exception($"Error loading default texture, {e.Message}"); }
            }
            spriteType = SpriteType.Texture;
        }

        /// <summary>
        /// Creates a new sprite in atlas mode
        /// </summary>
        /// <param name="atlas">atlas to index</param>
        /// <param name="index">index of the desired icon</param>
        public Sprite(SpriteAtlas atlas, string key)
        {
            Atlas = atlas;
            AtlasKey = key;
            spriteType = SpriteType.Atlas;
        }

        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        /// <param name="origin"></param>
        /// <param name="rotation"></param>
        public void Draw(SpriteBatch sb, Rectangle dest, Rectangle? source = null, Color? color = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0f)
        {
            // setup values
            if (color == null) color = Color.White;

            switch (spriteType)
            {
                case SpriteType.Atlas:
                    Atlas.Draw(sb, AtlasKey, dest, source, color, scale, origin, rotation);
                    break;
                case SpriteType.Texture:
                    if (source == null) source = Texture.Bounds;
                    sb.Draw(Texture, null, dest, source, origin, rotation, scale, color);
                    break;
            }
        }

        /// <summary>
        /// Draws the sprite using the nine cut method
        /// This is mostly useful for gui elements like forms and buttons
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="destEdge"></param>
        /// <param name="sourceEdge"></param>
        /// <param name="scale"></param>
        /// <param name="origin"></param>
        /// <param name="rotation"></param>
        public void DrawNineCut(SpriteBatch sb, Rectangle dest, Rectangle? source = null, Color? color = null, int? destEdge = null, int? sourceEdge = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0f)
        {
            // setup values
            if (color == null) color = Color.White;
            if (scale == null) scale = Vector2.One;
            if (origin == null) origin = new Vector2(dest.Width / 2, dest.Height / 2);

            switch (spriteType)
            {
                case SpriteType.Atlas:
                    if (destEdge == null) destEdge = dest.Width / 3;
                    if (sourceEdge == null) sourceEdge = Width / 3;
                    Atlas.DrawNineCut(sb, AtlasKey, dest, source, color, destEdge, sourceEdge);
                    break;
                case SpriteType.Texture:
                    if (destEdge == null) destEdge = 15;
                    if (sourceEdge == null) sourceEdge = Texture.Width / 3;
                    if (source == null) source = Texture.Bounds;
                    DrawTextureNineCut(sb, Texture, color.Value, destEdge.Value, sourceEdge.Value, dest, source.Value);
                    break;
            }
        }

        private void DrawTextureNineCut(SpriteBatch sb, Texture2D texture, Color c, int destEdge, int sourceEdge, Rectangle dest, Rectangle src)
        {
            int destEdgeX = Math.Min(destEdge, dest.Width);
            int destEdgeY = Math.Min(destEdge, dest.Height);

            // top
            Rectangle r_topLeftDest = new Rectangle(dest.X, dest.Y, destEdgeX, destEdgeY);
            Rectangle r_topLeftSource = new Rectangle(src.X, src.Y, sourceEdge, sourceEdge);
            sb.Draw(texture, r_topLeftDest, r_topLeftSource, c);
            Rectangle r_topRightDest = new Rectangle(dest.X + dest.Width - destEdgeX, dest.Y, destEdgeX, destEdgeY);
            Rectangle r_topRightSource = new Rectangle(src.X + 2 * sourceEdge, src.Y, sourceEdge, sourceEdge);
            sb.Draw(texture, r_topRightDest, r_topRightSource, c);
            Rectangle r_topMiddleDest = new Rectangle(dest.X + destEdgeX, dest.Y, dest.Width - destEdgeX * 2, destEdgeY);
            Rectangle r_topMiddleSource = new Rectangle(src.X + sourceEdge, src.Y, sourceEdge, sourceEdge);
            sb.Draw(texture, r_topMiddleDest, r_topMiddleSource, c);

            // bottom
            Rectangle r_bottomLeftDest = new Rectangle(dest.X, dest.Y + dest.Height - destEdgeY, destEdgeX, destEdgeY);
            Rectangle r_bottomLeftSource = new Rectangle(src.X, src.Y + 2 * sourceEdge, sourceEdge, sourceEdge);
            sb.Draw(texture, r_bottomLeftDest, r_bottomLeftSource, c);
            Rectangle r_bottomRightDest = new Rectangle(dest.X + dest.Width - destEdgeX, dest.Y + dest.Height - destEdgeY, destEdgeX, destEdgeY);
            Rectangle r_bottomRightSource = new Rectangle(src.X + 2 * sourceEdge, src.Y + 2 * sourceEdge, sourceEdge, sourceEdge);
            sb.Draw(texture, r_bottomRightDest, r_bottomRightSource, c);
            Rectangle r_bottomMiddleDest = new Rectangle(dest.X + destEdgeX, dest.Y + dest.Height - destEdgeY, dest.Width - destEdgeX * 2, destEdgeY);
            Rectangle r_bottomMiddleSource = new Rectangle(src.X + sourceEdge, src.Y + 2 * sourceEdge, sourceEdge, sourceEdge);
            sb.Draw(texture, r_bottomMiddleDest, r_bottomMiddleSource, c);

            // middle
            Rectangle r_centreLeftDest = new Rectangle(dest.X, dest.Y + destEdgeY, destEdgeX, dest.Height - destEdgeY * 2);
            Rectangle r_centreLeftSource = new Rectangle(src.X, src.Y + sourceEdge, sourceEdge, sourceEdge);
            sb.Draw(texture, r_centreLeftDest, r_centreLeftSource, c);
            Rectangle r_centreRightDest = new Rectangle(dest.X + dest.Width - destEdgeX, dest.Y + destEdgeY, destEdgeX, dest.Height - destEdgeY * 2);
            Rectangle r_centreRightSource = new Rectangle(src.X + 2 * sourceEdge, src.Y + sourceEdge, sourceEdge, sourceEdge);
            sb.Draw(texture, r_centreRightDest, r_centreRightSource, c);
            Rectangle r_centreMiddleDest = new Rectangle(dest.X + destEdgeX, dest.Y + destEdgeY, dest.Width - destEdgeX * 2, dest.Height - destEdgeY * 2);
            Rectangle r_centreMiddleSource = new Rectangle(src.X + sourceEdge, src.Y + sourceEdge, sourceEdge, sourceEdge);
            sb.Draw(texture, r_centreMiddleDest, r_centreMiddleSource, c);
        }
    }
}
