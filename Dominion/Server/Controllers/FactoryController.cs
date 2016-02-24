// Dominion - Copyright (C) Timothy Ings
// FactoryController.cs
// This file defines classes that defines the facotry controller

using Dominion.Common.Factories;

namespace Dominion.Server.Controllers
{
    public class FactoryController : Controller
    {
        /// <summary>
        /// Gets the factory that manages the construction of buildings
        /// </summary>
        public BuildingFactory Building { get; }

        /// <summary>
        /// Gets the factory that manages the construction of empires
        /// </summary>
        public EmpireFactory Empire { get; }

        /// <summary>
        /// Gets the factory that manages the construction of productions
        /// </summary>
        public ProductionFactory Production { get; }

        /// <summary>
        /// Gets the factory that manages the construction of units
        /// </summary>
        public UnitFactory Unit { get; }

        public FactoryController(ControllerManager manager)
            : base(manager)
        {
            // load the factories from file
            Building = BuildingFactory.FromFile("Content/Data/BuildingList.xml");
            Empire = EmpireFactory.FromFile("Content/Data/EmpireList.xml");
            Production = ProductionFactory.FromFile("Content/Data/ProductionList.xml");
            Unit = UnitFactory.FromFile("Content/Data/UnitList.xml");
        }
    }
}
