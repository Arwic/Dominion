// Dominion - Copyright (C) Timothy Ings
// TextLog.cs
// This file contains classes that define a text log

using ArwicEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArwicEngine.Forms
{
    public class TextLog : Control
    {
        private object _linesModifyLock = new object();

        #region Properties & Accessors
        /// <summary>
        /// Gets the lines of text in the text log
        /// </summary>
        [Browsable(false)]
        public RichText[] Lines
        {
            get
            {
                lock (_linesModifyLock)
                {
                    return _lines.ToArray();
                }
            }
        }
        private List<RichText> _lines;
        /// <summary>
        /// Gets or sets the number of lines to draw, a value of -1 will draw as many as Size.Height allows
        /// </summary>
        public int LinesToDraw { get; set; }
        /// <summary>
        /// Gets or sets the number of lines to keep, oldest lines are removed first
        /// </summary>
        public int LinesToKeep
        {
            get
            {
                return _linesToKeep;
            }
            set
            {
                _linesToKeep = value;
                TrimLines();
            }
        }
        private int _linesToKeep;
        /// <summary>
        /// Gets or sets the spacing between each line in pixels
        /// </summary>
        public int LineSpacing { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a line is appended to, Write()
        /// </summary>
        public event EventHandler<TextLogLineEventArgs> LineAppended;
        /// <summary>
        /// Occurs when a new line is added, WriteLine()
        /// </summary>
        public event EventHandler<TextLogLineEventArgs> LineAdded;
        /// <summary>
        /// Occurs when a line is removed as it is causing Lines to be longer than LinesToKeep
        /// </summary>
        public event EventHandler<TextLogLineEventArgs> LineRemoved;
        #endregion

        #region Event Handlers
        protected virtual void OnLineAppended(TextLogLineEventArgs args)
        {
            if (LineAppended != null)
                LineAppended(this, args);
        }
        protected virtual void OnLineAdded(TextLogLineEventArgs args)
        {
            if (LineAdded != null)
                LineAdded(this, args);
        }
        protected virtual void OnLineRemoved(TextLogLineEventArgs args)
        {
            if (LineRemoved != null)
                LineRemoved(this, args);
        }
        #endregion

        public TextLog(Rectangle pos, Control parent = null)
            : base (pos, parent)
        {
            Initialize();
        }

        public TextLog(TextLogConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize();
            LinesToDraw = config.LinesToDraw;
            LinesToKeep = config.LinesToKeep;
            LineSpacing = config.LineSpacing;
        }

        private void Initialize()
        {
            LineSpacing = 2;
            _lines = new List<RichText>();
            LinesToKeep = int.MaxValue;
            LinesToDraw = -1;
            WriteLine(new RichText(new RichTextSection("", Color)));
        }

        private void TrimLines()
        {
            while (Lines.Length > LinesToKeep)
                RemoveLine(0);
        }

        /// <summary>
        /// Removes a line from the text log at the given index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveLine(int index)
        {
            lock (_linesModifyLock)
            {
                if (index > 0 && index < Lines.Length)
                {
                    RichText line = _lines?[0];
                    _lines.RemoveAt(index);
                    OnLineRemoved(new TextLogLineEventArgs(line));
                }
            }
        }

        /// <summary>
        /// Writes to the current line
        /// </summary>
        /// <param name="rts">string to write</param>
        public void Write(RichTextSection rts)
        {
            lock (_linesModifyLock)
            {
                if (Lines.Length != 0)
                {
                    Lines[Lines.Length - 1].Sections.Add(rts);
                }
                else
                    WriteLine(new RichText(rts));
            }
        }

        /// <summary>
        /// Writes to a new line
        /// </summary>
        /// <param name="s">string to write</param>
        public void WriteLine(RichText line)
        {
            lock (_linesModifyLock)
            {
                _lines.Add(line);
                OnLineAdded(new TextLogLineEventArgs(line));
                TrimLines();
            }
        }

        /// <summary>
        /// Draws the text log
        /// </summary>
        /// <param name="sb"></param>
        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                // if in dynamic mode (linesToDraw = -1), calculate lines to draw based on font size and transform
                int linesToDraw = LinesToDraw;
                float lineHeight = Font.MeasureString("|").Y;
                if (linesToDraw == -1)
                    linesToDraw = Convert.ToInt32(Math.Floor(Bounds.Height / (lineHeight + LineSpacing)));

                for (int i = 0; i < linesToDraw; i++)
                {
                    int index = Lines.Length - 1 - i;
                    if (index < Lines.Length && index >= 0)
                        Lines[index].Draw(sb, new Vector2(AbsoluteLocation.X, AbsoluteLocation.Y + (linesToDraw - i - 1) * (lineHeight + LineSpacing)));
                    else
                        break;
                }
            }
            base.Draw(sb);
        }
    }
}
