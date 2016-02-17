using Dominion.Common.Factories;

namespace Dominion.Server.Controllers
{
    public class FactoryController : Controller
    {
        public BuildingFactory Building { get; }
        public EmpireFactory Empire { get; }
        public ProductionFactory Production { get; }
        public UnitFactory Unit { get; }

        public FactoryController(ControllerManager manager)
            : base(manager)
        {
            Building = BuildingFactory.FromFile("Content/Data/BuildingList.xml");
            Empire = EmpireFactory.FromFile("Content/Data/EmpireList.xml");
            Production = ProductionFactory.FromFile("Content/Data/ProductionList.xml");
            Unit = UnitFactory.FromFile("Content/Data/UnitList.xml");
        }
    }
}
