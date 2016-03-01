// Dominion - Copyright (C) Timothy Ings
// Button.cs
// This file contains classes that define a button

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class Button : Control
    {
        #region Defaults
        public static Sprite DefaultSprite;
        public static new Cursor DefaultCursor;

        public static new void InitDefaults()
        {
            DefaultSprite = Engine.Instance.Content.GetAsset<Sprite>(ASSET_CONTROL_BUTTON);
            DefaultCursor = Engine.Instance.Content.GetAsset<Cursor>(ASSET_CURSOR_LINK);
        }
        #endregion

        #region Properties & Accessors
        /// <summary>
        /// Gets or sets the sprite used to draw the control
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite Sprite { get; set; }
        /// <summary>
        /// Gets or sets the value used as a color modifier when the pointer is over the button
        /// </summary>
        public float OverColorPercent { get; set; }
        /// <summary>
        /// Gets or sets the value used as a color modifier when the pointer is over the button and a mouse button is down
        /// </summary>
        public float DownColorPercent { get; set; }
        /// <summary>
        /// Gets or sets the value used as a color modifier when the button is not enabled
        /// </summary>
        public float DisabledColorPercent { get; set; }
        /// <summary>
        /// Gets or sets the value that controls the size of the button nine split destination border
        /// </summary>
        public int DestBorderSize { get; set; }
        /// <summary>
        /// Gets or sets the value that controls the size of the button nine split source border
        /// </summary>
        public int SourceBorderSize { get; set; }
        /// <summary>
        /// Gets or sets the value which indicates whether the button sprite is to be drawn with nine cut according to the DestBorderSize and SourceBorderSize properties
        /// </summary>
        public bool NineCutDraw { get; set; }
        private bool mouseOver;
        private bool mouseDown;
        #endregion

        /// <summary>
        /// Initializes a new instance of the Button class with default settings and optional parent
        /// </summary>
        /// <param name="pos">Position of the Button</param>
        /// <param name="parent">Optional parent</param>
        public Button(Rectangle pos, Control parent = null)
            : base (pos, parent)
        {
            Initialize();
        }

        public Button(ButtonConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize();
            Sprite = DefaultSprite;
            OverColorPercent = config.OverColorPercent;
            DownColorPercent = config.DownColorPercent;
            DisabledColorPercent = config.DisabledColorPercent;
            DestBorderSize = config.DestBorderSize;
            SourceBorderSize = config.SourceBorderSize;
            NineCutDraw = config.NineCutDraw;
        }

        private void Initialize()
        {
            Cursor = DefaultCursor;
            Sprite = DefaultSprite;
            DisabledColorPercent = 0.5f;
            DownColorPercent = 0.4f;
            OverColorPercent = 0.6f;
            DestBorderSize = 15;
            SourceBorderSize = 15;
            NineCutDraw = true;

            MouseEnter += Button_MouseEnter;
            MouseLeave += Button_MouseLeave;
            MouseHover += Button_MouseHover;
            MouseMove += Button_MouseMove;
            MouseDown += Button_MouseDown;
            MouseUp += Button_MouseUp;
        }

        private void Button_MouseUp(object sender, MouseEventArgs e) => mouseDown = false;
        private void Button_MouseDown(object sender, MouseEventArgs e) => mouseDown = true;
        private void Button_MouseLeave(object sender, MouseEventArgs e) => mouseOver = false;
        private void Button_MouseEnter(object sender, MouseEventArgs e) => mouseOver = true;
        private void Button_MouseHover(object sender, MouseEventArgs e) => mouseOver = true;
        private void Button_MouseMove(object sender, MouseEventArgs e) => mouseOver = true;

        public override bool Update()
        {
            mouseDown = false;
            mouseOver = false;
            return base.Update();
        }

        /// <summary>
        /// Draws the Button
        /// </summary>
        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                Color color = Color;
                if (mouseOver)
                {
                    if (mouseDown)
                        color = Color.Multiply(color, DownColorPercent);
                    else
                        color = Color.Multiply(color, OverColorPercent);
                }

                if (!Enabled)
                    color = Color.Multiply(Color, DisabledColorPercent);

                if (Sprite != null)
                {
                    if (NineCutDraw)
                        Sprite.DrawNineCut(sb, AbsoluteBounds, null, color);
                    else
                        Sprite.Draw(sb, AbsoluteBounds, null, color);
                }

                if (Text != null && !string.IsNullOrWhiteSpace(Text.Text))
                {
                    Vector2 textMeasure = Text.Measure();
                    Vector2 textPos = new Vector2(AbsoluteLocation.X + Size.Width / 2 - textMeasure.X / 2, AbsoluteLocation.Y + Size.Height / 2 - textMeasure.Y / 2);

                    Text.Draw(sb, textPos);
                }
            }

            base.Draw(sb);
        }
    }
}
