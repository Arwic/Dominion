using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace ArwicEngine.Forms
{
    [XmlInclude(typeof(FormConfig))]
    [XmlInclude(typeof(ButtonConfig))]
    [XmlInclude(typeof(CheckBoxConfig))]
    [XmlInclude(typeof(ComboBoxConfig))]
    [XmlInclude(typeof(ImageConfig))]
    [XmlInclude(typeof(ProgressBarConfig))]
    [XmlInclude(typeof(ScrollBoxConfig))]
    [XmlInclude(typeof(TextBoxConfig))]
    [XmlInclude(typeof(TextLogConfig))]
    public class ControlConfig
    {
        [XmlElement("TypeName")]
        public string TypeName { get; set; }
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Text")]
        public string Text { get; set; }
        [XmlElement("Color")]
        public Color Color { get; set; }
        [XmlArray("Children"), XmlArrayItem(typeof(ControlConfig), ElementName = "Child")]
        public List<ControlConfig> Children { get; set; }
        [XmlElement("LocationX")]
        public int LocationX { get; set; }
        [XmlElement("LocationY")]
        public int LocationY { get; set; }
        [XmlElement("LocationWidth")]
        public int SizeWidth { get; set; }
        [XmlElement("LocationHeight")]
        public int SizeHeight { get; set; }

        public ControlConfig() { }

        public ControlConfig(Control control)
        {
            TypeName = control.GetType().ToString();
            Name = control.Name;
            Text = control.Text.Text;
            Color = control.Color;
            LocationX = control.Location.X;
            LocationY = control.Location.Y;
            SizeWidth = control.Size.Width;
            SizeHeight = control.Size.Height;
            Children = new List<ControlConfig>();
            foreach (Control child in control.Children)
            {
                if (child is Form)
                    Children.Add(new FormConfig((Form)child));
                else if (child is Button)
                    Children.Add(new ButtonConfig((Button)child));
                else if (child is CheckBox)
                    Children.Add(new CheckBoxConfig((CheckBox)child));
                else if (child is ComboBox)
                    Children.Add(new ComboBoxConfig((ComboBox)child));
                else if (child is Image)
                    Children.Add(new ImageConfig((Image)child));
                else if (child is ProgressBar)
                    Children.Add(new ProgressBarConfig((ProgressBar)child));
                else if (child is ScrollBox)
                    Children.Add(new ScrollBoxConfig((ScrollBox)child));
                else if (child is TextBox)
                    Children.Add(new TextBoxConfig((TextBox)child));
                else if (child is TextLog)
                    Children.Add(new TextLogConfig((TextLog)child));
                else
                    Children.Add(new ControlConfig(child));
            }
        }
    }

    public class FormConfig : ControlConfig
    {
        [XmlElement("Draggable")]
        public bool Draggable { get; set; }
        [XmlElement("CloseButtonEnabled")]
        public bool CloseButtonEnabled { get; set; }
        [XmlElement("HotKey")]
        public char HotKey { get; set; }
        [XmlElement("DrawTitlebar")]
        public bool DrawTitlebar { get; set; }

        public FormConfig() : base() { }

        public FormConfig(Form form)
            : base(form)
        {
            Draggable = form.Draggable;
            CloseButtonEnabled = form.CloseButtonEnabled;
            HotKey = form.HotKey;
            DrawTitlebar = form.DrawTitlebar;
        }

        public static FormConfig FromFile(string path)
        {
            return SerializationHelper.XmlDeserialize<FormConfig>(path);
        }
    }

    public class ButtonConfig : ControlConfig
    {
        [XmlElement("SpritePath")]
        public string SpritePath { get; set; }
        [XmlElement("OverColorPercent")]
        public float OverColorPercent { get; set; }
        [XmlElement("DownColorPercent")]
        public float DownColorPercent { get; set; }
        [XmlElement("DisabledColorPercent")]
        public float DisabledColorPercent { get; set; }
        [XmlElement("DestBorderSize")]
        public int DestBorderSize { get; set; }
        [XmlElement("SourceBorderSize")]
        public int SourceBorderSize { get; set; }
        [XmlElement("NineCutDraw")]
        public bool NineCutDraw { get; set; }

        public ButtonConfig() : base() { }

        public ButtonConfig(Button button)
            : base(button)
        {
            SpritePath = button.Sprite.Path;
            OverColorPercent = button.OverColorPercent;
            DownColorPercent = button.DownColorPercent;
            DisabledColorPercent = button.DisabledColorPercent;
            DestBorderSize = button.DestBorderSize;
            SourceBorderSize = button.SourceBorderSize;
            NineCutDraw = button.NineCutDraw;
        }
    }

    public class CheckBoxConfig : ControlConfig
    {
        [XmlElement("SpriteTruePath")]
        public string SpriteTruePath { get; set; }
        [XmlElement("SpriteFalsePath")]
        public string SpriteFalsePath { get; set; }
        [XmlElement("DisabledColorPercent")]
        public float DisabledColorPercent { get; set; }
        [XmlElement("Value")]
        public bool Value { get; set; }

        public CheckBoxConfig() : base() { }

        public CheckBoxConfig(CheckBox checkBox)
            : base(checkBox)
        {
            SpriteTruePath = checkBox.SpriteTrue.Path;
            SpriteFalsePath = checkBox.SpriteFalse.Path;
            DisabledColorPercent = checkBox.DisabledColorPercent;
            Value = checkBox.Value;
        }
    }

    public class ComboBoxConfig : ControlConfig
    {
        [XmlElement("HeadButtonSpritePath")]
        public string HeadButtonSpritePath { get; set; }
        [XmlElement("ListButtonSpritePath")]
        public string ListButtonSpritePath { get; set; }
        
        public ComboBoxConfig() : base() { }

        public ComboBoxConfig(ComboBox comboBox)
            : base(comboBox)
        {
            HeadButtonSpritePath = comboBox.HeadButtonSprite.Path;
            ListButtonSpritePath = comboBox.ListButtonSprite.Path;
        }
    }

    public class ImageConfig : ControlConfig
    {
        [XmlElement("SpritePath")]
        public string SpritePath { get; set; }
        [XmlElement("Source")]
        public Rectangle Source { get; set; }
        [XmlElement("Rotation")]
        public float Rotation { get; set; }
        [XmlElement("Origin")]
        public Point Origin { get; set; }

        public ImageConfig() : base() { }

        public ImageConfig(Image image)
            : base(image)
        {
            SpritePath = image.Sprite.Path;
            Source = image.Source;
            Rotation = image.Rotation;
            Origin = image.Origin;
        }
    }

    public class ProgressBarConfig : ControlConfig
    {
        [XmlElement("BarSpritePath")]
        public string BarSpritePath { get; set; }
        [XmlElement("BackSpritePath")]
        public string BackSpritePath { get; set; }
        [XmlElement("Maximum")]
        public float Maximum { get; set; }
        [XmlElement("Value")]
        public float Value { get; set; }

        public ProgressBarConfig() : base() { }

        public ProgressBarConfig(ProgressBar progressBar)
            : base(progressBar)
        {
            BarSpritePath = progressBar.BarSprite.Path;
            BackSpritePath = progressBar.BackSprite.Path;
            Maximum = progressBar.Maximum;
            Value = progressBar.Value;
        }
    }

    public class ScrollBoxConfig : ControlConfig
    {
        [XmlElement("BackgroundSpritePath")]
        public string BackgroundSpritePath { get; set; }
        [XmlElement("SelectedItemSpritePath")]
        public string SelectedItemSpritePath { get; set; }
        [XmlElement("ListItemSpritePath")]
        public string ListItemSpritePath { get; set; }
        [XmlElement("ScrubberSpritePath")]
        public string ScrubberSpritePath { get; set; }
        [XmlElement("ScrollBarWidth")]
        public int ScrollBarWidth { get; set; }
        [XmlElement("ItemHeight")]
        public int ItemHeight { get; set; }

        public ScrollBoxConfig() : base() { }

        public ScrollBoxConfig(ScrollBox scrollBox)
            : base(scrollBox)
        {
            BackgroundSpritePath = scrollBox.BackgroundSprite.Path;
            SelectedItemSpritePath = scrollBox.SelectedItemSprite.Path;
            ListItemSpritePath = scrollBox.ListItemSprite.Path;
            ScrubberSpritePath = scrollBox.ScrubberSprite.Path;
            ScrollBarWidth = scrollBox.ScrollBarWidth;
            ItemHeight = scrollBox.ItemHeight;
        }
    }

    public class TextBoxConfig : ControlConfig
    {
        [XmlElement("Masked")]
        public bool Masked { get; set; }
        [XmlElement("MaskedCharacter")]
        public char MaskedCharacter { get; set; }
        [XmlElement("MaxCharacters")]
        public int MaxCharacters { get; set; }
        [XmlElement("SpritePath")]
        public string SpritePath { get; set; }
        [XmlElement("TextOffset")]
        public Point TextOffset { get; set; }
        [XmlElement("CaretBlinkTimer")]
        public float CaretBlinkTimer { get; set; }

        public TextBoxConfig() : base() { }

        public TextBoxConfig(TextBox textBox)
            : base(textBox)
        {
            Masked = textBox.Masked;
            MaskedCharacter = textBox.MaskedCharacter;
            MaxCharacters = textBox.MaxCharacters;
            SpritePath = textBox.Sprite.Path;
            TextOffset = textBox.TextOffset;
            CaretBlinkTimer = textBox.CaretBlinkTimer.Length;
        }
    }

    public class TextLogConfig : ControlConfig
    {
        [XmlElement("LinesToDraw")]
        public int LinesToDraw { get; set; }
        [XmlElement("LinesToKeep")]
        public int LinesToKeep { get; set; }
        [XmlElement("LineSpacing")]
        public int LineSpacing { get; set; }

        public TextLogConfig() : base() { }

        public TextLogConfig(TextLog textLog)
            : base(textLog)
        {
            LinesToDraw = textLog.LinesToDraw;
            LinesToKeep = textLog.LinesToKeep;
            LineSpacing = textLog.LineSpacing;
        }
    }
}
