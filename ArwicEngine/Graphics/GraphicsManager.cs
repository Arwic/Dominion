using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows.Forms;
using static ArwicEngine.Constants;

namespace ArwicEngine.Graphics
{
    public sealed class GraphicsManager
    {
        // Singleton pattern
        private static object _lock_instance = new object();
        private static GraphicsManager _instance;
        public static GraphicsManager Instance
        {
            get
            {
                lock (_lock_instance)
                {
                    return _instance;
                }
            }
        }

        public Matrix ScaleMatrix { get; private set; }
        public float Scale { get; set; }
        public GraphicsDeviceManager DeviceManager { get; private set; }
        public GraphicsDevice Device => DeviceManager.GraphicsDevice;
        public Viewport Viewport => Device.Viewport;
        private IntPtr? drawSurface;

        private GraphicsManager() { }

        public static void Init(Game game, IntPtr? drawSurface = null)
        {
            if (_instance != null)
                throw new InvalidOperationException("Init cannot be called more than once");

            _instance = new GraphicsManager();

            Instance.DeviceManager = new GraphicsDeviceManager(game);
            if (drawSurface != null)
            {
                Instance.drawSurface = drawSurface.Value;
                Instance.DeviceManager.PreparingDeviceSettings += (s, a) =>
                {
                    if (drawSurface != null)
                        a.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface.Value;
                };
            }
            Instance.DeviceManager.CreateDevice();

            Instance.Apply();
        }

        public static float Angle(Vector2 v1, Vector2 v2)
        {
            double dot = v1.X * v2.X + v1.Y * v2.Y;
            double det = v1.X * v2.Y - v1.Y * v2.X;
            double angle = Math.Atan2(det, dot);
            return Convert.ToSingle(angle);
        }

        public void Apply()
        {
            try
            {
                if (true || drawSurface == null)
                {
                    // Sets graphics values
                    string[] resolution = ConfigManager.Instance.GetVar(CONFIG_GFX_RESOLUTION).Split('x');
                    int resX = Convert.ToInt32(resolution[0]);
                    int resY = Convert.ToInt32(resolution[1]);
                    bool fullscreen = Convert.ToInt32(ConfigManager.Instance.GetVar(CONFIG_GFX_DISPLAY_MODE)) == 1;
                    bool vsync = Convert.ToInt32(ConfigManager.Instance.GetVar(CONFIG_GFX_VSYNC)) == 1;

                    DeviceManager.PreferredBackBufferWidth = resX;
                    DeviceManager.PreferredBackBufferHeight = resY;
                    DeviceManager.IsFullScreen = fullscreen;
                    DeviceManager.SynchronizeWithVerticalRetrace = vsync;

                    DeviceManager.ApplyChanges();
                }

                // Get graphics properties for display scaling
                Scale = Device.Viewport.Width / 1920f;
                ScaleMatrix = Matrix.CreateScale(Scale, Scale, 1f);
            }
            catch (Exception)
            {
                var result = MessageBox.Show("Would you like to reset the configuration file to its defaults before exiting?", "Error Applying Graphics Settings", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                    ConfigManager.Instance.SetDefaults();
                Engine.Instance.Exit(EXIT_FAILURE);
            }
        }
    }
}
