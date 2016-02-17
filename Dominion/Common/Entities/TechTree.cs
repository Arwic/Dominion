using ArwicEngine.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Description("The name of the technology")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; }

        [Description("The descriptions of the technology")]
        [DisplayName("Description"), Browsable(true), Category("General")]
        [XmlElement("Description")]
        public string Description { get; set; }

        [Description("The amount of research points required to unlock this technology")]
        [DisplayName("Research Cost"), Browsable(true), Category("Game")]
        [XmlElement("ResearchCost")]
        public int ResearchCost { get; set; }

        [Description("The X position of this technology in the user interface")]
        [DisplayName("Grid X"), Browsable(true), Category("Graphics")]
        [XmlElement("GridX")]
        public int GridX { get; set; }

        [Description("The Y position of this technology in the user interface")]
        [DisplayName("Grid Y"), Browsable(true), Category("Graphics")]
        [XmlElement("GridY")]
        public int GridY { get; set; }

        [Description("The icon this technology will use")]
        [DisplayName("Icon ID"), Browsable(true), Category("Graphics")]
        [XmlElement("IconID")]
        public int IconID { get; set; }

        [Description("The icons that will apear under this technology")]
        [DisplayName("Unlock Icons"), Browsable(true), Category("Graphics")]
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("UnlockIcons")]
        public List<int> UnlockIcons { get; set; }

        [Description("The technologies that have to be unlocked to begin researching this technology")]
        [DisplayName("Prerequisite Technologies"), Browsable(true), Category("Game")]
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Prerequisite")]
        public List<int> Prerequisites { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public bool Unlocked { get; set; }

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
        [TypeConverter(typeof(ListConverter))]
        [XmlElement("Node")]
        public List<TechNode> Nodes { get; set; }

        public TechTree()
        {
            Nodes = new List<TechNode>();
        }

        public static TechTree FromFile(string path)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(TechTree));
            using (XmlReader reader = XmlReader.Create(File.OpenRead(path), new XmlReaderSettings() { CloseInput = true }))
            {
                TechTree t = (TechTree)xmls.Deserialize(reader);
                return t;
            }
        }

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
