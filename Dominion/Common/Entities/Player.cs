// Dominion - Copyright (C) Timothy Ings
// Player.cs
// This file defines classes that define a player

using ArwicEngine.Net;
using System;

namespace Dominion.Common.Entities
{
    public enum GameEra
    {
        Null,
        Ancient,
        Classical,
        Medieval,
        Renaissance,
        Industrial,
        Modern,
        Atomic,
        Infromation
    }

    [Serializable()]
    public class BasicPlayer
    {
        /// <summary>
        /// The player's unique id
        /// </summary>
        public int InstanceID { get; set; }

        /// <summary>
        /// The player's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The player's empire id
        /// </summary>
        public int EmpireID { get; set; }

        /// <summary>
        /// Indicates whether the player is the host of the game
        /// </summary>
        public bool Host { get; set; }

        /// <summary>
        /// The player's score
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Indicates whether the player has ended their turn
        /// </summary>
        public bool EndedTurn { get; set; }

        /// <summary>
        /// Contructs a basic player from a player
        /// </summary>
        /// <param name="player"></param>
        public BasicPlayer(Player player)
        {
            Name = player.Name;
            InstanceID = player.InstanceID;
            EmpireID = player.EmpireID;
            EndedTurn = player.EndedTurn;
        }
    }

    [Serializable()]
    public class Player
    {
        /// <summary>
        /// The player's unique id
        /// </summary>
        public int InstanceID { get; set; }

        /// <summary>
        /// The player's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The player's empire id
        /// </summary>
        public int EmpireID { get; set; }

        /// <summary>
        /// The player's network connection
        /// </summary>
        public Connection Connection { get { return _connection; } set { _connection = value; } }
        [NonSerialized()]
        private Connection _connection;

        /// <summary>
        /// The player's score
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Indicates whether the player has ended their turn
        /// </summary>
        public bool EndedTurn { get; set; }

        /// <summary>
        /// The player's current technological era
        /// </summary>
        public GameEra CurrentEra { get; set; }

        /// <summary>
        /// The player's tech tree
        /// </summary>
        public TechTree TechTree { get; set; }

        /// <summary>
        /// The player's selected tech node's id
        /// </summary>
        public int SelectedTechNodeID { get; set; }

        /// <summary>
        /// The player's science overflow
        /// </summary>
        public int ScienceOverflow { get; set; }

        /// <summary>
        /// The player's gold earned per turn
        /// </summary>
        public int IncomeGold { get; set; }

        /// <summary>
        /// The player's science earned per turn
        /// </summary>
        public int IncomeScience { get; set; }

        /// <summary>
        /// The player's culture earned per turn
        /// </summary>
        public int IncomeCulture { get; set; }

        /// <summary>
        /// The player's tourism earned per turn
        /// </summary>
        public int IncomeTourism { get; set; }

        /// <summary>
        /// The player's faith earned per turn
        /// </summary>
        public int IncomeFaith { get; set; }

        /// <summary>
        /// The player's current gold
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// The player's current happiness
        /// </summary>
        public int Happiness { get; set; }

        /// <summary>
        /// The player's current culture
        /// </summary>
        public int Culture { get; set; }

        /// <summary>
        /// The player's current faith
        /// </summary>
        public int Faith { get; set; }

        /// <summary>
        /// The player's available iron
        /// </summary>
        public int Iron { get; set; }

        /// <summary>
        /// The player's available horses
        /// </summary>
        public int Horses { get; set; }

        /// <summary>
        /// The player's available coal
        /// </summary>
        public int Coal { get; set; }

        /// <summary>
        /// The player's available oil
        /// </summary>
        public int Oil { get; set; }

        /// <summary>
        /// The player's available aluminium
        /// </summary>
        public int Aluminium { get; set; }

        /// <summary>
        /// The player's available uranium
        /// </summary>
        public int Uranium { get; set; }

        public Player(Connection conn, int playerD, int empireID, string username, TechTree blankTechTree)
        {
            Connection = conn;
            CurrentEra = GameEra.Ancient;
            InstanceID = playerD;
            EmpireID = empireID;
            Name = username;
            TechTree = blankTechTree;
        }
    }
}
