// Dominion - Copyright (C) Timothy Ings
// ProgressBar.cs
// This file contains classes that define a progress bar

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class ProgressBar : Control
    {
        #region Defaults
        public static Sprite DefaultBarSprite;
        public static Sprite DefaultBackSprite;

        public static new void InitDefaults()
        {
            DefaultBarSprite = Engine.Instance.Content.GetAsset<Sprite>(CONTROL_PROGRESSBAR_FILL);
            DefaultBackSprite = Engine.Instance.Content.GetAsset<Sprite>(CONTROL_PROGRESSBAR_BACK);
        }
        #endregion

        #region Properties & Accessors
        /// <summary>
        /// Gets or sets the maximum value of the progress bar, used for drawing and is not a limit
        /// </summary>
        public float Maximum
        {
            get
            {
                return _maximum;
            }
            set
            {
                float last = _maximum;
                _maximum = value;
                if (last != _maximum)
                    OnMaxChanged(EventArgs.Empty);
            }
        }
        private float _maximum;
        /// <summary>
        /// Gets or sets the value of the progress bar, this can be over the maximum
        /// </summary>
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                float last = _value;
                _value = value;
                if (last != _value)
                    OnValueChanged(EventArgs.Empty);
            }
        }
        private float _value;
        /// <summary>
        /// Get or set the sprite used to draw the bar
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite BarSprite { get; set; }
        /// <summary>
        /// Get or set the sprite used to draw the background
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite BackSprite { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the value of the maximum value is changed
        /// </summary>
        public event EventHandler MaxChanged;
        /// <summary>
        /// Occurs when the value of the value property is changed
        /// </summary>
        public event EventHandler ValueChanged;
        #endregion

        #region Event Handlers
        protected virtual void OnMaxChanged(EventArgs e)
        {
            if (MaxChanged != null)
                MaxChanged(this, e);
        }
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }
        #endregion

        public ProgressBar(Rectangle pos, float max = 1f, float value = 0f, Control parent = null)
            : base(pos, parent)
        {
            Color = DefaultColor;
            BackSprite = DefaultBackSprite;
            BarSprite = DefaultBarSprite;
            Maximum = max;
            Value = value;
        }

        public ProgressBar(ProgressBarConfig config, Control parent = null)
            : base(config, parent)
        {
            BarSprite = DefaultBarSprite;
            BackSprite = DefaultBackSprite;
            Maximum = config.Maximum;
            Value = config.Value;
        }

        /// <summary>
        /// Draws the ProgressBar
        /// </summary>
        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {

                BackSprite.DrawNineCut(sb, AbsoluteBounds, null, Color.White);
                if (Maximum != 0 && Value != 0)
                {
                    Rectangle barRect = new Rectangle(
                        AbsoluteLocation.X + 1,
                        AbsoluteLocation.Y + 1,
                        Convert.ToInt32((Value * Size.Width) / Maximum) - 2,
                        Size.Height - 3);
                    BarSprite.DrawNineCut(sb, barRect, null, Color);
                }
            }
            base.Draw(sb);
        }
    }
}
