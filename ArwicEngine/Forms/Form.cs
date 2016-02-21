// Dominion - Copyright (C) Timothy Ings
// Form.cs
// This file contains classes that define a form

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class Form : Control
    {
        #region Defaults
        public static Sprite DefaultBackgroundSprite;
        public static Sprite DefaultCloseButtonSprite;

        public static new void InitDefaults()
        {
            DefaultBackgroundSprite = new Sprite(CONTROL_FORM_BACK);
            DefaultCloseButtonSprite = new Sprite(CONTROL_FORM_CLOSE);
        }
        #endregion

        #region Properties & Accessors
        /// <summary>
        /// Gets or sets a value that indicates whether the form can be dragged by its title bar
        /// </summary>
        public bool Draggable { get; set; }

        /// <summary>
        /// Gets or sets a sprite used to draw the form's background
        /// </summary>
        [Browsable(false)]
        public Sprite BackgroundSprite { get; set; }

        /// <summary>
        /// Gets or set a value indicating whether the form's close button is enabled
        /// </summary>
        public bool CloseButtonEnabled
        {
            get
            {
                return CloseButton.Visible;
            }
            set
            {
                CloseButton.Visible = value;
            }
        }

        /// <summary>
        /// Gets the form's close button
        /// </summary>
        [Browsable(false)]
        public Button CloseButton { get; private set; }

        /// <summary>
        /// Gets or sets the key that can be used to toggle the form's visibility
        /// </summary>
        public char HotKey { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the form has a title bar
        /// </summary>
        public bool DrawTitlebar { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the Control class with default settings
        /// </summary>
        /// <param name="pos">Position of the Control</param>
        public Form(Rectangle pos, Control parent = null)
            : base(pos, parent)
        {
            Initialize();
        }

        public Form(FormConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize();
            Draggable = config.Draggable;
            CloseButtonEnabled = config.CloseButtonEnabled;
            HotKey = config.HotKey;
            DrawTitlebar = config.DrawTitlebar;
        }

        private void Initialize()
        {
            BackgroundSprite = DefaultBackgroundSprite;
            HotKey = '\0';
            Draggable = true;
            CloseButton = new Button(new Rectangle(Size.Width - FORM_CLOSEBUTTON_DIM - FORM_CLOSEBUTTON_PADDING, FORM_CLOSEBUTTON_PADDING, FORM_CLOSEBUTTON_DIM, FORM_CLOSEBUTTON_DIM), this);
            CloseButtonEnabled = true;
            CloseButton.Color = Color;
            CloseButton.NineCutDraw = false;
            CloseButton.Sprite = DefaultCloseButtonSprite;
            DrawTitlebar = true;

            EventInput.CharEntered += EventInput_CharEntered;
            CloseButton.MouseClick += CloseButton_MouseClick;
            CloseButton.MouseUp += CloseButton_MouseUp;
            SizeChanged += Form_SizeChanged;
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            if (CloseButton != null)
            {
                CloseButton.Location = new Point(Size.Width - FORM_CLOSEBUTTON_DIM - FORM_CLOSEBUTTON_PADDING, FORM_CLOSEBUTTON_PADDING);
            }
        }

        private void CloseButton_MouseUp(object sender, MouseEventArgs e)
        {
            Visible = false;
        }
        private void EventInput_CharEntered(object sender, CharacterEventArgs e)
        {
            if (e.Character == HotKey)
                Visible = !Visible;
        }
        private void CloseButton_MouseClick(object sender, MouseEventArgs e)
        {
            //if (e.Left)
                Visible = false;
        }

        public void RemoveAllControls()
        {
            foreach (Control child in Children)
                RemoveChild(child);
            AddChild(CloseButton);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                BackgroundSprite.DrawNineCut(sb, AbsoluteBounds, new Rectangle(0, 0, BackgroundSprite.Width, BackgroundSprite.Height), Color);

                if (DrawTitlebar)
                {
                    Rectangle titleBar = new Rectangle(AbsoluteLocation.X, AbsoluteLocation.Y, Size.Width, FORM_CLOSEBUTTON_DIM + FORM_CLOSEBUTTON_PADDING * 2);
                    BackgroundSprite.DrawNineCut(sb, titleBar, null, Color.Multiply(Color, 0.8f));
                }

                Vector2 measureText = Text.Measure();
                Text.Draw(sb, new Vector2(AbsoluteLocation.X + Size.Width / 2 - measureText.X / 2, AbsoluteLocation.Y + measureText.Y / 2));
            }
            base.Draw(sb);
            if (Visible)
            {
                foreach (Control child in ChildrenAndSubChildren)
                {
                    if (child is ToolTip)
                        child.Draw(sb);
                }
            }
        }
    }
}
