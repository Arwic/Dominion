using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ArwicEngine.Forms
{
    public class Label : Control
    {
        /// <summary>
        /// Initializes a new instance of the Label class with default settings and optional parent
        /// </summary>
        /// <param name="pos">Position of the Label</param>
        /// <param name="text">Text to be drawn</param>
        /// <param name="parent">Optional parent</param>
        public Label(Rectangle pos, RichText text, Control parent = null)
            : base(pos, parent)
        {
            TextChanged += Label_TextChanged;
            Text = text;
        }

        public Label(ControlConfig config, Control parent = null)
            : base(config, parent)
        {
            TextChanged += Label_TextChanged;
        }

        private void Label_TextChanged(object sender, EventArgs e)
        {
            Bounds = new Rectangle(Location, Text.Measure().ToPoint());
        }

        /// <summary>
        /// Draws the Label
        /// </summary>
        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                Text.Draw(sb, AbsoluteLocation.ToVector2());
            }
            base.Draw(sb);
        }
    }
}
