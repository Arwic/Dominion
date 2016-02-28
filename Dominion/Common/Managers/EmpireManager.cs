// Dominion - Copyright (C) Timothy Ings
// EmpireManager.cs
// This file defines classes that define the empire manager

using ArwicEngine.Core;
using ArwicEngine.TypeConverters;
using Dominion.Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dominion.Common.Managers
{
    [Serializable]
    public class EmpireDataPack
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = "New Empire Data Pack";

        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Empires"), XmlArrayItem(typeof(Empire), ElementName = "Empire")]
        public List<Empire> Empires { get; set; } = new List<Empire>();

        public EmpireDataPack() { }
    }

    [Serializable]
    public class EmpireManager
    {
        public int EmpireCount => data.Count;

        private Dictionary<string, Empire> data = new Dictionary<string, Empire>();

        private List<EmpireDataPack> dataPacks = new List<EmpireDataPack>();

        public EmpireManager()
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
                ConsoleManager.Instance.WriteLine("Missing empire data", MsgType.ServerWarning);
                return;
            }
            // load data pack
            EmpireDataPack pack = SerializationHelper.XmlDeserialize<EmpireDataPack>(stream);
            dataPacks.Add(pack);

            foreach (Empire e in pack.Empires)
            {
                e.DisplayName = Empire.FormatName(e.Name);
                data.Add(e.Name, e);
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
                data = new Dictionary<string, Empire>();
            else
                data.Clear();

            // fill it with Empires from all the data packs
            foreach (EmpireDataPack dp in dataPacks)
            {
                foreach (Empire e in dp.Empires)
                {
                    data.Add(e.Name, e);
                }
            }
        }

        /// <summary>
        /// Gets a collection of all the empires in the empire manager
        /// </summary>
        /// <returns></returns>
        public ICollection<Empire> GetAllEmpires()
        {
            List<Empire> allEmpires = new List<Empire>();
            foreach (KeyValuePair<string, Empire> kvp in data)
            {
                allEmpires.Add(kvp.Value);
            }
            return allEmpires;
        }

        /// <summary>
        /// Gets the empire with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Empire GetEmpire(string name)
        {
            Empire empire;
            bool res = data.TryGetValue(name, out empire);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"Empire '{name}' does not exist", MsgType.Failed);
                return null;
            }
            return empire;
        }

        /// <summary>
        /// Determines if a empire with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool EmpireExists(string name)
        {
            return data.ContainsKey(name);
        }

        /// <summary>
        /// Gets the index of the given empire
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int IndexOf(string name)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data.Values.ElementAt(i).Name == name)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
