// Dominion - Copyright (C) Timothy Ings
// TechnologyManager.cs
// This file defines classes that define the technology manager

using ArwicEngine.Core;
using ArwicEngine.TypeConverters;
using Dominion.Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Managers
{
    [Serializable]
    public class TechnologyInstance
    {
        private Dictionary<string, Technology> data = new Dictionary<string, Technology>();

        public TechnologyInstance(TechnologyManager manager)
        {
            List<Technology> techs = (List<Technology>)manager.GetAllTechnologies();
            foreach (Technology tech in techs)
            {
                tech.Unlocked = false;
                data.Add(tech.ID, tech);
            }
        }

        /// <summary>
        /// Gets a collection of all the technologies in the technology manager
        /// </summary>
        /// <returns></returns>
        public ICollection<Technology> GetAllTechnologies()
        {
            List<Technology> allTechs = new List<Technology>();
            foreach (KeyValuePair<string, Technology> kvp in data)
            {
                allTechs.Add(kvp.Value);
            }
            return allTechs;
        }

        /// <summary>
        /// Gets the building with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Technology GetTech(string name)
        {
            Technology tech;
            bool res = data.TryGetValue(name, out tech);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"Technology '{name}' does not exist", MsgType.Failed);
                return null;
            }
            return tech;
        }

        /// <summary>
        /// Determines if a technology with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool TechExists(string name)
        {
            return data.ContainsKey(name);
        }
    }

    [Serializable]
    public class TechnologyDataPack
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = "New Technology Data Pack";

        [Editor(typeof(Technology.Editor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Technologies"), XmlArrayItem(typeof(Technology), ElementName = "Technology")]
        public List<Technology> Technologies { get; set; } = new List<Technology>();

        public TechnologyDataPack() { }
    }

    [Serializable]
    public class TechnologyManager
    {
        public int BuildingCount => data.Count;

        private Dictionary<string, Technology> data = new Dictionary<string, Technology>();

        private List<TechnologyDataPack> dataPacks = new List<TechnologyDataPack>();

        public TechnologyManager()
        {
        }
        
        /// <summary>
        /// Adds a data pack to the manager
        /// </summary>
        /// <param name="stream"></param>
        public void AddDataPack(Stream stream)
        {
            if (stream == null)
            {
                ConsoleManager.Instance.WriteLine("Missing technology data", MsgType.ServerWarning);
                return;
            }
            // load data pack
            TechnologyDataPack pack = SerializationHelper.XmlDeserialize<TechnologyDataPack>(stream);
            dataPacks.Add(pack);

            foreach (Technology t in pack.Technologies)
            {
                t.Name = Technology.FormatName(t.ID);
                data.Add(t.ID, t);
            }
        }

        /// <summary>
        /// Removes all data packs with the given name from the manager
        /// </summary>
        /// <param name="name"></param>
        public void RemoveDataPack(string name)
        {
            dataPacks.RemoveAll(p => p.Name == name);
            ConstructData();
        }

        // clears data dictionary and recreates it from the list of data packs
        private void ConstructData()
        {
            // clear the data dictionary
            if (data == null)
                data = new Dictionary<string, Technology>();
            else
                data.Clear();

            // fill it with technologies from all the data packs
            foreach (TechnologyDataPack dp in dataPacks)
            {
                foreach (Technology t in dp.Technologies)
                {
                    data.Add(t.ID, t);
                }
            }
        }

        public TechnologyInstance GetNewTree()
        {
            TechnologyInstance tree = new TechnologyInstance(this);
            return tree;
        }

        /// <summary>
        /// Gets a collection of all the technologies in the technology manager
        /// </summary>
        /// <returns></returns>
        public ICollection<Technology> GetAllTechnologies()
        {
            List<Technology> allTechs = new List<Technology>();
            foreach (KeyValuePair<string, Technology> kvp in data)
            {
                allTechs.Add(kvp.Value);
            }
            return allTechs;
        }

        /// <summary>
        /// Gets the building with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Technology GetTech(string name)
        {
            Technology tech;
            bool res = data.TryGetValue(name, out tech);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"Technology '{name}' does not exist", MsgType.Failed);
                return null;
            }
            return tech;
        }

        /// <summary>
        /// Determines if a technology with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool TechExists(string name)
        {
            return data.ContainsKey(name);
        }
    }
}
