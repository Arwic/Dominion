using System;

namespace ArwicInterfaceDesigner
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MainForm form = new MainForm();
            Game1 game = new Game1(form.GetDrawSurface());
            form.Game = game;
            form.Show();
            game.Run();
        }
    }
#endif
}
