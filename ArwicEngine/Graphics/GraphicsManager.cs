using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows.Forms;
using static ArwicEngine.Constants;

namespace ArwicEngine.Graphics
{
    public class GraphicsManager
    {
        public Engine Engine { get; set; }

        public Matrix ScaleMatrix { get; private set; }
        public float Scale { get; set; }
        public GraphicsDeviceManager DeviceManager { get; private set; }
        public GraphicsDevice Device => DeviceManager.GraphicsDevice;
        public Viewport Viewport => Device.Viewport;
        private IntPtr? drawSurface;

        public GraphicsManager(Engine engine, Game game, IntPtr? drawSurface = null)
        {
            Engine = engine;
            DeviceManager = new GraphicsDeviceManager(game);
            if (drawSurface != null)
            {
                this.drawSurface = drawSurface.Value;
                DeviceManager.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            }
            DeviceManager.CreateDevice();

            Apply();
        }

        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if (drawSurface != null)
                e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface.Value;
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
                    string[] resolution = Engine.Config.GetVar(CONFIG_RESOLUTION).Split('x');
                    int resX = Convert.ToInt32(resolution[0]);
                    int resY = Convert.ToInt32(resolution[1]);
                    bool fullscreen = Convert.ToInt32(Engine.Config.GetVar(CONFIG_DISPLAYMODE)) == 1;
                    bool vsync = Convert.ToInt32(Engine.Config.GetVar(CONFIG_VSYNC)) == 1;

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
                    Engine.Config.SetDefaults();
                Engine.Exit(EXIT_FAILURE);
            }
        }
    }
}
