// Dominion - Copyright (C) Timothy Ings
// Image.cs
// This file contains classes that define an image container

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArwicEngine.Forms
{
    public class Image : Control
    {
        #region Properties & Accessors
        /// <summary>
        /// Gets or sets the sprite used to render the image
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite Sprite
        {
            get
            {
                return _sprite;
            }
            set
            {
                Sprite last = _sprite;
                _sprite = value;
                if (last != _sprite)
                {
                    OnSpriteChanged(EventArgs.Empty);
                    Source = new Rectangle(0, 0, Sprite.Width, Sprite.Height);
                }
            }
        }
        private Sprite _sprite;
        /// <summary>
        /// Gets or sets the rectangle the specifies what section of the sprite to draw
        /// </summary>
        [TypeConverter(typeof(RectangleDimConverter))]
        public Rectangle Source
        {
            get
            {
                return _source;
            }
            set
            {
                Rectangle last = _source;
                _source = value;
                if (last != _source)
                    OnSourceChanged(EventArgs.Empty);
            }
        }
        private Rectangle _source;
        /// <summary>
        /// Gets or sets the rotation of the image about the rotation origin
        /// </summary>
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                float last = _rotation;
                _rotation = value;
                if (last != _rotation)
                    OnRotationChanged(EventArgs.Empty);
            }
        }
        private float _rotation;
        /// <summary>
        /// Gets or sets the rotation origin to rotate about
        /// </summary>
        [TypeConverter(typeof(PointConverter))]
        public Point Origin
        {
            get
            {
                return _origin;
            }
            set
            {
                Point last = _origin;
                _origin = value;
                if (last != _origin)
                    OnRotationOriginChanged(EventArgs.Empty);
            }
        }
        private Point _origin;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the sprite property is changed
        /// </summary>
        public event EventHandler SpriteChanged;
        /// <summary>
        /// Occurs when the value of the source property is changed
        /// </summary>
        public event EventHandler SourceChanged;
        /// <summary>
        /// Occurs when the value of the rotation property is changed
        /// </summary>
        public event EventHandler RotationChanged;
        /// <summary>
        /// Occurs whent the value of the rotation origin property is changed
        /// </summary>
        public event EventHandler RotationOriginChanged;
        #endregion

        #region Event Handlers
        protected virtual void OnSpriteChanged(EventArgs e)
        {
            if (SpriteChanged != null)
                SpriteChanged(this, e);
        }
        protected virtual void OnSourceChanged(EventArgs e)
        {
            if (SourceChanged != null)
                SourceChanged(this, e);
        }
        protected virtual void OnRotationChanged(EventArgs e)
        {
            if (RotationChanged != null)
                RotationChanged(this, e);
        }
        protected virtual void OnRotationOriginChanged(EventArgs e)
        {
            if (RotationOriginChanged != null)
                RotationOriginChanged(this, e);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the Image class with default settings and optional parent
        /// </summary>
        /// <param name="pos">Position of the Image</param>
        /// <param name="img">Sprite to be drawn</param>
        /// <param name="src">Optional, part of the sprite to be drawn</param>
        /// <param name="parent">Optional parent</param>
        public Image(Rectangle pos, Sprite img, Rectangle? src = null, Control parent = null)
            : base (pos, parent)
        {
            Sprite = img;
            if (src == null) src = new Rectangle(0, 0, Sprite.Width, Sprite.Height);
            Source = src.Value;
        }

        public Image(ImageConfig config, Control parent = null)
            : base(config, parent)
        {
            Sprite = new Sprite(Engine.Instance.Content.Load<Texture2D>(config.SpritePath));
            Source = config.Source;
            Rotation = config.Rotation;
            Origin = config.Origin;
        }

        /// <summary>
        /// Draws the Image
        /// </summary>
        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                Sprite.Draw(sb, AbsoluteBounds, Source, Color, null, Origin.ToVector2(), Rotation);
            }
            base.Draw(sb);
        }
    }
}
