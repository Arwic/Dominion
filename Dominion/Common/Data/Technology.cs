// Dominion - Copyright (C) Timothy Ings
// Technology.cs
// This file defines classes that defines a technology

using ArwicEngine.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    [Serializable]
    public class Technology
    {
        /// <summary>
        /// The tech node's name
        /// </summary>
        [Description("The name of the technology")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; } = "TECH_NULL";

        /// <summary>
        /// The tech node's description
        /// </summary>
        [Description("The descriptions of the technology")]
        [DisplayName("Description"), Browsable(true), Category("General")]
        [XmlElement("Description")]
        public string Description { get; set; } = "New Tech";

        /// <summary>
        /// The amount of science required to unlock this tech node
        /// </summary>
        [Description("The amount of research points required to unlock this technology")]
        [DisplayName("Research Cost"), Browsable(true), Category("Game")]
        [XmlElement("ResearchCost")]
        public int ResearchCost { get; set; } = 0;

        /// <summary>
        /// The x position of the tech node
        /// </summary>
        [Description("The X position of this technology in the user interface")]
        [DisplayName("Grid X"), Browsable(true), Category("Graphics")]
        [XmlElement("GridX")]
        public int GridX { get; set; } = 0;

        /// <summary>
        /// The y position of the tech node
        /// </summary>
        [Description("The Y position of this technology in the user interface")]
        [DisplayName("Grid Y"), Browsable(true), Category("Graphics")]
        [XmlElement("GridY")]
        public int GridY { get; set; } = 0;

        /// <summary>
        /// The key to used to get the icon from the icon atlas
        /// </summary>
        [Description("The key to used to get the icon from the icon atlas")]
        [DisplayName("Icon Key"), Browsable(true), Category("Graphics")]
        [XmlElement("IconKey")]
        public string IconKey { get; set; } = "TECH_NULL";

        /// <summary>
        /// The icon atlas to source the technology's icon from
        /// </summary>
        [Description("The icon atlas to source the technology's icon from")]
        [DisplayName("Icon Atlas"), Browsable(true), Category("Graphics")]
        [XmlElement("IconAtlas")]
        public string IconAtlas { get; set; } = "Core:XML/AtlasDefinitions/TechnologyAtlasDefinition";

        /// <summary>
        /// A list of technologies that have to be unlocked in order to research this node
        /// </summary>
        [Description("A list of technologies that have to be unlocked in order to research this node")]
        [DisplayName("Prerequisite Technologies"), Browsable(true), Category("Game")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Prerequisites"), XmlArrayItem(typeof(string), ElementName = "Prerequisites")]
        public List<string> Prerequisites { get; set; } = new List<string>();

        /// <summary>
        /// Indicates whether this technology is unlocked
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public bool Unlocked { get; set; } = false;

        /// <summary>
        /// The amount of science point that have been put into this tech node
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public int Progress { get; set; } = 0;

        public Technology() { }
    }
}
