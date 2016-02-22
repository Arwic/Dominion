// Dominion - Copyright (C) Timothy Ings
// BuildingFactory.cs
// This file defines classes that define the building factory and its products

using ArwicEngine.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Factories
{

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    public class Building
    {
        public const int UniversityID = 1;

        /// <summary>
        /// The name of the building
        /// </summary>
        [Description("The name of the building")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The building's description
        /// </summary>
        [Description("The building's description")]
        [DisplayName("Description"), Browsable(true), Category("General")]
        [XmlElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// The building's icon id
        /// </summary>
        [Description("The building's icon id")]
        [DisplayName("Icon ID"), Browsable(true), Category("Graphics")]
        [XmlElement("IconID")]
        public int IconID { get; set; }

        /// <summary>
        /// The maintenence cost of the building per turn
        /// </summary>
        [Description("Maintenence cost")]
        [DisplayName("Gold Upkeep"), Browsable(true), Category("General")]
        [XmlElement("GoldUpkeep")]
        public int GoldUpkeep { get; set; }

        /// <summary>
        /// The science this building earns per turn
        /// </summary>
        [Description("Amount of science earned per turn")]
        [DisplayName("Science"), Browsable(true), Category("Flat Income")]
        [XmlElement("IncomeScience")]
        public int IncomeScience { get; set; }

        /// <summary>
        /// The gold this building earns per turn
        /// </summary>
        [Description("Amount of gold earned per turn")]
        [DisplayName("Gold"), Browsable(true), Category("Flat Income")]
        [XmlElement("IncomeGold")]
        public int IncomeGold { get; set; }

        /// <summary>
        /// The porduction this building earns per turn
        /// </summary>
        [Description("Amount of production earned per turn")]
        [DisplayName("Production"), Browsable(true), Category("Flat Income")]
        [XmlElement("IncomeProduction")]
        public int IncomeProduction { get; set; }

        /// <summary>
        /// The happiness this building earns per turn
        /// </summary>
        [Description("Amount of happiness earned per turn")]
        [DisplayName("Happiness"), Browsable(true), Category("Flat Income")]
        [XmlElement("IncomeHappiness")]
        public int IncomeHappiness { get; set; }

        /// <summary>
        /// The food this building earns per turn
        /// </summary>
        [Description("Amount of food earned per turn")]
        [DisplayName("Food"), Browsable(true), Category("Flat Income")]
        [XmlElement("IncomeFood")]
        public int IncomeFood { get; set; }

        /// <summary>
        /// The culture this building earns per turn
        /// </summary>
        [Description("Amount of culture earned per turn")]
        [DisplayName("Culture"), Browsable(true), Category("Flat Income")]
        [XmlElement("IncomeCulture")]
        public int IncomeCulture { get; set; }

        /// <summary>
        /// Modifier to the amount of science earned per turn
        /// </summary>
        [Description("Modifier to the amount of science earned per turn")]
        [DisplayName("Science"), Browsable(true), Category("Income Modifiers")]
        [XmlElement("ModScience")]
        public double ModScience { get; set; }

        /// <summary>
        /// Modifier to the amount of gold earned per turn
        /// </summary>
        [Description("Modifier to the amount of gold earned per turn")]
        [DisplayName("Gold"), Browsable(true), Category("Income Modifiers")]
        [XmlElement("ModGold")]
        public double ModGold { get; set; }

        /// <summary>
        /// Modifier to the amount of production earned per turn
        /// </summary>
        [Description("Modifier to the amount of production earned per turn")]
        [DisplayName("Production"), Browsable(true), Category("Income Modifiers")]
        [XmlElement("ModProduction")]
        public double ModProduction { get; set; }

        /// <summary>
        /// Modifier to the amount of happiness earned per turn
        /// </summary>
        [Description("Modifier to the amount of happiness earned per turn")]
        [DisplayName("Happiness"), Browsable(true), Category("Income Modifiers")]
        [XmlElement("ModHappiness")]
        public double ModHappiness { get; set; } // TODO add global and local

        /// <summary>
        /// Modifier to the amount of food earned per turn
        /// </summary>
        [Description("Modifier to the amount of food earned per turn")]
        [DisplayName("Food"), Browsable(true), Category("Income Modifiers")]
        [XmlElement("ModFood")]
        public double ModFood { get; set; }

        /// <summary>
        /// Modifier to the amount of culture earned per turn
        /// </summary>
        [Description("Modifier to the amount of culture earned per turn")]
        [DisplayName("Culture"), Browsable(true), Category("Income Modifiers")]
        [XmlElement("ModCulture")]
        public double ModCulture { get; set; }

        public Building()
        {
            Name = "New Building";
            Description = "Description";
            IconID = 0;
            IncomeScience = 0;
            IncomeGold = 0;
            IncomeProduction = 0;
            IncomeHappiness = 0;
            IncomeFood = 0;
            IncomeCulture = 0;
            ModScience = 1;
            ModGold = 1;
            ModProduction = 1;
            ModHappiness = 1;
            ModFood = 1;
            ModCulture = 1;
        }
    }

    [Serializable()]
    public class BuildingFactory
    {
        /// <summary>
        /// A list of all the buildings
        /// </summary>
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Building")]
        public List<Building> Buildings { get; set; }

        public BuildingFactory()
        {
            Buildings = new List<Building>();
        }

        /// <summary>
        /// Loads a building factory from file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BuildingFactory FromFile(string path)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(BuildingFactory));
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (BuildingFactory)xmls.Deserialize(fs);
            }
        }

        /// <summary>
        /// Returns a building with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Building GetBuilding(int id)
        {
            if (id < 0 || id >= Buildings.Count)
                throw new Exception("The building factory is out of sync with the server");
            return Buildings[id];
        }

        public override string ToString()
        {
            return "Buildings";
        }
    }
}
