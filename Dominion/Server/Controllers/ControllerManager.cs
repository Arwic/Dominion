// Dominion - Copyright (C) Timothy Ings
// ControllerManager.cs
// This file defines classes that defines the manager for all the game controllers

namespace Dominion.Server.Controllers
{
    public class ControllerManager
    {
        /// <summary>
        /// Gets the controller that manages game data
        /// </summary>
        public DataPackController Data { get; }

        /// <summary>
        /// Gets the controller that manages the board
        /// </summary>
        public BoardController Board { get; }

        /// <summary>
        /// Gets the controller that manages the players
        /// </summary>
        public PlayerController Player { get; }

        /// <summary>
        /// Gets the controller that manages the cities
        /// </summary>
        public CityController City { get; }

        /// <summary>
        /// Gets the controller that manages the units
        /// </summary>
        public UnitController Unit { get; }

        public ControllerManager()
        {
            Data = new DataPackController(this);
            Board = new BoardController(this);
            City = new CityController(this);
            Unit = new UnitController(this);
            Player = new PlayerController(this);
        }

        /// <summary>
        /// Prepares every controller managed by the controller manager for the next turn
        /// </summary>
        public void ProcessTurn()
        {
            Data.ProcessTurn();
            Board.ProcessTurn();
            Player.ProcessTurn();
            City.ProcessTurn();
            Unit.ProcessTurn();
        }
    }
}
