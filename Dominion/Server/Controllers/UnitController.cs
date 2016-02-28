// Dominion - Copyright (C) Timothy Ings
// UnitController.cs
// This file defines classes that defines the unit controller

using ArwicEngine.Core;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Server.Controllers
{
    public class UnitEventArgs : EventArgs
    {
        public UnitInstance Unit { get; }

        public UnitEventArgs(UnitInstance unit)
        {
            Unit = unit;
        }
    }

    public class UnitController : Controller
    {
        private List<UnitInstance> units = new List<UnitInstance>();

        /// <summary>
        /// Occurs when a unit is added to the unit controller
        /// </summary>
        public event EventHandler<UnitEventArgs> UnitAdded;

        /// <summary>
        /// Occurs when a unit is removed from the unit controller
        /// </summary>
        public event EventHandler<UnitEventArgs> UnitRemoved;

        /// <summary>
        /// Occurs when a unit is updated by the unit controller
        /// </summary>
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
        }

        /// <summary>
        /// Prepares the units managed by the unit controller for the next turn
        /// </summary>
        public override void ProcessTurn()
        {
            foreach (UnitInstance unit in units)
            {
                ProcessMovment(unit); // update the units movement
                BuildUnitCommands(unit); // build a list of commands based on the unit's context
                unit.Movement = unit.BaseUnit.Movement; // reset the units movment points
                unit.Actions = unit.BaseUnit.Actions; // reset the units action points
                unit.Skipping = false; // remove the skipping flag
            }
        }

        /// <summary>
        /// Gets a list of all units managed by the unit manager
        /// </summary>
        /// <returns></returns>
        public List<UnitInstance> GetAllUnits()
        {
            return units;
        }

        /// <summary>
        /// Gets a list of all the units owned by the player with the given id
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public List<UnitInstance> GetPlayerUnits(int playerID)
        {
            return units.FindAll(u => u.PlayerID == playerID);
        }

        /// <summary>
        /// Gets the unit at the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public UnitInstance GetUnit(Point location)
        {
            return units.Find(u => u.Location == location);
        }

        /// <summary>
        /// Gets the unit with the given id
        /// </summary>
        /// <param name="instanceID"></param>
        /// <returns></returns>
        public UnitInstance GetUnit(int instanceID)
        {
            return units.Find(u => u.InstanceID == instanceID);
        }

        /// <summary>
        /// Creates a new unit of the given id at the given location and puts it in control of the player of the given id
        /// </summary>
        /// <param name="unitID"></param>
        /// <param name="playerID"></param>
        /// <param name="location"></param>
        public void AddUnit(string unitID, int playerID, Point location)
        {
            // TODO unit error!
            UnitInstance unit = new UnitInstance(Controllers.Data.Unit, unitID, playerID, location);
            units.Add(unit);

            OnUnitAdded(new UnitEventArgs(unit));
        }

        /// <summary>
        /// Removes the given unit from the game
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveUnit(UnitInstance unit)
        {
            units.Remove(unit);

            OnUnitRemoved(new UnitEventArgs(unit));
        }

        // assigns a unit a list of commands that are valid based on its context
        private void BuildUnitCommands(UnitInstance unit)
        {
            unit.Commands = new List<UnitCommandID>();
            for (int i = 0; i < unit.BaseUnit.Commands.Count; i++)
            {
                string cmdID = unit.BaseUnit.Commands[i];
                UnitCommandID cmd = UnitCommandID.UNITCMD_NULL;
                if (!Enum.TryParse(cmdID, out cmd))
                {
                    ConsoleManager.Instance.WriteLine($"Unknown unit command '{cmdID}'", MsgType.ServerFailed);
                    continue;
                }
                Technology tech;
                switch (cmd)
                {
                    case UnitCommandID.UNITCMD_MOVE:
                    case UnitCommandID.UNITCMD_DISBAND:
                    case UnitCommandID.UNITCMD_SLEEP:
                    case UnitCommandID.UNITCMD_SKIP:
                    case UnitCommandID.UNITCMD_SETTLE:
                    case UnitCommandID.UNITCMD_MELEE:
                    case UnitCommandID.UNITCMD_BOMBARD:
                    case UnitCommandID.UNITCMD_BUILD_ACADEMY:
                    case UnitCommandID.UNITCMD_BUILD_CITIDEL:
                    case UnitCommandID.UNITCMD_BUILD_CUSTOMSHOUSE:
                    case UnitCommandID.UNITCMD_BUILD_HOLYSITE:
                    case UnitCommandID.UNITCMD_BUILD_LANDMARK:
                    case UnitCommandID.UNITCMD_BUILD_MANUFACTORY:
                        unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_FARM:
                        unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_FORT:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_ENGINEERING");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_LUMBERMILL:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_CONSTRUCTION");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_MINE:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_MINING");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_TRADINGPOST:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_GUILDS");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_ROADS:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_THE_WHEEL");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_RAILROADS:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_RAILROAD");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_CAMP:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_TRAPPING");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_FISHINGBOATS:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_SAILING");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_OFFSHOREPLATFORM:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_REFRIGERATION");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_PASTURE:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_ANIMAL_HUSBANDRY");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_PLANTATION:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_CALENDAR");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_BUILD_QUARRY:
                        tech = Controllers.Player.GetPlayer(unit.PlayerID).TechTree.GetTech("TECH_MASONRY");
                        if (tech != null && tech.Unlocked)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_REPAIR:
                        if (Controllers.Board.GetTile(unit.Location).Pillaged)
                            unit.Commands.Add(cmd);
                        break;
                    case UnitCommandID.UNITCMD_CLEAN:
                        if (Controllers.Board.GetTile(unit.Location).Fallout)
                            unit.Commands.Add(cmd);
                        break;
                }
            }
        }

        /// <summary>
        /// Issues a command to a unit
        /// </summary>
        /// <param name="cmd"></param>
        public void CommandUnit(UnitCommand cmd)
        {
            Tile tile = null;
            if (cmd.TileLocation != new Point(-1, -1))
                tile = Controllers.Board.GetTile(cmd.TileLocation);

            UnitInstance unit = GetUnit(cmd.UnitInstanceID);
            if (unit == null)
                return;

            UnitInstance unitTarget = null;
            if (tile != null)
                unitTarget = GetUnit(tile.Location);

            City cityTarget = null;
            if (tile != null)
                cityTarget = Controllers.City.GetCity(tile.Location);

            switch (cmd.CommandID)
            {
                case UnitCommandID.UNITCMD_MOVE:
                    unit.MovementQueue = Board.FindPath(unit.Location, tile.Location, Controllers.Board.Board);
                    ProcessMovment(unit);
                    break;
                case UnitCommandID.UNITCMD_DISBAND:
                    RemoveUnit(unit);
                    break;
                case UnitCommandID.UNITCMD_SLEEP:
                    SleepUnit(unit);
                    break;
                case UnitCommandID.UNITCMD_SKIP:
                    SkipUnit(unit);
                    break;
                case UnitCommandID.UNITCMD_SETTLE:
                    if (Controllers.City.SettleCity(unit))
                        RemoveUnit(unit);
                    break;
                case UnitCommandID.UNITCMD_MELEE:
                    if (unitTarget != null)
                        MeleeAttack(unit, unitTarget);
                    else if (cityTarget != null)
                        MeleeAttack(unit, cityTarget);
                    break;
                case UnitCommandID.UNITCMD_BOMBARD:
                    if (unitTarget != null)
                        RangedAttack(unit, unitTarget);
                    else if (cityTarget != null)
                        RangedAttack(unit, cityTarget);
                    break;
                case UnitCommandID.UNITCMD_BUILD_FARM:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.FARM);
                    break;
                case UnitCommandID.UNITCMD_BUILD_FORT:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Fort);
                    break;
                case UnitCommandID.UNITCMD_BUILD_LUMBERMILL:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.LumberMill);
                    break;
                case UnitCommandID.UNITCMD_BUILD_MINE:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.MINE);
                    break;
                case UnitCommandID.UNITCMD_BUILD_TRADINGPOST:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.TRADINGPOST);
                    break;
                case UnitCommandID.UNITCMD_BUILD_ROADS:
                    //Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Roads);
                    break;
                case UnitCommandID.UNITCMD_BUILD_RAILROADS:
                    //Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.RailRoads);
                    break;
                case UnitCommandID.UNITCMD_BUILD_CAMP:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Camp);
                    break;
                case UnitCommandID.UNITCMD_BUILD_FISHINGBOATS:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.FishingBoats);
                    break;
                case UnitCommandID.UNITCMD_BUILD_OFFSHOREPLATFORM:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.OffshorePlatform);
                    break;
                case UnitCommandID.UNITCMD_BUILD_PASTURE:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Pasture);
                    break;
                case UnitCommandID.UNITCMD_BUILD_PLANTATION:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.PLANTATION);
                    break;
                case UnitCommandID.UNITCMD_BUILD_QUARRY:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Quarry);
                    break;
                case UnitCommandID.UNITCMD_BUILD_ACADEMY:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Academy);
                    break;
                case UnitCommandID.UNITCMD_BUILD_CITIDEL:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Citidel);
                    break;
                case UnitCommandID.UNITCMD_BUILD_CUSTOMSHOUSE:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.CustomsHouse);
                    break;
                case UnitCommandID.UNITCMD_BUILD_HOLYSITE:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.HolySite);
                    break;
                case UnitCommandID.UNITCMD_BUILD_LANDMARK:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Landmark);
                    break;
                case UnitCommandID.UNITCMD_BUILD_MANUFACTORY:
                    Controllers.Board.BuildImprovment(Controllers.Board.GetTile(unit.Location), unit.PlayerID, TileImprovment.Manufactory);
                    break;
                case UnitCommandID.UNITCMD_REPAIR:
                    break;
                case UnitCommandID.UNITCMD_CLEAN:
                    break;
                default:
                    break;
            }

            if (cmd.CommandID != UnitCommandID.UNITCMD_DISBAND && cmd.CommandID != UnitCommandID.UNITCMD_SETTLE)
            {
                OnUnitUpdated(new UnitEventArgs(unit));
                if (unitTarget != null)
                    OnUnitUpdated(new UnitEventArgs(unitTarget));
            }
        }
        
        /// <summary>
        /// Moves the unit along its movment queue for as long as its movement points allow
        /// </summary>
        /// <param name="unit"></param>
        private void ProcessMovment(UnitInstance unit)
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

        /// <summary>
        /// Puts a unit to sleep
        /// </summary>
        /// <param name="unit"></param>
        public void SleepUnit(UnitInstance unit)
        {
            unit.Sleeping = true;
        }

        /// <summary>
        /// Flags a unit as not requiring input this turn
        /// </summary>
        /// <param name="unit"></param>
        public void SkipUnit(UnitInstance unit)
        {
            unit.Skipping = true;
        }

        // makes a unit melee attack another
        private void MeleeAttack(UnitInstance attacker, UnitInstance defender)
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
            if (Controllers.Board.GetTile(attacker.Location).TerrainFeature == TileTerrainFeature.HILL)
                attackerModifier += 0.25f;
            // +25% for forests
            if (Controllers.Board.GetTile(attacker.Location).Improvement == TileImprovment.FOREST)
                attackerModifier += 0.25f;
            // +25% for jungles
            if (Controllers.Board.GetTile(attacker.Location).Improvement == TileImprovment.JUNGLE)
                attackerModifier += 0.25f;

            float defenderModifier = 1f;
            // +25% for hills
            if (Controllers.Board.GetTile(defender.Location).TerrainFeature == TileTerrainFeature.HILL)
                defenderModifier += 0.25f;
            // +25% for forests
            if (Controllers.Board.GetTile(defender.Location).Improvement == TileImprovment.FOREST)
                defenderModifier += 0.25f;
            // +25% for jungles
            if (Controllers.Board.GetTile(defender.Location).Improvement == TileImprovment.JUNGLE)
                defenderModifier += 0.25f;

            attacker.Movement = 0;
            attacker.Actions = 0;
            int attackerTakes = UnitInstance.GetDamageAttackerSuffered(attacker.BaseUnit.CombatStrength * attackerModifier,
                defender.BaseUnit.CombatStrength * defenderModifier,
                attacker.HP, attacker.BaseUnit.MaxHP);
            int defenderTakes = UnitInstance.GetDamageDefenderSuffered(attacker.BaseUnit.CombatStrength * attackerModifier,
                defender.BaseUnit.CombatStrength * defenderModifier,
                attacker.HP, attacker.BaseUnit.MaxHP);

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
            ConsoleManager.Instance.WriteLine($"attacker had combat strength {attacker.BaseUnit.CombatStrength} (+{(attackerModifier - 1) * 100}%) and suffered {attackerTakes}", MsgType.ServerInfo);
            ConsoleManager.Instance.WriteLine($"defender had modifier of {defender.BaseUnit.CombatStrength} (+{(defenderModifier - 1) * 100}%) and suffered {defenderTakes}", MsgType.ServerInfo);
        }

        // makes a unit melee attack a city
        private void MeleeAttack(UnitInstance attacker, City defender)
        {
            if (attacker.Actions <= 0)
                return;

            if (attacker.PlayerID == defender.PlayerID)
                return;

            if (!Controllers.Board.GetTile(attacker.Location).GetNeighbourTileLocations().Contains(defender.Location))
                return;

            attacker.Movement = 0;
            attacker.Actions = 0;
            attacker.HP -= UnitInstance.GetDamageAttackerSuffered(attacker.BaseUnit.CombatStrength, defender.Defense, attacker.HP, attacker.BaseUnit.MaxHP);
            Controllers.City.DamageCity(defender, UnitInstance.GetDamageDefenderSuffered(attacker.BaseUnit.CombatStrength, defender.Defense, attacker.HP, attacker.BaseUnit.MaxHP));

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

        // makes a unit ranged attack another
        private void RangedAttack(UnitInstance attacker, UnitInstance defender)
        {
            if (attacker.PlayerID == defender.PlayerID)
                return;

            if (Board.HexDistance(attacker.Location, defender.Location) > attacker.BaseUnit.Range)
                return;

            attacker.Movement = 0;
            attacker.Actions = 0;
            defender.HP -= UnitInstance.GetDamageDefenderSuffered(attacker.BaseUnit.CombatStrength, defender.BaseUnit.CombatStrength, attacker.HP, attacker.BaseUnit.MaxHP);

            if (defender.HP <= 0)
                RemoveUnit(defender);
        }

        // makes a unit ranged attack a city
        private void RangedAttack(UnitInstance attacker, City defender)
        {
            if (attacker.PlayerID == defender.PlayerID)
                return;

            if (Board.HexDistance(attacker.Location, defender.Location) > attacker.BaseUnit.Range)
                return;

            attacker.Movement = 0;
            attacker.Actions = 0;
            Controllers.City.DamageCity(defender, UnitInstance.GetDamageDefenderSuffered(attacker.BaseUnit.CombatStrength, defender.Defense, attacker.HP, attacker.BaseUnit.MaxHP));
        }
    }
}
