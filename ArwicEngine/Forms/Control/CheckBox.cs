// Dominion - Copyright (C) Timothy Ings
// CheckBox.cs
// This file contains classes that define a check box

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
    public class CheckBox : Control
    {
        #region Defaults
        public static Sprite DefaultSpriteTrue;
        public static Sprite DefaultSpriteFalse;
        public static new Cursor DefaultCursor;

        public static new void InitDefaults()
        {
            DefaultSpriteTrue = Engine.Instance.Content.GetAsset<Sprite>(CONTROL_CHECKBOX_TRUE);
            DefaultSpriteFalse = Engine.Instance.Content.GetAsset<Sprite>(CONTROL_CHECKBOX_FALSE);
            DefaultCursor = Engine.Instance.Content.GetAsset<Cursor>(CURSOR_LINK_PATH);
        }
        #endregion

        #region Properties & Accessors
        /// <summary>
        /// Gets or sets the value used as a color modifier when the button is not enabled
        /// </summary>
        public float DisabledColorPercent { get; set; }
        /// <summary>
        /// Gets or sets the sprite used to draw the check box when value is true
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite SpriteTrue { get; set; }
        /// <summary>
        /// Gets or sets the sprite used to draw the check box when value is false
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite SpriteFalse { get; set; }
        /// <summary>
        /// Gets or sets the Value of the check box
        /// </summary>
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                bool last = _value;
                _value = value;
                if (last != _value)
                    OnValueChanged(EventArgs.Empty);
            }
        }
        private bool _value;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the Value property value has changed
        /// </summary>
        public event EventHandler<EventArgs> ValueChanged;
        #endregion

        #region Event Handlers
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the CheckBox class with default settings and optional parent
        /// </summary>
        /// <param name="pos">Position of the CheckBox</param>
        /// <param name="parent">Optional parent</param>
        public CheckBox(Rectangle pos, Control parent = null)
            : base (pos, parent)
        {
            Initialize();
        }

        private void Initialize()
        {
            DisabledColorPercent = 0.5f;
            SpriteTrue = DefaultSpriteTrue;
            SpriteFalse = DefaultSpriteFalse;
            Cursor = DefaultCursor;
            Value = false;
            MouseClick += CheckBox_MouseClick;
        }

        public CheckBox(CheckBoxConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize();
            SpriteTrue = DefaultSpriteTrue;
            SpriteFalse = DefaultSpriteFalse;
            DisabledColorPercent = config.DisabledColorPercent;
            Value = config.Value;
        }

        private void CheckBox_MouseClick(object sender, MouseEventArgs e)
        {
            Value = !Value;
        }

        /// <summary>
        /// Draws the CheckBox
        /// </summary>
        public override void Draw(SpriteBatch sb)
        {
            Sprite sprite = SpriteFalse;
            if (Value)
                sprite = SpriteTrue;
            Color color = Color;
            if (!Enabled)
                color = Color.Multiply(Color, DisabledColorPercent);
            sprite.Draw(sb, AbsoluteBounds, null, color);

            Vector2 measureText = Text.Measure();
            Text.Draw(sb, new Vector2(AbsoluteLocation.X + Size.Width + 5, AbsoluteLocation.Y + (Size.Height / 2) - (measureText.Y / 2)));
            base.Draw(sb);
        }
    }
}
