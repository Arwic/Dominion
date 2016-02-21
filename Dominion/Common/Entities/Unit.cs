using ArwicEngine.Core;
using Dominion.Common.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dominion.Common.Entities
{
    public enum UnitID
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
    public class Unit
    {
        [NonSerialized()]
        private UnitTemplate _unitTemplate;
        public UnitTemplate Constants
        {
            get
            {
                return _unitTemplate;
            }
            set
            {
                _unitTemplate = value;
            }
        }

        public int UnitID { get; set; }
        public int PlayerID { get; set; }
        public int InstanceID { get; set; }
        public string Name { get; set; }
        public int HP { get; set; }
        public int Actions { get; set; }
        public int Movement { get; set; }
        public bool Sleeping { get; set; }
        public bool Skipping { get; set; }
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
        public List<int> Commands { get; set; }

        [NonSerialized()]
        private LinkedList<Point> _movementQueue;
        private LinkedList<int[]> _movementQueueInt;
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

        public Unit(UnitFactory factory, int unitID, int playerID, Point location)
        {
            UnitID = unitID;
            PlayerID = playerID;
            Location = location;
            MovementQueue = new LinkedList<Point>();
            factory.Construct(this);
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

        public void Rebuild()
        {
            if (_movementQueueInt == null)
                return;
            MovementQueue = new LinkedList<Point>();
            foreach (int[] i in _movementQueueInt)
                MovementQueue.Enqueue(new Point(i[0], i[1]));
        }

        public static int GetDamageDefenderSuffered(float attackerCombatStr, float defenderCombatStr, int attackerHP, int attackerMaxHP)
        {
            float combatRatio = attackerCombatStr / defenderCombatStr;
            float woundedRatio = attackerHP / attackerMaxHP;
            float baseDamage = 3;
            float finalDamage = baseDamage / 2 + ((baseDamage / 2) * woundedRatio);
            return (int)Math.Round(finalDamage * combatRatio);
        }

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
