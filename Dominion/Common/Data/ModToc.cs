// Dominion - Copyright (C) Timothy Ings
// ModToc.cs
// This file defines classes that define mod table of contents

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    public class ModToc
    {
        [XmlArray("BuildingPacks"), XmlArrayItem(typeof(Empire), ElementName = "BuildingPack")]
        public List<string> BuildingPacks { get; set; }
        [XmlArray("EmpirePacks"), XmlArrayItem(typeof(Empire), ElementName = "EmpirePack")]
        public List<string> EmpirePacks { get; set; }
        [XmlArray("UnitPacks"), XmlArrayItem(typeof(Empire), ElementName = "UnitPack")]
        public List<string> UnitPacks { get; set; }
    }
}
