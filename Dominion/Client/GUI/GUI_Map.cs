// Dominion - Copyright (C) Timothy Ings
// GUI_Map.cs
// This file defines classes that manage the map gui elements

using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using Dominion.Client.Renderers;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dominion.Client.GUI
{
    public class GUI_Map : IGUIElement
    {
        private static Color mapTileColor_land = Color.ForestGreen;
        private static Color mapTileColor_waterSea = Color.Blue;
        private static Color mapTileColor_waterCoast = Color.CornflowerBlue;
        private static Color mapTileColor_unit = Color.White;
        private static Color mapTileColor_city = Color.White;

        private Canvas canvas;
        private Form form;
        private Camera2 camera;
        private Color color;
        private BoardRenderer boardRenderer;
        private Client client;
        private Sprite blankTileSprite;
        private Sprite tileOutlineSprite;

        public GUI_Map(Camera2 camera, Client client, BoardRenderer boardRenderer, Canvas canvas)
        {
            this.camera = camera;
            this.boardRenderer = boardRenderer;
            this.client = client;
            this.canvas = canvas;
            // load resources
            blankTileSprite = new Sprite("Graphics/Game/Tiles/BlankTile");
            tileOutlineSprite = new Sprite("Graphics/Game/Tiles/TileOutline");
            Show();
        }

        public void Hide()
        {
            form.Visible = false;
        }

        public void Show()
        {
            lock (Scenes.SceneGame._lock_guiDrawCall)
            {
                // setup the form programatically
                canvas.RemoveChild(form);
                int width = 350;
                int height = 200;
                int xOffset = -20;
                int yOffset = -20;
                form = new Form(new Rectangle(GraphicsManager.Instance.Viewport.Bounds.Width - width + xOffset, GraphicsManager.Instance.Viewport.Bounds.Height - height + yOffset, width, height), canvas);
                form.CloseButtonEnabled = false;
                form.DrawTitlebar = false;
                form.Draggable = false;
                // register events
                form.Drawn += Form_Drawn;
                form.MouseMove += Form_MouseMove;
                form.MouseDown += Form_MouseDown;
                form.Visible = true;
                color = Color.Gold;
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            // pan to the point the user clicked
            Vector2 panTarget = GetPanTarget();
            camera.TranslationTarget = panTarget;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            // instantly move the camera to the point the user is over
            // this prevents the camera from feeling laggy
            if (InputManager.Instance.IsMouseDown(MouseButton.Left))
            {
                Vector2 panTarget = GetPanTarget();
                camera.Translation = panTarget;
                camera.TranslationTarget = panTarget;
            }
        }

        // returns the world position from the map position under the mouse
        private Vector2 GetPanTarget()
        {
            Point mousePos = InputManager.Instance.MouseScreenPos();
            mousePos -= form.AbsoluteLocation;
            return new Vector2((mousePos.X / (form.Size.Width / GraphicsManager.Instance.Scale) / GraphicsManager.Instance.Scale) * boardRenderer.Bounds.Width, (mousePos.Y / (form.Size.Height / GraphicsManager.Instance.Scale) / GraphicsManager.Instance.Scale) * boardRenderer.Bounds.Height);
        }

        private void Form_Drawn(object sender, DrawEventArgs e)
        {
            // draw the tiles and camera rectangle
            DrawMapTiles(e.SpriteBatch);
            DrawCameraRect(e.SpriteBatch);
        }

        // draws the camera rectangle and crosshairs
        private void DrawCameraRect(SpriteBatch sb)
        {
            if (form != null && form.Visible)
            {
                // get the camera's world position
                Vector2 camTranslation = camera.Translation - new Vector2(GraphicsManager.Instance.Device.Viewport.Width / 2 / camera.Zoom.X, GraphicsManager.Instance.Viewport.Height / 2 / camera.Zoom.Y);
                // convert it from world to map coords
                Rectangle camRect = new Rectangle(
                    (int)(camTranslation.X / boardRenderer.Bounds.Width * form.Size.Width + form.AbsoluteLocation.X),
                    (int)(camTranslation.Y / boardRenderer.Bounds.Height * form.Size.Height + form.AbsoluteLocation.Y),
                    (int)((1920.0f / boardRenderer.Bounds.Width) * (float)form.Size.Width / camera.Zoom.X),
                    (int)((1080.0f / boardRenderer.Bounds.Height) * (float)form.Size.Height / camera.Zoom.Y));
                
                // calculate the vert and hori lines for the camera crosshairs
                Vector2 vertA = new Vector2(camRect.X + camRect.Width / 2, form.AbsoluteLocation.Y);
                Vector2 vertB = new Vector2(vertA.X, form.AbsoluteLocation.Y + form.AbsoluteBounds.Height);
                Vector2 horiA = new Vector2(form.AbsoluteLocation.X, camRect.Y + camRect.Height / 2);
                Vector2 horiB = new Vector2(form.AbsoluteLocation.X + form.AbsoluteBounds.Width, horiA.Y);

                // clamp cam rect to the form bounds
                if (camRect.X < form.AbsoluteLocation.X)
                {
                    camRect.Width -= form.AbsoluteLocation.X - camRect.X;
                    camRect.X = form.AbsoluteLocation.X;
                }
                if (camRect.Y < form.AbsoluteLocation.Y)
                {
                    camRect.Height -= form.AbsoluteLocation.Y - camRect.Y;
                    camRect.Y = form.AbsoluteLocation.Y;
                }
                if (camRect.X + camRect.Width > form.AbsoluteLocation.X + form.Size.Width)
                {
                    camRect.Width = form.AbsoluteLocation.X + form.Size.Width - camRect.X;
                }
                if (camRect.Y + camRect.Height > form.AbsoluteLocation.Y + form.Size.Height)
                {
                    camRect.Height = form.AbsoluteLocation.Y + form.Size.Height - camRect.Y;
                }

                // clamp crosshair to form bounds
                if (vertA.X < form.AbsoluteLocation.X)
                {
                    vertA.X = form.AbsoluteLocation.X;
                    vertB.X = vertA.X;
                }
                if (vertA.X > form.AbsoluteLocation.X + form.Size.Width)
                {
                    vertA.X = form.AbsoluteLocation.X + form.Size.Width;
                    vertB.X = vertA.X;
                }
                if (horiA.Y < form.AbsoluteLocation.Y)
                {
                    horiA.Y = form.AbsoluteLocation.Y;
                    horiB.Y = horiA.Y;
                }
                if (horiA.Y > form.AbsoluteLocation.Y + form.Size.Height)
                {
                    horiA.Y = form.AbsoluteLocation.Y + form.Size.Height;
                    horiB.Y = horiA.Y;
                }

                // draw the crosshairs
                GraphicsHelper.DrawLine(sb, vertA, vertB, 2, color);
                GraphicsHelper.DrawLine(sb, horiA, horiB, 2, color);

                // don't draw the cam rect if it does not have positive width or height
                if (camRect.Height < 0 || camRect.Width < 0)
                    return;
                // draw the cam rect
                GraphicsHelper.DrawRect(sb, camRect, 2, color);
            }
        }

        // draws the map tiles
        private void DrawMapTiles(SpriteBatch sb)
        {
            lock (Client._lock_cacheUpdate)
            {
                // draw every tile the client has seen
                foreach (Tile tile in client.GetAllCachedTiles())
                {
                    // if the city id isn't -1, then the tile is owned by a city
                    if (tile.CityID != -1)
                    {
                        // get the owner city
                        City city = client.Cities.Find(c => c.InstanceID == tile.CityID);
                        if (city != null)
                        {
                            // draw the tile with the empire's colours
                            Empire empire = client.EmpireFactory.GetEmpire(city.EmpireID);
                            DrawTile(sb, tile, empire.PrimaryColor, empire.SecondaryColor);
                        }
                    }
                    // otherwise just draw the tile based on terrain
                    else
                    {
                        switch (tile.TerrainBase)
                        {
                            case TileTerrainBase.Tundra:
                                DrawTile(sb, tile, Color.DarkGray, Color.Black);
                                break;
                            case TileTerrainBase.Grassland:
                                DrawTile(sb, tile, Color.ForestGreen, Color.Black);
                                break;
                            case TileTerrainBase.Desert:
                                DrawTile(sb, tile, Color.NavajoWhite, Color.Black);
                                break;
                            case TileTerrainBase.Sea:
                                DrawTile(sb, tile, Color.RoyalBlue, Color.Black);
                                break;
                            case TileTerrainBase.Coast:
                                DrawTile(sb, tile, Color.LightSeaGreen, Color.Black);
                                break;
                            case TileTerrainBase.Snow:
                                DrawTile(sb, tile, Color.Snow, Color.Black);
                                break;
                        }
                    }
                }
                // draw the client's cities
                foreach (City city in client.GetMyCities())
                {
                    DrawTile(sb, client.Board.GetTile(city.Location), Color.White, Color.Black);
                }
                // draw the client's units
                foreach (Unit unit in client.GetMyUnits())
                {
                    DrawTile(sb, client.Board.GetTile(unit.Location), Color.White, Color.Black);
                }
            }
        }

        // draw the given tile with the given colour
        private void DrawTile(SpriteBatch sb, Tile tile, Color fillColor, Color outlineColor)
        {
            Vector2 tilePos = boardRenderer.GetTileCentre(tile) - new Vector2(boardRenderer.TileWidth / 2, boardRenderer.TileHeight / 2);
            Rectangle dest = new Rectangle(
                (int)(tilePos.X / boardRenderer.Bounds.Width * form.Size.Width + form.AbsoluteLocation.X + 5),
                (int)(tilePos.Y / boardRenderer.Bounds.Height * form.Size.Height + form.AbsoluteLocation.Y),
                (int)(boardRenderer.TileWidth / (float)boardRenderer.Bounds.Width * form.Size.Width),
                (int)(boardRenderer.TileHeight / (float)boardRenderer.Bounds.Height * form.Size.Height));
            blankTileSprite.Draw(sb, dest, null, fillColor);
            tileOutlineSprite.Draw(sb, dest, null, outlineColor);
        }
    }
}
