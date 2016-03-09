// Dominion - Copyright (C) Timothy Ings
// Client.cs
// This file defines classes that define the client

using ArwicEngine;
using ArwicEngine.Core;
using ArwicEngine.Net;
using ArwicEngine.Scenes;
using Dominion.Common;
using Dominion.Common.Entities;
using Dominion.Common.Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dominion.Client
{
    public enum TurnState
    {
        Begin,
        ChooseResearch,
        ChooseSocialPolicy,
        ChooseProduction,
        UnitOrders,
        WaitingForPlayers,
    }

    public class TileListEventArgs : EventArgs
    {
        public List<Tile> Tiles { get; }

        public TileListEventArgs(List<Tile> tiles)
        {
            Tiles = tiles;
        }
    }

    public class UnitListEventArgs : EventArgs
    {
        public List<UnitInstance> Units { get; }

        public UnitListEventArgs(List<UnitInstance> units)
        {
            Units = units;
        }
    }

    public class UnitEventArgs : EventArgs
    {
        public UnitInstance Unit { get; }

        public UnitEventArgs(UnitInstance unit)
        {
            Unit = unit;
        }
    }

    public class PlayerEventArgs : EventArgs
    {
        public Player Player { get; }

        public PlayerEventArgs(Player player)
        {
            Player = player;
        }
    }

    public class CityEventArgs : EventArgs
    {
        public City City { get; }

        public CityEventArgs(City city)
        {
            City = city;
        }
    }

    public class CityListEventArgs : EventArgs
    {
        public List<City> Cities { get; }

        public CityListEventArgs(List<City> cities)
        {
            Cities = cities;
        }
    }

    public class TurnStateEventArgs : EventArgs
    {
        public TurnState TurnState { get; }

        public TurnStateEventArgs(TurnState turnState)
        {
            TurnState = turnState;
        }
    }

    public class LobbyStateEventArgs : EventArgs
    {
        public LobbyState LobbyState { get; }

        public LobbyStateEventArgs(LobbyState lobbyState)
        {
            LobbyState = lobbyState;
        }
    }

    public class UnitCommandIDEventArgs : EventArgs
    {
        public UnitCommandID UnitCommandID { get; }

        public UnitCommandIDEventArgs(UnitCommandID cmdID)
        {
            UnitCommandID = cmdID;
        }
    }

    public class Client
    {
        /// <summary>
        /// Thread lock object for cahce updates
        /// </summary>
        public static object _lock_cacheUpdate = new object();

        /// <summary>
        /// Timeout of net operations
        /// </summary>
        public int NetOperationTimeOut { get; private set; }

        /// <summary>
        /// Indicates whether the client is running
        /// </summary>
        public bool Running => client == null ? false : client.Listening;
        private NetClient client;
        /// <summary>
        /// Gets the client statistics
        /// </summary>
        public NetClientStats ClientStatistics => client.Statistics;
        
        /// <summary>
        /// Gets or sets the data manager
        /// </summary>
        public DataManager DataManager { get; private set; }

        /// <summary>
        /// Game options that are set in the lobby and other lobby information
        /// </summary>
        public LobbyState LobbyState
        {
            get
            {
                return _lobbyState;
            }
            private set
            {
                LobbyState last = _lobbyState;
                _lobbyState = value;
                if (_lobbyState != last)
                    OnLobbyStateChanged(new LobbyStateEventArgs(_lobbyState));
            }
        }
        private LobbyState _lobbyState;

        /// <summary>
        /// This client's player
        /// </summary>
        public Player Player
        {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
                OnPlayerUpdated(new PlayerEventArgs(Player));
            }
        }
        private Player _player;

        /// <summary>
        /// Caches tiles
        /// </summary>
        public Tile[][] CachedBoard { get; set; }

        /// <summary>
        /// Width of the board
        /// </summary>
        public int BoardWidth => Board.DimX;

        /// <summary>
        /// Height of the board
        /// </summary>
        public int BoardHeight => Board.DimY;

        /// <summary>
        /// Full board
        /// </summary>
        public Board Board
        {
            get
            {
                return _board;
            }
            set
            {
                Board last = _board;
                _board = value;
                if (last != _board)
                    OnBoardChanged(EventArgs.Empty);
            }
        }
        private Board _board;

        /// <summary>
        /// Cached units
        /// </summary>
        public List<UnitInstance> CachedUnits { get; set; }
        
        /// <summary>
        /// Full list of units
        /// </summary>
        public List<UnitInstance> AllUnits
        {
            get
            {
                return _allUnits;
            }
            set
            {
                _allUnits = value;
                if (_allUnits != null)
                    foreach (UnitInstance unit in _allUnits)
                        DataManager.Unit.Reconstruct(unit);
                OnUnitsUpdated(new UnitListEventArgs(_allUnits));
            }
        }
        private List<UnitInstance> _allUnits;

        /// <summary>
        /// The user's selected unit
        /// </summary>
        public UnitInstance SelectedUnit
        {
            get
            {
                if (_selectedUnitID == -1)
                    return null;
                return AllUnits.Find(u => u.InstanceID == _selectedUnitID);
            }
            set
            {
                if (value == null)
                {
                    _selectedUnitID = -1;
                    return;
                }
                _lastSelectedUnitID = _selectedUnitID;
                _selectedUnitID = value.InstanceID;
                if (_selectedUnitID != _lastSelectedUnitID)
                    OnSelectedUnitChnaged(new UnitEventArgs(value));
            }
        }
        private int _selectedUnitID = -1;
        private int _lastSelectedUnitID = -1;

        /// <summary>
        /// The user's selected unit command
        /// </summary>
        public UnitCommandID SelectedCommand
        {
            get
            {
                return _selectedCommand;
            }
            set
            {
                UnitCommandID last = _selectedCommand;
                _selectedCommand = value;
                if (last != _selectedCommand)
                    OnSelectedCommandChanged(new UnitCommandIDEventArgs(_selectedCommand));
            }
        }
        private UnitCommandID _selectedCommand;

        /// <summary>
        /// List of cached cities
        /// </summary>
        public List<City> CachedCities { get; set; }

        /// <summary>
        /// List of all cities
        /// </summary>
        public List<City> Cities
        {
            get
            {
                return _cities;
            }
            set
            {
                _cities = value;
                OnCitiesUpdated(new CityListEventArgs(_cities));
                OnCityUpdated(new CityEventArgs(SelectedCity));
            }
        }
        private List<City> _cities;

        /// <summary>
        /// The user's selected city
        /// </summary>
        public City SelectedCity
        {
            get
            {
                if (_selectedCityID == -1)
                    return null;
                return Cities.Find(c => c.InstanceID == _selectedCityID);
            }
            set
            {
                if (value == null)
                {
                    _selectedCityID = -1;
                    return;
                }
                int last = _selectedCityID;
                _selectedCityID = value.InstanceID;
                if (_selectedCityID != last)
                    OnSelectedCityChnaged(new CityEventArgs(value));
            }
        }
        private int _selectedCityID;

        /// <summary>
        /// The turn number
        /// </summary>
        public int TurnNumber { get; private set; }

        /// <summary>
        /// Time before the user is forced to end thier turn
        /// </summary>
        public int TurnTimeLimit { get; private set; }

        /// <summary>
        /// Stopwatch that time the turn
        /// </summary>
        public Stopwatch TurnTimer { get; private set; }

        /// <summary>
        /// State of the user's turn
        /// </summary>
        public TurnState TurnState
        {
            get
            {
                return _turnState;
            }
            private set
            {
                TurnState last = _turnState;
                _turnState = value;
                if (_turnState != last)
                    OnTurnStateChanged(new TurnStateEventArgs(_turnState));
            }
        }
        private TurnState _turnState;

        private int consecutiveParsingErrors;
        private int maxConsecutiveParsingErrors = 5;
        
        #region Events
        public event EventHandler LostConnection;
        public event EventHandler BoardChanged;
        public event EventHandler<UnitCommandIDEventArgs> SelectedCommandChanged;
        public event EventHandler<LobbyStateEventArgs> LobbyStateChanged;
        public event EventHandler<TileListEventArgs> TilesUpdated;
        public event EventHandler<UnitListEventArgs> UnitsUpdated;
        public event EventHandler<CityListEventArgs> CitiesUpdated;
        public event EventHandler<CityEventArgs> CityUpdated;
        public event EventHandler<CityEventArgs> CityAdded;
        public event EventHandler<CityEventArgs> CityRemoved;
        public event EventHandler<PlayerEventArgs> PlayerUpdated;
        public event EventHandler<TurnStateEventArgs> TurnStateChanged;
        public event EventHandler<UnitEventArgs> SelectedUnitChnaged;
        public event EventHandler<CityEventArgs> SelectedCityChnaged;
        protected virtual void OnLostConnection(EventArgs args)
        {
            if (LostConnection != null)
                LostConnection(this, args);
        }
        protected virtual void OnBoardChanged(EventArgs args)
        {
            if (BoardChanged != null)
                BoardChanged(this, args);
        }
        protected virtual void OnSelectedCommandChanged(UnitCommandIDEventArgs args)
        {
            if (SelectedCommandChanged != null)
                SelectedCommandChanged(this, args);
        }
        protected virtual void OnLobbyStateChanged(LobbyStateEventArgs args)
        {
            if (LobbyStateChanged != null)
                LobbyStateChanged(this, args);
        }
        protected virtual void OnTilesUpdated(TileListEventArgs args)
        {
            if (TilesUpdated != null)
                TilesUpdated(this, args);
        }
        protected virtual void OnUnitsUpdated(UnitListEventArgs args)
        {
            if (UnitsUpdated != null)
                UnitsUpdated(this, args);
        }
        protected virtual void OnCitiesUpdated(CityListEventArgs args)
        {
            if (CitiesUpdated != null)
                CitiesUpdated(this, args);
        }
        protected virtual void OnCityUpdated(CityEventArgs args)
        {
            if (CityUpdated != null)
                CityUpdated(this, args);
        }
        protected virtual void OnCityAdded(CityEventArgs args)
        {
            if (CityAdded != null)
                CityAdded(this, args);
        }
        protected virtual void OnCityRemoved(CityEventArgs args)
        {
            if (CityRemoved != null)
                CityRemoved(this, args);
        }
        protected virtual void OnPlayerUpdated(PlayerEventArgs args)
        {
            if (PlayerUpdated != null)
                PlayerUpdated(this, args);
        }
        protected virtual void OnTurnStateChanged(TurnStateEventArgs args)
        {
            if (TurnStateChanged != null)
                TurnStateChanged(this, args);
        }
        protected virtual void OnSelectedUnitChnaged(UnitEventArgs args)
        {
            if (SelectedUnitChnaged != null)
                SelectedUnitChnaged(this, args);
        }
        protected virtual void OnSelectedCityChnaged(CityEventArgs args)
        {
            if (SelectedCityChnaged != null)
                SelectedCityChnaged(this, args);
        }
        #endregion

        public Client()
        {
            AllUnits = new List<UnitInstance>();
            Cities = new List<City>();
            CachedUnits = new List<UnitInstance>();
            CachedCities = new List<City>();
            DataManager = new DataManager();
            NetOperationTimeOut = Convert.ToInt32(ConfigManager.Instance.GetVar(Constants.CONFIG_NET_CLIENT_TIMEOUT));
        }

        /// <summary>
        /// Connects to the server with the given details
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Connect(string userName, string address = "127.0.0.1", int port = 7894, string password = "")
        {
            client = new NetClient();
            client.PacketRecieved += Client_PacketRecieved;
            client.LostConnection += Client_LostConnection;
            bool connected = client.Connect(address, port);
            if (!connected)
                return false;
            client.BeginListenAsync();
            TurnTimer = new Stopwatch();
            Packet pOut = new Packet((int)PacketHeader.LobbyInit, userName, password);
            client.SendData(pOut);
            return true;
        }

        /// <summary>
        /// Dissconnects from the connected server
        /// </summary>
        public void Dissconnect()
        {
            if (client != null)
                client.Dissconnect();
        }

        private void Client_PacketRecieved(object sender, PacketRecievedEventArgs e)
        {
            RoutePacket(e.Packet);
        }

        private void Client_LostConnection(object sender, EventArgs e)
        {
            ConsoleManager.Instance.WriteLine("Client lost connection to the serer", MsgType.Warning);
            OnLostConnection(e);
            Dissconnect();
            SceneManager.Instance.ChangeScene((int)Scene.Menu);
        }

        #region Packet Parsing
        // parses an incoming packet
        private void RoutePacket(Packet p)
        {
            PacketHeader header = (PacketHeader)p.Header;
            ConsoleManager.Instance.WriteLine($"Parsing a packet with header {header}");
            try
            {
                switch (header)
                {
                    case PacketHeader.LobbyInit: // Their player id and factory data
                        ParseLobbyInit(p);
                        break;
                    case PacketHeader.LobbyStateSync: // The LobbyState
                        ParseLobbyStateSync(p);
                        break;
                    case PacketHeader.LobbyKick: // Message that they have been kicked
                        ParseLobbyKick(p);
                        break;
                    case PacketHeader.LobbyBan: //Message that they have been banned
                        ParseLobbyBan(p);
                        break;
                    case PacketHeader.LobbyStartGame: // Message that signals the start of the game
                        ParseLobbyStartGame(p);
                        break;
                    case PacketHeader.TurnData: // Their player data
                        ParseTurnData(p);
                        break;
                    case PacketHeader.PlayerUpdate: // An updated player object
                        ParsePlayerUpdate(p);
                        break;
                    case PacketHeader.TileUpdate: // A list of tiles that need updating mid turn
                        ParseTileUpdate(p);
                        break;
                    case PacketHeader.UnitUpdate: // A list of units that need updating mid turn
                        ParseUnitUpdate(p);
                        break;
                    case PacketHeader.UnitAdded: // A unit that has been produced/bought
                        ParseUnitAdded(p);
                        break;
                    case PacketHeader.UnitRemoved: // A unit that has been disbanded
                        ParseUnitRemoved(p);
                        break;
                    case PacketHeader.CityUpdate: // A list of cities that need updating mid turn
                        ParseCityUpdate(p);
                        break;
                    case PacketHeader.CityAdded: // A list of cities that need updating mid turn
                        ParseCityAdded(p);
                        break;
                    case PacketHeader.CityRemoved: // A list of cities that need updating mid turn
                        ParseCityRemoved(p);
                        break;

                    case PacketHeader.GameOver: // game over
                        ParseGameOver(p);
                        break;
                }
                consecutiveParsingErrors = 0;
            }
            catch (Exception e)
            {
                consecutiveParsingErrors++;
                ConsoleManager.Instance.WriteLine($"Error parsing {(PacketHeader)p.Header}", MsgType.Failed);
                Exception exp = e;
                int i = 0;
                while (exp != null)
                {
                    ConsoleManager.Instance.WriteLine($"{i++}: {exp.Message}", MsgType.Failed);
                    exp = exp.InnerException;
                }
                if (consecutiveParsingErrors >= maxConsecutiveParsingErrors)
                    Dissconnect();
                else
                    RoutePacket(p);
            }
        }
        
        // parses a packet with the header LobbyInit
        private void ParseLobbyInit(Packet p)
        {
            int i = 0;
            Player = (Player)p.Items[i++];
            DataManager.Building = (BuildingManager)p.Items[i++];
            DataManager.Empire = (EmpireManager)p.Items[i++];
            DataManager.Unit = (UnitManager)p.Items[i++];
            DataManager.Tech = (TechnologyManager)p.Items[i++];
            DataManager.SocialPolicy = (SocialPolicyManager)p.Items[i++];
            ConsoleManager.Instance.WriteLine("Initialised data managers", MsgType.Info);
        }
        
        // parses a packet with the header LobbyStateSync
        private void ParseLobbyStateSync(Packet p)
        {
            LobbyState = (LobbyState)p.Item;
            ConsoleManager.Instance.WriteLine($"LobbyState was updated", MsgType.Info);
        }
        
        // parses a packet with the header LobbyKick
        private void ParseLobbyKick(Packet p)
        {
            string reason = (string)p.Item;
            Dissconnect();
            SceneManager.Instance.ChangeScene((int)Scene.Menu);
            ConsoleManager.Instance.WriteLine($"The host has kicked you from the game\nReason: {reason}");
        }
        
        // parses a packet with the header LobbyBan
        private void ParseLobbyBan(Packet p)
        {
            string reason = (string)p.Item;
            Dissconnect();
            SceneManager.Instance.ChangeScene((int)Scene.Menu);
            ConsoleManager.Instance.WriteLine($"The host has banned you from the game\nReason: {reason}");
        }
        
        // parses a packet with the header LobbyStartGame
        private void ParseLobbyStartGame(Packet p)
        {
            int i = 0;
            Board = (Board)p.Items[i++];

            CachedBoard = new Tile[Board.DimY][];
            for (int y = 0; y < CachedBoard.Length; y++)
                CachedBoard[y] = new Tile[Board.DimX];

            Player = (Player)p.Items[i++];
            Cities = (List<City>)p.Items[i++];
            AllUnits = (List<UnitInstance>)p.Items[i++];
            _selectedCityID = -1;
            _selectedUnitID = AllUnits.Find(u => u.PlayerID == Player.InstanceID).InstanceID;
            _lastSelectedUnitID = _selectedUnitID;
            TurnState = TurnState.Begin;
            ConsoleManager.Instance.WriteLine("The host has started the game");
            SceneManager.Instance.ChangeScene((int)Scene.Game);
        }
        
        // parses a packet with the header TurnData
        private void ParseTurnData(Packet p)
        {
            int i = 0;
            TurnNumber = (int)p.Items[i++];
            TurnTimeLimit = (int)p.Items[i++];
            TurnEndReason reason = (TurnEndReason)p.Items[i++];
            Player = (Player)p.Items[i++];
            Cities = (List<City>)p.Items[i++];
            AllUnits = (List<UnitInstance>)p.Items[i++];
            UpdateCache();
            ConsoleManager.Instance.WriteLine($"Recieved data for turn {TurnNumber}");
            TurnState = TurnState.Begin;
            TurnTimer.Restart();
        }
        
        // parses a packet with the header TileUpdate
        private void ParseTileUpdate(Packet p)
        {
            List<Tile> tiles = (List<Tile>)p.Item;
            Board.UpdateTiles(tiles);
            OnTilesUpdated(new TileListEventArgs(tiles));
        }
        
        // parses a packet with the header UnitUpdate
        private void ParseUnitUpdate(Packet p)
        {
            UnitInstance unit = (UnitInstance)p.Item;
            UnitInstance oldUnit = AllUnits.Find(u => u.InstanceID == unit.InstanceID);
            if (!AllUnits.Remove(oldUnit))
                return;
            DataManager.Unit.Reconstruct(unit);
            AllUnits.Add(unit);
            UpdateCache();
            ConsoleManager.Instance.WriteLine($"Updated unit {unit.InstanceID}:{unit.UnitID}:{unit.Name}");
            OnUnitsUpdated(new UnitListEventArgs(AllUnits));
        }
        
        // parses a packet with the header UnitAdded
        private void ParseUnitAdded(Packet p)
        {
            UnitInstance unit = (UnitInstance)p.Item;
            DataManager.Unit.Reconstruct(unit);
            AllUnits.Add(unit);
            UpdateCache();
            ConsoleManager.Instance.WriteLine($"Added a unit {unit.InstanceID}:{unit.UnitID}:{unit.Name}");
            OnUnitsUpdated(new UnitListEventArgs(AllUnits));
        }
        
        // parses a packet with the header UnitRemoved
        private void ParseUnitRemoved(Packet p)
        {
            int unitInstanceID = (int)p.Item;
            UnitInstance unit = AllUnits.Find(u => u.InstanceID == unitInstanceID);
            AllUnits.Remove(unit);
            UpdateCache();
            ConsoleManager.Instance.WriteLine($"Removed unit {unit.InstanceID}:{unit.UnitID}:{unit.Name}");
            OnUnitsUpdated(new UnitListEventArgs(AllUnits));
        }
        
        // parses a packet with the header CityUpdate
        private void ParseCityUpdate(Packet p)
        {
            City city = (City)p.Item;
            Cities.RemoveAll(c => c.InstanceID == city.InstanceID);
            Cities.Add(city);
            UpdateCache();
            ConsoleManager.Instance.WriteLine($"Updated a city {city.InstanceID}:{city.EmpireID}:{city.Name}");
            OnCityUpdated(new CityEventArgs(city));
        }
        
        // parses a packet with the header CityAdded
        private void ParseCityAdded(Packet p)
        {
            City city = (City)p.Item;
            Cities.Add(city);
            UpdateCache();
            ConsoleManager.Instance.WriteLine($"Added a city {city.InstanceID}:{city.EmpireID}:{city.Name}");
            OnCityAdded(new CityEventArgs(city));
        }
        
        // parses a packet with the header CityRemoved
        private void ParseCityRemoved(Packet p)
        {
            int cityID = (int)p.Item;
            City city = Cities.Find(c => c.InstanceID == cityID);
            Cities.Remove(city);
            UpdateCache();
            ConsoleManager.Instance.WriteLine($"Removed a city {city.InstanceID}:{city.EmpireID}:{city.Name}");
            OnCityRemoved(new CityEventArgs(city));
        }
        
        // parses a packet with the header PlayerUpdate
        private void ParsePlayerUpdate(Packet p)
        {
            Player player = (Player)p.Item;
            if (player.InstanceID != Player.InstanceID)
                return;
            Player = player;
            ConsoleManager.Instance.WriteLine($"Updated player {player.InstanceID}:{player.EmpireID}:{player.Name}");
        }
        
        // parses a packet with the header GameOver
        private void ParseGameOver(Packet p)
        {
            int i = 0;
            Player winner = (Player)p.Items[i++];
            VictoryType vtype = (VictoryType)p.Items[i++];
            ConsoleManager.Instance.WriteLine($"Game over! {winner.Name} of {winner.EmpireID} has won a {vtype} victory");
            Dissconnect();
            SceneManager.Instance.ChangeScene((int)Scene.Menu);
        }
        #endregion

        /// <summary>
        /// Tries to advance the player's turn
        /// </summary>
        public void AdvanceTurn()
        {
            //// Our turn is already over
            //if (TurnState == TurnState.WaitingForPlayers)
            //{
            //    ConsoleManager.Instance.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}
            //// Check if we need to choose a new research node
            //if (RequireNewResearch())
            //{
            //    TurnState = TurnState.ChooseResearch;
            //    ConsoleManager.Instance.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            //// Check if we need to spend culture on a social policy
            //if (RequireNewSocialPolicy())
            //{
            //    TurnState = TurnState.ChooseSocialPolicy;
            //    ConsoleManager.Instance.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            //// Check if we need to fill any production queues
            //if (RequireNewProduction())
            //{
            //    TurnState = TurnState.ChooseProduction;
            //    ConsoleManager.Instance.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            //// Check if we need to give any units orders
            //if (RequireUnitOrders())
            //{
            //    TurnState = TurnState.UnitOrders;
            //    ConsoleManager.Instance.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            // If nothing needs to be done, end our turn
            TurnState = TurnState.WaitingForPlayers;
            ConsoleManager.Instance.WriteLine($"Ending turn");
            Player.EndedTurn = true;
            Packet pOut = new Packet((int)PacketHeader.TurnState, Player.EndedTurn);
            client.SendData(pOut);
        }

        // checks if the user is required to select a new research node
        private bool RequireNewResearch()
        {
            if (Player.SelectedTechNodeID == "TECH_NULL" || Player.TechTree.GetTech(Player.SelectedTechNodeID).Unlocked)
                return true;
            return false;
        }
        
        // checks if the user is required to select a social policy
        private bool RequireNewSocialPolicy()
        {
            return false;
        }
        
        // checks if the user is required to select a new porduction
        private bool RequireNewProduction()
        {
            foreach (City city in GetMyCities())
            {
                if (city.ProductionQueue.Count == 0)
                    return true;
            }
            return false;
        }
        
        // checks if the user is required to order a unit
        private bool RequireUnitOrders()
        {
            foreach (UnitInstance unit in GetMyUnits())
            {
                if (unit.RequiresOrders)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all the cached tiles
        /// </summary>
        /// <returns></returns>
        public List<Tile> GetAllCachedTiles()
        {
            List<Tile> tiles = new List<Tile>();
            for (int y = 0; y < CachedBoard.Length; y++)
            {
                for (int x = 0; x < CachedBoard[y].Length; x++)
                {
                    Tile tile = GetCachedTile(x, y);
                    if (tile != null)
                        tiles.Add(tile);
                }
            }
            return tiles;
        }

        /// <summary>
        /// Returns a list of all the units this client controls
        /// </summary>
        /// <returns></returns>
        public List<UnitInstance> GetMyUnits()
        {
            return AllUnits.FindAll(u => u.PlayerID == Player.InstanceID);
        }

        /// <summary>
        /// Returns a list of all the cities this client controls
        /// </summary>
        /// <returns></returns>
        public List<City> GetMyCities()
        {
            return Cities.FindAll(c => c.PlayerID == Player.InstanceID);
        }

        /// <summary>
        /// Returns a list of ints representing the ids of this client's controlled cities
        /// </summary>
        /// <returns></returns>
        public List<int> GetMyCityIDs()
        {
            List<int> cityIDs = new List<int>();
            foreach (City city in GetMyCities())
                cityIDs.Add(city.InstanceID);
            return cityIDs;
        }

        /// <summary>
        /// Returns the cahced tile at the given loation
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Tile GetCachedTile(Point location)
        {
            return GetCachedTile(location.X, location.Y);
        }

        /// <summary>
        /// Returns the cahced tile at the given loation
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Tile GetCachedTile(int x, int y)
        {
            if (y < 0 || y >= CachedBoard.Length || x < 0 || x >= CachedBoard[y].Length)
                return null;
            return CachedBoard[y][x];
        }

        /// <summary>
        /// Updates the clients cache
        /// Marks certain tiles, cities and, units as visible
        /// </summary>
        private void UpdateCache()
        {
            if (Board == null)
                return;
            lock (_lock_cacheUpdate)
            {
                CachedUnits.Clear();

                foreach (UnitInstance unit in GetMyUnits())
                {
                    UpdateUnitCache(unit);
                    foreach (Tile tile in Board.GetAllTiles())
                    {
                        if (Board.HexDistance(unit.Location, tile.Location) <= unit.BaseUnit.Sight)
                            UpdateTileCache(tile.Location);
                    }

                    foreach (UnitInstance otherUnit in AllUnits)
                    {
                        if (otherUnit.PlayerID == Player.InstanceID)
                            continue;
                        if (Board.HexDistance(unit.Location, otherUnit.Location) <= unit.BaseUnit.Sight)
                            UpdateUnitCache(otherUnit);
                    }
                }

                List<int> myCityIDs = GetMyCityIDs();

                foreach (Tile tile in Board.GetAllTiles())
                {
                    if (tile.CityID == -1)
                        continue;
                    if (myCityIDs.Contains(tile.CityID))
                    {
                        UpdateTileCache(tile.Location);
                        foreach (Point nLoc in tile.GetNeighbourTileLocations())
                        {
                            UpdateTileCache(nLoc);
                            foreach (UnitInstance unit in AllUnits)
                            {
                                if (unit.PlayerID == Player.InstanceID)
                                    continue;
                                if (unit.Location == nLoc)
                                    UpdateUnitCache(unit);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the cache of the tile at the given location
        /// </summary>
        /// <param name="location"></param>
        public void UpdateTileCache(Point location)
        {
            if (location.X < 0 || location.Y < 0 || location.Y >= CachedBoard.Length || location.X >= CachedBoard[0].Length)
                return;
            CachedBoard[location.Y][location.X] = Board.GetTile(location);
        }

        /// <summary>
        /// Updates the given unit int the unit cache
        /// </summary>
        /// <param name="unit"></param>
        public void UpdateUnitCache(UnitInstance unit)
        {
            CachedUnits.RemoveAll(u => u.InstanceID == unit.InstanceID);
            CachedUnits.Add(unit);
        }

        /// <summary>
        /// Commands a unit
        /// </summary>
        /// <param name="cmd"></param>
        public void CommandUnit(UnitCommand cmd)
        {
            ConsoleManager.Instance.WriteLine($"Commanded {AllUnits.Find(u => u.InstanceID == cmd.UnitInstanceID).Name} - {cmd.CommandID}");
            Packet p = new Packet((int)PacketHeader.UnitCommand, cmd);
            SelectedCommand = UnitCommandID.UNITCMD_NULL;
            client.SendData(p);
        }

        /// <summary>
        /// Commands a city
        /// </summary>
        /// <param name="cmd"></param>
        public void CommandCity(CityCommand cmd)
        {
            ConsoleManager.Instance.WriteLine($"Commanded {Cities.Find(c => c.InstanceID == cmd.CityID).Name} - {cmd.CommandID}");
            Packet p = new Packet((int)PacketHeader.CityCommand, cmd);
            client.SendData(p);
        }

        /// <summary>
        /// Commands the player
        /// </summary>
        /// <param name="cmd"></param>
        public void CommandPlayer(PlayerCommand cmd)
        {
            ConsoleManager.Instance.WriteLine($"Commanded Player - {cmd.CommandID}");
            Packet p = new Packet((int)PacketHeader.PlayerCommand, cmd);
            client.SendData(p);
        }

        /// <summary>
        /// Asks the server to select the given empire for this client
        /// </summary>
        /// <param name="id"></param>
        public void Lobby_SelectNewEmpire(string id)
        {
            Packet p = new Packet((int)PacketHeader.LobbyEmpire, id);
            client.SendData(p);
        }
    }
}
