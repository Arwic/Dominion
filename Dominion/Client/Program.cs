using System;

namespace Dominion.Client
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var dominion = new Dominion())
                dominion.Run();
        }
    }
#endif
}
