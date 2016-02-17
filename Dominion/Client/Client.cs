using ArwicEngine;
using ArwicEngine.Core;
using ArwicEngine.Net;
using Dominion.Common;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
        public List<Unit> Units { get; }

        public UnitListEventArgs(List<Unit> units)
        {
            Units = units;
        }
    }

    public class UnitEventArgs : EventArgs
    {
        public Unit Unit { get; }

        public UnitEventArgs(Unit unit)
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

    public class Client : IEngineComponent
    {
        public static object _lock_cacheUpdate = new object();

        public Engine Engine { get; }
        public int NetOperationTimeOut { get; private set; }
        public bool Running => client == null ? false : client.Listening;
        private NetClient client;
        public NetClientStats ClientStatistics => client.Statistics;

        public BuildingFactory BuildingFactory { get; private set; }
        public EmpireFactory EmpireFactory { get; private set; }
        public ProductionFactory ProductionFactory { get; private set; }
        public UnitFactory UnitFactory { get; private set; }

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

        public Tile[][] CachedBoard { get; set; }
        public int BoardWidth => Board.DimX;
        public int BoardHeight => Board.DimY;
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

        public List<Unit> CachedUnits { get; set; }
        public List<Unit> AllUnits
        {
            get
            {
                return _allUnits;
            }
            set
            {
                _allUnits = value;
                if (_allUnits != null)
                    foreach (Unit unit in _allUnits)
                        UnitFactory.Reconstruct(unit);
                OnUnitsUpdated(new UnitListEventArgs(_allUnits));
            }
        }
        private List<Unit> _allUnits;
        public Unit SelectedUnit
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
        private int _selectedUnitID;
        private int _lastSelectedUnitID;
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

        public List<City> CachedCities { get; set; }
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

        public int TurnNumber { get; private set; }
        public int TurnTimeLimit { get; private set; }
        public Stopwatch TurnTimer { get; private set; }
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

        public Client(Engine engine)
        {
            Engine = engine;
            AllUnits = new List<Unit>();
            Cities = new List<City>();
            CachedUnits = new List<Unit>();
            CachedCities = new List<City>();
            NetOperationTimeOut = Convert.ToInt32(Engine.Config.GetVar(Constants.CONFIG_NET_CLIENT_TIMEOUT));
        }

        public async Task<bool> ConnectAsync(string address = "127.0.0.1", int port = 7894)
        {
            client = new NetClient(Engine);
            client.PacketRecieved += Client_PacketRecieved;
            bool connected = await client.ConnectAsync(address, port);
            if (!connected)
                return false;
            client.BeginListenAsync();
            return true;
        }
        public bool Connect(string userName, string address = "127.0.0.1", int port = 7894, string password = "")
        {
            client = new NetClient(Engine);
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
        public void Dissconnect()
        {
            if (client != null)
                client.Dissconnect();
        }
        private void Client_PacketRecieved(object sender, PacketRecievedEventArgs e)
        {
            ParsePacket(e.Packet);
        }
        private void Client_LostConnection(object sender, EventArgs e)
        {
            Engine.Console.WriteLine("Client lost connection to the serer", MsgType.Warning);
            OnLostConnection(e);
            Dissconnect();
            Engine.Scene.ChangeScene(0);
        }

        private void ParsePacket(Packet p)
        {
            PacketHeader header = (PacketHeader)p.Header;
            Engine.Console.WriteLine($"Parsing a packet with header {header}");
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
                Engine.Console.WriteLine($"Error parsing {(PacketHeader)p.Header}", MsgType.Failed);
                Exception exp = e;
                int i = 0;
                while (exp != null)
                {
                    Engine.Console.WriteLine($"{i++}: {exp.Message}", MsgType.Failed);
                    exp = exp.InnerException;
                }
                if (consecutiveParsingErrors >= maxConsecutiveParsingErrors)
                    Dissconnect();
                else
                    ParsePacket(p);
            }
        }
        private void ParseLobbyInit(Packet p)
        {
            int i = 0;
            Player = (Player)p.Items[i++];
            BuildingFactory = (BuildingFactory)p.Items[i++];
            EmpireFactory = (EmpireFactory)p.Items[i++];
            ProductionFactory = (ProductionFactory)p.Items[i++];
            UnitFactory = (UnitFactory)p.Items[i++];
            Engine.Console.WriteLine("Initialised factories");
        }
        private void ParseLobbyStateSync(Packet p)
        {
            LobbyState = (LobbyState)p.Item;
            Engine.Console.WriteLine($"LobbyState was updated", MsgType.Info);
        }
        private void ParseLobbyKick(Packet p)
        {
            string reason = (string)p.Item;
            Dissconnect();
            Engine.Scene.ChangeScene(0);
            Engine.Console.WriteLine($"The host has kicked you from the game\nReason: {reason}");
        }
        private void ParseLobbyBan(Packet p)
        {
            string reason = (string)p.Item;
            Dissconnect();
            Engine.Scene.ChangeScene(0);
            Engine.Console.WriteLine($"The host has banned you from the game\nReason: {reason}");
        }
        private void ParseLobbyStartGame(Packet p)
        {
            int i = 0;
            Board = (Board)p.Items[i++];

            CachedBoard = new Tile[Board.DimY][];
            for (int y = 0; y < CachedBoard.Length; y++)
                CachedBoard[y] = new Tile[Board.DimX];

            Player = (Player)p.Items[i++];
            Cities = (List<City>)p.Items[i++];
            AllUnits = (List<Unit>)p.Items[i++];
            _selectedCityID = -1;
            _selectedUnitID = -1;
            TurnState = TurnState.Begin;
            Engine.Console.WriteLine("The host has started the game");
            Engine.Scene.ChangeScene(1);
        }
        private void ParseTurnData(Packet p)
        {
            int i = 0;
            TurnNumber = (int)p.Items[i++];
            TurnTimeLimit = (int)p.Items[i++];
            TurnEndReason reason = (TurnEndReason)p.Items[i++];
            Player = (Player)p.Items[i++];
            Cities = (List<City>)p.Items[i++];
            AllUnits = (List<Unit>)p.Items[i++];
            UpdateCache();
            SelectedUnit = AllUnits.Find(u => u.InstanceID == _lastSelectedUnitID);
            Engine.Console.WriteLine($"Recieved data for turn {TurnNumber}");
            TurnState = TurnState.Begin;
            TurnTimer.Restart();
        }
        private void ParseTileUpdate(Packet p)
        {
            List<Tile> tiles = (List<Tile>)p.Item;
            Board.UpdateTiles(tiles);
            OnTilesUpdated(new TileListEventArgs(tiles));
        }
        private void ParseUnitUpdate(Packet p)
        {
            Unit unit = (Unit)p.Item;
            Unit oldUnit = AllUnits.Find(u => u.InstanceID == unit.InstanceID);
            AllUnits.Remove(oldUnit);
            UnitFactory.Reconstruct(unit);
            AllUnits.Add(unit);
            UpdateCache();
            Engine.Console.WriteLine($"Updated unit {unit.InstanceID}:{unit.UnitID}:{unit.Name}");
            OnUnitsUpdated(new UnitListEventArgs(AllUnits));
        }
        private void ParseUnitAdded(Packet p)
        {
            Unit unit = (Unit)p.Item;
            UnitFactory.Reconstruct(unit);
            AllUnits.Add(unit);
            UpdateCache();
            Engine.Console.WriteLine($"Added a unit {unit.InstanceID}:{unit.UnitID}:{unit.Name}");
            OnUnitsUpdated(new UnitListEventArgs(AllUnits));
        }
        private void ParseUnitRemoved(Packet p)
        {
            int unitInstanceID = (int)p.Item;
            Unit unit = AllUnits.Find(u => u.InstanceID == unitInstanceID);
            AllUnits.Remove(unit);
            UpdateCache();
            Engine.Console.WriteLine($"Removed unit {unit.InstanceID}:{unit.UnitID}:{unit.Name}");
            OnUnitsUpdated(new UnitListEventArgs(AllUnits));
        }
        private void ParseCityUpdate(Packet p)
        {
            City city = (City)p.Item;
            Cities.RemoveAll(c => c.InstanceID == city.InstanceID);
            Cities.Add(city);
            UpdateCache();
            Engine.Console.WriteLine($"Updated a city {city.InstanceID}:{city.EmpireID}:{city.Name}");
            OnCityUpdated(new CityEventArgs(city));
        }
        private void ParseCityAdded(Packet p)
        {
            City city = (City)p.Item;
            Cities.Add(city);
            UpdateCache();
            Engine.Console.WriteLine($"Added a city {city.InstanceID}:{city.EmpireID}:{city.Name}");
            OnCityAdded(new CityEventArgs(city));
        }
        private void ParseCityRemoved(Packet p)
        {
            int cityID = (int)p.Item;
            City city = Cities.Find(c => c.InstanceID == cityID);
            Cities.Remove(city);
            UpdateCache();
            Engine.Console.WriteLine($"Removed a city {city.InstanceID}:{city.EmpireID}:{city.Name}");
            OnCityRemoved(new CityEventArgs(city));
        }
        private void ParsePlayerUpdate(Packet p)
        {
            Player player = (Player)p.Item;
            if (player.InstanceID != Player.InstanceID)
                return;
            Player = player;
            Engine.Console.WriteLine($"Updated player {player.InstanceID}:{player.EmpireID}:{player.Name}");
        }
        private void ParseGameOver(Packet p)
        {
            int i = 0;
            Player winner = (Player)p.Items[i++];
            VictoryType vtype = (VictoryType)p.Items[i++];
            Engine.Console.WriteLine($"Game over! {winner.Name} of {EmpireFactory.GetEmpire(winner.EmpireID).Name} has won a {vtype} victory");
            Dissconnect();
            Engine.Scene.ChangeScene(0);
        }

        public void AdvanceTurn()
        {
            //// Our turn is already over
            //if (TurnState == TurnState.WaitingForPlayers)
            //{
            //    Engine.Console.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}
            //// Check if we need to choose a new research node
            //if (RequireNewResearch())
            //{
            //    TurnState = TurnState.ChooseResearch;
            //    Engine.Console.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            //// Check if we need to spend culture on a social policy
            //if (RequireNewSocialPolicy())
            //{
            //    TurnState = TurnState.ChooseSocialPolicy;
            //    Engine.Console.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            //// Check if we need to fill any production queues
            //if (RequireNewProduction())
            //{
            //    TurnState = TurnState.ChooseProduction;
            //    Engine.Console.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            //// Check if we need to give any units orders
            //if (RequireUnitOrders())
            //{
            //    TurnState = TurnState.UnitOrders;
            //    Engine.Console.WriteLine($"TurnState = {TurnState}");
            //    return;
            //}

            // If nothing needs to be done, end our turn
            TurnState = TurnState.WaitingForPlayers;
            Engine.Console.WriteLine($"Ending turn");
            Player.EndedTurn = true;
            Packet pOut = new Packet((int)PacketHeader.TurnState, Player.EndedTurn);
            client.SendData(pOut);
        }
        public bool RequireNewResearch()
        {
            if (Player.SelectedTechNodeID == -1 || Player.TechTree.GetNode(Player.SelectedTechNodeID).Unlocked)
                return true;
            return false;
        }
        public bool RequireNewSocialPolicy()
        {
            return false;
        }
        public bool RequireNewProduction()
        {
            foreach (City city in GetMyCities())
            {
                if (city.ProductionQueue.Count == 0)
                    return true;
            }
            return false;
        }
        public bool RequireUnitOrders()
        {
            foreach (Unit unit in GetMyUnits())
            {
                if (unit.RequiresOrders)
                    return true;
            }
            return false;
        }

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
        public List<Unit> GetMyUnits()
        {
            return AllUnits.FindAll(u => u.PlayerID == Player.InstanceID);
        }
        public List<City> GetMyCities()
        {
            return Cities.FindAll(c => c.PlayerID == Player.InstanceID);
        }
        public List<int> GetMyCityIDs()
        {
            List<int> cityIDs = new List<int>();
            foreach (City city in GetMyCities())
                cityIDs.Add(city.InstanceID);
            return cityIDs;
        }

        public Tile GetCachedTile(Point location)
        {
            return GetCachedTile(location.X, location.Y);
        }
        public Tile GetCachedTile(int x, int y)
        {
            if (y < 0 || y >= CachedBoard.Length || x < 0 || x >= CachedBoard[y].Length)
                return null;
            return CachedBoard[y][x];
        }
        private void UpdateCache()
        {
            if (Board == null)
                return;
            lock (_lock_cacheUpdate)
            {
                CachedUnits.Clear();

                foreach (Unit unit in GetMyUnits())
                {
                    UpdateUnitCache(unit);
                    foreach (Tile tile in Board.GetAllTiles())
                    {
                        if (Board.HexDistance(unit.Location, tile.Location) <= unit.Constants.Sight)
                            UpdateTileCache(tile.Location);
                    }

                    foreach (Unit otherUnit in AllUnits)
                    {
                        if (otherUnit.PlayerID == Player.InstanceID)
                            continue;
                        if (Board.HexDistance(unit.Location, otherUnit.Location) <= unit.Constants.Sight)
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
                            foreach (Unit unit in AllUnits)
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
        public void UpdateTileCache(Point location)
        {
            if (location.X < 0 || location.Y < 0 || location.Y >= CachedBoard.Length || location.X >= CachedBoard[0].Length)
                return;
            CachedBoard[location.Y][location.X] = Board.GetTile(location);
        }
        public void UpdateUnitCache(Unit unit)
        {
            CachedUnits.RemoveAll(u => u.InstanceID == unit.InstanceID);
            CachedUnits.Add(unit);
        }

        public void CommandUnit(UnitCommand cmd)
        {
            Engine.Console.WriteLine($"Commanded {AllUnits.Find(u => u.InstanceID == cmd.UnitInstanceID).Name} - {cmd.CommandID}");
            Packet p = new Packet((int)PacketHeader.UnitCommand, cmd);
            SelectedCommand = UnitCommandID.Null;
            client.SendData(p);
        }
        public void CommandCity(CityCommand cmd)
        {
            Engine.Console.WriteLine($"Commanded {Cities.Find(c => c.InstanceID == cmd.CityID).Name} - {cmd.CommandID}");
            Packet p = new Packet((int)PacketHeader.CityCommand, cmd);
            client.SendData(p);
        }
        public void CommandPlayer(PlayerCommand cmd)
        {
            Engine.Console.WriteLine($"Commanded Player - {cmd.CommandID}");
            Packet p = new Packet((int)PacketHeader.PlayerCommand, cmd);
            client.SendData(p);
        }
        public void Lobby_SelectNewEmpire(int id)
        {
            Packet p = new Packet((int)PacketHeader.LobbyEmpire, id);
            client.SendData(p);
        }
    }
}
