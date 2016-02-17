using ArwicEngine.Core;
using ArwicEngine.Net;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public event EventHandler<PlayerEventArgs> PlayerAdded;
        public event EventHandler<PlayerEventArgs> PlayerRemoved;
        public event EventHandler<PlayerEventArgs> PlayerUpdated;
        public event EventHandler TurnStateChanged;
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
        protected virtual void OnTurnStateChanged(EventArgs e)
        {
            if (TurnStateChanged != null)
                TurnStateChanged(this, e);
        }

        public PlayerController(ControllerManager manager)
            : base(manager)
        {
            players = new List<Player>();
        }

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
                        player.SelectedTechNodeID = (int)cmd.Arguments[0];
                        break;
                }
            }
            catch (Exception)
            {
                //Engine.Console.WriteLine($"CityCommand.{cmd.CommandID.ToString()} Error: malformed data", MsgType.ServerWarning);
            }

            OnPlayerUpdated(new PlayerEventArgs(player));
        }

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
                player.Happiness += city.IncomeHappiness;
                player.IncomeScience += city.IncomeScience;
            }
        }

        private void ProcessPlayerResearch(Player player)
        {
            if (player.SelectedTechNodeID == -1)
                return;

            TechNode currentNode = player.TechTree.GetNode(player.SelectedTechNodeID);
            currentNode.Progress += player.ScienceOverflow;
            currentNode.Progress += player.IncomeScience;
            if (currentNode.Progress >= currentNode.ResearchCost)
            {
                currentNode.Unlocked = true;
                player.SelectedTechNodeID = -1;
                player.ScienceOverflow += currentNode.Progress - currentNode.ResearchCost;
            }
        }

        public void AddPlayer(Connection connection, string userName)
        {
            Player player = new Player(connection, players.Count, 0, userName, TechTree.FromFile("Content/Data/TechnologyList.xml"));
            player.EmpireID = RandomHelper.Next(0, Controllers.Factory.Empire.Empires.Count);
            player.TechTree.GetNode(0).Unlocked = true;
            players.Add(player);
            OnPlayerAdded(new PlayerEventArgs(player));
        }

        public void RemovePlayer(int playerID)
        {
            Player player = GetPlayer(playerID);
            players.Remove(player);
            OnPlayerRemoved(new PlayerEventArgs(player));
        }

        public List<Player> GetAllPlayers()
        {
            return players;
        }

        public Player GetPlayer(Connection conn)
        {
            return players.Find(p => p.Connection == conn);
        }

        public Player GetPlayer(int id)
        {
            return players.Find(p => p.InstanceID == id);
        }

        public void UpdateTurnState(Player player, bool turnEnded)
        {
            player.EndedTurn = turnEnded;
            OnTurnStateChanged(EventArgs.Empty);
        }
    }
}
