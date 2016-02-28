// Dominion - Copyright (C) Timothy Ings
// Player.cs
// This file defines classes that define a player

using ArwicEngine.Net;
using Dominion.Common.Managers;
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
        public string EmpireID { get; set; }

        /// <summary>
        /// Indicates whether the player is the host of the game
        /// </summary>
        public bool Host { get; set; }

        /// <summary>
        /// The player's score
        /// </summary>
        public int VictoryPoints { get; set; }

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
        public string EmpireID { get; set; }

        /// <summary>
        /// The player's network connection
        /// </summary>
        public Connection Connection { get { return _connection; } set { _connection = value; } }
        [NonSerialized()]
        private Connection _connection;

        /// <summary>
        /// The player's score
        /// </summary>
        public int VictoryPoints { get; set; }

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
        public TechnologyTree TechTree { get; set; }

        /// <summary>
        /// The player's selected tech node's id
        /// </summary>
        public string SelectedTechNodeID { get; set; } = "TECH_NULL";

        /// <summary>
        /// The player's science overflow
        /// </summary>
        public int ScienceOverflow { get; set; }

        #region income
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
        /// The player's faith earned per turn
        /// </summary>
        public int IncomeFaith { get; set; }

        /// <summary>
        /// The player's tourism earned per turn
        /// </summary>
        public int IncomeTourism { get; set; } = 0;
        #endregion

        #region stats
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
        /// The player's number of diplomatic votes available
        /// </summary>
        public int DiplomaticVotes { get; set; }
        #endregion

        #region basic income/yield global modifiers
        /// <summary>
        /// Global food income modifier
        /// </summary>
        public float IncomeFoodModifier { get; set; } = 1f;

        /// <summary>
        /// Global production income modifier
        /// </summary>
        public float IncomeProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Global gold income modifier
        /// </summary>
        public float IncomeGoldModifier { get; set; } = 1f;

        /// <summary>
        /// Global science income modifier
        /// </summary>
        public float IncomeScienceModifier { get; set; } = 1f;

        /// <summary>
        /// Global culture income modifier
        /// </summary>
        public float IncomeCultureModifier { get; set; } = 1f;

        /// <summary>
        /// Global faith income modifier
        /// </summary>
        public float IncomeFaithModifier { get; set; } = 1f;

        /// <summary>
        /// Global tourism income modifier
        /// </summary>
        public float IncomeTourismModifier { get; set; } = 1f;

        /// <summary>
        /// Global food yield modifier
        /// </summary>
        public float YieldFoodModifier { get; set; } = 1f;

        /// <summary>
        /// Global production yield modifier
        /// </summary>
        public float YieldProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Global gold yield modifier
        /// </summary>
        public float YieldGoldModifier { get; set; } = 1f;

        /// <summary>
        /// Global science yield modifier
        /// </summary>
        public float YieldScienceModifier { get; set; } = 1f;

        /// <summary>
        /// Global culture yield modifier
        /// </summary>
        public float YieldCultureModifier { get; set; } = 1f;

        /// <summary>
        /// Global faith yield modifier
        /// </summary>
        public float YieldFaithModifier { get; set; } = 1f;

        /// <summary>
        /// Global tourism yield modifier
        /// </summary>
        public float YieldTourismModifier { get; set; } = 1f;
        #endregion

        #region other global modifiers
        /// <summary>
        /// Global worker speed modifier
        /// </summary>
        public float WorkerSpeedModifier { get; set; } = 1f;

        /// <summary>
        /// Global trade route effectivness modifier
        /// </summary>
        public float TradeRouteModifier { get; set; } = 1f;

        /// <summary>
        /// Policy cost modifier
        /// </summary>
        public float PolicyCostModifier { get; set; } = 1f;

        /// <summary>
        /// Global unit upgrade cost modifier
        /// </summary>
        public float UnitUpgradeCostModifier { get; set; } = 1f;

        /// <summary>
        /// Amount of experience granted to new units
        /// </summary>
        public int BaseUnitExperienceGrant { get; set; } = 0;

        /// <summary>
        /// Global tile culture cost modifier
        /// </summary>
        public float TileCultureCostModifier { get; set; } = 1f;

        /// <summary>
        /// Global tile buy cost modifier
        /// </summary>
        public float TileBuyCostModifier { get; set; } = 1f;

        /// <summary>
        /// Global city defense modifier
        /// </summary>
        public float DefenseModifier { get; set; } = 1f;
        #endregion

        #region available free items
        /// <summary>
        /// Number of free great people able to be chosen
        /// </summary>
        public int FreeGreatPeopleAvailable { get; set; } = 1;

        /// <summary>
        /// Number of free techs able to be chosen
        /// </summary>
        public int FreeTechsAvailable { get; set; } = 1;

        /// <summary>
        /// Number of free social policies able to be chosen
        /// </summary>
        public int FreeSocialPoliciesAvailable { get; set; } = 1;
        #endregion

        public Player(Connection conn, int playerD, string empireID, string username, TechnologyTree blankTechManager)
        {
            Connection = conn;
            CurrentEra = GameEra.Ancient;
            InstanceID = playerD;
            EmpireID = empireID;
            Name = username;
            TechTree = blankTechManager;
        }

    }
}
