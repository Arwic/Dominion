// Dominion - Copyright (C) Timothy Ings
// Program.cs
// This file defines classes that define the application entry point

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
