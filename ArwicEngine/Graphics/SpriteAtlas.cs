// Dominion - Copyright (C) Timothy Ings
// SpriteAtlas.cs
// This file contains classes that define a 2d sprite atlas

using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ArwicEngine.Graphics
{
    public class XmlRectangle
    {
        [XmlElement("X")]
        public int X { get; set; }

        [XmlElement("Y")]
        public int Y { get; set; }

        [XmlElement("Width")]
        public int Width { get; set; }

        [XmlElement("Height")]
        public int Height { get; set; }

        public XmlRectangle(Rectangle xnaRect)
        {
            X = xnaRect.X;
            Y = xnaRect.Y;
            Width = xnaRect.Width;
            Height = xnaRect.Height;
        }

        public XmlRectangle() { }
    }

    public class SpriteDefinition
    {
        [XmlElement("Key")]
        public string Key { get; set; }

        [XmlElement("Source")]
        public XmlRectangle XmlSource { get; set; }

        [XmlIgnore]
        public Rectangle Source => new Rectangle(XmlSource.X, XmlSource.Y, XmlSource.Width, XmlSource.Height);

        public SpriteDefinition(string key, Rectangle source)
        {
            Key = key;
            XmlSource = new XmlRectangle(source);
        }

        public SpriteDefinition() { }
    }

    public class SpriteAtlasDefinition
    {
        [XmlElement("TexturePath")]
        public string BaseTexturePath { get; set; }

        [XmlArray("SpriteDefinitions"), XmlArrayItem(typeof(SpriteDefinition), ElementName = "SpriteDefinition")]
        public List<SpriteDefinition> RawDefinitions { get; set; }
    }

    public class SpriteAtlas
    {
        /// <summary>
        /// Gets or sets the sprite used as a source
        /// </summary>
        public Sprite BaseTexture { get; private set; }

        public Dictionary<string, Rectangle> SpriteDefinitions { get; private set; }

        /// <summary>
        /// Creates a new sprite atlas
        /// </summary>
        /// <param name="cm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="iconDim"></param>
        public SpriteAtlas(Sprite baseSprite, Dictionary<string, Rectangle> spriteDefinitions)
        {
            SpriteDefinitions = spriteDefinitions;
            BaseTexture = baseSprite;
        }

        public SpriteAtlas() { }

        public void Define(SpriteAtlasDefinition definition)
        {
            SpriteDefinitions = new Dictionary<string, Rectangle>();
            foreach (SpriteDefinition sdef in definition.RawDefinitions)
                SpriteDefinitions.Add(sdef.Key, sdef.Source);
            BaseTexture = Engine.Instance.Content.GetAsset<Sprite>(definition.BaseTexturePath);
        }

        /// <summary>
        /// Draws the sub sprite with the given index from the atlas
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="index"></param>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        public void Draw(SpriteBatch sb, string spriteDef, Rectangle dest, Rectangle? source = null, Color? color = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0)
        {
            Rectangle def;
            bool res = SpriteDefinitions.TryGetValue(spriteDef, out def);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"Could not find sprite '{spriteDef}' in '{BaseTexture.Path}'");
                return;
            }

            if (source == null) source = new Rectangle(0, 0, def.Width, def.Height);
            if (color == null) color = Color.White;

            Rectangle finalSource = new Rectangle(def.X + source.Value.X, def.Y + source.Value.Y, source.Value.Width, source.Value.Height);

            BaseTexture.Draw(sb, dest, finalSource, color, scale, origin, rotation);
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
        public void DrawNineCut(SpriteBatch sb, string spriteDef, Rectangle dest, Rectangle? source = null, Color? color = null, int? destEdge = null, int? sourceEdge = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0)
        {
            Rectangle def;
            bool res = SpriteDefinitions.TryGetValue(spriteDef, out def);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"Could not find sprite '{spriteDef}' in '{BaseTexture.Path}'");
                return;
            }

            if (source == null) source = def;

            Rectangle finalSource = new Rectangle(def.X + source.Value.X, def.Y + source.Value.Y, source.Value.Width, source.Value.Height);

            BaseTexture.DrawNineCut(sb, dest, finalSource, color, destEdge, sourceEdge, scale, origin, rotation);
        }
    }
}
