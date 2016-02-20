using ArwicEngine.Core;
using ArwicEngine.Net;
using Dominion.Common;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
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
        public Engine Engine { get; }
        public bool Running => server == null ? false : server.Running;
        public LobbyState LobbyState { get; private set; }
        private NetServer server;
        public NetServerStats ServerStatistics => server.Statistics;
        public string ServerPassword { get; private set; }
        public HashSet<string> BannedAddresses { get; private set; }
        public ServerState ServerState { get; private set; }
        public List<VictoryType> VictoryTypes { get; private set; }
        private ControllerManager controllers;
        public Stopwatch TurnTimer { get; private set; }
        public int TurnTimeLimit { get; private set; }
        public int TurnNumber { get; private set; }

        public Server(Engine engine)
        {
            Engine = engine;
        }

        // Initialisation & Finalisation
        public void StartServer(int port, string password)
        {
            BannedAddresses = new HashSet<string>();
            ServerPassword = password;
            ServerState = ServerState.Lobby;
            LobbyState = new LobbyState();
            server = new NetServer(Engine);
            server.PacketRecieved += Server_PacketRecieved;
            server.ConnectionLost += Server_ConnectionLost;
            server.Start(port);

            controllers = new ControllerManager();
            controllers.Board.TilesUpdated += Board_TilesUpdated;
            controllers.City.CityCaptured += City_CityCaptured;
            controllers.City.CitySettled += City_CitySettled;
            controllers.City.CityUpdated += City_CityUpdated;
            controllers.City.CityBorderExpanded += City_CityBorderExpanded;
            controllers.Player.PlayerAdded += Player_PlayerAdded;
            controllers.Player.PlayerRemoved += Player_PlayerRemoved;
            controllers.Player.PlayerUpdated += Player_PlayerUpdated;
            controllers.Player.TurnStateChanged += Player_TurnStateChanged;
            controllers.Unit.UnitAdded += Unit_UnitAdded;
            controllers.Unit.UnitRemoved += Unit_UnitRemoved;
            controllers.Unit.UnitUpdated += Unit_UnitUpdated;
        }

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
            ParsePacket(e.Packet);
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
                controllers.Unit.AddUnit(0, player.InstanceID, GetStartTile(player).Location);
            }

            foreach (Player player in controllers.Player.GetAllPlayers().ToArray())
            {
                Packet pStartGame = new Packet((int)PacketHeader.LobbyStartGame,
                    controllers.Board.GetBoard(),
                    player,
                    controllers.City.GetAllCities(),
                    controllers.Unit.GetAllUnits());
                server.SendData(pStartGame, player.Connection);
            }

            EndTurn(TurnEndReason.HostForced);

            Thread loopThread = new Thread(UpdateLoop);
            loopThread.Start();
        }
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
                Empire empire = controllers.Factory.Empire.GetEmpire(player.EmpireID);
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
                                if (n.TerrainBase == TileTerrainBase.Coast)
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
                            if (startTile.TerrainBase == TileTerrainBase.Desert)
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
                            if (startTile.Improvement == TileImprovment.Jungle)
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
                            if (startTile.Improvement == TileImprovment.Forest)
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
                            if (startTile.TerrainBase == TileTerrainBase.Grassland)
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
                            if (startTile.TerrainBase == TileTerrainBase.Tundra)
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
                            if (startTile.TerrainBase == TileTerrainBase.Plains)
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
                            if (startTile.TerrainFeature == TileTerrainFeature.Hill)
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
                                if (n.Improvement == TileImprovment.Forest)
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
                                if (n.Improvement == TileImprovment.Jungle)
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
                                if (n.Improvement == TileImprovment.Jungle)
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
                                if (n.TerrainFeature == TileTerrainFeature.Hill)
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
                                if (n.TerrainBase == TileTerrainBase.Tundra)
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
        private Tile GetRandomTile()
        {
            return controllers.Board.GetTile(RandomHelper.Next(0, controllers.Board.DimX), RandomHelper.Next(0, controllers.Board.DimY));
        }

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
            Engine.Console.WriteLine($"Banned {bannedAddress}", MsgType.ServerInfo);
        }
        public void KickPlayer(int playerID)
        {
            Player player = controllers.Player.GetPlayer(playerID);
            if (player == null)
                return;
            controllers.Player.RemovePlayer(playerID);
            Packet pOut = new Packet((int)PacketHeader.LobbyKick, "Host kicked you");
            server.SendData(pOut, player.Connection);
            player.Connection.Close(1000);
            Engine.Console.WriteLine($"Kicked {player.Connection.Address.Split(':')}", MsgType.ServerInfo);
        }

        // React to changes
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
        private void Player_TurnStateChanged(object sender, EventArgs e)
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
                controllers.Factory.Building,
                controllers.Factory.Empire,
                controllers.Factory.Production,
                controllers.Factory.Unit);
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

        // Packet parser
        private void ParsePacket(Packet p)
        {
            PacketHeader header = (PacketHeader)p.Header;
            Engine.Console.WriteLine($"Parsing a packet with header {header}", MsgType.ServerInfo);
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
                Engine.Console.WriteLine($"Error parsing {(PacketHeader)p.Header}, {e.Message}", MsgType.ServerFailed);
            }
        }
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
        private void ParseLobbyEmpire(Packet p)
        {
            // Find the player
            Player player = controllers.Player.GetPlayer(p.Sender);
            if (player == null)
                return;
            
            // Update their desired empire
            int emprieID = (int)p.Item;
            player.EmpireID = emprieID;

            // Sync lobby state
            SendLobbyStateToAll();
        }
        private void ParseTurnState(Packet p)
        {
            Player player = controllers.Player.GetPlayer(p.Sender);
            bool turnState = (bool)p.Item;
            if (player == null)
                return;
            controllers.Player.UpdateTurnState(player, turnState);
        }
        private void ParseUnitCommand(Packet p)
        {
            UnitCommand cmd = (UnitCommand)p.Item;
            controllers.Unit.CommandUnit(cmd);
        }
        private void ParseCityCommand(Packet p)
        {
            CityCommand cmd = (CityCommand)p.Item;
            cmd.PlayerID = controllers.Player.GetPlayer(p.Sender).InstanceID;
            controllers.City.CommandCity(cmd);
        }
        private void ParsePlayerCommand(Packet p)
        {
            PlayerCommand cmd = (PlayerCommand)p.Item;
            cmd.PlayerID = controllers.Player.GetPlayer(p.Sender).InstanceID;
            controllers.Player.CommandPlayer(cmd);
        }

        // Game state checkers
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
        private void EndTurn(TurnEndReason reason)
        {
            Engine.Console.WriteLine($"Turn {TurnNumber} has ended, reason {reason}", MsgType.ServerInfo);

            controllers.ProcessTurn();
            TurnNumber++;
            TurnTimer.Restart();

            Engine.Console.WriteLine($"Turn {TurnNumber} has begun", MsgType.ServerInfo);
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

        // Send data to clients
        public void SendLobbyStateToAll()
        {
            LobbyState.Players.Clear();
            foreach (Player player in controllers.Player.GetAllPlayers())
                LobbyState.Players.Add(new BasicPlayer(player));
            Packet pOutLobbySync = new Packet((int)PacketHeader.LobbyStateSync, LobbyState);
            server.SendDataToAll(pOutLobbySync);
        }
        private void SendGameOver(Player winner, VictoryType vtype)
        {
            Engine.Console.WriteLine($"Game over! {winner.Name}, {controllers.Factory.Empire.GetEmpire(winner.EmpireID).Name}, has won with a {vtype} victory", MsgType.ServerInfo);
            Packet pOut = new Packet((int)PacketHeader.GameOver, winner, vtype);
            server.SendDataToAll(pOut);
            StopServer();
        }
    }
}
