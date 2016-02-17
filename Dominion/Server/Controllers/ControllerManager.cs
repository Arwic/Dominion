using Dominion.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Server.Controllers
{
    public class ControllerManager
    {
        public FactoryController Factory { get; }
        public BoardController Board { get; }
        public PlayerController Player { get; }
        public CityController City { get; }
        public UnitController Unit { get; }

        public ControllerManager()
        {
            Factory = new FactoryController(this);
            Board = new BoardController(this);
            Player = new PlayerController(this);
            City = new CityController(this);
            Unit = new UnitController(this);
        }

        public void ProcessTurn()
        {
            Factory.ProcessTurn();
            Board.ProcessTurn();
            Player.ProcessTurn();
            City.ProcessTurn();
            Unit.ProcessTurn();
        }
    }
}
