// Dominion - Copyright (C) Timothy Ings
// UnitCommand.cs
// This file defines classes that define a unit command

using ArwicEngine.Forms;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace Dominion.Common.Entities
{
    public enum UnitCommandID
    {
        UNITCMD_NULL,
        UNITCMD_MOVE,
        UNITCMD_DISBAND,
        UNITCMD_SLEEP,
        UNITCMD_SKIP,
        UNITCMD_GUARD,
        UNITCMD_SETTLE,
        UNITCMD_MELEE,
        UNITCMD_BOMBARD,
        UNITCMD_BUILD_FARM,
        UNITCMD_BUILD_FORT,
        UNITCMD_BUILD_LUMBERMILL,
        UNITCMD_BUILD_MINE,
        UNITCMD_BUILD_TRADINGPOST,
        UNITCMD_BUILD_ROADS,
        UNITCMD_BUILD_RAILROADS,
        UNITCMD_BUILD_CAMP,
        UNITCMD_BUILD_FISHINGBOATS,
        UNITCMD_BUILD_OFFSHOREPLATFORM,
        UNITCMD_BUILD_PASTURE,
        UNITCMD_BUILD_PLANTATION,
        UNITCMD_BUILD_QUARRY,
        UNITCMD_BUILD_ACADEMY,
        UNITCMD_BUILD_CITIDEL,
        UNITCMD_BUILD_CUSTOMSHOUSE,
        UNITCMD_BUILD_HOLYSITE,
        UNITCMD_BUILD_LANDMARK,
        UNITCMD_BUILD_MANUFACTORY,
        UNITCMD_REPAIR,
        UNITCMD_CLEAN
    }

    public enum UnitCommandTargetType
    {
        Null,
        Tile,
        Instant,
        Aura
    }

    [Serializable()]
    public class UnitCommand
    {
        /// <summary>
        /// The command's id
        /// </summary>
        public UnitCommandID CommandID { get; set; }

        /// <summary>
        /// The id of the unit being commanded
        /// </summary>
        public int UnitInstanceID { get; set; }
        
        /// <summary>
        /// The location of the tile target of the command
        /// </summary>
        public Point TileLocation
        {
            get
            {
                return new Point(_locationX, _locationY);
            }
            set
            {
                _locationX = value.X;
                _locationY = value.Y;
            }
        }
        private int _locationX;
        private int _locationY;

        public UnitCommand(UnitCommandID cmd, UnitInstance unit, Tile tile)
        {
            CommandID = cmd;
            UnitInstanceID = unit.InstanceID;
            if (tile == null)
                TileLocation = new Point(-1, -1);
            else
                TileLocation = tile.Location;
        }

        /// <summary>
        /// Returns the target type of the command
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static UnitCommandTargetType GetTargetType(UnitCommandID id)
        {
            switch (id)
            {
                case UnitCommandID.UNITCMD_MOVE:
                case UnitCommandID.UNITCMD_MELEE:
                    return UnitCommandTargetType.Tile;
                case UnitCommandID.UNITCMD_DISBAND:
                case UnitCommandID.UNITCMD_SLEEP:
                case UnitCommandID.UNITCMD_SKIP:
                case UnitCommandID.UNITCMD_SETTLE:
                default:
                    return UnitCommandTargetType.Instant;
            }
        }

        /// <summary>
        /// Returns the rich text icon of the command
        /// </summary>
        /// <param name="cmdid"></param>
        /// <returns></returns>
        public static RichText GetCommandIcon(UnitCommandID cmdid)
        {
            string iconCode = ((char)FontSymbol.CrossCircle).ToString();
            switch (cmdid)
            {
                case UnitCommandID.UNITCMD_MOVE:
                    iconCode = ((char)FontSymbol.Footsteps).ToString();
                    break;
                case UnitCommandID.UNITCMD_DISBAND:
                    iconCode = ((char)FontSymbol.UserRemove).ToString();
                    break;
                case UnitCommandID.UNITCMD_SLEEP:
                    iconCode = ((char)FontSymbol.PowerIcon).ToString();
                    break;
                case UnitCommandID.UNITCMD_SKIP:
                    iconCode = ((char)FontSymbol.Cog).ToString();
                    break;
                case UnitCommandID.UNITCMD_SETTLE:
                    iconCode = ((char)FontSymbol.Flag).ToString();
                    break;
                case UnitCommandID.UNITCMD_MELEE:
                    iconCode = ((char)FontSymbol.Crosshairs).ToString();
                    break;
                case UnitCommandID.UNITCMD_BOMBARD:
                    iconCode = ((char)FontSymbol.Crosshairs).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_FARM:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_FORT:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_LUMBERMILL:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_MINE:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_TRADINGPOST:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_ROADS:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_RAILROADS:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_CAMP:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_FISHINGBOATS:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_OFFSHOREPLATFORM:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_PASTURE:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_PLANTATION:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_QUARRY:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_ACADEMY:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_CITIDEL:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_CUSTOMSHOUSE:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_HOLYSITE:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_LANDMARK:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_BUILD_MANUFACTORY:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_REPAIR:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_CLEAN:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.UNITCMD_NULL:
                default:
                    iconCode = ((char)FontSymbol.CrossCircle).ToString();
                    break;
            }
            return iconCode.ToRichText(null, RichText.SymbolFont);
        }

        /// <summary>
        /// Converts a unit command name to a presentable form
        /// I.e. converts "UNITCMD_MY_NAME" to "My Name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatName(string name)
        {
            string prefix = "UNITCMD_";

            // check if the string is valid
            if (!name.Contains(prefix))
                return name;

            // strip "UNITCMD_"
            // "UNITCMD_MY_NAME" -> "MY_NAME"
            name = name.Remove(0, prefix.Length);

            // replace all "_"
            // "MY_NAME" -> "MY NAME"
            name = name.Replace('_', ' ');

            // convert to lower case
            // "MY NAME" -> "my name"
            name = name.ToLowerInvariant();

            // convert to title case
            // "my name" -> "My Name"
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            name = textInfo.ToTitleCase(name);

            return name;
        }
    }
}