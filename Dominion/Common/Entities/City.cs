// Dominion - Copyright (C) Timothy Ings
// Board.cs
// This file defines classes that define a city

using Dominion.Common.Data;
using Dominion.Common.Managers;
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
        public string EmpireID { get; set; }

        /// <summary>
        /// The name of the city
        /// </summary>
        public string Name { get; set; }

        #region flags
        /// <summary>
        /// Indicates whetehr the city is a captial
        /// </summary>
        public bool IsCapital { get; set; }

        /// <summary>
        /// Indicates whether the city has attacked this turn
        /// </summary>
        public bool HasAttacked { get; set; }

        /// <summary>
        /// Indicates whether the city is being raised by its owner
        /// A city being raised will stop growing and lose 1 population per turn until it is destroyed
        /// </summary>
        public bool IsBeingRaised { get; set; }
        #endregion

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

        #region income
        /// <summary>
        /// The amount of food the city earns per turn
        /// </summary>
        public int IncomeFood { get; set; }

        /// <summary>
        /// The amount of production the city earns per turn
        /// </summary>
        public int IncomeProduction { get; set; }

        /// <summary>
        /// The amount of gold the city earns per turn
        /// </summary>
        public int IncomeGold { get; set; }

        /// <summary>
        /// The amount of science the city earns per turn
        /// </summary>
        public int IncomeScience { get; set; }
        
        /// <summary>
        /// The amount of culture the city earns per turn
        /// </summary>
        public int IncomeCulture { get; set; }

        /// <summary>
        /// The amount of faith the city earns per turn
        /// </summary>
        public int IncomeFaith { get; set; }

        /// <summary>
        /// The amount of faith the city earns per turn
        /// </summary>
        public int IncomeTourism { get; set; }
        #endregion

        #region stats
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
        public int Defense { get; set; }

        /// <summary>
        /// The population of the city
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// The amount of excess food the city has, used for population growth
        /// </summary>
        public int ExcessFood { get; set; }

        /// <summary>
        /// The amount of production that has overflown from the last production
        /// </summary>
        public int ProductionOverflow { get; set; }

        /// <summary>
        /// The amount of culture the city has earned
        /// </summary>
        public int Culture { get; set; }
        #endregion

        #region basic income/yield local modifiers
        /// <summary>
        /// Local food income modifier
        /// </summary>
        public float IncomeFoodModifier { get; set; } = 1f;

        /// <summary>
        /// Local production income modifier
        /// </summary>
        public float IncomeProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local gold income modifier
        /// </summary>
        public float IncomeGoldModifier { get; set; } = 1f;

        /// <summary>
        /// Local science income modifier
        /// </summary>
        public float IncomeScienceModifier { get; set; } = 1f;

        /// <summary>
        /// Local culture income modifier
        /// </summary>
        public float IncomeCultureModifier { get; set; } = 1f;

        /// <summary>
        /// Local faith income modifier
        /// </summary>
        public float IncomeFaithModifier { get; set; } = 1f;

        /// <summary>
        /// Local tourism income modifier
        /// </summary>
        public float IncomeTourismModifier { get; set; } = 1f;

        /// <summary>
        /// Local food yield modifier
        /// </summary>
        public float YieldFoodModifier { get; set; } = 1f;

        /// <summary>
        /// Local production yield modifier
        /// </summary>
        public float YieldProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local gold yield modifier
        /// </summary>
        public float YieldGoldModifier { get; set; } = 1f;

        /// <summary>
        /// Local science yield modifier
        /// </summary>
        public float YieldScienceModifier { get; set; } = 1f;

        /// <summary>
        /// Local culture yield modifier
        /// </summary>
        public float YieldCultureModifier { get; set; } = 1f;

        /// <summary>
        /// Local faith yield modifier
        /// </summary>
        public float YieldFaithModifier { get; set; } = 1f;

        /// <summary>
        /// Local tourism yield modifier
        /// </summary>
        public float YieldTourismModifier { get; set; } = 1f;
        #endregion

        #region other local modifiers
        /// <summary>
        /// Nuke effectivness modifier
        /// </summary>
        public float NukeModifier { get; set; } = 1f;

        /// <summary>
        /// Modifier to city heal rate
        /// </summary>
        public float HealRateChange { get; set; } = 1f;

        /// <summary>
        /// Local modifier to military unit production
        /// </summary>
        public float MilitaryProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to space ship unit production
        /// </summary>
        public float SpaceProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to building production
        /// </summary>
        public float BuildingProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to wonder production
        /// </summary>
        public float WonderProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local trade route effectivness modifier
        /// </summary>
        public float TradeRouteModifier { get; set; } = 1f;

        /// <summary>
        /// Local tile culture cost modifier
        /// </summary>
        public float TileCultureCostModifier { get; set; } = 1f;

        /// <summary>
        /// Local tile buy cost modifier
        /// </summary>
        public float TileBuyCostModifier { get; set; } = 1f;

        /// <summary>
        /// Local city defense modifier
        /// </summary>
        public float DefenseModifier { get; set; } = 1f;
        #endregion

        #region other obscure props
        /// <summary>
        /// If the city hinders enemy unit movement in it's borders
        /// </summary>
        public bool BorderObstacle { get; set; } = false;

        /// <summary>
        /// If the city can establish water trade routes
        /// </summary>
        public bool AllowsWaterRoutes { get; set; } = false;

        /// <summary>
        /// If the city produces extra luxuries
        /// </summary>
        public bool ExtraLuxuries { get; set; } = false;

        /// <summary>
        /// Percentage of excess food carried over on population growth
        /// </summary>
        public float FoodKept { get; set; } = 0f;

        /// <summary>
        /// If the city recieves unhappiness for being occupied
        /// </summary>
        public bool NoOccupiedUnhappiness { get; set; } = false;
        
        #endregion

        /// <summary>
        /// The estimated number of turns until the city's borders grow
        /// </summary>
        public int TurnsUntilBorderGrowth { get; set; }

        /// <summary>
        /// The estimated number of turns until the city's population grows
        /// </summary>
        public int TurnsUntilPopulationGrowth { get; set; }

        /// <summary>
        /// The estemated progress of the city's population gorwth
        /// </summary>
        public float PopulationGorwthProgress { get; set; }

        /// <summary>
        /// The city's production queue
        /// </summary>
        public LinkedList<Production> ProductionQueue { get; set; } = new LinkedList<Production>();

        /// <summary>
        /// The city's possible productions
        /// </summary>
        public LinkedList<Production> ValidProductions { get; set; } = new LinkedList<Production>();

        /// <summary>
        /// The buildings that have been constructed in the city
        /// </summary>
        public List<string> Buildings { get; set; } = new List<string>();

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
            Defense = 1;
            Location = location;
            Buildings = new List<string>();
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
        /// Calculates the amount of production that will be generated for the given production at this city
        /// This takes modifiers into account
        /// </summary>
        /// <param name="production"></param>
        /// <param name="buildingManager"></param>
        /// <param name="unitManager"></param>
        /// <returns></returns>
        public int GetProductionIncome(Production production, BuildingManager buildingManager, UnitManager unitManager)
        {
            float prodIncome = 0f;
            // calculate production this turn
            switch (production.ProductionType)
            {
                case ProductionType.UNIT:
                    Unit u = unitManager.GetUnit(production.Name);
                    prodIncome = IncomeProduction;
                    if (u.Tags.Contains("MILITARY"))
                        prodIncome *= MilitaryProductionModifier;
                    if (u.Tags.Contains("SPACESHIP"))
                        prodIncome *= SpaceProductionModifier;
                    break;
                case ProductionType.BUILDING:
                    Building b = buildingManager.GetBuilding(production.Name);
                    prodIncome = IncomeProduction;
                    prodIncome *= BuildingProductionModifier;
                    if (b.Tags.Contains("WONDER"))
                        prodIncome *= WonderProductionModifier;
                    break;
            }
            return (int)prodIncome;
        }
    }
}
