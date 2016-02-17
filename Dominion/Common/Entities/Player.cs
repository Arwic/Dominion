using ArwicEngine.Net;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
using Dominion.Server;
using System;
using System.Collections.Generic;

namespace Dominion.Common.Entities
{
    public enum GameEra
    {
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
        public int PlayerID { get; set; }
        public string Name { get; set; }
        public int EmpireID { get; set; }
        public bool Host { get; set; }
        public int Score { get; set; }
        public bool EndedTurn { get; set; }

        public BasicPlayer(Player player)
        {
            Name = player.Name;
            PlayerID = player.InstanceID;
            EmpireID = player.EmpireID;
            EndedTurn = player.EndedTurn;
        }
    }

    [Serializable()]
    public class Player
    {
        public int InstanceID { get; set; }
        public string Name { get; set; }
        public int EmpireID { get; set; }
        [NonSerialized()]
        private Connection _connection;
        public Connection Connection { get { return _connection; } set { _connection = value; } }
        public int Score { get; set; }
        public bool EndedTurn { get; set; }
        public GameEra CurrentEra { get; set; }
        public TechTree TechTree { get; set; }
        public int SelectedTechNodeID { get; set; }
        public int ScienceOverflow { get; set; }

        // Income
        public int IncomeGold { get; set; }
        public int IncomeScience { get; set; }
        public int IncomeCulture { get; set; }
        public int IncomeTourism { get; set; }
        public int IncomeFaith { get; set; }

        // Resources
        public int Gold { get; set; }
        public int Happiness { get; set; }
        public int Culture { get; set; }
        public int Faith { get; set; }

        // Strategic Resources
        public int Iron { get; set; }
        public int Horses { get; set; }
        public int Coal { get; set; }
        public int Oil { get; set; }
        public int Aluminium { get; set; }
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
