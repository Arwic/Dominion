// Dominion - Copyright (C) Timothy Ings
// DataPackController.cs
// This file defines classes that defines the facotry controller

using ArwicEngine.Core;
using Dominion.Common.Data;
using Dominion.Common.Managers;
using System.IO;

namespace Dominion.Server.Controllers
{
    public class DataPackController : Controller
    {
        /// <summary>
        /// Gets the building data manager
        /// </summary>
        public BuildingManager Building { get; } = new BuildingManager();

        /// <summary>
        /// Gets the empire data manager
        /// </summary>
        public EmpireManager Empire { get; } = new EmpireManager();

        /// <summary>
        /// Gets the unit data manager
        /// </summary>
        public UnitManager Unit { get; } = new UnitManager();

        public DataPackController(ControllerManager manager)
            : base(manager)
        {
            LoadStandardData();
            LoadModData();
        }

        // loads the sandard game data
        private void LoadStandardData()
        {
            // load standard data packs
            Empire.AddDataPack("Content/Data/Empires.xml");
            Building.AddDataPack("Content/Data/Buildings.xml");
            Unit.AddDataPack("Content/Data/Units.xml");
        }

        // loads the game data in the mod directory
        private void LoadModData()
        {
            Directory.CreateDirectory("Content/Mods");
            foreach (string modDir in Directory.EnumerateDirectories("Content/Mods"))
            {
                // check if a table of contents exists
                string tocPath = $"Content/Mods/{modDir}/TOC.xml";
                if (!File.Exists(tocPath))
                {
                    ConsoleManager.Instance.WriteLine($"Failed to load mod '{modDir}', no TOC found", MsgType.ServerFailed);
                    continue;
                }

                // load toc
                ModToc toc = SerializationHelper.XmlDeserialize<ModToc>(tocPath);

                // add data packs
                foreach (string path in toc.BuildingPacks)
                    Building.AddDataPack(path);
                foreach (string path in toc.EmpirePacks)
                    Empire.AddDataPack(path);
                foreach (string path in toc.UnitPacks)
                    Unit.AddDataPack(path);
            }
        }
    }
}
