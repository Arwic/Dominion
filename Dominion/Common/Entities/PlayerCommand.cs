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
        SelectTech
    }

    [Serializable]
    public class PlayerCommand
    {
        public PlayerCommandID CommandID { get; set; }
        public int PlayerID { get; set; }
        public object[] Arguments { get; set; }

        public PlayerCommand(PlayerCommandID cmd, params object[] args)
        {
            CommandID = cmd;
            Arguments = args;
        }
    }
}
