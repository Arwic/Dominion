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
