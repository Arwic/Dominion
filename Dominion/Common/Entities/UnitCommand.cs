using ArwicEngine.Forms;
using Microsoft.Xna.Framework;
using System;

namespace Dominion.Common.Entities
{
    public enum UnitCommandID
    {
        Null,
        Move,
        Disband,
        Sleep,
        Skip,
        Settle,
        MeleeAttack,
        RangedAttack,
        BuildImprovment_Farm,
        BuildImprovment_Fort,
        BuildImprovment_LumberMill,
        BuildImprovment_Mine,
        BuildImprovment_TradingPost,
        BuildImprovment_Roads,
        BuildImprovment_RailRoads,
        BuildImprovment_Camp,
        BuildImprovment_FishingBoats,
        BuildImprovment_OffshorePlatform,
        BuildImprovment_Pasture,
        BuildImprovment_Plantation,
        BuildImprovment_Quarry,
        BuildImprovment_Academy,
        BuildImprovment_Citidel,
        BuildImprovment_CustomsHouse,
        BuildImprovment_HolySite,
        BuildImprovment_Landmark,
        BuildImprovment_Manufactory,
        RepairImprovement,
        CleanFallout
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
        public UnitCommandID CommandID { get; set; }
        public int UnitInstanceID { get; set; }
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

        public UnitCommand(UnitCommandID cmd, Unit unit, Tile tile)
        {
            CommandID = cmd;
            UnitInstanceID = unit.InstanceID;
            if (tile == null)
                TileLocation = new Point(-1, -1);
            else
                TileLocation = tile.Location;
        }

        public static UnitCommandTargetType GetTargetType(UnitCommandID id)
        {
            switch (id)
            {
                case UnitCommandID.Move:
                case UnitCommandID.MeleeAttack:
                    return UnitCommandTargetType.Tile;
                case UnitCommandID.Disband:
                case UnitCommandID.Sleep:
                case UnitCommandID.Skip:
                case UnitCommandID.Settle:
                default:
                    return UnitCommandTargetType.Instant;
            }
        }

        public static RichText GetCommandIcon(UnitCommandID cmdid)
        {
            string iconCode = ((char)FontSymbol.CrossCircle).ToString();
            switch (cmdid)
            {
                case UnitCommandID.Move:
                    iconCode = ((char)FontSymbol.Footsteps).ToString();
                    break;
                case UnitCommandID.Disband:
                    iconCode = ((char)FontSymbol.UserRemove).ToString();
                    break;
                case UnitCommandID.Sleep:
                    iconCode = ((char)FontSymbol.PowerIcon).ToString();
                    break;
                case UnitCommandID.Skip:
                    iconCode = ((char)FontSymbol.Cog).ToString();
                    break;
                case UnitCommandID.Settle:
                    iconCode = ((char)FontSymbol.Flag).ToString();
                    break;
                case UnitCommandID.MeleeAttack:
                    iconCode = ((char)FontSymbol.Crosshairs).ToString();
                    break;
                case UnitCommandID.RangedAttack:
                    iconCode = ((char)FontSymbol.Crosshairs).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Farm:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Fort:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_LumberMill:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Mine:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_TradingPost:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Roads:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_RailRoads:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Camp:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_FishingBoats:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_OffshorePlatform:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Pasture:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Plantation:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Quarry:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Academy:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Citidel:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_CustomsHouse:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_HolySite:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Landmark:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.BuildImprovment_Manufactory:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.RepairImprovement:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.CleanFallout:
                    iconCode = ((char)FontSymbol.Gavel).ToString();
                    break;
                case UnitCommandID.Null:
                default:
                    iconCode = ((char)FontSymbol.CrossCircle).ToString();
                    break;
            }
            return iconCode.ToRichText(null, RichText.SymbolFont);
        }
    }
}