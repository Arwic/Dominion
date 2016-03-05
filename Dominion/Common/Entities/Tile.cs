// Dominion - Copyright (C) Timothy Ings
// Tile.cs
// This file defines classes that define a board tile

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Common.Entities
{
    public enum TileResource
    {
        Null,
        HORSES,
        IRON,
        COAL,
        URANIUM,
        OIL
    }

    public enum TileTerrainBase
    {
        Null,
        TUNDRA,
        GRASSLAND,
        DESERT,
        SEA,
        COAST,
        SNOW,
        PLAINS
    }

    public enum TileTerrainFeature
    {
        Null,
        OPEN,
        RIVER,
        HILL,
        MOUNTAIN,
    }

    public enum RoadType
    {
        Null,
        ROAD,
        RAILROAD,
    }

    public enum TileImprovment
    {
        Null,
        FOREST,
        JUNGLE,
        MINE,
        PLANTATION,
        FARM,
        TRADINGPOST,
        Quarry,
        Pasture,
        OffshorePlatform,
        Manufactory,
        LumberMill,
        Landmark,
        HolySite,
        Fort,
        FishingBoats,
        CustomsHouse,
        Citidel,
        Camp,
        Academy
    }

    public enum TileDirection
    {
        Null,
        NE,
        E,
        SE,
        SW,
        W,
        NW
    }

    public enum TileIncomeFormat
    {
        Gold,
        Science,
        Happiness,
        Production,
        Food,
        Culture,
        Faith,
        Tourism,
        Horses,
        Iron,
        Coal,
        Oil,
        Uranium
    }

    [Serializable()]
    public class Tile
    {
        /// <summary>
        /// The resource this tile contains
        /// </summary>
        public TileResource Resource { get; set; }

        /// <summary>
        /// The quantitiy of the tiles resource
        /// </summary>
        public int ResourceQuantity { get; set; }

        /// <summary>
        /// The base terrain type of the tile
        /// </summary>
        public TileTerrainBase TerrainBase { get; set; }

        /// <summary>
        /// The tile's terrain feature
        /// </summary>
        public TileTerrainFeature TerrainFeature { get; set; }

        /// <summary>
        /// The tile's current improvment construction
        /// </summary>
        public TileImprovment CurrentConstruction { get; set; }

        /// <summary>
        /// The tile's current improvments
        /// </summary>
        public TileImprovment Improvement { get; set; }

        /// <summary>
        /// The tile's location on the board
        /// </summary>
        public Point Location
        {
            get
            {
                return new Point(_locationX, _locationY);
            }
            set
            {
                _locationX = value.X;
                _locationY = value.Y;
            }
        }
        private int _locationX;
        private int _locationY;

        /// <summary>
        /// Indicates whether the tile is passable
        /// </summary>
        public bool Passable
        {
            get
            {
                return GetMovementCost() != -1;
            }
        }
        
        /// <summary>
        /// Indicates whether the tile is land
        /// </summary>
        public bool Land
        {
            get
            {
                switch (TerrainBase)
                {
                    case TileTerrainBase.SEA:
                    case TileTerrainBase.COAST:
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// The progress of the tile's current construction
        /// </summary>
        public int ConstructionProgress { get; set; }

        /// <summary>
        /// The type of road built on the tile
        /// </summary>
        public RoadType RoadType { get; set; }

        /// <summary>
        /// Indicates whether the tile has fallout on it
        /// </summary>
        public bool Fallout { get; set; }
        
        /// <summary>
        /// Indicates whether the tile's improvment needs to be repaired
        /// </summary>
        public bool Pillaged { get; set; }

        /// <summary>
        /// The city this tile belongs to
        /// </summary>
        public int CityID { get; set; }

        public Tile(Point location, TileResource res, TileTerrainBase tbase, TileTerrainFeature tfeature, TileImprovment impr)
        {
            Pillaged = false;
            Location = location;
            Resource = res;
            TerrainBase = tbase;
            TerrainFeature = tfeature;
            Improvement = impr;
            CityID = -1;
        }

        /// <summary>
        /// Returns the tile one tile away in the given direction
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Point GetNeighbour(TileDirection dir)
        {
            if (Location.Y % 2 == 0) // even y
            {
                switch (dir)
                {
                    case TileDirection.NE:
                        return new Point(Location.X + 1, Location.Y - 1);
                    case TileDirection.E:
                        return new Point(Location.X + 1, Location.Y);
                    case TileDirection.SE:
                        return new Point(Location.X + 1, Location.Y + 1);
                    case TileDirection.SW:
                        return new Point(Location.X, Location.Y + 1);
                    case TileDirection.W:
                        return new Point(Location.X - 1, Location.Y);
                    case TileDirection.NW:
                        return new Point(Location.X, Location.Y - 1);
                }
            }
            else // odd y
            {
                switch (dir)
                {
                    case TileDirection.NE:
                        return new Point(Location.X, Location.Y - 1);
                    case TileDirection.E:
                        return new Point(Location.X + 1, Location.Y);
                    case TileDirection.SE:
                        return new Point(Location.X, Location.Y + 1);
                    case TileDirection.SW:
                        return new Point(Location.X - 1, Location.Y + 1);
                    case TileDirection.W:
                        return new Point(Location.X - 1, Location.Y);
                    case TileDirection.NW:
                        return new Point(Location.X - 1, Location.Y - 1);
                }
            }
            return new Point(-1, -1);
        }

        /// <summary>
        /// Returns a list of all of the tile's neighbours locations
        /// </summary>
        /// <returns></returns>
        public List<Point> GetNeighbourTileLocations()
        {
            List<Point> tiles = new List<Point>();
            for (int i = 0; i < Enum.GetNames(typeof(TileDirection)).Length; i++)
            {
                Point tile = GetNeighbour((TileDirection)i);
                if (tile != new Point(-1, -1))
                    tiles.Add(tile);
            }
            return tiles;
        }

        /// <summary>
        /// Returns a list of all of the tile's neighbours
        /// </summary>
        /// <param name="tiles">board to get the neighbour references from</param>
        /// <returns></returns>
        public List<Tile> GetNeighbourTiles(Tile[][] tiles)
        {
            List<Point> neightbourLocations = GetNeighbourTileLocations();
            List<Tile> neighbours = new List<Tile>();
            foreach (Point nLoc in neightbourLocations)
            {
                if (nLoc.X < 0 || nLoc.Y < 0 || nLoc.Y >= tiles.Length || nLoc.X >= tiles[0].Length)
                    continue;
                Tile tile = tiles[nLoc.Y][nLoc.X];
                if (tile == null)
                    continue;
                neighbours.Add(tile);
            }
            return neighbours;
        }

        /// <summary>
        /// Returns the movement cost of the tile
        /// </summary>
        /// <returns></returns>
        public int GetMovementCost()
        {
            int cost = 0;
            switch (TerrainBase)
            {
                case TileTerrainBase.TUNDRA:
                case TileTerrainBase.GRASSLAND:
                case TileTerrainBase.DESERT:
                case TileTerrainBase.SNOW:
                    cost += 1;
                    break;
                case TileTerrainBase.SEA:
                case TileTerrainBase.COAST:
                    return -1;
            }
            switch (TerrainFeature)
            {
                case TileTerrainFeature.OPEN:
                    break;
                case TileTerrainFeature.RIVER:
                    cost += 1;
                    break;
                case TileTerrainFeature.HILL:
                    cost += 2;
                    break;
                case TileTerrainFeature.MOUNTAIN:
                    return -1;
            }
            switch (Improvement)
            {
                case TileImprovment.Null:
                    break;
                case TileImprovment.FOREST:
                case TileImprovment.JUNGLE:
                    cost += 1;
                    break;
                case TileImprovment.MINE:
                    break;
                case TileImprovment.PLANTATION:
                    break;
                case TileImprovment.FARM:
                    break;
            }
            return cost;
        }

        /// <summary>
        /// Returns the income the tile generates per turn
        /// </summary>
        /// <returns></returns>
        public int[] GetIncome()
        {
            int[] income = new int[Enum.GetValues(typeof(TileIncomeFormat)).Length];
            switch (TerrainBase)
            {
                case TileTerrainBase.GRASSLAND:
                    income[(int)TileIncomeFormat.Food] = 2;
                    break;
                case TileTerrainBase.TUNDRA:
                case TileTerrainBase.SEA:
                case TileTerrainBase.COAST:
                    income[(int)TileIncomeFormat.Food] = 1;
                    break;
                case TileTerrainBase.DESERT:
                case TileTerrainBase.SNOW:
                    break;
            }
            switch (TerrainFeature)
            {
                case TileTerrainFeature.OPEN:
                    break;
                case TileTerrainFeature.RIVER:
                    break;
                case TileTerrainFeature.HILL:
                    income[(int)TileIncomeFormat.Production] += 2;
                    break;
                case TileTerrainFeature.MOUNTAIN:
                    return new int[Enum.GetValues(typeof(TileIncomeFormat)).Length];
            }
            switch (Improvement)
            {
                case TileImprovment.Null:
                    break;
                case TileImprovment.FOREST:
                    income[(int)TileIncomeFormat.Food] = 1;
                    income[(int)TileIncomeFormat.Production] = 1;
                    break;
                case TileImprovment.JUNGLE:
                    income[(int)TileIncomeFormat.Food] = 1;
                    income[(int)TileIncomeFormat.Production] = -1;
                    break;
                case TileImprovment.MINE:
                    break;
                case TileImprovment.PLANTATION:
                    break;
                case TileImprovment.FARM:
                    income[(int)TileIncomeFormat.Food] += 1;
                    break;
            }
            switch (Resource)
            {
                case TileResource.HORSES:
                    income[(int)TileIncomeFormat.Horses] += ResourceQuantity;
                    break;
                case TileResource.IRON:
                    income[(int)TileIncomeFormat.Iron] += ResourceQuantity;
                    break;
                case TileResource.COAL:
                    income[(int)TileIncomeFormat.Coal] += ResourceQuantity;
                    break;
                case TileResource.URANIUM:
                    income[(int)TileIncomeFormat.Uranium] += ResourceQuantity;
                    break;
                case TileResource.OIL:
                    income[(int)TileIncomeFormat.Oil] += ResourceQuantity;
                    break;
            }
            return income;
        }
    }
}
