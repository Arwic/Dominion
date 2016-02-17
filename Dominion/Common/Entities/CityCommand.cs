using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Common.Entities
{
    public enum CityCommandID
    {
        Null,
        Rename,
        ChangeProduction,
        QueueProduction,
        CancelProduction,
        ReorderProductionMoveUp,
        ReorderProductionMoveDown,
        BuyProduction,
        ChangeCitizenFocus
    }

    [Serializable]
    public class CityCommand
    {
        public CityCommandID CommandID { get; set; }
        public int CityID { get; set; }
        public int PlayerID { get; set; }
        public object[] Arguments { get; set; }

        public CityCommand(CityCommandID cmd, City city, params object[] args)
        {
            CommandID = cmd;
            CityID = city.InstanceID;
            Arguments = args;
        }
    }
}
