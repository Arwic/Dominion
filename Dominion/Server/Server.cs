// Dominion - Copyright (C) Timothy Ings
// Server.cs
// This file defines classes that defines the server

using ArwicEngine.Core;
using ArwicEngine.Net;
using Dominion.Common;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Dominion.Server.Controllers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Dominion.Server
{
    public enum ServerState
    {
        Lobby,
        Game
    }

    public class Server
    {
        /// <summary>
        /// Indicates whether the server is currently running
        /// </summary>
        public bool Running => server == null ? false : server.Running;

        /// <summary>
        /// Gets or sets the server's lobby state
        /// </summary>
        public LobbyState LobbyState { get; private set; }
        
        /// <summary>
        /// Gets an object that tracks net server statistics
        /// </summary>
        public NetServerStats ServerStatistics => server.Statistics;

        /// <summary>
        /// Gets or sets the server's password
        /// </summary>
        public string ServerPassword { get; private set; }

        /// <summary>
        /// Gets or sets a has set of all the address's the server has banned
        /// </summary>
        public HashSet<string> BannedAddresses { get; private set; }

        /// <summary>
        /// Gets or sets the state of the server
        /// </summary>
        public ServerState ServerState { get; private set; }

        /// <summary>
        /// Gets or sets a list of the enabled victory types
        /// </summary>
        public List<VictoryType> VictoryTypes { get; private set; }

        /// <summary>
        /// Gets or sets the turn timer
        /// </summary>
        public Stopwatch TurnTimer { get; private set; }

        /// <summary>
        /// Gets or sets the turn time limit
        /// </summary>
        public int TurnTimeLimit { get; private set; }

        /// <summary>
        /// Gets or sets the turn number
        /// </summary>
        public int TurnNumber { get; private set; }

        private ControllerManager controllers;
        private NetServer server;

        public Server()
        {
        }

        /// <summary>
        /// Starts the server and begins listening on the given port
        /// Will reject any connections with the incorrect password
        /// </summary>
        /// <param name="port"></param>
        /// <param name="password"></param>
        public void StartServer(int port, string password)
        {
            // init properties
            BannedAddresses = new HashSet<string>();
            ServerPassword = password;
            ServerState = ServerState.Lobby;
            LobbyState = new LobbyState();
            server = new NetServer();

            // register events
            server.PacketRecieved += Server_PacketRecieved;
            server.ConnectionLost += Server_ConnectionLost;

            // setart the net server
            server.Start(port);

            // init game controllers and register events
            controllers = new ControllerManager();
            controllers.Board.TilesUpdated += Board_TilesUpdated;
            controllers.City.CityCaptured += City_CityCaptured;
            controllers.City.CitySettled += City_CitySettled;
            controllers.City.CityUpdated += City_CityUpdated;
            controllers.City.CityBorderExpanded += City_CityBorderExpanded;
            controllers.Player.PlayerAdded += Player_PlayerAdded;
            controllers.Player.PlayerRemoved += Player_PlayerRemoved;
            controllers.Player.PlayerUpdated += Player_PlayerUpdated;
            controllers.Player.PlayerTurnStateChanged += Player_TurnStateChanged;
            controllers.Unit.UnitAdded += Unit_UnitAdded;
            controllers.Unit.UnitRemoved += Unit_UnitRemoved;
            controllers.Unit.UnitUpdated += Unit_UnitUpdated;
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void StopServer()
        {
            if (server == null)
                return;
            Packet pOut = new Packet((int)PacketHeader.LobbyKick, "Server closed");
            server.SendDataToAll(pOut);
            server.Stop(1000);
        }

        private void Server_PacketRecieved(object sender, PacketRecievedEventArgs e)
        {
            RoutePacket(e.Packet);
        }

        private void Server_ConnectionLost(object sender, ConnectionEventArgs e)
        {
            if (controllers.Player.GetAllPlayers().Count == 0)
                StopServer();

            Player player = controllers.Player.GetPlayer(e.Connection);
            controllers.Player.RemovePlayer(player.InstanceID);
            if (ServerState == ServerState.Lobby)
                SendLobbyStateToAll();
        }

        /// <summary>
        /// Signals to all clients the game has begun and prevents new connections
        /// </summary>
        public void StartGame()
        {
            VictoryTypes = new List<VictoryType>();
            for (int i = 0; i < LobbyState.VictoryTypes.Length; i++)
                if (LobbyState.VictoryTypes[i])
                    VictoryTypes.Add((VictoryType)i);

            ServerState = ServerState.Game;
            TurnNumber = 0;
            TurnTimeLimit = int.MaxValue;
            TurnTimer = new Stopwatch();

            controllers.Board.GenerateBoard(LobbyState.WorldType, LobbyState.WorldSize);

            foreach (Player player in controllers.Player.GetAllPlayers())
            {
                controllers.Unit.AddUnit("UNIT_SETTLER", player.InstanceID, GetStartTile(player).Location);
            }

            foreach (Player player in controllers.Player.GetAllPlayers().ToArray())
            {
                Packet pStartGame = new Packet((int)PacketHeader.LobbyStartGame,
                    controllers.Board.Board,
                    player,
                    controllers.City.GetAllCities(),
                    controllers.Unit.GetAllUnits());
                server.SendData(pStartGame, player.Connection);
            }

            EndTurn(TurnEndReason.HostForced);

            Thread loopThread = new Thread(UpdateLoop);
            loopThread.Start();
        }

        // picks a start tile for the given player, respecting their start bias if required
        private Tile GetStartTile(Player player)
        {
            Tile startTile = controllers.Board.GetTile(RandomHelper.Next(0, controllers.Board.DimX), RandomHelper.Next(0, controllers.Board.DimY));
            if (LobbyState.OtherOptions[(int)LobbyOtherOption.DisableStartBias])
            {
                while (!startTile.Land)
                    startTile = controllers.Board.GetTile(RandomHelper.Next(0, controllers.Board.DimX), RandomHelper.Next(0, controllers.Board.DimY));
            }
            else
            {
                int biasFindAttempts = 0;
                int maxBiasFindAttempts = 500;
                Empire empire = controllers.Data.Empire.GetEmpire(player.EmpireID);
                bool biasMet = false;
                switch (empire.StartBias)
                {
                    case EmpireStartBiasFlags.None:
                        while (!startTile.Land)
                            startTile = GetRandomTile();
                        break;
                    case EmpireStartBiasFlags.Coast:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            List<Point> neighbours = startTile.GetNeighbourTileLocations();
                            foreach (Point nLoc in neighbours)
                            {
                                Tile n = controllers.Board.GetTile(nLoc);
                                if (n == null)
                                    continue;
                                if (n.TerrainBase == TileTerrainBase.COAST)
                                {
                                    biasMet = true;
                                    break;
                                }
                            }

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Desert:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            if (startTile.TerrainBase == TileTerrainBase.DESERT)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Jungle:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            if (startTile.Improvement == TileImprovment.JUNGLE)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Forest:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            if (startTile.Improvement == TileImprovment.FOREST)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Grassland:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            if (startTile.TerrainBase == TileTerrainBase.GRASSLAND)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Tundra:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            if (startTile.TerrainBase == TileTerrainBase.TUNDRA)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Plains:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            if (startTile.TerrainBase == TileTerrainBase.PLAINS)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Hill:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            if (startTile.TerrainFeature == TileTerrainFeature.HILL)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Avoid_Forest:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            List<Point> neighbours = startTile.GetNeighbourTileLocations();
                            bool forest = true;
                            foreach (Point nLoc in neighbours)
                            {
                                Tile n = controllers.Board.GetTile(nLoc);
                                if (n == null)
                                    continue;
                                if (n.Improvement == TileImprovment.FOREST)
                                {
                                    forest = true;
                                    break;
                                }
                            }
                            if (!forest)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Avoid_Jungle:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            List<Point> neighbours = startTile.GetNeighbourTileLocations();
                            bool jungle = true;
                            foreach (Point nLoc in neighbours)
                            {
                                Tile n = controllers.Board.GetTile(nLoc);
                                if (n == null)
                                    continue;
                                if (n.Improvement == TileImprovment.JUNGLE)
                                {
                                    jungle = true;
                                    break;
                                }
                            }
                            if (!jungle)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Avoid_ForestAndJungle:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            List<Point> neighbours = startTile.GetNeighbourTileLocations();
                            bool forestOrJungle = true;
                            foreach (Point nLoc in neighbours)
                            {
                                Tile n = controllers.Board.GetTile(nLoc);
                                if (n == null)
                                    continue;
                                if (n.Improvement == TileImprovment.JUNGLE)
                                {
                                    forestOrJungle = true;
                                    break;
                                }
                            }
                            if (!forestOrJungle)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Avoid_Hill:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            List<Point> neighbours = startTile.GetNeighbourTileLocations();
                            bool hill = true;
                            foreach (Point nLoc in neighbours)
                            {
                                Tile n = controllers.Board.GetTile(nLoc);
                                if (n == null)
                                    continue;
                                if (n.TerrainFeature == TileTerrainFeature.HILL)
                                {
                                    hill = true;
                                    break;
                                }
                            }
                            if (!hill)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                    case EmpireStartBiasFlags.Avoid_Tundra:
                        while (!biasMet)
                        {
                            startTile = GetRandomTile();
                            if (!startTile.Land)
                                continue;
                            List<Point> neighbours = startTile.GetNeighbourTileLocations();
                            bool tundra = true;
                            foreach (Point nLoc in neighbours)
                            {
                                Tile n = controllers.Board.GetTile(nLoc);
                                if (n == null)
                                    continue;
                                if (n.TerrainBase == TileTerrainBase.TUNDRA)
                                {
                                    tundra = true;
                                    break;
                                }
                            }
                            if (!tundra)
                                biasMet = true;

                            biasFindAttempts++;
                            if (biasFindAttempts > maxBiasFindAttempts)
                                break;
                        }
                        break;
                }
            }

            if (startTile.GetMovementCost() == -1)
                startTile = GetStartTile(player);

            return startTile;
        }

        // gets a random tile on the board
        private Tile GetRandomTile()
        {
            return controllers.Board.GetTile(RandomHelper.Next(0, controllers.Board.DimX), RandomHelper.Next(0, controllers.Board.DimY));
        }

        // loop thread: runs processes that can't be event driven
        private void UpdateLoop()
        {
            TurnTimer.Start();
            while (Running)
            {
                if (TurnTimer.ElapsedMilliseconds > TurnTimeLimit)
                {
                    foreach (Player player in controllers.Player.GetAllPlayers())
                        player.EndedTurn = true;
                    EndTurn(TurnEndReason.TimeOut);
                    TurnTimer.Restart();
                }
            }
        }

        /// <summary>
        /// Bans the player with the given id
        /// </summary>
        /// <param name="playerID"></param>
        public void BanPlayer(int playerID)
        {
            Player player = controllers.Player.GetPlayer(playerID);
            if (player == null)
                return;
            string bannedAddress = player.Connection.Address.Split(':')[0];
            BannedAddresses.Add(bannedAddress);
            controllers.Player.RemovePlayer(playerID);
            Packet pOut = new Packet((int)PacketHeader.LobbyBan, "Host banned you");
            server.SendData(pOut, player.Connection);
            player.Connection.Close(1000);
            ConsoleManager.Instance.WriteLine($"Banned {bannedAddress}", MsgType.ServerInfo);
        }

        /// <summary>
        /// Kicks the player with the given id
        /// </summary>
        /// <param name="playerID"></param>
        public void KickPlayer(int playerID)
        {
            Player player = controllers.Player.GetPlayer(playerID);
            if (player == null)
                return;
            controllers.Player.RemovePlayer(playerID);
            Packet pOut = new Packet((int)PacketHeader.LobbyKick, "Host kicked you");
            server.SendData(pOut, player.Connection);
            player.Connection.Close(1000);
            ConsoleManager.Instance.WriteLine($"Kicked {player.Connection.Address.Split(':')}", MsgType.ServerInfo);
        }

        private void Unit_UnitUpdated(object sender, UnitEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.UnitUpdate, e.Unit);
            server.SendDataToAll(pOut);
        }

        private void Unit_UnitRemoved(object sender, UnitEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.UnitRemoved, e.Unit.InstanceID);
            server.SendDataToAll(pOut);
        }

        private void Unit_UnitAdded(object sender, UnitEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.UnitAdded, e.Unit);
            server.SendDataToAll(pOut);
        }

        private void Player_TurnStateChanged(object sender, PlayerEventArgs e)
        {
            if (IsTurnOver())
                EndTurn(TurnEndReason.PlayersEnded);
        }

        private void Player_PlayerUpdated(object sender, PlayerEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.PlayerUpdate, e.Player);
            server.SendData(pOut, e.Player.Connection);
        }

        private void Player_PlayerRemoved(object sender, PlayerEventArgs e)
        {
            SendLobbyStateToAll();
        }

        private void Player_PlayerAdded(object sender, PlayerEventArgs e)
        {
            Packet pOutLobbyInit = new Packet((int)PacketHeader.LobbyInit,
                e.Player,
                controllers.Data.Building,
                controllers.Data.Empire,
                controllers.Data.Unit,
                controllers.Data.Tech);
            server.SendData(pOutLobbyInit, e.Player.Connection);

            SendLobbyStateToAll();
        }

        private void City_CityBorderExpanded(object sender, TileEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.TileUpdate, e.Tiles);
            server.SendDataToAll(pOut);
        }

        private void City_CityUpdated(object sender, CityEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.CityUpdate, e.City);
            server.SendDataToAll(pOut);
        }

        private void City_CitySettled(object sender, CityEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.CityAdded, e.City);
            server.SendDataToAll(pOut);
        }

        private void City_CityCaptured(object sender, CityEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.CityUpdate, e.City);
            server.SendDataToAll(pOut);
        }

        private void Board_TilesUpdated(object sender, TileEventArgs e)
        {
            Packet pOut = new Packet((int)PacketHeader.TileUpdate, e.Tiles);
            server.SendDataToAll(pOut);
        }

        // routes an incoming packet
        private void RoutePacket(Packet p)
        {
            PacketHeader header = (PacketHeader)p.Header;
            ConsoleManager.Instance.WriteLine($"Parsing a packet with header {header}", MsgType.ServerInfo);
            try
            {
                switch (header)
                {
                    case PacketHeader.LobbyInit: // Desired username
                        ParseLobbyInit(p);
                        break;
                    case PacketHeader.LobbyEmpire: // Desired empire id
                        ParseLobbyEmpire(p);
                        break;
                    case PacketHeader.TurnState: // Desired turn state, ended/not ended
                        ParseTurnState(p);
                        break;
                    case PacketHeader.UnitCommand: // A command to execute on a unit
                        ParseUnitCommand(p);
                        break;
                    case PacketHeader.CityCommand: // A command to execute on a city
                        ParseCityCommand(p);
                        break;
                    case PacketHeader.PlayerCommand: // A command to execute on a player
                        ParsePlayerCommand(p);
                        break;
                }
            }
            catch (Exception e)
            {
                ConsoleManager.Instance.WriteLine($"Error parsing {(PacketHeader)p.Header}, {e.Message}", MsgType.ServerFailed);
            }
        }

        // parses a LobbyInit packet
        private void ParseLobbyInit(Packet p)
        {
            if (ServerState == ServerState.Game)
            {
                Packet pOut = new Packet((int)PacketHeader.LobbyKick, "Game has already started");
                server.SendData(pOut, p.Sender);
            }

            // Create a new player
            int i = 0;
            string userName = (string)p.Items[i++];
            string userPassword = (string)p.Items[i++];
            if (BannedAddresses.Contains(p.Sender.Address.Split(':')[0]))
            {
                Packet pOut = new Packet((int)PacketHeader.LobbyKick, "Your are banned");
                server.SendData(pOut, p.Sender);
                return;
            }
            if (userPassword != ServerPassword)
            {
                Packet pOut = new Packet((int)PacketHeader.LobbyKick, "Incorrect password");
                server.SendData(pOut, p.Sender);
                return;
            }

            controllers.Player.AddPlayer(p.Sender, userName);

            // Sync lobby state
            SendLobbyStateToAll();
        }

        // parses a LobbyEmpire packet
        private void ParseLobbyEmpire(Packet p)
        {
            // Find the player
            Player player = controllers.Player.GetPlayer(p.Sender);
            if (player == null)
                return;
            
            // Update their desired empire
            string emprieID = (string)p.Item;
            player.EmpireID = emprieID;

            // Sync lobby state
            SendLobbyStateToAll();
        }

        // parses a TurnState packet
        private void ParseTurnState(Packet p)
        {
            Player player = controllers.Player.GetPlayer(p.Sender);
            bool turnState = (bool)p.Item;
            if (player == null)
                return;
            controllers.Player.UpdateTurnState(player, turnState);
        }

        // parses a UnitCommand packet
        private void ParseUnitCommand(Packet p)
        {
            UnitCommand cmd = (UnitCommand)p.Item;
            controllers.Unit.CommandUnit(cmd);
        }

        // parses a CityCommand packet
        private void ParseCityCommand(Packet p)
        {
            CityCommand cmd = (CityCommand)p.Item;
            cmd.PlayerID = controllers.Player.GetPlayer(p.Sender).InstanceID;
            controllers.City.CommandCity(cmd);
        }

        // parses a PlayerCommand packet
        private void ParsePlayerCommand(Packet p)
        {
            PlayerCommand cmd = (PlayerCommand)p.Item;
            cmd.PlayerID = controllers.Player.GetPlayer(p.Sender).InstanceID;
            controllers.Player.CommandPlayer(cmd);
        }

        // checks if the game is over, respecting the selected victory types
        private Tuple<bool, Player, VictoryType> IsGameOver()
        {
            if (VictoryTypes.Contains(VictoryType.Culture))
            {
                // TODO implement this after tourism is implemented
            }
            if (VictoryTypes.Contains(VictoryType.Diplomatic))
            {
                // TODO implement this after world parliament is implemented
            }
            if (VictoryTypes.Contains(VictoryType.Domination))
            {
                Player winner = null;
                int playersLeft = 0;
                foreach (Player player in controllers.Player.GetAllPlayers())
                {
                    if (controllers.City.GetPlayerCities(player.InstanceID).Count > 0 || controllers.Unit.GetPlayerUnits(player.InstanceID).Count > 0)
                    {
                        playersLeft++;
                        winner = player;
                    }
                }

                if (playersLeft == 1)
                    return new Tuple<bool, Player, VictoryType>(true, winner, VictoryType.Domination);
            }
            if (VictoryTypes.Contains(VictoryType.Science))
            {
                // TODO implement this after space shuttle can be built
            }
            return new Tuple<bool, Player, VictoryType>(false, null, 0);
        }

        // checks whether the current turn is over
        private bool IsTurnOver()
        {
            int playersEndedTurnCount = 0;
            foreach (Player player in controllers.Player.GetAllPlayers())
            {
                if (player.EndedTurn)
                    playersEndedTurnCount++;
            }
            if (playersEndedTurnCount == controllers.Player.GetAllPlayers().Count)
                return true;
            return false;
        }

        // ends the current turn
        private void EndTurn(TurnEndReason reason)
        {
            ConsoleManager.Instance.WriteLine($"Turn {TurnNumber} has ended, reason {reason}", MsgType.ServerInfo);

            controllers.ProcessTurn();
            TurnNumber++;
            TurnTimer.Restart();

            ConsoleManager.Instance.WriteLine($"Turn {TurnNumber} has begun", MsgType.ServerInfo);
            foreach (Player player in controllers.Player.GetAllPlayers())
            {
                Packet p = new Packet((int)PacketHeader.TurnData,
                    TurnNumber,
                    TurnTimeLimit,
                    reason,
                    player,
                    controllers.City.GetAllCities(),
                    controllers.Unit.GetAllUnits()
                );

                server.SendData(p, player.Connection);
            }

            //// TODO this is where we check for game over
            //Tuple<bool, Player, VictoryType> res = IsGameOver();
            //if (res.Item1)
            //    SendGameOver(res.Item2, res.Item3);
        }

        // sends the lobby state to all connected clients
        public void SendLobbyStateToAll()
        {
            LobbyState.Players.Clear();
            foreach (Player player in controllers.Player.GetAllPlayers())
                LobbyState.Players.Add(new BasicPlayer(player));
            Packet pOutLobbySync = new Packet((int)PacketHeader.LobbyStateSync, LobbyState);
            server.SendDataToAll(pOutLobbySync);
        }

        // tells every client that the game is over
        private void SendGameOver(Player winner, VictoryType vtype)
        {
            ConsoleManager.Instance.WriteLine($"Game over! {winner.Name}, {controllers.Data.Empire.GetEmpire(winner.EmpireID).DisplayName}, has won with a {vtype} victory", MsgType.ServerInfo);
            Packet pOut = new Packet((int)PacketHeader.GameOver, winner, vtype);
            server.SendDataToAll(pOut);
            StopServer();
        }
    }
}
