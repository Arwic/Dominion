// Dominion - Copyright (C) Timothy Ings
// Unit.cs
// This file defines classes that define a unit

using ArwicEngine.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    public class Unit
    {
        /// <summary>
        /// ID of the graphic used to render the unit
        /// </summary>
        [Description("ID of the graphic used to render the unit")]
        [DisplayName("GraphicID"), Browsable(true), Category("Graphics")]
        [XmlElement("GraphicID")]
        public int GraphicID { get; set; } = -1;

        /// <summary>
        /// The name of unit
        /// </summary>
        [Description("The name of unit")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; } = "UNIT_NULL";

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
        [XmlArray("Commands"), XmlArrayItem(typeof(int), ElementName = "Command")]
        [TypeConverter(typeof(ListConverter))]
        public List<int> Commands { get; set; } = new List<int>();

        public Unit() { }
    }
}
