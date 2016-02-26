// Dominion - Copyright (C) Timothy Ings
// Unit.cs
// This file defines classes that define a unit

using ArwicEngine.Core;
using Dominion.Common.Data;
using Dominion.Common.Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dominion.Common.Entities
{
    public enum UnitGraphicID
    {
        Settler,
        Worker,
        Scout,
        Warrior,
        Archer,
        Spearman,
        ChariotArcher,
        Trireme,

        Great_Artist,
        Great_Musician,
        Great_Writer,
        Great_Engineer,
        Great_General,
        Great_Merchant,
        Great_Scientist,
        Great_Admiral,
        Great_Prophet
    }

    [Serializable()]
    public class UnitInstance
    {
        /// <summary>
        /// The template the unit is based on
        /// </summary>
        public Unit BaseUnit
        {
            get
            {
                return _baseUnit;
            }
            set
            {
                _baseUnit = value;
            }
        }
        [NonSerialized()]
        private Unit _baseUnit;

        /// <summary>
        /// The unit's id
        /// </summary>
        public string UnitID { get; set; }

        /// <summary>
        /// The id of the player that owns the unit
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// The unit's unique id
        /// </summary>
        public int InstanceID { get; set; }

        /// <summary>
        /// The name of the unit
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The unit's current hp
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// The units actions available this turn
        /// </summary>
        public int Actions { get; set; }

        /// <summary>
        /// The units movement available this turn
        /// </summary>
        public int Movement { get; set; }

        /// <summary>
        /// Indicates whether the unit is sleeping
        /// </summary>
        public bool Sleeping { get; set; }

        /// <summary>
        /// Indicates whether the unit is skipping this turn
        /// </summary>
        public bool Skipping { get; set; }

        /// <summary>
        /// The unit's location on the board
        /// </summary>
        public Point Location
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

        /// <summary>
        /// The unit's location on the board last turn
        /// </summary>
        public Point LastLocation
        {
            get
            {
                return new Point(_lastLocationX, _lastLocationY);
            }
            set
            {
                _lastLocationX = value.X;
                _lastLocationY = value.Y;
            }
        }
        private int _lastLocationX;
        private int _lastLocationY;

        /// <summary>
        /// A list of command id's that the unit can use
        /// This list is aware of the units current context
        /// </summary>
        public List<int> Commands { get; set; }

        /// <summary>
        /// A list of moves the unit is to make
        /// </summary>
        public LinkedList<Point> MovementQueue
        {
            get
            {
                return _movementQueue;
            }
            set
            {
                _movementQueue = value;
            }
        }
        [NonSerialized()]
        private LinkedList<Point> _movementQueue;
        private LinkedList<int[]> _movementQueueInt;

        /// <summary>
        /// Indicates whether the unit requires orders from the player
        /// </summary>
        public bool RequiresOrders
        {
            get
            {
                if (Sleeping || Skipping)
                    return false;
                if (Movement > 0 && (MovementQueue == null || MovementQueue.Count == 0))
                    return true;
                return false;
            }
        }

        public UnitInstance(UnitManager manager, string unitID, int playerID, Point location)
        {
            UnitID = unitID;
            PlayerID = playerID;
            Location = location;
            MovementQueue = new LinkedList<Point>();
            manager.Construct(this);
        }

        [OnSerializing()]
        private void OnSerializing(StreamingContext context)
        {
            if (MovementQueue == null)
                return;
            _movementQueueInt = new LinkedList<int[]>();
            foreach (Point point in MovementQueue)
                _movementQueueInt.Enqueue(new int[2] { point.X, point.Y });
        }

        /// <summary>
        /// Rebuilds the unit's properties after being sent over a network
        /// </summary>
        public void Rebuild()
        {
            if (_movementQueueInt == null)
                return;
            MovementQueue = new LinkedList<Point>();
            foreach (int[] i in _movementQueueInt)
                MovementQueue.Enqueue(new Point(i[0], i[1]));
        }

        /// <summary>
        /// Returns the damage a defending unit will take when it is attacked
        /// </summary>
        /// <param name="attackerCombatStr"></param>
        /// <param name="defenderCombatStr"></param>
        /// <param name="attackerHP"></param>
        /// <param name="attackerMaxHP"></param>
        /// <returns></returns>
        public static int GetDamageDefenderSuffered(float attackerCombatStr, float defenderCombatStr, int attackerHP, int attackerMaxHP)
        {
            float combatRatio = attackerCombatStr / defenderCombatStr;
            float woundedRatio = attackerHP / attackerMaxHP;
            float baseDamage = 3;
            float finalDamage = baseDamage / 2 + ((baseDamage / 2) * woundedRatio);
            return (int)Math.Round(finalDamage * combatRatio);
        }

        /// <summary>
        /// Returns the damage an attacking unit will take when it melee attacks another unit or city
        /// </summary>
        /// <param name="attackerCombatStr"></param>
        /// <param name="defenderCombatStr"></param>
        /// <param name="attackerHP"></param>
        /// <param name="attackerMaxHP"></param>
        /// <returns></returns>
        public static int GetDamageAttackerSuffered(float attackerCombatStr, float defenderCombatStr, int attackerHP, int attackerMaxHP)
        {
            float combatRatio = attackerCombatStr / defenderCombatStr;
            float woundedRatio = attackerHP / attackerMaxHP;
            float baseDamage = 3;
            float finalDamage = baseDamage / 2 + ((baseDamage / 2) * woundedRatio);
            return (int)Math.Round(finalDamage / combatRatio);
        }
    }
}
