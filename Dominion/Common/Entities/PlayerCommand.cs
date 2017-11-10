// Dominion - Copyright (C) Timothy Ings
// PlayerCommand.cs
// This file defines classes that define a player command

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Common.Entities
{
    public enum PlayerCommandID
    {
        Null,
        SelectTech,
        UnlockPolicy
    }

    [Serializable]
    public class PlayerCommand
    {
        /// <summary>
        /// The command's id
        /// </summary>
        public PlayerCommandID CommandID { get; set; }

        /// <summary>
        /// The player executing the command
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// The command's arguments
        /// </summary>
        public object[] Arguments { get; set; }

        public PlayerCommand(PlayerCommandID cmd, params object[] args)
        {
            CommandID = cmd;
            Arguments = args;
        }
    }
}
