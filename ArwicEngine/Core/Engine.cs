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
    public class Engine
    {
        public string Version { get; }

        public GraphicsManager Graphics { get; }
        public AudioManager Audio { get; }
        public ConsoleManager Console { get; }
        public SceneManager Scene { get; }
        public ConfigManager Config { get; }
        public InputManager Input { get; }
        public FrameCounter FrameCounter { get; }
        public ContentManager Content { get; }
        public bool WindowActive => game.IsActive;
        public GameWindow Window => game.Window;
        public Color ClearColor { get; set; }

        private Game game;

        public Engine(Game game, IntPtr? drawSurface = null)
        {
            ClearColor = Color.Black;
            this.game = game;
            game.IsFixedTimeStep = false;
            game.IsMouseVisible = true;
            Content = game.Content;
            game.Exiting += (s, a) =>
            {
                Audio.Shutdown();
                Config.Write(CONFIG_PATH);
                Environment.Exit(0);
            };

            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            Version = RetrieveLinkerTimestamp().ToString("dd MMM yyyy HH:mm");

            if (drawSurface == null)
            {
                game.Window.Title = $"{ENGINE_NAME} - {Version}";
                System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(game.Window.Handle);
                form.Location = new System.Drawing.Point(0, 0);
            }
            else
            {
                System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(game.Window.Handle);
                form.Hide();
            }

            RandomHelper.Init();
            EventInput.Init(game.Window.Handle);

            Content.RootDirectory = "Content";
            Config = new ConfigManager(this);
            Console = new ConsoleManager(this);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.RunScript(CONFIG_PATH);
            Audio = new AudioManager(this);
            Graphics = new GraphicsManager(this, game, drawSurface);
            InitGUIDefaults();
            GraphicsHelper.Init(Graphics.Device);
            Input = new InputManager(this);
            Scene = new SceneManager(this);
            FrameCounter = new FrameCounter();
            RichText.Init(Content);
#if DEBUG
            Console.WriteLine("ASSEMBLY IN DEBUG MODE", MsgType.Warning);
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"UNHANDLED EXCEPTION: {e.ExceptionObject.ToString()}", MsgType.Failed);
        }

        private void InitGUIDefaults()
        {
            Control.InitDefaults(this);
            Button.InitDefaults(this);
            CheckBox.InitDefaults(this);
            ComboBox.InitDefaults(this);
            Form.InitDefaults(this);
            ProgressBar.InitDefaults(this);
            ScrollBox.InitDefaults(this);
            TextBox.InitDefaults(this);
            ToolTip.InitDefaults(this);
        }
        
        public void Exit(int exitCode)
        {
            Config.Write(CONFIG_PATH);
            Environment.Exit(exitCode);
        }

        public static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();
            return dt;
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FrameCounter.Update(delta);
            Scene.Update(delta);
            Input.Update();
        }

        public void Draw(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Graphics.Device.Clear(ClearColor);
            Scene.Draw(delta);
        }
    }
}
