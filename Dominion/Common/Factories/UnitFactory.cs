// Dominion - Copyright (C) Timothy Ings
// UnitFactory.cs
// This file defines classes that define the unit factory and its products

using ArwicEngine.TypeConverters;
using Dominion.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Factories
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    public class UnitTemplate
    {
        /// <summary>
        /// ID of the graphic used to render the unit
        /// </summary>
        [Description("ID of the graphic used to render the unit")]
        [DisplayName("GraphicID"), Browsable(true), Category("Graphics")]
        [XmlElement("GraphicID")]
        public int GraphicID { get; set; }

        /// <summary>
        /// The name of unit
        /// </summary>
        [Description("The name of unit")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Maximum HP of the unit
        /// </summary>
        [Description("Maximum HP of the unit")]
        [DisplayName("MaxHP"), Browsable(true), Category("Combat")]
        [XmlElement("MaxHP")]
        public int MaxHP { get; set; }

        /// <summary>
        /// The maximum range of the untis ranged attack
        /// </summary>
        [Description("The maximum range of the untis ranged attack")]
        [DisplayName("Range"), Browsable(true), Category("Combat")]
        [XmlElement("Range")]
        public int Range { get; set; }

        /// <summary>
        /// The unit's ranged combat effectivness
        /// </summary>
        [Description("The unit's ranged combat effectivness")]
        [DisplayName("Ranged Strength"), Browsable(true), Category("Combat")]
        [XmlElement("RangedStrength")]
        public int RangedStrength { get; set; }

        /// <summary>
        /// The unit's combat effectivness
        /// </summary>
        [Description("The unit's combat effectivness")]
        [DisplayName("Combat Strength"), Browsable(true), Category("Combat")]
        [XmlElement("CombatStrength")]
        public int CombatStrength { get; set; }

        /// <summary>
        /// The amount of movement points the unit has per turn
        /// </summary>
        [Description("The amount of movement points the unit has per turn")]
        [DisplayName("Movement"), Browsable(true), Category("General")]
        [XmlElement("Movement")]
        public int Movement { get; set; }

        /// <summary>
        /// The amount of action points the unit has per turn
        /// </summary>
        [Description("The amount of action points the unit has per turn")]
        [DisplayName("Actions"), Browsable(true), Category("General")]
        [XmlElement("Actions")]
        public int Actions { get; set; }

        /// <summary>
        /// The number of tiles this unit can see
        /// </summary>
        [Description("The number of tiles this unit can see")]
        [DisplayName("Sight"), Browsable(true), Category("General")]
        [XmlElement("Sight")]
        public int Sight { get; set; }

        /// <summary>
        /// A list of commands the unit will have access to
        /// </summary>
        [Description("A list of commands the unit will have access to")]
        [DisplayName("Commands"), Browsable(true), Category("General")]
        [XmlArray("Commands"), XmlArrayItem(typeof(int), ElementName = "Command")]
        [TypeConverter(typeof(ListConverter))]
        public List<int> Commands { get; set; }

        public UnitTemplate()
        {
            GraphicID = 0;
            Name = "New Unit";
            MaxHP = 10;
            Range = 0;
            CombatStrength = 0;
            Movement = 2;
            Actions = 1;
            Sight = 2;
            Commands = new List<int>();
        }
    }

    [Serializable()]
    public class UnitFactory
    {
        /// <summary>
        /// A list of all unit templates
        /// </summary>
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Unit")]
        public List<UnitTemplate> Units { get; set; }

        [NonSerialized()]
        private int lastInstanceID = 0;

        public UnitFactory()
        {
            Units = new List<UnitTemplate>();
        }

        /// <summary>
        /// Loads a unit factory from file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static UnitFactory FromFile(string path)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(UnitFactory));
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (UnitFactory)xmls.Deserialize(fs);
            }
        }

        /// <summary>
        /// Reconstructs a unit after being sent over a network
        /// </summary>
        /// <param name="u"></param>
        public void Reconstruct(Unit u)
        {
            if (u.UnitID < 0 || u.UnitID >= Units.Count)
                throw new Exception("The unit factory is out of sync with the server");
            u.Template = Units[u.UnitID];
            if (u.Name == null || u.Name.Equals(""))
                u.Name = u.Template.Name;

            u.Rebuild();
        }

        /// <summary>
        /// Returns the unit template with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UnitTemplate GetUnit(int id)
        {
            if (id < 0 || id >= Units.Count)
                throw new Exception("The unit factory is out of sync with the server");
            return Units[id];
        }

        public override string ToString()
        {
            return "Units";
        }

        /// <summary>
        /// Constructs a unit
        /// </summary>
        /// <param name="u"></param>
        public void Construct(Unit u)
        {
            u.InstanceID = lastInstanceID++;
            Reconstruct(u);
            u.HP = u.Template.MaxHP;
        }
    }
}
