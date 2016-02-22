// Dominion - Copyright (C) Timothy Ings
// TechTree.cs
// This file defines classes that define a tech tree

using ArwicEngine.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Dominion.Common.Entities
{
    public enum TechNodes
    {
        Agriculture,
        Pottery,
        AnimalHusbandry,
        Archery,
        Mining,
        Sailing,
        Calender,
        Writing,
        Trapping,
        TheWheel,
        Masonry,
        BronzeWorking,

        // -1 indicates the tech is used in the code but not implemented in the data files
        Engineering = -1,
        Refrigeration = -1,
        Railroad = -1,
        Guilds = -1,
        Construction = -1,
        Calendar = -1
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable()]
    public class TechNode
    {
        /// <summary>
        /// The tech node's name
        /// </summary>
        [Description("The name of the technology")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The tech node's description
        /// </summary>
        [Description("The descriptions of the technology")]
        [DisplayName("Description"), Browsable(true), Category("General")]
        [XmlElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// The amount of science required to unlock this tech node
        /// </summary>
        [Description("The amount of research points required to unlock this technology")]
        [DisplayName("Research Cost"), Browsable(true), Category("Game")]
        [XmlElement("ResearchCost")]
        public int ResearchCost { get; set; }

        /// <summary>
        /// The x position of the tech node
        /// </summary>
        [Description("The X position of this technology in the user interface")]
        [DisplayName("Grid X"), Browsable(true), Category("Graphics")]
        [XmlElement("GridX")]
        public int GridX { get; set; }

        /// <summary>
        /// The y position of the tech node
        /// </summary>
        [Description("The Y position of this technology in the user interface")]
        [DisplayName("Grid Y"), Browsable(true), Category("Graphics")]
        [XmlElement("GridY")]
        public int GridY { get; set; }

        /// <summary>
        /// The tech node's icon id
        /// </summary>
        [Description("The icon this technology will use")]
        [DisplayName("Icon ID"), Browsable(true), Category("Graphics")]
        [XmlElement("IconID")]
        public int IconID { get; set; }

        /// <summary>
        /// A list of icons that will apear in the tech node's tool tip
        /// These icons are meant to indicate the benefits of researching this tech
        /// </summary>
        [Description("The icons that will apear under this technology")]
        [DisplayName("Unlock Icons"), Browsable(true), Category("Graphics")]
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("UnlockIcons")]
        public List<int> UnlockIcons { get; set; }

        /// <summary>
        /// A list of tech nodes that have to be unlocked to research this node
        /// </summary>
        [Description("The technologies that have to be unlocked to begin researching this technology")]
        [DisplayName("Prerequisite Technologies"), Browsable(true), Category("Game")]
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Prerequisite")]
        public List<int> Prerequisites { get; set; }

        /// <summary>
        /// Indicates whether this node is unlocked
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public bool Unlocked { get; set; }

        /// <summary>
        /// The amount of science point that have been put into this tech node
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public int Progress { get; set; }

        public TechNode()
        {
            Prerequisites = new List<int>();
            Name = "New Tech Node";
            ResearchCost = 0;
            Progress = 0;
            Unlocked = false;
            Description = "Description";
        }
    }

    [Serializable]
    public class TechTree
    {
        /// <summary>
        /// A list of the nodes in the tech tree
        /// </summary>
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Node")]
        public List<TechNode> Nodes { get; set; }

        public TechTree()
        {
            Nodes = new List<TechNode>();
        }

        /// <summary>
        /// Loads the tech tree from file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static TechTree FromFile(string path)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(TechTree));
            using (XmlReader reader = XmlReader.Create(File.OpenRead(path), new XmlReaderSettings() { CloseInput = true }))
            {
                TechTree t = (TechTree)xmls.Deserialize(reader);
                return t;
            }
        }

        /// <summary>
        /// Returns the tech node with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TechNode GetNode(int id)
        {
            if (id == -1) // TODO remove this when the tech tree contains no null nodes
            {
                return new TechNode() { Unlocked = false };
            }
            if (id < 0 || id >= Nodes.Count)
                throw new Exception("The tech tree is out of sync with the server");
            return Nodes[id];
        }
    }
}
