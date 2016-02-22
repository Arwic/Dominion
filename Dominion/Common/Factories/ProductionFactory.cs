// Dominion - Copyright (C) Timothy Ings
// ProductionFactory.cs
// This file defines classes that define the production factory and its products

using ArwicEngine.TypeConverters;
using Dominion.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Factories
{
    public enum ProductionRequirements
    {
        Res_Horse,
        Res_Ivory,
        Res_Marble,
        Res_Stone,
        No_Plains,
        Ft_River,
        City_Occupied,
        Res_Gold,
        Res_Silver,
        Imp_Pasture,
    }

    public enum ProductionResultType
    {
        Building,
        Unit
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    [DefaultProperty("Tag")]
    public class Production
    {
        /// <summary>
        /// Cost to produce with hammers
        /// </summary>
        [Description("Cost to produce with hammers")]
        [DisplayName("Production"), Browsable(true), Category("Cost")]
        [XmlElement("ProductionCost")]
        public int ProductionCost { get; set; }

        /// <summary>
        /// Cost to buy with gold
        /// </summary>
        [Description("Cost to buy with gold")]
        [DisplayName("Gold"), Browsable(true), Category("Cost")]
        [XmlElement("GoldCost")]
        public int GoldCost { get; set; }

        /// <summary>
        /// The production's progress in hammers
        /// </summary>
        [Browsable(false)]
        [XmlIgnore()]
        public int Progress { get; set; }

        /// <summary>
        /// The ID of the result
        /// </summary>
        [Description("The ID of the result")]
        [DisplayName("ID"), Browsable(true), Category("Result")]
        [XmlElement("ResultID")]
        public int ResultID { get; set; }

        /// <summary>
        /// The result type
        /// </summary>
        [Description("The result type")]
        [DisplayName("Type"), Browsable(true), Category("Result")]
        [TypeConverter(typeof(EnumConverter))]
        [XmlElement("ResultType")]
        public ProductionResultType ResultType { get; set; }

        /// <summary>
        /// A list of buildings that are required to produce this item
        /// </summary>
        [Description("A list of buildings that are required to produce this item")]
        [DisplayName("Buildings"), Browsable(true), Category("Prerequisites")]
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("PrerequisiteBuilding")]
        public List<int> PrerequisiteBuildings { get; set; }

        /// <summary>
        /// A list of technologies that are required to produce this item
        /// </summary>
        [Description("A list of technologies that are required to produce this item")]
        [DisplayName("Technologies"), Browsable(true), Category("Prerequisites")]
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("PrerequisiteTech")]
        public List<int> PrerequisiteTechs { get; set; }

        /// <summary>
        /// The era at which this building first is producible
        /// </summary>
        [Description("The era at which this building first is producible")]
        [DisplayName("Game Era"), Browsable(true), Category("Prerequisites")]
        [XmlElement("GameEra")]
        public GameEra GameEra { get; set; }

        /// <summary>
        /// A tag for organisation in the xml editor
        /// </summary>
        [Description("A tag for organisation in the xml editor")]
        [DisplayName("Name"), Browsable(true), Category("Editor")]
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The production's id
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public int ID { get; set; }

        public Production()
        {
            ProductionCost = 0;
            GoldCost = 0;
            ResultID = 0;
            ResultType = 0;
            PrerequisiteBuildings = new List<int>();
            PrerequisiteTechs = new List<int>();
            GameEra = 0;
        }
    }

    [Serializable()]
    public class ProductionFactory
    {
        /// <summary>
        /// A list of all productions
        /// </summary>
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Production")]
        public List<Production> Productions { get; set; }

        public ProductionFactory()
        {
            Productions = new List<Production>();
        }

        /// <summary>
        /// Loads a production factory from file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ProductionFactory FromFile(string path)
        {
            ProductionFactory factory = null;
            XmlSerializer xmls = new XmlSerializer(typeof(ProductionFactory));
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                factory = (ProductionFactory)xmls.Deserialize(fs);
            }
            for (int i = 0; i < factory.Productions.Count; i++)
            {
                factory.Productions[i].ID = i;
            }
            return factory;
        }

        public override string ToString()
        {
            return "Productions";
        }

        /// <summary>
        /// Returns the production with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Production GetProduction(int id)
        {
            if (id < 0 || id >= Productions.Count)
                throw new Exception("The production factory is out of sync with the server");
            return Productions[id];
        }
    }
}
