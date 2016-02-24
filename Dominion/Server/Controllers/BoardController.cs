// Dominion - Copyright (C) Timothy Ings
// BoardController.cs
// This file defines classes that control the game board

using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Gets or sets the board that is being controlled
        /// </summary>
        public Board Board { get; set; }

        /// <summary>
        /// Gets the width of board in tiles
        /// </summary>
        public int DimX => Board.DimX;

        /// <summary>
        /// Gets the height of the board in tiles
        /// </summary>
        public int DimY => Board.DimY;

        /// <summary>
        /// Occurs when tiles on the board are updated
        /// </summary>
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

        /// <summary>
        /// Generates a new board
        /// </summary>
        /// <param name="type"></param>
        /// <param name="size"></param>
        public void GenerateBoard(WorldType type, WorldSize size)
        {
            Board = new Board(type, size);
        }

        /// <summary>
        /// Gets a list of every tile on the board
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a list of tile locations that belong to a city with the given id
        /// </summary>
        /// <param name="cityID"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a list of tiles that belong to a city with the given id
        /// </summary>
        /// <param name="cityID"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the tile at the given location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Tile GetTile(int x, int y) => Board.GetTile(x, y);

        /// <summary>
        /// Gets the tile at the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Tile GetTile(Point location) => Board.GetTile(location);

        /// <summary>
        /// Updates the board with a list of updated tiles
        /// </summary>
        /// <param name="tiles"></param>
        public void UpdateTiles(List<Tile> tiles)
        {
            Board.UpdateTiles(tiles);
            OnTilesUpdated(new TileEventArgs(tiles));
        }

        /// <summary>
        /// Updates the board with the given tile
        /// </summary>
        /// <param name="tile"></param>
        public void UpdateTile(Tile tile)
        {
            Board.UpdateTile(tile);
            OnTilesUpdated(new TileEventArgs(new List<Tile>() { tile }));
        }

        /// <summary>
        /// Builds the given improvment on the given tile for the player with the given id
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="playerID"></param>
        /// <param name="improvment"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Prepares the board for the next turn
        /// </summary>
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
