// Dominion - Copyright (C) Timothy Ings
// UnitManager.cs
// This file defines classes that define the unit manager

using ArwicEngine.Core;
using ArwicEngine.TypeConverters;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Managers
{
    [Serializable]
    public class UnitDataPack
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = "New Unit Data Pack";

        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Units"), XmlArrayItem(typeof(Unit), ElementName = "Unit")]
        public List<Unit> Units { get; set; } = new List<Unit>();

        public UnitDataPack() { }
    }

    [Serializable]
    public class UnitManager
    {
        public int UnitCount => data.Count;

        private Dictionary<string, Unit> data = new Dictionary<string, Unit>();

        private List<UnitDataPack> dataPacks = new List<UnitDataPack>();

        private int lastInstanceID = 0;

        public UnitManager()
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
                ConsoleManager.Instance.WriteLine("Missing unit data", MsgType.ServerWarning);
                return;
            }
            // load data pack
            UnitDataPack pack = SerializationHelper.XmlDeserialize<UnitDataPack>(stream);
            dataPacks.Add(pack);

            foreach (Unit u in pack.Units)
            {
                u.DisplayName = Unit.FormatName(u.Name);
                data.Add(u.Name, u);
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

        /// <summary>
        /// Gets a collection of all the unit in the unit manager
        /// </summary>
        /// <returns></returns>
        public ICollection<Unit> GetAllUnits()
        {
            List<Unit> allUnits = new List<Unit>();
            foreach (KeyValuePair<string, Unit> kvp in data)
            {
                allUnits.Add(kvp.Value);
            }
            return allUnits;
        }

        // clears data dictionary and recreates it from the list of data packs
        private void ConstructData()
        {
            // clear the data dictionary
            if (data == null)
                data = new Dictionary<string, Unit>();
            else
                data.Clear();

            // fill it with units from all the data packs
            foreach (UnitDataPack dp in dataPacks)
            {
                foreach (Unit u in dp.Units)
                {
                    data.Add(u.Name, u);
                }
            }
        }

        /// <summary>
        /// Gets the unit with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Unit GetUnit(string name)
        {
            Unit unit;
            bool res = data.TryGetValue(name, out unit);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"Unit '{name}' does not exist", MsgType.Failed);
                return null;
            }
            return unit;
        }

        /// <summary>
        /// Determines if a unit with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UnitExists(string name)
        {
            return data.ContainsKey(name);
        }

        public void Construct(UnitInstance unit)
        {
            unit.InstanceID = lastInstanceID++;
            Reconstruct(unit);
            unit.HP = unit.BaseUnit.MaxHP;
        }

        public void Reconstruct(UnitInstance unit)
        {
            unit.BaseUnit = data[unit.UnitID];
            if (unit.Name == null || unit.Name.Equals(""))
                unit.Name = unit.BaseUnit.DisplayName;

            unit.Rebuild();
        }
    }

}
