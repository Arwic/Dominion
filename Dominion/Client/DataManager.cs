using Dominion.Common.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Client
{
    public class DataManager
    {
        /// <summary>
        /// Gets the building data manager
        /// </summary>
        public BuildingManager Building { get; set; }

        /// <summary>
        /// Gets the empire data manager
        /// </summary>
        public EmpireManager Empire { get; set; }

        /// <summary>
        /// Gets the unit data manager
        /// </summary>
        public UnitManager Unit { get; set; }

        /// <summary>
        /// Gets the tech data manager
        /// </summary>
        public TechnologyManager Tech { get; set; }

        public DataManager() { }
    }
}
