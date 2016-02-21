// Dominion - Copyright (C) Timothy Ings
// Font.cs
// This file contains wrapper classes for xna sprite fonts

using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ArwicEngine.Graphics
{
    public class Font
    {
        private const char DEFAULT_CHAR = '*';
        private const float FONT_SCALE = 0.5f;

        private SpriteFont spriteFont;

        /// <summary>
        /// Create a new font
        /// </summary>
        /// <param name="cm"></param>
        /// <param name="path"></param>
        /// <param name="defaultChar"></param>
        public Font(string path, char defaultChar = DEFAULT_CHAR)
        {
            spriteFont = Engine.Instance.Content.Load<SpriteFont>(path);
            spriteFont.DefaultCharacter = defaultChar;
        }

        /// <summary>
        /// Draws the given string with in the given font
        /// </summary>
        /// <param name="sb">sprite batch to draw to</param>
        /// <param name="text">text to draw</param>
        /// <param name="pos">position to draw to</param>
        /// <param name="col">tint to be applied, defaults to white</param>
        /// <param name="scale">scale to be applied</param>
        public void DrawString(SpriteBatch sb, string text, Vector2 pos, Color? col = null, float scale = FONT_SCALE)
        {
            if (col == null) col = Color.White; // default to white is no colour provided
            if (text == null) // don't try and draw nothing
                return;
            sb.DrawString(spriteFont, text, pos, col.Value, 0f, new Vector2(0,0), scale, SpriteEffects.None, 1f);
        }

        /// <summary>
        /// Measures the size of the given string in the given font
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Vector2 MeasureString(string text)
        {
            return spriteFont.MeasureString(text) * FONT_SCALE;
        }

        /// <summary>
        /// Wraps the given string into a number of lines that are not longer than the given width
        /// </summary>
        /// <param name="s">the string of words to wrap</param>
        /// <param name="width">the max width</param>
        /// <returns></returns>
        public List<string> Wrap(string s, float width)
        {
            // lines to return
            List<string> lines = new List<string>();

            // calc length of the string
            float stringLen = spriteFont.MeasureString(s).X;
            // calc the number of lines required if we are to split string up and have every line below the given width
            float numLines = stringLen / width;

            // if the number of lines required is less than one, the wrpa is already done
            if (numLines < 1f)
            {
                lines.Add(s);
                return lines;
            }

            // calc the avg width of a character in the string
            float avgCharWidth = stringLen / s.Length;
            // calc the number of characters there will be per line, based on the average char width
            float charPerLine = width / avgCharWidth;

            // set up the lines list with foramtted lines
            for (int i = 0; i < numLines; i++)
            {
                string line = "";
                // add the next charPerLine chars to the current line
                for (int j = 0; j < charPerLine - 1; j++)
                {
                    int index = j + Convert.ToInt32(Math.Floor(charPerLine)) * i;
                    if (index < s.Length)
                        line += s[index];
                }
                // add the line to the list of all lines
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>
        /// Wraps the given string into a number of lines containing words that are not longer than the given width
        /// Words are defined as strings of characters seperated by a space ' '
        /// </summary>
        /// <param name="s">the string of words to wrap</param>
        /// <param name="width">the max width</param>
        /// <returns></returns>
        public List<string> WordWrap(string s, float width)
        {
            List<string> lines = new List<string>();
            List<string> words = new List<string>();

            // separate words
            int startIndex = 0;
            for (int ci = 0; ci < s.Length; ci++)
            {
                if (s[ci] == ' ')
                {
                    words.Add(s.Substring(startIndex, ci - startIndex));
                    startIndex = ci;
                }
            }
            words.Add(s.Substring(startIndex, s.Length - startIndex));

            // compose lines that are less than the desired width
            lines.Add("");
            int lineIndex = 0;
            for (int wi = 0; wi < words.Count; wi++)
            {
                float lineWidth = MeasureString(lines[lineIndex]).X;
                float wordWidth = MeasureString($" {words[wi]}").X;
                if (lineWidth + wordWidth < width)
                {
                    lines[lineIndex] = $"{lines[lineIndex]} {words[wi]}";
                }
                else
                {
                    lines.Add(words[wi]);
                    lineIndex++;
                }
            }

            return lines;
        }
    }
}
