using ArwicEngine.Audio;
using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using ArwicEngine.Scenes;
using Dominion.Client.GUI;
using Dominion.Client.Renderers;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Dominion.Client.Scenes
{
    public class SceneGame : BaseScene
    {
        public static object _lock_guiDrawCall = new object();

        private GameManager manager;

        private SpriteBatch sbGUI;
        private SpriteBatch sbFore;
        private SpriteBatch sbBack;
        private FrameCounter fps;
        private Font font;

        private Canvas canvas;
        private BoardRenderer boardRenderer;
        private UnitRenderer unitRenderer;
        public GUI_GameMenu GameMenu { get; private set; }
        public GUI_Tech TechTree { get; private set; }
        public GUI_CityList CityList { get; private set; }
        public GUI_UnitList UnitList { get; private set; }
        public GUI_CityManagment CityManagment { get; private set; }
        public GUI_UnitActions UnitActions { get; private set; }

        private bool flag_drawInfo_client;
        private bool flag_drawInfo_server;
        private Cursor unitCommandTargetCursor;
        private Cursor defaultCursor;

        private Camera2 camera;
        private float cameraTranslationSpeed = 20f;
        private float cameraZoomDelta = 20f;
        private bool cameraPanning;
        private Vector2 cameraPanLastMouse;
        private Vector2 cameraZoomMax = new Vector2(2f, 2f);
        private Vector2 cameraZoomMin = new Vector2(0.5f, 0.5f);

        public SceneGame(GameManager manager)
            : base()
        {
            defaultCursor = new Cursor("Content/Cursors/normal.cur");
            unitCommandTargetCursor = new Cursor("Content/Cursors/link.cur");
            this.manager = manager;
            manager.Client.SelectedCommandChanged += Client_SelectedCommandChanged;

#if DEBUG
            ConsoleManager.Instance.RegisterCommand("cache_all", f_cachAll);
#endif
        }

        private int f_cachAll(List<string> args)
        {
            if (args.Count != 1)
            {
                ConsoleManager.Instance.WriteLine("Usage: <cachable-object>");
                ConsoleManager.Instance.WriteLine("Usage: Objects;");
                ConsoleManager.Instance.WriteLine("Usage: tile");
                ConsoleManager.Instance.WriteLine("Usage: unit");
                ConsoleManager.Instance.WriteLine("Usage: city");
                return 1;
            }

            switch (args[0])
            {
                case "tile":
                    foreach (Tile tile in manager.Client.Board.GetAllTiles())
                        manager.Client.UpdateTileCache(tile.Location);
                    break;
                case "unit":
                    foreach (Unit unit in manager.Client.AllUnits)
                        manager.Client.UpdateUnitCache(unit);
                    break;
                case "city":
                    manager.Client.CachedCities.Clear();
                    foreach (City city in manager.Client.Cities)
                        manager.Client.CachedCities.Add(city);
                    break;
            }

            return 0;
        }

        private void Client_SelectedCommandChanged(object sender, UnitCommandIDEventArgs e)
        {
            if (UnitCommand.GetTargetType(e.UnitCommandID) == UnitCommandTargetType.Tile)
                unitCommandTargetCursor.Enable();
            else
                defaultCursor.Enable();
        }

        public override void Enter()
        {
            sbGUI = new SpriteBatch(GraphicsManager.Instance.Device);
            sbFore = new SpriteBatch(GraphicsManager.Instance.Device);
            sbBack = new SpriteBatch(GraphicsManager.Instance.Device);
            font = new Font("fonts/consolas");
            
            camera = new Camera2();
            boardRenderer = new BoardRenderer(manager.Client, camera);
            unitRenderer = new UnitRenderer(boardRenderer, camera, manager.Client);
            camera.Translation = boardRenderer.GetTileCentre(manager.Client.GetMyUnits()[0].Location);

            camera.TranslationTarget = camera.Translation;
            canvas = new Canvas(GraphicsManager.Instance.Viewport.Bounds);
            ConsoleForm consoleForm = new ConsoleForm(canvas);
            GUI_Map map = new GUI_Map(camera, manager.Client, boardRenderer, canvas);
            GUI_StatusBar statusBar = new GUI_StatusBar(manager.Client, this, canvas);
            GUI_EndTurn endTurn = new GUI_EndTurn(manager.Client, canvas);
            CityList = new GUI_CityList(manager.Client, this, canvas);
            UnitList = new GUI_UnitList(manager.Client, this, canvas);
            UnitActions = new GUI_UnitActions(manager.Client, this, camera, boardRenderer, canvas);
            CityManagment = new GUI_CityManagment(manager.Client, this, canvas);
            GUI_NamePlates namePlates = new GUI_NamePlates(manager.Client, canvas, camera, boardRenderer);
            TechTree = new GUI_Tech(manager.Client, this, canvas);
            GameMenu = new GUI_GameMenu(manager.Client, this, canvas);
        }

        public void HideForms(bool hideCityManagment = true, bool hideUnitActions = true)
        {
            CityList.Hide();
            UnitList.Hide();
            if (hideUnitActions)
                UnitActions.Hide();
            if (hideCityManagment)
                CityManagment.Hide();
            TechTree.Hide();
        }

        public override void Leave()
        {
        }

        public override void Update()
        {
            bool interacted = canvas.Update();
            UpdateBoardInteraction(interacted);
            UpdateCamera(Engine.Instance.DeltaTime, interacted);

            if (!interacted && !canvas.IsCapturingText())
            {
                if (InputManager.Instance.WasKeyDown(Keys.Enter))
                    manager.Client.AdvanceTurn();
            }
            
            if (InputManager.Instance.WasKeyDown(Keys.F1))
                flag_drawInfo_client = !flag_drawInfo_client;
            if (InputManager.Instance.WasKeyDown(Keys.F2))
                flag_drawInfo_server = !flag_drawInfo_server;

            if (InputManager.Instance.WasKeyDown(Keys.F5))
                boardRenderer.DrawGrid = !boardRenderer.DrawGrid;
            if (InputManager.Instance.WasKeyDown(Keys.F6))
                boardRenderer.DrawHighlight = !boardRenderer.DrawHighlight;
            if (InputManager.Instance.WasKeyDown(Keys.F7))
                boardRenderer.DrawResourceIcons = !boardRenderer.DrawResourceIcons;
        }

        private void UpdateCamera(float delta, bool interacted)
        {
            camera.Update(delta);

            if (InputManager.Instance.IsKeyDown(Keys.Up))
                camera.TranslationTarget += new Vector2(0, -cameraTranslationSpeed) * delta;
            if (InputManager.Instance.IsKeyDown(Keys.Down))
                camera.TranslationTarget += new Vector2(0, cameraTranslationSpeed) * delta;
            if (InputManager.Instance.IsKeyDown(Keys.Left))
                camera.TranslationTarget += new Vector2(cameraTranslationSpeed, 0) * delta;
            if (InputManager.Instance.IsKeyDown(Keys.Right))
                camera.TranslationTarget += new Vector2(-cameraTranslationSpeed, 0) * delta;

            if (!interacted)
            {
                // Panning
                if (InputManager.Instance.OnMouseDown(MouseButton.Left))
                {
                    cameraPanning = true;
                    cameraPanLastMouse = InputManager.Instance.MouseScreenPos().ToVector2() / camera.Zoom.X;
                }
                // Zoom
                if (InputManager.Instance.ScrolledUp())
                {
                    cameraPanning = false;
                    camera.ZoomTarget += Vector2.One * cameraZoomDelta * delta;
                    if (camera.ZoomTarget.X > cameraZoomMax.X)
                        camera.ZoomTarget = cameraZoomMax;
                }
                else if (InputManager.Instance.ScrolledDown())
                {
                    cameraPanning = false;
                    camera.ZoomTarget -= Vector2.One * cameraZoomDelta * delta;
                    if (camera.ZoomTarget.X < cameraZoomMin.X)
                        camera.ZoomTarget = cameraZoomMin;
                }
            }
            if (cameraPanning)
            {
                Vector2 curMouse = InputManager.Instance.MouseScreenPos().ToVector2() / camera.Zoom;
                Vector2 diff = cameraPanLastMouse - curMouse;
                camera.Translation = camera.TranslationTarget + diff;
                camera.TranslationTarget = camera.Translation;
                cameraPanLastMouse = curMouse;

                if (InputManager.Instance.OnMouseUp(MouseButton.Left))
                    cameraPanning = false;
            }
        }

        private void UpdateBoardInteraction(bool interacted)
        {
            if (!interacted)
            {
                if (InputManager.Instance.OnMouseUp(MouseButton.Left))
                {
                    // Get the tile clicked
                    Tile tile = boardRenderer.GetCachedTileAtPoint(InputManager.Instance.MouseWorldPos(camera));
                    if (tile == null)
                        return;
                    // Check if we cicked a unit
                    Unit unit = manager.Client.GetMyUnits().Find(u => u.Location == tile.Location);
                    if (unit != null)
                    {
                        manager.Client.SelectedUnit = unit;
                        manager.Client.SelectedCommand = UnitCommandID.Null;
                        ConsoleManager.Instance.WriteLine($"Selected a unit, id:{manager.Client.SelectedUnit.UnitID}, name:{manager.Client.SelectedUnit.Name}");
                    }
                    // Check if we should execute a unti command
                    else if (manager.Client.SelectedCommand != UnitCommandID.Null)
                    {
                        if (manager.Client.SelectedUnit != null)
                            manager.Client.CommandUnit(new UnitCommand(manager.Client.SelectedCommand, manager.Client.SelectedUnit, tile));
                        manager.Client.SelectedCommand = UnitCommandID.Null;
                    }
                    // Check if we clicked a city
                    else
                    {
                        foreach (City city in manager.Client.GetMyCities())
                        {
                            if (city.Location == tile.Location)
                            {
                                manager.Client.SelectedCity = city;
                                camera.TranslationTarget = boardRenderer.GetTileCentre(city.Location);
                                camera.ZoomTarget = new Vector2(2, 2);
                                break;
                            }
                        }
                    }
                }
                else if (InputManager.Instance.OnMouseDown(MouseButton.Right) && manager.Client.SelectedUnit != null)
                {
                    manager.Client.SelectedCommand = UnitCommandID.Move;
                }
                else if (InputManager.Instance.OnMouseUp(MouseButton.Right))
                {
                    Tile tile = boardRenderer.GetCachedTileAtPoint(InputManager.Instance.MouseWorldPos(camera));
                    if (tile == null)
                        return;
                    if (manager.Client.SelectedUnit != null)
                        manager.Client.CommandUnit(new UnitCommand(UnitCommandID.Move, manager.Client.SelectedUnit, tile));
                }
            }
        }

        public override void Draw()
        {

            sbBack.Begin();
            sbFore.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camera.GetForegroundTransformation(GraphicsManager.Instance.Scale));
            sbGUI.Begin();

            boardRenderer.Draw(sbFore, font);
            unitRenderer.Draw(sbFore, font);
            lock (_lock_guiDrawCall)
            {
                canvas.Draw(sbGUI);
            }
            if (flag_drawInfo_client) DrawInfoLayer_Client(font);
            if (flag_drawInfo_server) DrawInfoLayer_Server(font);

            sbBack.End();
            sbFore.End();
            sbGUI.End();
        }

        private void DrawInfoLayer_Client(Font f)
        {
            string[] texts =
            {
                $"DOMINION CLIENT INFO",
                $"Version: {Engine.Instance.Version}",
                $"CLR Version: {Environment.Version}",
                $"OS Version: {Environment.OSVersion}",
                $"FPS: {Engine.Instance.FrameCounter.AverageFramesPerSecond}",
                $"Cursor Screen: {InputManager.Instance.MouseScreenPos()}",
                $"Cursor World: {InputManager.Instance.MouseWorldPos(camera)}",
                $"Camera Position: {camera.Translation}",
                $"Resolution: {GraphicsManager.Instance.Viewport.Width}x{GraphicsManager.Instance.Viewport.Height}",
                $"Processor Count: {Environment.ProcessorCount}",
                $"Memory: {(Environment.WorkingSet * 1e-6).ToString("0.00")} MB",
                $"Bytes Recieved: {(manager.Client.ClientStatistics.BytesRecieved * 0.001).ToString("0.00")} KB",
                $"Bytes Sent: {(manager.Client.ClientStatistics.BytesSent * 0.001).ToString("0.00")} KB",
                $"Packets Recieved: {manager.Client.ClientStatistics.PacketsRecieved}",
                $"Packets Sent: {manager.Client.ClientStatistics.PacketsSent}",
                $"Recieve Buffer Size: {(manager.Client.ClientStatistics.RecieveBufferSize * 0.001).ToString("0.00")} KB",
                $"Send Buffer Size: {(manager.Client.ClientStatistics.SendBufferSize * 0.001).ToString("0.00")} KB",
                $"CurrentScene: {SceneManager.Instance.CurrentSceneName}",
                $"SpriteScale: {GraphicsManager.Instance.Scale}",
                $"Audio Track Title: {AudioManager.Instance.CurrentTrackName}",
                $"Audio Music Volume: {AudioManager.Instance.MusicVolume}",
                $"Audio PlayerState: {AudioManager.Instance.PlayerState}",
                $"Cached Tile Count: {manager.Client.GetAllCachedTiles().Count}",
                $"Tile Count: {manager.Client.Board.GetAllTiles().Count}",
                $"Cached City Count: {manager.Client.CachedCities.Count}",
                $"City Count: {manager.Client.Cities.Count}",
                $"Cached Unit Count: {manager.Client.CachedUnits.Count}",
                (manager.Client.SelectedUnit == null) ? $"Selected Unit: null" : $"Selected Unit: {manager.Client.SelectedUnit.PlayerID}:{manager.Client.SelectedUnit.UnitID}:{manager.Client.SelectedUnit.Name}",
                (manager.Client.SelectedCity == null) ? $"Selected City: null" : $"Selected City: {manager.Client.SelectedCity.PlayerID}:{manager.Client.SelectedCity.Population}:{manager.Client.SelectedCity.Name}",
                $"Board Dim: {{{manager.Client.BoardWidth}, {manager.Client.BoardHeight}}}",
                $"Turn State: {manager.Client.TurnState}",
                $"Turn Number: {manager.Client.TurnNumber}",
                $"Turn Time Limit: {manager.Client.TurnTimeLimit}",
                $"Turn Timer Progress: {manager.Client.TurnTimer.ElapsedMilliseconds}",

            };
            DrawInfoText(f, new Vector2(10, 45), texts);
        }

        private void DrawInfoLayer_Server(Font f)
        {
            if (manager.Server.Running)
            {
                string[] texts =
                {
                    $"DOMINION SERVER INFO",
                    $"Server Running: {manager.Server.Running}",
                    $"Server Password: {manager.Server.ServerPassword}",
                    $"Banned Address Count: {manager.Server.BannedAddresses.Count}",
                    $"Connection Count: {manager.Server.ServerStatistics.ConnectionCount}",
                    $"Bytes Recieved: {(manager.Server.ServerStatistics.BytesRecieved * 0.001).ToString("0.00")} KB",
                    $"Bytes Sent: {(manager.Server.ServerStatistics.BytesSent * 0.001).ToString("0.00")} KB",
                    $"Packets Recieved: {manager.Server.ServerStatistics.PacketsRecieved}",
                    $"Packets Sent: {manager.Server.ServerStatistics.PacketsSent}",
                    $"Recieve Buffer Size: {(manager.Server.ServerStatistics.RecieveBufferSize * 0.001).ToString("0.00")} KB",
                    $"Send Buffer Size: {(manager.Server.ServerStatistics.SendBufferSize * 0.001).ToString("0.00")} KB",
                    $"Port: {manager.Server.ServerStatistics.Port}",
                    $"Player Count: {manager.Server.LobbyState.Players.Count}",
                    $"Game Speed: {manager.Server.LobbyState.GameSpeed}",
                    $"World Size: {manager.Server.LobbyState.WorldSize}",
                    $"World Type: {manager.Server.LobbyState.WorldType}",
                    $"Turn Number: {manager.Server.TurnNumber}",
                    $"Turn Time Limit: {manager.Server.TurnTimeLimit}",
                    $"Turn Timer Progress: {manager.Server.TurnTimer.ElapsedMilliseconds}",
                };
                DrawInfoText(f, new Vector2(500, 45), texts);
            }
            else
            {
                string[] texts = { "Running in client only mode" };
                DrawInfoText(f, new Vector2(500, 45), texts);
            }
        }

        private void DrawInfoText(Font f, Vector2 offset, string[] texts)
        {
            int textHeight = Convert.ToInt32(font.MeasureString("|").Y);
            int charWidth = Convert.ToInt32(font.MeasureString("W").X);

            int spacing = 25;
            int xOffset = (int)offset.X;
            int yOffset = (int)offset.Y;

            for (int i = 0; i < texts.Length; i++)
            {
                int textLen = Convert.ToInt32(font.MeasureString(texts[i]).X);
                int x = xOffset;
                int y = i * spacing + yOffset;
                GraphicsHelper.DrawLine(sbGUI, new Vector2(x + 5 + charWidth / 2, y + textHeight / 2), new Vector2(x + 10 + charWidth / 2 + textLen, y + textHeight / 2), textHeight, Color.Black);
                f.DrawString(sbGUI, texts[i], new Vector2(x, y), Color.White);
            }
        }
    }
}
