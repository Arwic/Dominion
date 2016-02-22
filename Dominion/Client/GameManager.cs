// Dominion - Copyright (C) Timothy Ings
// GameManager.cs
// This file defines classes that define the game manager

namespace Dominion.Client
{
    // the game manager holds references to the client and the server
    // this is so we can re instantiate the client and the server while keeping every reference to it up to date
    public class GameManager
    {
        /// <summary>
        /// The client
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// The server
        /// </summary>
        public Server.Server Server { get; set; }
    }
}
