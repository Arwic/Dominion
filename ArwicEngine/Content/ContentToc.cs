using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ArwicEngine.Content
{
    [XmlRoot("File")]
    public class ContentTocEntry
    {
        [XmlAttribute("FileType", DataType = "string", Type = typeof(string))]
        public string FileType { get; set; }

        [XmlElement("Name", DataType = "string", Type = typeof(string))]
        public string File { get; set; }
    }

    [XmlRoot("ContentPackDefinition")]
    public class ContentToc
    {
        [XmlAttribute("Name", DataType = "string", Type = typeof(string))]
        public string Name { get; set; }

        [XmlArray("Entries"), XmlArrayItem(typeof(string), ElementName = "Entry")]
        public List<ContentTocEntry> Entries { get; set; }
    }
}
