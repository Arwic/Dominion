using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Common.Entities
{
    public enum TileResource
    {
        None,
        Horses,
        Iron,
        Coal,
        Uranium,
        Oil
    }

    public enum TileTerrainBase
    {
        Tundra,
        Grassland,
        Desert,
        Sea,
        Coast,
        Snow,
        Plains
    }

    public enum TileTerrainFeature
    {
        Open,
        River,
        Hill,
        Mountain,
    }

    public enum RoadType
    {
        None,
        Road,
        RailRoad,
    }

    public enum TileImprovment
    {
        None,
        Forest,
        Jungle,
        Mine,
        Plantation,
        Farm,
        TradingPost,
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
        Horses,
        Iron,
        Coal,
        Oil,
        Uranium
    }

    [Serializable()]
    public class Tile
    {
        public TileResource Resource { get; set; }
        public int ResourceQuantity { get; set; }
        public TileTerrainBase TerrainBase { get; set; }
        public TileTerrainFeature TerrainFeature { get; set; }
        public TileImprovment CurrentConstruction { get; set; }
        public TileImprovment Improvement { get; set; }
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
        public bool Passable
        {
            get
            {
                return GetMovementCost() != -1;
            }
        }
        public bool Land
        {
            get
            {
                switch (TerrainBase)
                {
                    case TileTerrainBase.Sea:
                    case TileTerrainBase.Coast:
                        return false;
                }
                return true;
            }
        }
        public int ConstructionProgress { get; set; }
        public RoadType RoadType { get; set; }
        public bool Fallout { get; set; }
        public bool Pillaged { get; set; }
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

        public int GetMovementCost()
        {
            int cost = 0;
            switch (TerrainBase)
            {
                case TileTerrainBase.Tundra:
                case TileTerrainBase.Grassland:
                case TileTerrainBase.Desert:
                case TileTerrainBase.Snow:
                    cost += 1;
                    break;
                case TileTerrainBase.Sea:
                case TileTerrainBase.Coast:
                    return -1;
            }
            switch (TerrainFeature)
            {
                case TileTerrainFeature.Open:
                    break;
                case TileTerrainFeature.River:
                    cost += 1;
                    break;
                case TileTerrainFeature.Hill:
                    cost += 2;
                    break;
                case TileTerrainFeature.Mountain:
                    return -1;
            }
            switch (Improvement)
            {
                case TileImprovment.None:
                    break;
                case TileImprovment.Forest:
                case TileImprovment.Jungle:
                    cost += 1;
                    break;
                case TileImprovment.Mine:
                    break;
                case TileImprovment.Plantation:
                    break;
                case TileImprovment.Farm:
                    break;
            }
            return cost;
        }

        public int[] GetIncome()
        {
            int[] income = new int[Enum.GetValues(typeof(TileIncomeFormat)).Length];
            switch (TerrainBase)
            {
                case TileTerrainBase.Grassland:
                    income[(int)TileIncomeFormat.Food] = 2;
                    break;
                case TileTerrainBase.Tundra:
                case TileTerrainBase.Sea:
                case TileTerrainBase.Coast:
                    income[(int)TileIncomeFormat.Food] = 1;
                    break;
                case TileTerrainBase.Desert:
                case TileTerrainBase.Snow:
                    break;
            }
            switch (TerrainFeature)
            {
                case TileTerrainFeature.Open:
                    break;
                case TileTerrainFeature.River:
                    break;
                case TileTerrainFeature.Hill:
                    income[(int)TileIncomeFormat.Production] += 2;
                    break;
                case TileTerrainFeature.Mountain:
                    return new int[Enum.GetValues(typeof(TileIncomeFormat)).Length];
            }
            switch (Improvement)
            {
                case TileImprovment.None:
                    break;
                case TileImprovment.Forest:
                    income[(int)TileIncomeFormat.Food] = 1;
                    income[(int)TileIncomeFormat.Production] = 1;
                    break;
                case TileImprovment.Jungle:
                    income[(int)TileIncomeFormat.Food] = 1;
                    income[(int)TileIncomeFormat.Production] = -1;
                    break;
                case TileImprovment.Mine:
                    break;
                case TileImprovment.Plantation:
                    break;
                case TileImprovment.Farm:
                    income[(int)TileIncomeFormat.Food] += 1;
                    break;
            }
            switch (Resource)
            {
                case TileResource.Horses:
                    income[(int)TileIncomeFormat.Horses] += ResourceQuantity;
                    break;
                case TileResource.Iron:
                    income[(int)TileIncomeFormat.Iron] += ResourceQuantity;
                    break;
                case TileResource.Coal:
                    income[(int)TileIncomeFormat.Coal] += ResourceQuantity;
                    break;
                case TileResource.Uranium:
                    income[(int)TileIncomeFormat.Uranium] += ResourceQuantity;
                    break;
                case TileResource.Oil:
                    income[(int)TileIncomeFormat.Oil] += ResourceQuantity;
                    break;
            }
            return income;
        }
    }
}
