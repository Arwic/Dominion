using ArwicEngine.Core;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Server.Controllers
{
    public class UnitEventArgs : EventArgs
    {
        public Unit Unit { get; }

        public UnitEventArgs(Unit unit)
        {
            Unit = unit;
        }
    }

    public class UnitController : Controller
    {
        private List<Unit> units;

        public event EventHandler<UnitEventArgs> UnitAdded;
        public event EventHandler<UnitEventArgs> UnitRemoved;
        public event EventHandler<UnitEventArgs> UnitUpdated;
        protected virtual void OnUnitAdded(UnitEventArgs e)
        {
            if (UnitAdded != null)
                UnitAdded(this, e);
        }
        protected virtual void OnUnitRemoved(UnitEventArgs e)
        {
            if (UnitRemoved != null)
                UnitRemoved(this, e);
        }
        protected virtual void OnUnitUpdated(UnitEventArgs e)
        {
            if (UnitUpdated != null)
                UnitUpdated(this, e);
        }

        public UnitController(ControllerManager manager)
            : base(manager)
        {
            units = new List<Unit>();
        }

        public override void ProcessTurn()
        {
            foreach (Unit unit in units)
            {
                Move(unit);
                BuildUnitCommands(unit);
                unit.Movement = unit.Constants.Movement;
                unit.Actions = unit.Constants.Actions;
                unit.Skipping = false;
            }
        }

        public List<Unit> GetAllUnits()
        {
            return units;
        }

        public List<Unit> GetPlayerUnits(int playerID)
        {
            return units.FindAll(u => u.PlayerID == playerID);
        }

        public Unit GetUnit(Point location)
        {
            return units.Find(u => u.Location == location);
        }

        public Unit GetUnit(int instanceID)
        {
            return units.Find(u => u.InstanceID == instanceID);
        }

        public void AddUnit(int unitID, int playerID, Point location)
        {
            Unit unit = new Unit(Controllers.Factory.Unit, unitID, playerID, location);
            units.Add(unit);

            OnUnitAdded(new UnitEventArgs(unit));
        }

        public void RemoveUnit(Unit unit)
        {
            units.Remove(unit);

            OnUnitRemoved(new UnitEventArgs(unit));
        }

        public void BuildUnitCommands(Unit unit)
        {
            unit.Commands = new List<int>();
            for (int i = 0; i < unit.Constants.Commands.Count; i++)
            {
                int cmdID = unit.Constants.Commands[i];
                UnitCommandID cmd = (UnitCommandID)cmdID;
                switch (cmd)
                {
                    case UnitCommandID.Move:
                    case UnitCommandID.Disband:
                    case UnitCommandID.Sleep:
                    case UnitCommandID.Skip:
                    case UnitCommandID.Settle:
                    case UnitCommandID.MeleeAttack:
                    case UnitCommandID.RangedAttack:
                        unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Farm:
                        unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Fort:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Engineering).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_LumberMill:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Construction).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Mine:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Mining).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_TradingPost:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Guilds).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Roads:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.TheWheel).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_RailRoads:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Railroad).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Camp:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Trapping).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_FishingBoats:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Sailing).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_OffshorePlatform:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Refrigeration).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Pasture:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.AnimalHusbandry).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Plantation:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Calendar).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Quarry:
                        if (Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetNode((int)TechNodes.Masonry).Unlocked)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Academy:
                        if ((UnitID)unit.UnitID == UnitID.Great_Scientist)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Citidel:
                        if ((UnitID)unit.UnitID == UnitID.Great_General)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_CustomsHouse:
                        if ((UnitID)unit.UnitID == UnitID.Great_Merchant)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_HolySite:
                        if ((UnitID)unit.UnitID == UnitID.Great_Prophet)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Landmark:
                        if ((UnitID)unit.UnitID == UnitID.Great_Artist)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.BuildImprovment_Manufactory:
                        if ((UnitID)unit.UnitID == UnitID.Great_Engineer)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.RepairImprovement:
                        if ((UnitID)unit.UnitID == UnitID.Worker && Controllers.Board.GetTile(unit.Location).Pillaged)
                            unit.Commands.Add(cmdID);
                        break;
                    case UnitCommandID.CleanFallout:
                        if ((UnitID)unit.UnitID == UnitID.Worker && Controllers.Board.GetTile(unit.Location).Fallout)
                            unit.Commands.Add(cmdID);
                        break;
                }
            }
        }

        public void CommandUnit(UnitCommand cmd)
        {
            Tile tile = null;
            if (cmd.TileLocation != new Point(-1, -1))
                tile = Controllers.Board.GetTile(cmd.TileLocation);

            Unit unit = GetUnit(cmd.UnitInstanceID);
            if (unit == null)
                return;

            Unit unitTarget = null;
            if (tile != null)
                unitTarget = GetUnit(tile.Location);

            City cityTarget = null;
            if (tile != null)
                cityTarget = Controllers.City.GetCity(tile.Location);

            switch (cmd.CommandID)
            {
                case UnitCommandID.Move:
                    unit.MovementQueue = Board.FindPath(unit.Location, tile.Location, Controllers.Board.Board);
                    Move(unit);
                    break;
                case UnitCommandID.Disband:
                    RemoveUnit(unit);
                    break;
                case UnitCommandID.Sleep:
                    SleepUnit(unit);
                    break;
                case UnitCommandID.Skip:
                    SkipUnit(unit);
                    break;
                case UnitCommandID.Settle:
                    if (Controllers.City.SettleCity(unit))
                        RemoveUnit(unit);
                    break;
                case UnitCommandID.MeleeAttack:
                    if (unitTarget != null)
                        MeleeAttack(unit, unitTarget);
                    else if (cityTarget != null)
                        MeleeAttack(unit, cityTarget);
                    break;
                case UnitCommandID.RangedAttack:
                    if (unitTarget != null)
                        RangedAttack(unit, unitTarget);
                    else if (cityTarget != null)
                        RangedAttack(unit, cityTarget);
                    break;
                case UnitCommandID.BuildImprovment_Farm:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Farm);
                    break;
                case UnitCommandID.BuildImprovment_Fort:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Fort);
                    break;
                case UnitCommandID.BuildImprovment_LumberMill:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.LumberMill);
                    break;
                case UnitCommandID.BuildImprovment_Mine:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Mine);
                    break;
                case UnitCommandID.BuildImprovment_TradingPost:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.TradingPost);
                    break;
                case UnitCommandID.BuildImprovment_Roads:
                    //Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Roads);
                    break;
                case UnitCommandID.BuildImprovment_RailRoads:
                    //Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.RailRoads);
                    break;
                case UnitCommandID.BuildImprovment_Camp:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Camp);
                    break;
                case UnitCommandID.BuildImprovment_FishingBoats:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.FishingBoats);
                    break;
                case UnitCommandID.BuildImprovment_OffshorePlatform:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.OffshorePlatform);
                    break;
                case UnitCommandID.BuildImprovment_Pasture:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Pasture);
                    break;
                case UnitCommandID.BuildImprovment_Plantation:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Plantation);
                    break;
                case UnitCommandID.BuildImprovment_Quarry:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Quarry);
                    break;
                case UnitCommandID.BuildImprovment_Academy:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Academy);
                    break;
                case UnitCommandID.BuildImprovment_Citidel:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Citidel);
                    break;
                case UnitCommandID.BuildImprovment_CustomsHouse:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.CustomsHouse);
                    break;
                case UnitCommandID.BuildImprovment_HolySite:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.HolySite);
                    break;
                case UnitCommandID.BuildImprovment_Landmark:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Landmark);
                    break;
                case UnitCommandID.BuildImprovment_Manufactory:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Manufactory);
                    break;
                case UnitCommandID.RepairImprovement:
                    break;
                case UnitCommandID.CleanFallout:
                    break;
                default:
                    break;
            }

            if (cmd.CommandID != UnitCommandID.Disband && cmd.CommandID != UnitCommandID.Settle)
            {
                OnUnitUpdated(new UnitEventArgs(unit));
                if (unitTarget != null)
                    OnUnitUpdated(new UnitEventArgs(unitTarget));
            }
        }
        
        public void Move(Unit unit)
        {
            if (unit.MovementQueue == null)
                return;
            if (unit.MovementQueue.Count == 0)
                return;
            while (unit.Movement > 0 && unit.MovementQueue.Count > 0)
            {
                Point move = unit.MovementQueue.Dequeue();
                unit.LastLocation = unit.Location;
                unit.Location = move;
                unit.Movement--;
            }
        }

        public void SleepUnit(Unit unit)
        {
            unit.Sleeping = true;
        }

        public void SkipUnit(Unit unit)
        {
            unit.Skipping = true;
        }

        public void MeleeAttack(Unit attacker, Unit defender)
        {
            if (attacker.Actions <= 0)
            {
                ConsoleManager.Instance.WriteLine("attack has no actions remaining", MsgType.ServerInfo);
                return;
            }

            if (attacker.PlayerID == defender.PlayerID)
                return;

            if (!Controllers.Board.GetTile(attacker.Location).GetNeighbourTileLocations().Contains(defender.Location))
                return;

            float attackerModifier = 1f;
            // +25% for hills
            if (Controllers.Board.GetTile(attacker.Location).TerrainFeature == TileTerrainFeature.Hill)
                attackerModifier += 0.25f;
            // +25% for forests
            if (Controllers.Board.GetTile(attacker.Location).Improvement == TileImprovment.Forest)
                attackerModifier += 0.25f;
            // +25% for jungles
            if (Controllers.Board.GetTile(attacker.Location).Improvement == TileImprovment.Jungle)
                attackerModifier += 0.25f;

            float defenderModifier = 1f;
            // +25% for hills
            if (Controllers.Board.GetTile(defender.Location).TerrainFeature == TileTerrainFeature.Hill)
                defenderModifier += 0.25f;
            // +25% for forests
            if (Controllers.Board.GetTile(defender.Location).Improvement == TileImprovment.Forest)
                defenderModifier += 0.25f;
            // +25% for jungles
            if (Controllers.Board.GetTile(defender.Location).Improvement == TileImprovment.Jungle)
                defenderModifier += 0.25f;

            attacker.Movement = 0;
            attacker.Actions = 0;
            int attackerTakes = Unit.GetDamageAttackerSuffered(attacker.Constants.CombatStrength * attackerModifier,
                defender.Constants.CombatStrength * defenderModifier,
                attacker.HP, attacker.Constants.MaxHP);
            int defenderTakes = Unit.GetDamageDefenderSuffered(attacker.Constants.CombatStrength * attackerModifier,
                defender.Constants.CombatStrength * defenderModifier,
                attacker.HP, attacker.Constants.MaxHP);

            attacker.HP -= attackerTakes;
            defender.HP -= defenderTakes;

            attacker.Actions = 0;

            if (defender.HP <= 0)
            {
                RemoveUnit(defender);
                // keep the attacker alive if the defender dies
                if (attacker.HP <= 0)
                    attacker.HP = 1;
                attacker.MovementQueue.Clear();
                attacker.Location = defender.Location;
            }
            if (attacker.HP <= 0)
                RemoveUnit(attacker);
            ConsoleManager.Instance.WriteLine($"{Controllers.Player.GetPlayer(attacker.PlayerID).Name}'s {attacker.Name} is melee attacking {Controllers.Player.GetPlayer(defender.PlayerID).Name}'s {defender.Name}", MsgType.ServerInfo);
            ConsoleManager.Instance.WriteLine($"attacker had combat strength {attacker.Constants.CombatStrength} (+{(attackerModifier - 1) * 100}%) and suffered {attackerTakes}", MsgType.ServerInfo);
            ConsoleManager.Instance.WriteLine($"defender had modifier of {defender.Constants.CombatStrength} (+{(defenderModifier - 1) * 100}%) and suffered {defenderTakes}", MsgType.ServerInfo);
        }

        public void MeleeAttack(Unit attacker, City defender)
        {
            if (attacker.Actions <= 0)
                return;

            if (attacker.PlayerID == defender.PlayerID)
                return;

            if (!Controllers.Board.GetTile(attacker.Location).GetNeighbourTileLocations().Contains(defender.Location))
                return;

            attacker.Movement = 0;
            attacker.Actions = 0;
            attacker.HP -= Unit.GetDamageAttackerSuffered(attacker.Constants.CombatStrength, defender.CombatStrength, attacker.HP, attacker.Constants.MaxHP);
            Controllers.City.DamageCity(defender, Unit.GetDamageDefenderSuffered(attacker.Constants.CombatStrength, defender.CombatStrength, attacker.HP, attacker.Constants.MaxHP));

            attacker.Actions = 0;

            if (defender.HP <= 1)
                Controllers.City.CaptureCity(defender, attacker.PlayerID);
            if (attacker.HP <= 0)
                RemoveUnit(attacker);
            if (attacker.HP > 0 && defender.HP <= 1)
            {
                attacker.MovementQueue.Clear();
                attacker.Location = defender.Location;
            }
        }

        public void RangedAttack(Unit attacker, Unit defender)
        {
            if (attacker.PlayerID == defender.PlayerID)
                return;

            if (Board.HexDistance(attacker.Location, defender.Location) > attacker.Constants.Range)
                return;

            attacker.Movement = 0;
            attacker.Actions = 0;
            defender.HP -= Unit.GetDamageDefenderSuffered(attacker.Constants.CombatStrength, defender.Constants.CombatStrength, attacker.HP, attacker.Constants.MaxHP);

            if (defender.HP <= 0)
                RemoveUnit(defender);
        }

        public void RangedAttack(Unit attacker, City defender)
        {
            if (attacker.PlayerID == defender.PlayerID)
                return;

            if (Board.HexDistance(attacker.Location, defender.Location) > attacker.Constants.Range)
                return;

            attacker.Movement = 0;
            attacker.Actions = 0;
            Controllers.City.DamageCity(defender, Unit.GetDamageDefenderSuffered(attacker.Constants.CombatStrength, defender.CombatStrength, attacker.HP, attacker.Constants.MaxHP));
        }
    }
}
