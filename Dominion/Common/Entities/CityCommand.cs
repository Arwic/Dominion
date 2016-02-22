// Dominion - Copyright (C) Timothy Ings
// CityCommand.cs
// This file defines classes that define a city command

using System;

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
        /// <summary>
        /// The id of the command
        /// </summary>
        public CityCommandID CommandID { get; set; }

        /// <summary>
        /// The city executing the command's id
        /// </summary>
        public int CityID { get; set; }

        /// <summary>
        /// The player executing the command's id
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// The command's arguments
        /// </summary>
        public object[] Arguments { get; set; }

        public CityCommand(CityCommandID cmd, City city, params object[] args)
        {
            CommandID = cmd;
            CityID = city.InstanceID;
            Arguments = args;
        }
    }
}
