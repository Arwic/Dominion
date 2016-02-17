using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Server.Controllers
{
    public abstract class Controller
    {
        public ControllerManager Controllers { get; }

        public Controller(ControllerManager manager)
        {
            Controllers = manager;
        }

        public virtual void ProcessTurn()
        {

        }
    }
}
