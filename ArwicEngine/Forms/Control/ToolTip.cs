// Dominion - Copyright (C) Timothy Ings
// ToolTip.cs
// This file contains classes that define a tool tip

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class ToolTip : Control
    {
        #region Defaults
        public static Sprite DefaultSprite;
        private static Viewport viewport;

        public static new void InitDefaults(Engine e)
        {
            viewport = e.Graphics.Viewport;
            DefaultSprite = new Sprite(e.Content, CONTROL_FORM_BACK);
        }
        #endregion

        #region Properties & Accessors
        /// <summary>
        /// Gets or sets the text associated with this control
        /// </summary>
        public new string Text
        {
            get
            {
                return _text;
            }
            set
            {
                string last = _text;
                _text = value;
                if (last != _text)
                    OnTextChanged(EventArgs.Empty);
            }
        }
        private string _text;
        /// <summary>
        /// Gets or sets a value that indicate whether the tooltip follows the cursor
        /// </summary>
        public bool FollowCurosr
        {
            get
            {
                return _followCursor;
            }
            set
            {
                bool last = _followCursor;
                _followCursor = value;
                if (last != _followCursor)
                    OnFollowCursorChanged(EventArgs.Empty);
            }
        }
        private bool _followCursor;
        /// <summary>
        /// Gets or sets the value that is used as an offset when rendering the tooltip at the cursor
        /// </summary>
        public Point CursorOffset { get; set; }
        /// <summary>
        /// Gets or sets the sprite used to draw the background of the tooltip
        /// </summary>
        public Sprite Sprite { get; set; }
        private string[] wrappedText;
        private int padding;
        private int lineSpacing;
        private int lineHeight;
        #endregion

        #region Events
        public event EventHandler FollowCursorChanged;
        #endregion

        #region Event Handlers
        protected virtual void OnFollowCursorChanged(EventArgs args)
        {
            if (FollowCursorChanged != null)
                FollowCursorChanged(this, args);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the ToolTip class with default settings
        /// </summary>
        /// <param name="pos">Position of the Button</param>
        /// <param name="parent">required parent</param>
        public ToolTip(string text, int width)
            : base(new Rectangle(0, 0, width, 0), null)
        {
            Sprite = DefaultSprite;
            Visible = false;
            padding = 10;
            lineSpacing = 5;
            FollowCurosr = true;
            CursorOffset = new Point(20, 20);
            MouseMove += ToolTip_MouseMove;
            TextChanged += ToolTip_TextChanged;
            FontChanged += ToolTip_FontChanged;
            Font = DefaultFont;
            Text = text;
        }
        
        private void ToolTip_FontChanged(object sender, EventArgs e)
        {
            UpdateLines();
        }
        private void ToolTip_TextChanged(object sender, EventArgs e)
        {
            UpdateLines();
        }
        private void ToolTip_MouseMove(object sender, MouseEventArgs e)
        {
            if (Parent.Visible && Parent.AbsoluteBounds.Contains(e.Position))
            {
                Visible = true;
                if (FollowCurosr)
                    AbsoluteLocation = e.Position + CursorOffset;
            }
            else
            {
                Visible = false;
            }
        }

        private void UpdateLines()
        {
            wrappedText = Font.WordWrap(Text, Size.Width - padding * 2).ToArray();
            foreach (string s in wrappedText)
            {
                int height = (int)Font.MeasureString(s).Y;
                if (height > lineHeight)
                    lineHeight = height;
            }
        }

        public override bool Update(InputManager input)
        {
            if (Parent != null)
            {
                Visible = Parent.AbsoluteBounds.Contains(input.MouseScreenPos());

                if (Visible)
                {
                    if (FollowCurosr)
                    {
                        AbsoluteBounds = new Rectangle(
                            CursorOffset.X + input.MouseScreenPos().X,
                            CursorOffset.Y + input.MouseScreenPos().Y,
                            Size.Width,
                            (2 * padding + (lineSpacing + lineHeight) * wrappedText.Length - 1));
                    }
                    else
                    {
                        Bounds = new Rectangle(
                            Location.X,
                            Location.Y,
                            Size.Width,
                            (2 * padding + (lineSpacing + lineHeight) * wrappedText.Length - 1));
                    }
                }
            }

            return base.Update(input);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                if (AbsoluteLocation.X + Size.Width > viewport.Width)
                {
                    AbsoluteLocation = new Point(AbsoluteLocation.X - Size.Width, AbsoluteLocation.Y);
                }
                Sprite.DrawNineCut(sb, AbsoluteBounds, null, Color);
                for (int i = 0; i < wrappedText.Length; i++)
                {
                    Font.DrawString(sb, wrappedText[i], AbsoluteLocation.ToVector2() + new Vector2(padding, padding + (lineSpacing + lineHeight) * i));
                }
            }
            base.Draw(sb);
        }
    }
}
