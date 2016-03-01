using Dominion.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Common.Entities
{
    public enum ProductionType
    {
        UNIT,
        BUILDING
    }

    [Serializable]
    public class Production
    {
        public string Name { get; set; }
        public ProductionType ProductionType { get; set; }
        public int Progress { get; set; }
        public int Cost { get; set; }

        public Production(Unit unit)
        {
            ProductionType = ProductionType.UNIT;
            Name = unit.Name;
            Cost = unit.Cost;
            Progress = 0;
        }

        public Production(Building building)
        {
            ProductionType = ProductionType.BUILDING;
            Name = building.Name;
            Cost = building.Cost;
            Progress = 0;
        }

        public Production(string name)
        {
            Name = name;
            if (name.Contains("BUILDING_"))
                ProductionType = ProductionType.BUILDING;
            else if (name.Contains("UNIT_"))
                ProductionType = ProductionType.UNIT;
        }
    }
}
