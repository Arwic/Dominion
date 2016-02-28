// Dominion - Copyright (C) Timothy Ings
// Empire.cs
// This file defines classes that define an empire

using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    [Flags]
    public enum EmpireStartBiasFlags
    {
        None = 0,
        Coast = 1,
        Desert = 2,
        Jungle = 4,
        Forest = 8,
        Grassland = 16,
        Tundra = 32,
        Plains = 64,
        Hill = 128,
        Avoid_Forest = 256,
        Avoid_Jungle = 512,
        Avoid_ForestAndJungle = 1024,
        Avoid_Hill = 2048,
        Avoid_Tundra = 4096,
    }

    public enum EmpireArtSet
    {
        European,
        Mediterranean,
        MiddleEastern,
        Asian,
        African,
        American
    }

    public enum EmpireArchitecture
    {
        European,
        Mediterranean,
        MiddleEastern,
        Asian,
        African,
        American
    }

    public enum EmpirePreferredReligion
    {
        Buddhism,
        Catholicism,
        EasternOrthodoxy,
        Hinduism,
        Islam,
        Judaism,
        Protestantism,
        Shinto,
        Sikhism,
        Taoism,
        Tengriism,
        Zoroastrianism
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    public class Empire
    {
        /// <summary>
        /// The name of the empire
        /// </summary>
        [Description("The name of the empire")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; } = "EMPIRE_NULL";

        /// <summary>
        /// Adjective used to describe items owned by this empire
        /// </summary>
        [Description("Adjective used to describe items owned by this empire")]
        [DisplayName("Adjective"), Browsable(true), Category("General")]
        [XmlElement("Adjective")]
        public string Adjective { get; set; } = "adj";

        /// <summary>
        /// A list of names to use for this empire's cities
        /// </summary>
        [Description("A list of names to use for this empire's cities")]
        [DisplayName("Default City Names"), Browsable(true), Category("General")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("DefaultCityNames"), XmlArrayItem(typeof(string), ElementName = "Name")]
        public List<string> DefaultCityNames { get; set; } = new List<string>();

        /// <summary>
        /// A list of names to use for this empire's spies
        /// </summary>
        [Description("A list of names to use for this empire's spies")]
        [DisplayName("Spy Names"), Browsable(true), Category("General")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("SpyNames"), XmlArrayItem(typeof(string), ElementName = "Name")]
        public List<string> SpyNames { get; set; } = new List<string>();

        /// <summary>
        /// The style of architecture used to render this empire's cities and units
        /// </summary>
        [Description("The style of architecture used to render this empire's cities and units")]
        [DisplayName("Architecture"), Browsable(true), Category("Graphics")]
        [TypeConverter(typeof(EnumConverter))]
        [XmlElement("Architecture")]
        public EmpireArchitecture Architecture { get; set; } = EmpireArchitecture.European;

        /// <summary>
        /// The set of art that the great artists this emprie generates will be from
        /// </summary>
        [Description("The set of art that the great artists this emprie generates will be from")]
        [DisplayName("Art Set"), Browsable(true), Category("Graphics")]
        [TypeConverter(typeof(EnumConverter))]
        [XmlElement("ArtSet")]
        public EmpireArtSet ArtSet { get; set; } = EmpireArtSet.European;

        /// <summary>
        /// The empire's preferred religion
        /// </summary>
        [Description("The empire's preferred religion")]
        [DisplayName("Preferred Religion"), Browsable(true), Category("Game")]
        [TypeConverter(typeof(EnumConverter))]
        [XmlElement("PreferredReligion")]
        public EmpirePreferredReligion PreferredReligion { get; set; } = EmpirePreferredReligion.Protestantism;

        /// <summary>
        /// The terrain that this empire is biased to start near
        /// </summary>
        [Description("The terrain that this empire is biased to start near")]
        [DisplayName("Start Bias"), Browsable(true), Category("Game")]
        [TypeConverter(typeof(EnumConverter))]
        [XmlElement("Start Bias")]
        public EmpireStartBiasFlags StartBias { get; set; } = EmpireStartBiasFlags.None;

        /// <summary>
        /// The empire's primary color
        /// </summary>
        [Description("The empire's primary color")]
        [DisplayName("Primary"), Browsable(true), Category("Color")]
        [Editor(typeof(ColorEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ColorConverter))]
        public Color PrimaryColor
        {
            get
            {
                return new Color(_primaryColor_R, _primaryColor_G, _primaryColor_B, _primaryColor_A);
            }
            set
            {
                _primaryColor_A = value.A;
                _primaryColor_R = value.R;
                _primaryColor_G = value.G;
                _primaryColor_B = value.B;
            }
        }
        [XmlIgnore]
        private int _primaryColor_A;
        [XmlIgnore]
        private int _primaryColor_R;
        [XmlIgnore]
        private int _primaryColor_G;
        [XmlIgnore]
        private int _primaryColor_B;

        /// <summary>
        /// The empire's secondary color
        /// </summary>
        [Description("The empire's secondary color")]
        [DisplayName("Secondary"), Browsable(true), Category("Color")]
        [Editor(typeof(ColorEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ColorConverter))]
        public Color SecondaryColor
        {
            get
            {
                return new Color(_secondaryColor_R, _secondaryColor_G, _secondaryColor_B, _secondaryColor_A);
            }
            set
            {
                _secondaryColor_A = value.A;
                _secondaryColor_R = value.R;
                _secondaryColor_G = value.G;
                _secondaryColor_B = value.B;
            }
        }
        [XmlIgnore]
        private int _secondaryColor_A;
        [XmlIgnore]
        private int _secondaryColor_R;
        [XmlIgnore]
        private int _secondaryColor_G;
        [XmlIgnore]
        private int _secondaryColor_B;

        /// <summary>
        /// The empire's text color
        /// </summary>
        [Description("The empire's text color")]
        [DisplayName("Text"), Browsable(true), Category("Color")]
        [Editor(typeof(ColorEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ColorConverter))]
        public Color TextColor
        {
            get
            {
                return new Color(_textColor_R, _textColor_G, _textColor_B, _textColor_A);
            }
            set
            {
                _textColor_A = value.A;
                _textColor_R = value.R;
                _textColor_G = value.G;
                _textColor_B = value.B;
            }
        }
        [XmlIgnore]
        private int _textColor_A;
        [XmlIgnore]
        private int _textColor_R;
        [XmlIgnore]
        private int _textColor_G;
        [XmlIgnore]
        private int _textColor_B;

        /// <summary>
        /// Keeps track of the last used default city name
        /// </summary>
        [Browsable(false)]
        [XmlIgnore()]
        public int DefaultCityNameIndex { get; set; } = 0;

        /// <summary>
        /// The key to used to get the icon from the icon atlas
        /// </summary>
        [Description("The key to used to get the icon from the icon atlas")]
        [DisplayName("Icon Key"), Browsable(true), Category("Graphics")]
        [XmlElement("IconKey")]
        public string IconKey { get; set; } = "EMPIRE_NULL";

        /// <summary>
        /// The icon atlas to source the empire's icon from
        /// </summary>
        [Description("The icon atlas to source the empire's icon from")]
        [DisplayName("Icon Atlas"), Browsable(true), Category("Graphics")]
        [XmlElement("IconAtlas")]
        public string IconAtlas { get; set; } = "Core:XML/AtlasDefinitions/EmpireAtlasDefinition";

        public Empire() { }

        public string GetNextDefaultCityName()
        {
            if (DefaultCityNameIndex >= DefaultCityNames.Count)
                return "New City";
            return DefaultCityNames[DefaultCityNameIndex++];
        }
    }
}
