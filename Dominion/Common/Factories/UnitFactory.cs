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
        [Description("ID of the graphic used to render the unit")]
        [DisplayName("GraphicID"), Browsable(true), Category("Graphics")]
        [XmlElement("GraphicID")]
        public int GraphicID { get; set; }

        [Description("The name of unit")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; }

        [Description("Maximum HP of the unit")]
        [DisplayName("MaxHP"), Browsable(true), Category("Combat")]
        [XmlElement("MaxHP")]
        public int MaxHP { get; set; }

        [Description("The maximum range of the untis ranged attack")]
        [DisplayName("Range"), Browsable(true), Category("Combat")]
        [XmlElement("Range")]
        public int Range { get; set; }

        [Description("The unit's ranged combat effectivness")]
        [DisplayName("Ranged Strength"), Browsable(true), Category("Combat")]
        [XmlElement("RangedStrength")]
        public int RangedStrength { get; set; }

        [Description("The unit's combat effectivness")]
        [DisplayName("Combat Strength"), Browsable(true), Category("Combat")]
        [XmlElement("CombatStrength")]
        public int CombatStrength { get; set; }

        [Description("The amount of movement points the unit has per turn")]
        [DisplayName("Movement"), Browsable(true), Category("General")]
        [XmlElement("Movement")]
        public int Movement { get; set; }

        [Description("The number of tiles this unit can see")]
        [DisplayName("Sight"), Browsable(true), Category("General")]
        [XmlElement("Sight")]
        public int Sight { get; set; }

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
            Sight = 2;
            Commands = new List<int>();
        }
    }

    [Serializable()]
    public class UnitFactory
    {
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Unit")]
        public List<UnitTemplate> Units { get; set; }

        [NonSerialized()]
        private int lastInstanceID = 0;

        public UnitFactory()
        {
            Units = new List<UnitTemplate>();
        }

        public static UnitFactory FromFile(string path)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(UnitFactory));
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (UnitFactory)xmls.Deserialize(fs);
            }
        }

        public void Reconstruct(Unit u)
        {
            if (u.UnitID < 0 || u.UnitID >= Units.Count)
                throw new Exception("The unit factory is out of sync with the server");
            u.Constants = Units[u.UnitID];
            if (u.Name == null || u.Name.Equals(""))
                u.Name = u.Constants.Name;

            u.Rebuild();
        }

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

        public void Construct(Unit u)
        {
            u.InstanceID = lastInstanceID++;
            Reconstruct(u);
            u.HP = u.Constants.MaxHP;
        }
    }
}
