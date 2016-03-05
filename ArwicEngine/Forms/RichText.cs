// Dominion - Copyright (C) Timothy Ings
// RichText.cs
// This file contains classes that define the rich text system

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ArwicEngine.Forms
{
    public enum FontSymbol
    {
        Anchor = 'a',
        Bolt = 'b',
        Bomb = 'c',
        Unavailable = 'd',
        BookClosed = 'e',
        Megaphone = 'f',
        Bug = 'g',
        Tick = 'h',
        Cog = 'i',
        Cogs = 'j',
        Paste = 'k',
        Clock = 'l',
        Crosshairs = 'm',
        ResizeArrows = 'n',
        ExclamationCircle = 'o',
        ExclamationTriangle = 'p',
        NewIcon = 'q',
        Flag = 'r',
        Flask = 's',
        SaveIcon = 't',
        OpenIcon = 'u',
        CopyIcon = 'v',
        BookOpen = 'w',
        Aeroplane = 'x',
        PowerIcon = 'y',
        Rocket = 'z',
        Refresh = 'A',
        MagnifyingGlassPlus = 'B',
        MagnifyingGlassMinus = 'C',
        MagnifyingGlass = 'D',
        SteamIcon = 'E',
        StarBlack = 'F',
        Star = 'G',
        Suitcase = 'H',
        Tag = 'I',
        ThumbsDown = 'J',
        ThumbsUp = 'K',
        CrossCircle = 'L',
        RubbishBin = 'M',
        Trophy = 'N',
        Lorry = 'O',
        TwitchTVIcon = 'P',
        //TwitterIcon = 'Q',
        UploadIcon = 'R',
        Users = 'S',
        VolumeLow = 'T',
        VolumeOff = 'U',
        VolumeHigh = 'V',
        Spanner = 'W',
        Diamond = 'X',
        Download = 'Y',
        PieChart = 'Z',
        MusicNote = '0',
        FaceStraight = '1',
        Shuffle = '2',
        ShareIcon = '3',
        FaceSad = '4',
        Person = '5',
        Cloud = '6',
        FacebookIcon = '7',
        PadlockLocked = '8',
        Microphone = '9',
        MicrophoneDisabled = '!',
        Apple = '\"',
        Scales = '#',
        BarChart = '$',
        Envelope = '%',
        Calender = '&',
        ArrowDown = '\'',
        ArrowLeft = '(',
        ArrowRight = ')',
        ArrowUp = '*',
        Gavel = '+',
        MortarBoard = ',',
        GlobeEarth = '-',
        Heart = '.',
        Hourglass3 = '/',
        Hourglass2 = ':',
        HourglassEmpty = ';',
        Hourglass1 = '<',
        FactoryIcon = '=',
        InformationIcon = '>',
        Pencil = '?',
        MinusIcon = '@',
        Plug = '[',
        PlusIcon = ']',
        RedditIcon = '^',
        FaceHappy = '_',
        Compass = '`',
        Shield = '{',
        WiFiSignalFull = '|',
        TrainIcon = '}',
        Reload = '~',
        WaterDrop = '\\',
        CourhouseIcon = (char)0x000,
        TwitterIcon = (char)0x001,
        PadlockUnlocked = (char)0x002,
        YoutubeIcon = (char)0x003,
        Coin = (char)0x004,
        UserAdd = (char)0x005,
        UserRemove = (char)0x006,
        Pause = (char)0x007,
        Play = (char)0x008,
        Stop = (char)0x009,
        Pulse = (char)0x00a,
        Footsteps = (char)0x00b,
        Crown = (char)0x00c,
        Lightbulb = (char)0x00d,
        //PieChart = (char)0x00e,
        //Heart = (char)0x00f,
        House = (char)0x010,
        //MortarBoard = (char)0x011,
        SignalBar0 = (char)0x012,
        SignalBar1 = (char)0x013,
        SignalBar2 = (char)0x014,
        SignalBar3 = (char)0x015,
        SignalBar4 = (char)0x016,
        ThumbTack = (char)0x017,
        FaceVeryHappy = (char)0x018,
        PreviousTrack = (char)0x019,
        NextTrack = (char)0x01a
    }

    public static class StringExtensions
    {
        public static RichText ToRichText(this string s, Color? color = null, Font font = null, List<RichTextParseRule> rules = null)
        {
            return RichText.ParseText(s, color, font, rules);
        }
    }

    [Serializable()]
    public class RichTextSection
    {
        public string Text { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Color Color { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Font Font { get { return _font; } set { _font = value; } }
        public float Scale { get; set; } = 1f;
        [NonSerialized]
        private Font _font;
        
        public RichTextSection(string text, Color? color = null, Font font = null, float scale = 1f)
        {
            if (font == null)
                font = Control.DefaultFont;
            if (color == null)
                color = Color.White;
            Text = text;
            Color = color.Value;
            Font = font;
            Scale = scale;
        }
        
        public void Draw(SpriteBatch sb, Vector2 pos)
        {
            Font.DrawString(sb, Text, pos, Color, Scale * 0.5f);
        }

        public Vector2 Measure()
        {
            return Font.MeasureString(Text);
        }
    }

    public class RichTextParseRule
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public Color Color { get; set; }
        public Font Font { get; set; }

        public RichTextParseRule(string key, string value, Color color, Font font)
        {
            Key = key;
            Value = value;
            Color = color;
            Font = font;
        }
    }

    [Serializable]
    public class RichText
    {
        public static Font SymbolFont { get; private set; }
        public static List<RichTextParseRule> RichTextRules { get; set; }

        public static void Init()
        {
            SymbolFont = Engine.Instance.Content.GetAsset<Font>(Constants.ASSET_FONT_SYMBOL);
        }

        [TypeConverter(typeof(CollectionConverter))]
        public List<RichTextSection> Sections { get; set; }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Sections = new List<RichTextSection>();
                Sections.Add(new RichTextSection(_text, Color.White, Control.DefaultFont));
            }
        }
        protected string _text;

        public RichText(params RichTextSection[] sections)
        {
            Sections = new List<RichTextSection>();
            Sections.AddRange(sections);
            _text = ToString();
        }

        public RichText(string text, Color? color = null, Font font = null)
        {
            _text = text;
            if (color == null)
                color = Color.White;
            Sections = new List<RichTextSection>();
            Sections.Add(new RichTextSection(text, color.Value, font));
        }

        public RichText()
        {
            Sections = new List<RichTextSection>();
        }

        public static RichText ParseText(string s, Color? generalColor = null, Font generalFont = null, List<RichTextParseRule> rules = null)
        {
            if (generalColor == null)
                generalColor = Color.White;
            if (generalFont == null)
                generalFont = Control.DefaultFont;
            if (rules == null)
                rules = RichTextRules;
            if (rules == null)
                    return s.ToRichText(generalColor, generalFont);

            RichText richText = new RichText();
            richText._text = s;
            string[] rawParts = s.Split('$', '<');
            List<string> finalParts = new List<string>();

            foreach (string part in rawParts)
            {
                if (part.Length < 1)
                    continue;
                if (part[0] == '(')
                {
                    // (key)
                    StringBuilder sb = new StringBuilder();
                    int index = 0;
                    while (part[index] != ')')
                        sb.Append(part[index++]);
                    // $key
                    finalParts.Add('$' + sb.ToString().Substring(1));
                    finalParts.Add(part.Substring(index + 1));
                }
                else if (part[0] == '[')
                {
                    // [r,b,g]>
                    // [r,b,g]
                    int endIndex = part.IndexOf('>');
                    finalParts.Add(part.Substring(0, endIndex));
                    finalParts.Add(part.Substring(endIndex + 1, part.Length - endIndex - 1));
                }
                else
                {
                    finalParts.Add(part);
                }
            }

            foreach (string part in finalParts)
            {
                if (part.Length < 1)
                    continue;
                if (part[0] == '$')
                {
                    foreach (RichTextParseRule rule in rules)
                    {
                        if (part.Substring(1) == rule.Key)
                        {
                            richText.Sections.Add(new RichTextSection(rule.Value, rule.Color, rule.Font));
                        }
                    }
                }
                else if (part[0] == '[')
                {
                    // <[255,255,255]This is coloured text!>
                    string sr = "";
                    int index = 1;
                    while (part[index] != ',')
                        sr += part[index++];
                    int r = Convert.ToInt32(sr);

                    string sb = "";
                    index++;
                    while (part[index] != ',')
                        sb += part[index++];
                    int b = Convert.ToInt32(sb);

                    string sg = "";
                    index++;
                    while (part[index] != ']')
                        sg += part[index++];
                    int g = Convert.ToInt32(sg);
                    richText.Sections.Add(new RichTextSection(part.Substring(index + 1), new Color(r, b, g), generalFont));
                }
                else
                {
                    richText.Sections.Add(new RichTextSection(part, generalColor, generalFont));
                }
            }

            return richText;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (RichTextSection section in Sections)
                sb.Append(section.Text);
            return sb.ToString();
        }

        public void Draw(SpriteBatch sb, Vector2 pos)
        {
            Vector2 lastPos = pos;
            foreach (RichTextSection section in Sections.ToArray())
            {
                section.Draw(sb, lastPos);
                lastPos.X += section.Font.MeasureString(section.Text).X;
            }
        }

        public Vector2 Measure()
        {
            Vector2 res = new Vector2();
            foreach (RichTextSection section in Sections)
            {
                Vector2 sectionMeasure = section.Measure();
                res.X += sectionMeasure.X;
                if (res.Y < sectionMeasure.Y)
                    res.Y = sectionMeasure.Y;
            }
            return res;
        }

        public static RichText operator +(RichText rt1, RichText rt2)
        {
            RichTextSection[] sections = new RichTextSection[rt1.Sections.Count + rt2.Sections.Count];
            int i = 0;
            foreach (RichTextSection section in rt1.Sections)
                sections[i++] = section;
            foreach (RichTextSection section in rt2.Sections)
                sections[i++] = section;
            return new RichText(sections);
        }

        public static RichText operator +(RichText rt, string s)
        {
            RichTextSection[] sections = new RichTextSection[rt.Sections.Count + 1];
            int i = 0;
            foreach (RichTextSection section in rt.Sections)
                sections[i++] = section;
            sections[i++] = new RichTextSection(s, Color.White);
            return new RichText(sections);
        }

        public static string operator +(string s, RichText rt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s);
            foreach (RichTextSection section in rt.Sections)
                sb.Append(section.Text);
            return sb.ToString();
        }
    }
}
