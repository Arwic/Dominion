// Dominion - Copyright (C) Timothy Ings
// Engine.cs
// This file defines classes that form the basis of the engine
// There should be only one engine, so changing this to be a static class might be a good idea

using ArwicEngine.Audio;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using ArwicEngine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using static ArwicEngine.Constants;

namespace ArwicEngine.Core
{
    /// <summary>
    /// A class that provides references to most things in the engine
    /// </summary>
    public sealed class Engine
    {
        // Singleton pattern
        private static object _lock_instance = new object();
        private static Engine _instance;
        public static Engine Instance
        {
            get
            {
                lock (_lock_instance)
                {
                    if (_instance == null)
                        throw new InvalidOperationException("Engine.Init() must be called first");
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Gets the version of the engine
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the frame counter
        /// </summary>
        public FrameCounter FrameCounter { get; } = new FrameCounter();

        /// <summary>
        /// Gets the content manager
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Gets the a value indicating whether or not the game window is active
        /// </summary>
        public bool WindowActive => game.IsActive;

        /// <summary>
        /// Gets the xna game window
        /// </summary>
        public GameWindow Window => game.Window;

        /// <summary>
        /// Gets or sets the xna gameTime value
        /// </summary>
        public GameTime GameTime { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float DeltaTime
        {
            get
            {
                if (GameTime == null)
                    return 0f;
                return (float)GameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Gets or sets the color to which the graphics device is set to before rendering the next frame
        /// </summary>
        public Color ClearColor { get; set; }

        private Game game;

        // Singleton
        private Engine() { }

        /// <summary>
        /// Creates a new engine with an optional pointer to a draw surface, if none is provided a window is used
        /// </summary>
        /// <param name="game"></param>
        /// <param name="drawSurface"></param>
        public static void Init(Game game, IntPtr? drawSurface = null)
        {
            if (_instance != null)
                throw new InvalidOperationException("Init cannot be called more than once");

            _instance = new Engine();

            // Set engine variables
            Instance.ClearColor = Color.Black;
            Instance.game = game;
            game.IsFixedTimeStep = false;
            game.IsMouseVisible = true;
            Instance.Content = game.Content;

            // Make sure to stop audio and save the config before exiting
            game.Exiting += (s, a) =>
            {
                AudioManager.Instance.Shutdown();
                ConfigManager.Instance.Write(CONFIG_PATH);
                Environment.Exit(0);
            };

            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            Instance.Version = RetrieveLinkerTimestamp().ToString("dd MMM yyyy HH:mm");

            // set up a normal window
            if (drawSurface == null)
            {
                game.Window.Title = $"{ENGINE_NAME} - {Instance.Version}";
                System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(game.Window.Handle);
                form.Location = new System.Drawing.Point(0, 0);
            }
            // set up a custom draw surface
            else
            {
                System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(game.Window.Handle);
                form.Hide();
            }

            // initialise helper classes
            RandomHelper.Init();
            EventInput.Init(game.Window.Handle);

            // set up managers
            Instance.Content.RootDirectory = "Content";
            AppDomain.CurrentDomain.UnhandledException += (s, a) => Console.WriteLine($"UNHANDLED EXCEPTION: {a.ExceptionObject.ToString()}", MsgType.Failed);
            // run the config file as a script
            ConsoleManager.Instance.RunScript(CONFIG_PATH);
            GraphicsManager.Init(game, drawSurface);
            InitGUIDefaults();
            GraphicsHelper.Init(GraphicsManager.Instance.Device);
            RichText.Init(Instance.Content);
#if DEBUG
            Console.WriteLine("ASSEMBLY IN DEBUG MODE", MsgType.Warning);
#endif
        }

        private static void InitGUIDefaults()
        {
            // initialise defaults for the gui system
            Control.InitDefaults();
            Button.InitDefaults();
            CheckBox.InitDefaults();
            ComboBox.InitDefaults();
            Form.InitDefaults();
            ProgressBar.InitDefaults();
            ScrollBox.InitDefaults();
            TextBox.InitDefaults();
            ToolTip.InitDefaults();
        }
        
        /// <summary>
        /// Exits the engine with an exit code
        /// </summary>
        /// <param name="exitCode"></param>
        public void Exit(int exitCode)
        {
            ConfigManager.Instance.Write(CONFIG_PATH);
            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Retrieves the linker timestamp which indicates the compile date of the assembly
        /// </summary>
        /// <returns></returns>
        public static DateTime RetrieveLinkerTimestamp()
        {
            // get the current assembly's filepath
            string filePath = Assembly.GetCallingAssembly().Location;
            // define constants and a buffer
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            Stream s = null;

            try
            {
                // try reading the exe into memory
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                // close the stream
                if (s != null)
                {
                    s.Close();
                }
            }

            // parse the data in the buffer
            int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            // create a date time object representing the time the assembly was compiled
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();
            return dt;
        }

        /// <summary>
        /// Updates the engine, should be run every frame before draw is called
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            GameTime = gameTime;
            FrameCounter.Update(DeltaTime);
            SceneManager.Instance.Update();
            InputManager.Instance.Update();
        }

        /// <summary>
        /// Draws the current scene, should be run every frame after update is called
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            GameTime = gameTime;
            GraphicsManager.Instance.Device.Clear(ClearColor);
            SceneManager.Instance.Draw();
        }
    }
}
