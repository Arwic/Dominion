// Dominion - Copyright (C) Timothy Ings
// BuildingManager.cs
// This file defines classes that define the building manager

using ArwicEngine.Core;
using ArwicEngine.TypeConverters;
using Dominion.Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Managers
{
    [Serializable]
    public class BuildingDataPack
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = "New Building Data Pack";

        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Buildings"), XmlArrayItem(typeof(Building), ElementName = "Building")]
        public List<Building> Buildings { get; set; } = new List<Building>();

        public BuildingDataPack() { }
    }

    [Serializable]
    public class BuildingManager
    {
        public int BuildingCount => data.Count;

        private Dictionary<string, Building> data = new Dictionary<string, Building>();

        private List<BuildingDataPack> dataPacks = new List<BuildingDataPack>();

        public BuildingManager()
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
                ConsoleManager.Instance.WriteLine("Missing building data", MsgType.ServerWarning);
                return;
            }
            // load data pack
            BuildingDataPack pack = SerializationHelper.XmlDeserialize<BuildingDataPack>(stream);
            dataPacks.Add(pack);

            foreach (Building b in pack.Buildings)
            {
                b.DisplayName = Building.FormatName(b.Name);
                data.Add(b.Name, b);
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
                data = new Dictionary<string, Building>();
            else
                data.Clear();

            // fill it with buildings from all the data packs
            foreach (BuildingDataPack dp in dataPacks)
            {
                foreach (Building b in dp.Buildings)
                {
                    data.Add(b.Name, b);
                }
            }
        }

        /// <summary>
        /// Gets a collection of all the buildings in the building manager
        /// </summary>
        /// <returns></returns>
        public ICollection<Building> GetAllBuildings()
        {
            List<Building> allBuildings = new List<Building>();
            foreach (KeyValuePair<string, Building> kvp in data)
            {
                allBuildings.Add(kvp.Value);
            }
            return allBuildings;
        }

        /// <summary>
        /// Gets the building with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Building GetBuilding(string name)
        {
            Building building;
            bool res = data.TryGetValue(name, out building);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"Building '{name}' does not exist", MsgType.Failed);
                return null;
            }
            return building;
        }

        /// <summary>
        /// Determines if a building with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool BuildingExists(string name)
        {
            return data.ContainsKey(name);
        }
    }
}
