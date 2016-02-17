using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Server.Controllers
{
    public class TileEventArgs : EventArgs
    {
        public List<Tile> Tiles { get; }

        public TileEventArgs(List<Tile> tiles)
        {
            Tiles = tiles;
        }
    }

    public class BoardController : Controller
    {
        private const int tileImprovmentTurnCost = 5;

        public Board Board { get; set; }
        public int DimX => Board.DimX;
        public int DimY => Board.DimY;

        public event EventHandler<TileEventArgs> TilesUpdated;
        protected virtual void OnTilesUpdated(TileEventArgs e)
        {
            if (TilesUpdated != null)
                TilesUpdated(this, e);
        }

        public BoardController(ControllerManager manager)
            : base (manager)
        {
        }

        public void GenerateBoard(WorldType type, WorldSize size)
        {
            Board = new Board(type, size);
        }

        public List<Tile> GetAllTiles()
        {
            List<Tile> tiles = new List<Tile>();
            for (int y = 0; y < Board.DimY; y++)
            {
                for (int x = 0; x < Board.DimX; x++)
                {
                    tiles.Add(Board.Tiles[y][x]);
                }
            }
            return tiles;
        }

        public List<Point> GetCityTileLocations(int cityID)
        {
            List<Point> tiles = new List<Point>();
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.CityID == cityID)
                {
                    tiles.Add(tile.Location);
                }
            }
            return tiles;
        }

        public List<Tile> GetCityTiles(int cityID)
        {
            List<Tile> tiles = new List<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.CityID == cityID)
                {
                    tiles.Add(tile);
                }
            }
            return tiles;
        }

        public Board GetBoard()
        {
            return Board;
        }

        public Tile GetTile(int x, int y) => Board.GetTile(x, y);

        public Tile GetTile(Point location) => Board.GetTile(location);

        public void UpdateTiles(List<Tile> tiles)
        {
            Board.UpdateTiles(tiles);
            OnTilesUpdated(new TileEventArgs(tiles));
        }

        public void UpdateTile(Tile tile)
        {
            Board.UpdateTile(tile);
            OnTilesUpdated(new TileEventArgs(new List<Tile>() { tile }));
        }

        public bool BuildImprovment(Tile tile, int playerID, TileImprovment improvment)
        {
            // Check if the player owns the tile
            City city = Controllers.City.GetCity(tile.CityID);
            if (city == null || city.PlayerID != playerID)
                return false;

            tile.ConstructionProgress = 0;
            tile.CurrentConstruction = improvment;

            return true;
        }

        public override void ProcessTurn()
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.CurrentConstruction != TileImprovment.None)
                {
                    tile.ConstructionProgress++;
                    if (tile.ConstructionProgress >= tileImprovmentTurnCost) // TODO account for game speed
                    {
                        tile.Improvement = tile.CurrentConstruction;
                        tile.ConstructionProgress = 0;
                        OnTilesUpdated(new TileEventArgs(new List<Tile>() { tile }));
                    }
                }
            }
        }
    }
}
