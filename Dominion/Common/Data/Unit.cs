// Dominion - Copyright (C) Timothy Ings
// Unit.cs
// This file defines classes that define a unit

using ArwicEngine.TypeConverters;
using Dominion.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    public class Unit
    {
        /// <summary>
        /// The key to used to get the icon from the icon atlas
        /// </summary>
        [Description("The key to used to get the icon from the icon atlas")]
        [DisplayName("Icon Key"), Browsable(true), Category("Graphics")]
        [XmlElement("IconKey")]
        public string IconKey { get; set; } = "UNIT_NULL";

        /// <summary>
        /// The icon atlas to source the unit's icon from
        /// </summary>
        [Description("The icon atlas to source the unit's icon from")]
        [DisplayName("Icon Atlas"), Browsable(true), Category("Graphics")]
        [XmlElement("IconAtlas")]
        public string IconAtlas { get; set; } = "Core:XML/AtlasDefinitions/UnitAtlasDefinition";

        /// <summary>
        /// The ID of unit
        /// </summary>
        [Description("The ID of unit")]
        [DisplayName("ID"), Browsable(true), Category("General")]
        [XmlElement("ID")]
        public string ID { get; set; } = "UNIT_NULL";

        /// <summary>
        /// The name of the unit in a display ready format
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public string Name { get; set; } = "Null";

        /// <summary>
        /// Indicates whether the unit can be purchased with gold
        /// </summary>
        [Description("Indicates whether the unit can be purchased with gold")]
        [DisplayName("Purchasable"), Browsable(true), Category("General")]
        [XmlElement("Purchasable")]
        public bool Purchasable { get; set; } = true;

        /// <summary>
        /// Maximum HP of the unit
        /// </summary>
        [Description("Maximum HP of the unit")]
        [DisplayName("MaxHP"), Browsable(true), Category("Combat")]
        [XmlElement("MaxHP")]
        public int MaxHP { get; set; } = 10;

        /// <summary>
        /// The maximum range of the untis ranged attack
        /// </summary>
        [Description("The maximum range of the untis ranged attack")]
        [DisplayName("Range"), Browsable(true), Category("Combat")]
        [XmlElement("Range")]
        public int Range { get; set; } = 1;

        /// <summary>
        /// The unit's ranged combat effectivness
        /// </summary>
        [Description("The unit's ranged combat effectivness")]
        [DisplayName("Ranged Strength"), Browsable(true), Category("Combat")]
        [XmlElement("RangedStrength")]
        public int RangedStrength { get; set; } = 0;

        /// <summary>
        /// The unit's combat effectivness
        /// </summary>
        [Description("The unit's combat effectivness")]
        [DisplayName("Combat Strength"), Browsable(true), Category("Combat")]
        [XmlElement("CombatStrength")]
        public int CombatStrength { get; set; } = 0;

        /// <summary>
        /// The amount of movement points the unit has per turn
        /// </summary>
        [Description("The amount of movement points the unit has per turn")]
        [DisplayName("Movement"), Browsable(true), Category("General")]
        [XmlElement("Movement")]
        public int Movement { get; set; } = 2;

        /// <summary>
        /// The amount of action points the unit has per turn
        /// </summary>
        [Description("The amount of action points the unit has per turn")]
        [DisplayName("Actions"), Browsable(true), Category("General")]
        [XmlElement("Actions")]
        public int Actions { get; set; } = 1;

        /// <summary>
        /// The number of tiles this unit can see
        /// </summary>
        [Description("The number of tiles this unit can see")]
        [DisplayName("Sight"), Browsable(true), Category("General")]
        [XmlElement("Sight")]
        public int Sight { get; set; } = 2;

        /// <summary>
        /// A list of commands the unit will have access to
        /// </summary>
        [Description("A list of commands the unit will have access to")]
        [DisplayName("Commands"), Browsable(true), Category("General")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Commands"), XmlArrayItem(typeof(string), ElementName = "Command")]
        public List<string> Commands { get; set; } = new List<string>();

        /// <summary>
        /// A list of buildings that need to be built in the same city before this unit becomes available
        /// </summary>
        [Description("A list of buildings that need to be built in the same city before this unit becomes available")]
        [DisplayName("Building Prereq"), Browsable(true), Category("Prerequisites")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("BuildingPrereqs"), XmlArrayItem(typeof(string), ElementName = "BuildingPrereq")]
        public List<string> BuildingPrereq { get; set; } = new List<string>();

        /// <summary>
        /// A list of technologies that need to be unlocked before this unit becomes available
        /// </summary>
        [Description("A list of technologies that need to be unlocked before this unit becomes available")]
        [DisplayName("Tech Prereq"), Browsable(true), Category("Prerequisites")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("TechPrereqs"), XmlArrayItem(typeof(string), ElementName = "TechPrereq")]
        public List<string> TechPrereq { get; set; } = new List<string>();

        /// <summary>
        /// Production cost of the building
        /// </summary>
        [Description("Production cost of the building")]
        [DisplayName("Cost"), Browsable(true), Category("General")]
        [XmlElement("Cost")]
        public int Cost { get; set; } = 0;

        /// <summary>
        /// Tags the building has
        /// </summary>
        [Description("Tags the building has")]
        [DisplayName("Tags"), Browsable(true), Category("General")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Tags"), XmlArrayItem(typeof(string), ElementName = "Tag")]
        public List<string> Tags { get; set; } = new List<string>();

        public Unit() { }

        /// <summary>
        /// Converts a unit name to a presentable form
        /// I.e. converts "UNIT_MY_NAME" to "My Name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatName(string name)
        {
            string prefix = "UNIT_";

            // check if the string is valid
            if (!name.Contains(prefix))
                return name;

            // strip "UNIT_"
            // "UNIT_MY_NAME" -> "MY_NAME"
            name = name.Remove(0, prefix.Length);

            // replace all "_"
            // "MY_NAME" -> "MY NAME"
            name = name.Replace('_', ' ');

            // convert to lower case
            // "MY NAME" -> "my name"
            name = name.ToLowerInvariant();

            // convert to title case
            // "my name" -> "My Name"
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            name = textInfo.ToTitleCase(name);

            return name;
        }
    }
}
