using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace ArwicEngine.Graphics
{
    public class Font
    {
        private const char DEFAULT_CHAR = '*';
        private const float FONT_SCALE = 0.5f;

        private SpriteFont spriteFont;

        public Font(ContentManager cm, string path, char defaultChar = DEFAULT_CHAR)
        {
            spriteFont = cm.Load<SpriteFont>(path);
            spriteFont.DefaultCharacter = defaultChar;
        }

        public void DrawString(SpriteBatch sb, string text, Vector2 pos, Color? col = null, float scale = FONT_SCALE)
        {
            if (col == null) col = Color.White;
            if (text == null)
                return;
            sb.DrawString(spriteFont, text, pos, col.Value, 0f, new Vector2(0,0), scale, SpriteEffects.None, 1f);
        }

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
            List<string> lines = new List<string>();

            float stringLen = spriteFont.MeasureString(s).X;
            float numLines = stringLen / width;

            if (numLines < 1f)
            {
                lines.Add(s);
                return lines;
            }

            float avgCharWidth = stringLen / s.Length;
            float charPerLine = width / avgCharWidth;

            for (int i = 0; i < numLines; i++)
            {
                string line = "";
                for (int j = 0; j < charPerLine - 1; j++)
                {
                    int index = j + Convert.ToInt32(Math.Floor(charPerLine)) * i;
                    if (index < s.Length)
                        line += s[index];
                }
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>
        /// Wraps the given string into a number of lines containing words that are not longer than the given width
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
