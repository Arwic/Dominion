// Dominion - Copyright (C) Timothy Ings
// BoardRenderer.cs
// This file defines classes that render the game board

using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Dominion.Client.Renderers
{
    public class BoardRenderer
    {
        private static object _lock_tileIconUpdate = new object();

        private const float sqrt3on2 = 0.86602540378f;

        // defines a group of rich text icons
        // used for mainly tile yield icons
        private class TileIcon
        {
            public int[] Income { get; set; }
            public List<RichText> Icons { get; set; }
            private List<Vector2> iconPos;
            private Vector2 tileCentre;

            public TileIcon(Tile tile, BoardRenderer boardRenderer)
            {
                Income = tile.GetIncome();
                RichText resourceIcons = new RichText();
                Icons = new List<RichText>();
                for (int inc = 0; inc < Income[(int)TileIncomeFormat.Culture]; inc++)
                    Icons.Add("$(culture)".ToRichText());
                for (int inc = 0; inc < Income[(int)TileIncomeFormat.Food]; inc++)
                    Icons.Add("$(food)".ToRichText());
                for (int inc = 0; inc < Income[(int)TileIncomeFormat.Gold]; inc++)
                    Icons.Add("$(gold)".ToRichText());
                for (int inc = 0; inc < Income[(int)TileIncomeFormat.Happiness]; inc++)
                    Icons.Add("$(happiness)".ToRichText());
                for (int inc = 0; inc < Income[(int)TileIncomeFormat.Production]; inc++)
                    Icons.Add("$(production)".ToRichText());
                for (int inc = 0; inc < Income[(int)TileIncomeFormat.Science]; inc++)
                    Icons.Add("$(science)".ToRichText());

                tileCentre = boardRenderer.GetTileCentre(tile);
                iconPos = new List<Vector2>(Icons.Count);
                int i = 0;
                switch (Icons.Count)
                {
                    case 1:
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2);
                        break;
                    case 2:
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 - new Vector2(Icons[i].Measure().X, 0));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 + new Vector2(Icons[i].Measure().X, 0));
                        break;
                    case 3:
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 - new Vector2(Icons[i].Measure().X, 0));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 + new Vector2(Icons[i].Measure().X, 0));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 - new Vector2(0, Icons[i].Measure().Y));
                        break;
                    case 4:
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 - new Vector2(Icons[i].Measure().X, 0));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 + new Vector2(Icons[i].Measure().X, 0));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 - new Vector2(0, Icons[i].Measure().Y));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 + new Vector2(0, Icons[i].Measure().Y));
                        break;
                    case 5:
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 - new Vector2(Icons[i].Measure().X * 1.5f, 0));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 + new Vector2(Icons[i].Measure().X * 1.5f, 0));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 - new Vector2(0, Icons[i].Measure().Y * 1.5f));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2 + new Vector2(0, Icons[i].Measure().Y * 1.5f));
                        i++;
                        iconPos.Add(tileCentre - Icons[i].Measure() / 2);
                        break;
                }
            }

            public void Draw(SpriteBatch sb)
            {
                for (int i = 0; i < iconPos.Count; i++)
                {
                    Icons[i].Draw(sb, iconPos[i]);
                }
            }
        }

        public bool DrawResourceIcons { get; set; }
        public bool DrawGrid { get; set; }
        public bool DrawHighlight { get; set; }

        // Underlying data
        private Client client;
        private Camera2 camera;
        private TileIcon[][] tileIcons;

        // Resources
        private Sprite[] resourceSprites;
        private Sprite[] terrainBaseSprites;
        private Sprite[] terrainFeatureSprites;
        private Sprite[] improvmentSprites;
        private Sprite cloudSprite;
        private Sprite tileOverlaySprite;
        private Sprite tileOutlineSprite;
        private Sprite citySprite;

        private Vector2[] corners;
        public int TileSize
        {
            get
            {
                return _tileSize;
            }
            set
            {
                int last = _tileSize;
                _tileSize = value;
                if (_tileSize != last)
                {
                    corners = new Vector2[6];
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.ToRadians(60 * i + 30); // pointy top (for flat, remove "+ 30")
                        corners[i] = new Vector2(_tileSize * (float)Math.Cos(angle), _tileSize * (float)Math.Sin(angle));
                    }
                }
            }
        }
        private int _tileSize;
        public int TileHeight => TileSize * 2;
        public int TileWidth => (int)(sqrt3on2 * TileHeight);
        public Rectangle Bounds => new Rectangle(0, 0, client.Board.Tiles[0].Length * TileWidth, client.Board.Tiles.Length * TileHeight - (int)(0.25 * (client.Board.Tiles.Length + 1) * TileHeight));

        public BoardRenderer(Client client, Camera2 camera)
        {
            this.client = client;
            this.camera = camera;

            DrawHighlight = true;
            DrawGrid = false;
            DrawResourceIcons = true;

            TileSize = 50;
            // register events
            client.BoardChanged += Client_BoardChanged;
            client.TilesUpdated += Client_TilesUpdated;
            // load resources and build icons
            LoadResources();
            BuildTileIcons();
        }

        private void Client_TilesUpdated(object sender, TileListEventArgs e)
        {
            BuildTileIcons();
        }

        private void Client_BoardChanged(object sender, EventArgs e)
        {
            BuildTileIcons();
        }
        
        // loads resources used to render the board
        private void LoadResources()
        {
            cloudSprite = new Sprite("Graphics/Game/Tiles/Cloud");
            citySprite = new Sprite("Graphics/Game/Cities/City");
            tileOverlaySprite = new Sprite("Graphics/Game/Tiles/TileOverlay");
            tileOutlineSprite = new Sprite("Graphics/Game/Tiles/TileOutline");

            string[] resource = Enum.GetNames(typeof(TileResource));
            resourceSprites = new Sprite[resource.Length];
            for (int i = 0; i < resourceSprites.Length; i++)
                resourceSprites[i] = new Sprite($"Graphics/Game/Tiles/Resource/{i}");

            string[] terrainBase = Enum.GetNames(typeof(TileTerrainBase));
            terrainBaseSprites = new Sprite[terrainBase.Length];
            for (int i = 0; i < terrainBaseSprites.Length; i++)
                terrainBaseSprites[i] = new Sprite($"Graphics/Game/Tiles/TerrainBase/{i}");

            string[] terrainFeature = Enum.GetNames(typeof(TileTerrainFeature));
            terrainFeatureSprites = new Sprite[terrainFeature.Length];
            for (int i = 0; i < terrainFeatureSprites.Length; i++)
                terrainFeatureSprites[i] = new Sprite($"Graphics/Game/Tiles/TerrainFeature/{i}");

            string[] tileImprovment = Enum.GetNames(typeof(TileImprovment));
            improvmentSprites = new Sprite[tileImprovment.Length + 1];
            for (int i = 0; i < improvmentSprites.Length; i++)
                improvmentSprites[i] = new Sprite($"Graphics/Game/Tiles/Improvment/{i}");
        }

        // builds tile yield icons
        private void BuildTileIcons()
        {
            lock (_lock_tileIconUpdate)
            {
                tileIcons = new TileIcon[client.Board.DimY][];
                for (int y = 0; y < tileIcons.Length; y++)
                {
                    tileIcons[y] = new TileIcon[client.Board.DimX];
                    for (int x = 0; x < tileIcons[y].Length; x++)
                    {
                        tileIcons[y][x] = new TileIcon(client.Board.GetTile(x, y), this);
                    }
                }
            }
        }

        // returns the bounds of a tile
        public Rectangle GetTileRenderRect(Tile tile)
        {
            return GetTileRenderRect(tile.Location);
        }
        
        // returns the bounds of a tile
        public Rectangle GetTileRenderRect(Point location)
        {
            return GetTileRenderRect(location.X, location.Y);
        }
        
        // returns the bounds of a tile
        public Rectangle GetTileRenderRect(int x, int y)
        {
            Vector2[] localCorners = GetTileCorners(x, y);
            return new Rectangle((int)localCorners[2].X, (int)localCorners[4].Y, TileWidth, TileHeight);
        }
        
        // returns the corner's of a tile at a given coord
        public Vector2[] GetTileCorners(int x, int y)
        {
            Vector2[] localCorners = new Vector2[6];
            Vector2 tileCentre = GetTileCentre(x, y);
            for (int i = 0; i < 6; i++)
                localCorners[i] = tileCentre + corners[i];
            return localCorners;
        }
        
        // returns the corner's of a given tile
        public Vector2[] GetTileCorners(Tile tile)
        {
            return GetTileCorners(tile.Location.X, tile.Location.Y);
        }
        
        // returns the centre of the tile at a given coord
        public Vector2 GetTileCentre(int x, int y)
        {
            float xOffset = 0f;
            if (y % 2 == 0)
                xOffset = sqrt3on2 * TileSize;
            return new Vector2(x * TileWidth + xOffset, y * 1.5f * TileSize);
        }
        
        // returns the centre of the tile at a given coord
        public Vector2 GetTileCentre(Point point)
        {
            return GetTileCentre(point.X, point.Y);
        }
        
        // returns the centre of a given tile
        public Vector2 GetTileCentre(Tile tile)
        {
            if (tile == null)
                return Vector2.Zero;
            return GetTileCentre(tile.Location.X, tile.Location.Y);
        }

        // returns the tile at a given point, point is in world space
        public Tile GetTileAtPoint(Vector2 point)
        {
            for (int y = 0; y < client.Board.Tiles.Length; y++)
            {
                for (int x = 0; x < client.Board.Tiles[y].Length; x++)
                {
                    if (GraphicsHelper.InsidePolygon(GetTileCorners(x, y), point))
                        return client.Board.Tiles[y][x];
                }
            }
            return null;
        }

        // returns the cached tile at a given point, point is in world space
        public Tile GetCachedTileAtPoint(Vector2 point)
        {
            for (int y = 0; y < client.CachedBoard.Length; y++)
            {
                for (int x = 0; x < client.CachedBoard[y].Length; x++)
                {
                    if (GraphicsHelper.InsidePolygon(GetTileCorners(x, y), point))
                        return client.CachedBoard[y][x];
                }
            }
            return null;
        }

        // draws the board
        public void Draw(SpriteBatch sb, Font font)
        {
            lock (Client._lock_cacheUpdate)
            {
                DrawTiles(sb);
                DrawCities(sb);
                if (DrawGrid)
                    DrawHexGrid(sb);
                if (DrawHighlight)
                    DrawHexUnderMouse(sb);
                //DrawTilePositions(sb, font);
            }
        }

        // draws every tile
        // tiles the client hasn't explored are covered by clouds
        // tiles the client has explored but cannot currently see are tinted gray
        private void DrawTiles(SpriteBatch sb)
        {
            // get the tile under the mouse
            Tile tileUnderMouse = GetTileAtPoint(InputManager.Instance.MouseWorldPos(camera));
            List<int> myCityIDs = client.GetMyCityIDs();
            for (int y = 0; y < client.Board.Tiles.Length; y++)
            {
                for (int x = 0; x < client.Board.Tiles[y].Length; x++)
                {
                    // get data
                    Tile cachedTile = client.GetCachedTile(x, y);
                    Tile boardTile = client.Board.GetTile(x, y);
                    Vector2[] localCorners = GetTileCorners(boardTile);

                    // check for cull
                    bool camContainsTile = false;
                    foreach (Vector2 corner in localCorners)
                    {
                        if (new Rectangle(0, 0, 1920, 1080).Contains(camera.ConvertWorldToScreen(corner)))
                        {
                            camContainsTile = true;
                            break;
                        }
                    }
                    if (!camContainsTile)
                        continue; // cull

                    // calc data
                    Rectangle rect = new Rectangle((int)localCorners[2].X, (int)localCorners[4].Y, TileWidth, TileHeight);

                    // check if explored
                    if (cachedTile == null)
                    {
                        cloudSprite.Draw(sb, rect); // draw a cloud over the unexplored tile
                        continue; // dont draw the underlying tile
                    }

                    // check if a unit or a city can see the tile
                    bool visible = false;
                    foreach (Unit unit in client.GetMyUnits())
                    {
                        if (unit == null || unit.Template == null || cachedTile == null)
                            continue;
                        if (Board.HexDistance(unit.Location, cachedTile.Location) <= unit.Template.Sight)
                        {
                            visible = true;
                            break;
                        }
                    }
                    if (!visible)
                    {
                        foreach (Point nLoc in cachedTile.GetNeighbourTileLocations())
                        {
                            Tile n = client.GetCachedTile(nLoc);
                            if (n == null || n.CityID == -1)
                                continue;
                            if (myCityIDs.Contains(n.CityID))
                            {
                                visible = true;
                                break;
                            }
                        }
                    }

                    // visible tiles are white
                    Color color = Color.White;
                    if (!visible) // while explored but not currently visible tiles are gray
                        color = Color.Gray;

                    // draw the tile
                    Sprite terrainBase = terrainBaseSprites[(int)cachedTile.TerrainBase];
                    terrainBase.Draw(sb, rect, null, color);
                    if (cachedTile.TerrainFeature != TileTerrainFeature.Open)
                    {
                        Sprite tileFeature = terrainFeatureSprites[(int)cachedTile.TerrainFeature];
                        tileFeature.Draw(sb, rect, null, color);
                    }
                    // draw the improvment if the tile has one
                    if (cachedTile.Improvement != TileImprovment.Null)
                    {
                        Sprite tileImprovment = improvmentSprites[(int)cachedTile.Improvement];
                        //if (tile.TerrainFeature == TileTerrainFeature.Hill)
                        //    tileImprovment = improvmentSprites[$"{tile.Improvement.ToString()}_{tile.TerrainFeature}"];
                        tileImprovment.Draw(sb, rect, null, color);
                    }
                    // draw a resource icon if the tile has a resource
                    if (cachedTile.Resource != TileResource.Null)
                    {
                        Sprite tileResource = resourceSprites[(int)cachedTile.Resource];
                        tileResource.Draw(sb, rect);
                    }
                    // draw the city border is the tile belongs to a city
                    if (cachedTile.CityID != -1)
                    {
                        City city = client.Cities.Find(c => c.InstanceID == cachedTile.CityID);
                        if (city != null)
                        {
                            Color empColorPri = client.EmpireFactory.GetEmpire(city.EmpireID).PrimaryColor;
                            Color empColorSec = client.EmpireFactory.GetEmpire(city.EmpireID).SecondaryColor;
                            if (city.PlayerID == client.Player.InstanceID && tileUnderMouse != null && tileUnderMouse.Location == city.Location)
                                empColorPri = Color.Lerp(empColorPri, Color.White, 0.1f);
                            empColorPri.A = 255;
                            tileOverlaySprite.Draw(sb, rect, null, empColorPri);
                            tileOutlineSprite.Draw(sb, rect, null, empColorSec);
                        }
                    }
                    // draw yield icons if enabled
                    if (DrawResourceIcons)
                    {
                        lock (_lock_tileIconUpdate)
                        {
                            tileIcons[y][x]?.Draw(sb);
                        }
                    }
                }
            }
        }

        // draws every city that the client can see
        private void DrawCities(SpriteBatch sb)
        {
            foreach (City city in client.Cities.ToArray())
            {
                Tile tile = client.GetCachedTile(city.Location);
                if (tile == null)
                    continue;
                Rectangle cityDest = GetTileRenderRect(tile);
                citySprite.Draw(sb, cityDest);

                if (client.SelectedCity != null && client.SelectedCity.InstanceID == city.InstanceID && city.PlayerID == client.Player.InstanceID)
                {
                    foreach (Point loc in city.CitizenLocations)
                    {
                        Rectangle dest = GetTileRenderRect(loc);
                        tileOverlaySprite.Draw(sb, dest);
                    }
                }
            }
        }

        // draws the hexgrid over the board
        private void DrawHexGrid(SpriteBatch sb)
        {
            for (int y = 0; y < client.Board.Tiles.Length; y++)
            {
                for (int x = 0; x < client.Board.Tiles[y].Length; x++)
                {
                    Color color = Color.White;
                    Rectangle rect = GetTileRenderRect(x, y);
                    tileOutlineSprite.Draw(sb, rect, null, color);
                }
            }
        }

        // draws an outline around the hex currently under the mouse cursor
        private void DrawHexUnderMouse(SpriteBatch sb)
        {
            for (int y = 0; y < client.Board.Tiles.Length; y++)
            {
                for (int x = 0; x < client.Board.Tiles[y].Length; x++)
                {
                    // check if the mouse is inside the tile
                    if (GraphicsHelper.InsidePolygon(GetTileCorners(x, y), InputManager.Instance.MouseWorldPos(camera)))
                    {
                        // the the tile border
                        Rectangle rect = GetTileRenderRect(x, y);
                        tileOutlineSprite.Draw(sb, rect, null, Color.CornflowerBlue);
                    }
                }
            }
        }
    }
}
