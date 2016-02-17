using ArwicEngine.Core;
using Dominion.Common.Factories;
using Dominion.Server.Controllers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dominion.Common.Entities
{
    public enum CityCitizenFocus
    {
        Default,
        Food,
        Production,
        Gold,
        Science,
        Culture
    }

    [Serializable()]
    public class City
    {
        private const float popGrowthBase = 1.1f;
        private const float popUnhappinessCoeff = 1.1f;
        
        public int InstanceID { get; set; }
        public int PlayerID { get; set; }
        public int EmpireID { get; set; }
        public string Name { get; set; }
        public bool IsCapital { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int CombatStrength { get; set; }
        public bool Attacked { get; set; }
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
        public int Population { get; set; }
        public int IncomeGold { get; set; }
        public int IncomeScience { get; set; }
        public int IncomeHappiness { get; set; }
        public int IncomeProduction { get; set; }
        public int IncomeFood { get; set; }
        public int ExcessFood { get; set; }
        public int IncomeCulture { get; set; }
        public int Culture { get; set; }
        public int Horses { get; set; }
        public int Iron { get; set; }
        public int Coal { get; set; }
        public int Oil { get; set; }
        public int Uranium { get; set; }
        public int ProductionOverflow { get; set; }
        public int TurnsUntilBorderGrowth { get; set; }
        public int TurnsUntilPopulationGrowth { get; set; }
        public LinkedList<Production> ProductionQueue { get; set; }
        public List<int> Buildings { get; set; }
        public List<Point> CitizenLocations
        {
            get
            {
                return _citizenLocations;
            }
            set
            {
                _citizenLocations = value;
            }
        }
        [NonSerialized]
        private List<Point> _citizenLocations;
        private List<int[]> _citizenLocationInts;
        public CityCitizenFocus CitizenFocus { get; set; }

        public City(Player player, string name, Point location)
        {
            Name = name;
            PlayerID = player.InstanceID;
            EmpireID = player.EmpireID;
            MaxHP = 200;
            HP = MaxHP;
            CombatStrength = 50;
            Location = location;
            Buildings = new List<int>();
            CitizenLocations = new List<Point>();
            ProductionQueue = new LinkedList<Production>();
            Population = 1;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            _citizenLocationInts = new List<int[]>();
            foreach (Point p in CitizenLocations)
                _citizenLocationInts.Add(new int[2] { p.X, p.Y });
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            CitizenLocations = new List<Point>();
            foreach (int[] workerLocation in _citizenLocationInts)
                _citizenLocations.Add(new Point(workerLocation[0], workerLocation[1]));
        }

        public List<Production> GetProductionList(ProductionFactory factory, Player player, TechTree techTree)
        {
            List<Production> productions = new List<Production>();
            foreach (Production prod in factory.Productions)
            {
                if (Buildings.Contains(prod.ResultID))
                    continue;

                bool alreadyInQueue = false;
                foreach (Production qProd in ProductionQueue)
                {
                    if (qProd.ID == prod.ID)
                    {
                        alreadyInQueue = true;
                        break;
                    }
                }
                if (alreadyInQueue)
                    continue;

                int currentEra = (int)player.CurrentEra;
                int requiredEra = (int)prod.GameEra;
                if (currentEra < requiredEra)
                    continue;

                bool haveBuildings = true;
                foreach (int building in prod.PrerequisiteBuildings)
                {
                    if (!Buildings.Contains(building))
                    {
                        haveBuildings = false;
                        break;
                    }
                }
                if (!haveBuildings)
                    continue;

                bool haveTech = true;
                foreach (int tech in prod.PrerequisiteTechs)
                {
                    TechNode tn = techTree.GetNode(tech);
                    if (tn != null && !tn.Unlocked)
                    {
                        haveTech = false;
                        break;
                    }
                }
                if (!haveTech)
                    continue;

                productions.Add(prod);
            }
            return productions;
        }
    }
}
