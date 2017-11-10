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

        /// <summary>
        /// Gets the tech data manager
        /// </summary>
        public TechnologyManager Tech { get; } = new TechnologyManager();

        /// <summary>
        /// Gets the social policy manager
        /// </summary>
        public SocialPolicyManager SocialPolicy { get; } = new SocialPolicyManager();

        public DataPackController(ControllerManager manager)
            : base(manager)
        {
            LoadContentPackData("Core");
        }

        // loads the sandard game data
        private void LoadContentPackData(string contentPackName)
        {
            // load standard data packs
            Empire.AddDataPack(Engine.Instance.Content.GetAsset<Stream>($"{contentPackName}:XML/GameData/Empires"));
            Building.AddDataPack(Engine.Instance.Content.GetAsset<Stream>($"{contentPackName}:XML/GameData/Buildings"));
            Unit.AddDataPack(Engine.Instance.Content.GetAsset<Stream>($"{contentPackName}:XML/GameData/Units"));
            Tech.AddDataPack(Engine.Instance.Content.GetAsset<Stream>($"{contentPackName}:XML/GameData/Technologies"));
            SocialPolicy.AddDataPack(Engine.Instance.Content.GetAsset<Stream>($"{contentPackName}:XML/GameData/SocialPolicies"));
        }
    }
}
