// Dominion - Copyright (C) Timothy Ings
// Board.cs
// This file defines classes that define a city

using Dominion.Common.Factories;
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
        
        /// <summary>
        /// The city's unique id
        /// </summary>
        public int InstanceID { get; set; }

        /// <summary>
        /// The id of the player that owns the city
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// The id of the empire that owns the city
        /// </summary>
        public int EmpireID { get; set; }

        /// <summary>
        /// The name of the city
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates whetehr the city is a captial
        /// </summary>
        public bool IsCapital { get; set; }

        /// <summary>
        /// The current hp of the city
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// The maximum hp of the city
        /// </summary>
        public int MaxHP { get; set; }

        /// <summary>
        /// The combat strength of the city
        /// </summary>
        public int CombatStrength { get; set; }

        /// <summary>
        /// Indicates whether the city has attacked this turn
        /// </summary>
        public bool HasAttacked { get; set; }

        /// <summary>
        /// The location of the city on the board
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
        /// The population of the city
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// The amount of gold the city earns per turn
        /// </summary>
        public int IncomeGold { get; set; }

        /// <summary>
        /// The amount of science the city earns per turn
        /// </summary>
        public int IncomeScience { get; set; }
        
        /// <summary>
        /// The amount of happiness the city earns per turn
        /// </summary>
        public int IncomeHappiness { get; set; }

        /// <summary>
        /// The amount of production the city earns per turn
        /// </summary>
        public int IncomeProduction { get; set; }

        /// <summary>
        /// The amount of food the city earns per turn
        /// </summary>
        public int IncomeFood { get; set; }

        /// <summary>
        /// The amount of excess food the city has, used for population growth
        /// </summary>
        public int ExcessFood { get; set; }

        /// <summary>
        /// The amount of culture the city earns per turn
        /// </summary>
        public int IncomeCulture { get; set; }

        /// <summary>
        /// The amount of culture the city has earned
        /// </summary>
        public int Culture { get; set; }

        /// <summary>
        /// The amount of horses the city has access to
        /// </summary>
        public int Horses { get; set; }

        /// <summary>
        /// The amount of iron the city has access to
        /// </summary>
        public int Iron { get; set; }

        /// <summary>
        /// The amount of coal the city has access to
        /// </summary>
        public int Coal { get; set; }

        /// <summary>
        /// The amount of oil the city has access to
        /// </summary>
        public int Oil { get; set; }

        /// <summary>
        /// The amount of uranium the city has access to
        /// </summary>
        public int Uranium { get; set; }

        /// <summary>
        /// The amount of production that has overflown from the last production
        /// </summary>
        public int ProductionOverflow { get; set; }

        /// <summary>
        /// The estimated number of turns until the city's borders grow
        /// </summary>
        public int TurnsUntilBorderGrowth { get; set; }

        /// <summary>
        /// The estimated number of turns until the city's population grows
        /// </summary>
        public int TurnsUntilPopulationGrowth { get; set; }

        /// <summary>
        /// The city's production queue
        /// </summary>
        public LinkedList<Production> ProductionQueue { get; set; }

        /// <summary>
        /// The buildings that have been constructed in the city
        /// </summary>
        public List<int> Buildings { get; set; }

        /// <summary>
        /// A list of the locations of the city's citizens
        /// </summary>
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

        /// <summary>
        /// The city's citizen focus
        /// </summary>
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

        /// <summary>
        /// Returns a list of the cities possible productions
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="player"></param>
        /// <param name="techTree"></param>
        /// <returns></returns>
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
