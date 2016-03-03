﻿// Dominion - Copyright (C) Timothy Ings
// PlayerController.cs
// This file defines classes that defines the player controller

using ArwicEngine.Core;
using ArwicEngine.Net;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Dominion.Common.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dominion.Server.Controllers
{
    public class PlayerEventArgs : EventArgs
    {
        public Player Player { get; }

        public PlayerEventArgs(Player player)
        {
            Player = player;
        }
    }

    public class PlayerController : Controller
    {
        private List<Player> players;

        /// <summary>
        /// Occurs when a player is added to the player controller
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerAdded;

        /// <summary>
        /// Occurs when a player is removed from the player controller
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerRemoved;

        /// <summary>
        /// Occurs when a player managed by the player controller is updated
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerUpdated;

        /// <summary>
        /// Occurs when the turn state of a player managed by the player controller has changed
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerTurnStateChanged;

        protected virtual void OnPlayerAdded(PlayerEventArgs e)
        {
            if (PlayerAdded != null)
                PlayerAdded(this, e);
        }

        protected virtual void OnPlayerRemoved(PlayerEventArgs e)
        {
            if (PlayerRemoved != null)
                PlayerRemoved(this, e);
        }

        protected virtual void OnPlayerUpdated(PlayerEventArgs e)
        {
            if (PlayerUpdated != null)
                PlayerUpdated(this, e);
        }

        protected virtual void OnTurnStateChanged(PlayerEventArgs e)
        {
            if (PlayerTurnStateChanged != null)
                PlayerTurnStateChanged(this, e);
        }

        public PlayerController(ControllerManager manager)
            : base(manager)
        {
            players = new List<Player>();
            Controllers.City.CityUpdated += City_CityUpdated;
            Controllers.City.CityCaptured += City_CityCaptured;
            Controllers.City.CityBorderExpanded += City_CityBorderExpanded;
        }

        private void City_CityBorderExpanded(object sender, TileEventArgs e)
        {
            Tile tile = e.Tiles.First();
            if (tile == null)
                return;
            Player player = GetPlayer(Controllers.City.GetCity(tile.CityID).PlayerID);
            CalculateIncome(player);
            OnPlayerUpdated(new PlayerEventArgs(player));
        }

        private void City_CityCaptured(object sender, CityEventArgs e)
        {
            Player player = GetPlayer(e.City.PlayerID);
            CalculateIncome(player);
            OnPlayerUpdated(new PlayerEventArgs(player));
        }

        private void City_CityUpdated(object sender, CityEventArgs e)
        {
            Player player = GetPlayer(e.City.PlayerID);
            CalculateIncome(player);
            OnPlayerUpdated(new PlayerEventArgs(player));
        }

        /// <summary>
        /// Prepares the players managed by the player manager for the next turn
        /// </summary>
        public override void ProcessTurn()
        {
            foreach (Player player in players)
            {
                CalculateIncome(player);
                player.EndedTurn = false;
                player.Gold += player.IncomeGold;
                player.Culture += player.IncomeCulture;
                ProcessPlayerResearch(player);
                OnPlayerUpdated(new PlayerEventArgs(player));
            }
        }

        /// <summary>
        /// Issues a command to a player 
        /// </summary>
        /// <param name="cmd"></param>
        public void CommandPlayer(PlayerCommand cmd)
        {
            Player player = GetPlayer(cmd.PlayerID);
            if (player == null)
                return;

            try
            {
                switch (cmd.CommandID)
                {
                    case PlayerCommandID.SelectTech:
                        player.SelectedTechNodeID = (string)cmd.Arguments[0];
                        break;
                }
            }
            catch (Exception)
            {
                //Engine.Console.WriteLine($"CityCommand.{cmd.CommandID.ToString()} Error: malformed data", MsgType.ServerWarning);
            }

            OnPlayerUpdated(new PlayerEventArgs(player));
        }

        // calculates the income of the given player
        private void CalculateIncome(Player player)
        {
            player.IncomeCulture = 0;
            player.IncomeGold = 0;
            player.Happiness = 0;
            player.IncomeScience = 0;

            List<City> playerCities = Controllers.City.GetPlayerCities(player.InstanceID);
            foreach (City city in playerCities)
            {
                player.IncomeCulture += city.IncomeCulture;
                player.IncomeGold += city.IncomeGold;
                //player.Happiness += city.IncomeHappiness;
                player.IncomeScience += city.IncomeScience;
            }
        }

        // processes the given players research
        private void ProcessPlayerResearch(Player player)
        {
            if (player.SelectedTechNodeID == "TECH_NULL") // don't process a research node if it doesn't exist
                return;

            // get the selected node
            Technology currentTech = player.TechTree.GetTech(player.SelectedTechNodeID);
            // add the player's overflow and income to the nodes progress
            currentTech.Progress += player.ScienceOverflow;
            currentTech.Progress += player.IncomeScience;
            // check if the node has been completed
            if (currentTech.Progress >= currentTech.ResearchCost)
            {
                currentTech.Unlocked = true; // mark the node as unlocked
                player.SelectedTechNodeID = "TECH_NULL"; // TODO make this work with multiple tech nodes selected
                player.ScienceOverflow += currentTech.Progress - currentTech.ResearchCost; // add the left over science to the player's overflow
            }
        }

        /// <summary>
        /// Creates a new player that is managed by the player controller
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userName"></param>
        public void AddPlayer(Connection connection, string userName)
        {
            Player player = new Player(connection, players.Count, "NULL", userName, Controllers.Data.Tech.GetNewTree());
            player.EmpireID =  Controllers.Data.Empire.GetAllEmpires().ElementAt(RandomHelper.Next(0, Controllers.Data.Empire.EmpireCount)).Name;
            player.TechTree.GetTech("TECH_AGRICULTURE").Unlocked = true;
            players.Add(player);
            OnPlayerAdded(new PlayerEventArgs(player));
        }

        /// <summary>
        /// Removes a player from the player controller
        /// </summary>
        /// <param name="playerID"></param>
        public void RemovePlayer(int playerID)
        {
            Player player = GetPlayer(playerID);
            players.Remove(player);
            OnPlayerRemoved(new PlayerEventArgs(player));
        }

        /// <summary>
        /// Gets all the players managed by the player controller
        /// </summary>
        /// <returns></returns>
        public List<Player> GetAllPlayers()
        {
            return players;
        }

        /// <summary>
        /// Gets the player with the given connection
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public Player GetPlayer(Connection conn)
        {
            return players.Find(p => p.Connection == conn);
        }

        /// <summary>
        /// Gets the player with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Player GetPlayer(int id)
        {
            return players.Find(p => p.InstanceID == id);
        }

        /// <summary>
        /// Updates the turn state of the given player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="turnEnded"></param>
        public void UpdateTurnState(Player player, bool turnEnded)
        {
            player.EndedTurn = turnEnded;
            OnTurnStateChanged(new PlayerEventArgs(player));
        }
    }
}
