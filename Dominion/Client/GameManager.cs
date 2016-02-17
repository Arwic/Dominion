using Dominion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Client
{
    public class GameManager
    {
        public Client Client { get; set; }
        public Server.Server Server { get; set; }
    }
}
