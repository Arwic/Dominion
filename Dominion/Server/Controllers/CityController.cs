// Dominion - Copyright (C) Timothy Ings
// CityController.cs
// This file defines classes that control cities

using ArwicEngine.Core;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Server.Controllers
{
    public class CityEventArgs : EventArgs
    {
        public City City { get; }

        public CityEventArgs(City city)
        {
            City = city;
        }
    }

    public class CityController : Controller
    {
        private class TileScore
        {
            public Tile Tile { get; set; }
            public int Score { get; set; }

            public TileScore(Tile t, int s)
            {
                Tile = t;
                Score = s;
            }
        }

        private List<City> cities;

        private int lastCityID = 0;

        /// <summary>
        /// Occurs when a city is settled
        /// </summary>
        public event EventHandler<CityEventArgs> CitySettled;

        /// <summary>
        /// Occurs when a city is captured
        /// </summary>
        public event EventHandler<CityEventArgs> CityCaptured;

        /// <summary>
        /// Occurs when a city is updated
        /// </summary>
        public event EventHandler<CityEventArgs> CityUpdated;

        /// <summary>
        /// Occurs when a city expands its borders
        /// </summary>
        public event EventHandler<TileEventArgs> CityBorderExpanded;

        protected virtual void OnCitySettled(CityEventArgs e)
        {
            if (CitySettled != null)
                CitySettled(this, e);
        }

        protected virtual void OnCityCaptured(CityEventArgs e)
        {
            if (CityCaptured != null)
                CityCaptured(this, e);
        }

        protected virtual void OnCityUpdated(CityEventArgs e)
        {
            if (CityUpdated != null)
                CityUpdated(this, e);
        }

        protected virtual void OnCityBorderExpanded(TileEventArgs e)
        {
            if (CityBorderExpanded != null)
                CityBorderExpanded(this, e);
        }

        public CityController(ControllerManager manager)
            : base(manager)
        {
            cities = new List<City>();
        }

        /// <summary>
        /// Preapres the cities for the next turn
        /// </summary>
        public override void ProcessTurn()
        {
            foreach (City city in cities)
            {
                RegenHP(city);
                CalculateIncome(city);
                //CalculateHappiness(city);
                CalculatePopulation(city);
                CalculateProduction(city);
                CalculateValidProductions(city);
                TryExpandBorders(city);
                CalculateTurnsUntilPopulationGrowth(city);
                CalculateTurnsUntilBorderGrowth(city);
                CalculateDefense(city);
            }
        }

        /// <summary>
        /// Commands a city to perform an action
        /// </summary>
        /// <param name="cmd"></param>
        public void CommandCity(CityCommand cmd)
        {
            City city = GetCity(cmd.CityID);
            if (city == null)
                return;

            try
            {
                switch (cmd.CommandID)
                {
                    case CityCommandID.Rename: // renames the city
                        city.Name = (string)cmd.Arguments[0];
                        break;
                    case CityCommandID.ChangeProduction: // changes the city's production
                        ChangeProduction(city, (string)cmd.Arguments[0]);
                        break;
                    case CityCommandID.QueueProduction: // add a production to the end of the city's production queue
                        QueueProduction(city, (string)cmd.Arguments[0]);
                        break;
                    case CityCommandID.CancelProduction: // cancels the given production
                        city.ProductionQueue.Remove((int)cmd.Arguments[0]);
                        break;
                    case CityCommandID.ReorderProductionMoveUp: // moves the given production up the queue
                        LinkedListNode<Production> selectedNode = city.ProductionQueue.GetNode((int)cmd.Arguments[0]);
                        city.ProductionQueue.Swap(selectedNode, selectedNode.Previous);
                        break;
                    case CityCommandID.ReorderProductionMoveDown: // moves the given production down the queue
                        selectedNode = city.ProductionQueue.GetNode((int)cmd.Arguments[0]);
                        city.ProductionQueue.Swap(selectedNode, selectedNode.Next);
                        break;
                    case CityCommandID.PurchaseProduction: // completes a production with gold
                        string prodID = (string)cmd.Arguments[0];
                        PurchaseProduction(city, prodID);
                        break;
                    case CityCommandID.ChangeCitizenFocus: // changes the focus of the citizens in the city
                        city.CitizenFocus = (CityCitizenFocus)cmd.Arguments[0];
                        RelocateCitizens(city);
                        UpdateCity(city);
                        break;
                    case CityCommandID.DemolishBuilding: // removes the given building from the city
                        string buildingID = (string)cmd.Arguments[0];
                        Building building = Controllers.Data.Building.GetBuilding(buildingID);
                        if (buildingID == null)
                            break;
                        if (!building.Demolishable) // don't demolish a buildign that can't be demolished
                            break;
                        city.Buildings.Remove(buildingID);
                        UpdateCity(city);
                        break;
                }
            }
            catch (Exception)
            {
                //Engine.Console.WriteLine($"CityCommand.{cmd.CommandID.ToString()} Error: malformed data", MsgType.ServerWarning);
            }

            OnCityUpdated(new CityEventArgs(city));
        }

        // updates properties of the given city
        private void UpdateCity(City city)
        {
            CalculateIncome(city);
            CalculateDefense(city);
            CalculateTurnsUntilPopulationGrowth(city);
            CalculateTurnsUntilBorderGrowth(city);
            CalculateValidProductions(city);
        }

        // purchases a production with gold
        private void PurchaseProduction(City city, string name)
        {
            Production prod;
            if (name.StartsWith("BUILDING_"))
            {
                Building building = Controllers.Data.Building.GetBuilding(name);
                if (!building.Purchasable)
                    return;
                prod = new Production(building);
            }
            else if (name.StartsWith("UNIT_"))
            {
                Unit unit = Controllers.Data.Unit.GetUnit(name);
                if (!unit.Purchasable)
                    return;
                prod = new Production(unit);
            }
            else
                return;

            // calculate gold cost
            float ratio = 5; // TODO move this to a game settings file
            int goldCost = (int)Math.Round(prod.Cost * ratio);

            // check if we can afford the purchase
            Player player = Controllers.Player.GetPlayer(city.PlayerID);
            if (player.Gold < goldCost)
                return;

            // make the purchase
            player.Gold -= goldCost;

            // produce the production
            switch (prod.ProductionType)
            {
                case ProductionType.BUILDING:
                    // add the produced building to the city's building list
                    city.Buildings.Add(prod.Name);
                    CalculateIncome(city);
                    break;
                case ProductionType.UNIT:
                    // add the produced unit to the unit controller
                    Controllers.Unit.AddUnit(prod.Name, city.PlayerID, city.Location);
                    break;
            }
        }

        // changes a production at the given city, if it is valid
        private void ChangeProduction(City city, string name)
        {
            ProductionType prodType = ProductionType.BUILDING;
            if (name.StartsWith("BUILDING_"))
                prodType = ProductionType.BUILDING;
            else if(name.StartsWith("UNIT_"))
                prodType = ProductionType.UNIT;
            else
                return;

            Production proposedProd;
            switch (prodType)
            {
                case ProductionType.UNIT:
                    proposedProd = new Production(Controllers.Data.Unit.GetUnit(name));
                    city.ProductionQueue.AddFirst(proposedProd);
                    break;
                case ProductionType.BUILDING:
                    // don't queue more than one of the same building
                    foreach (Production prod in city.ProductionQueue)
                        if (prod.Name == name)
                            return;
                    proposedProd = new Production(Controllers.Data.Building.GetBuilding(name));
                    city.ProductionQueue.AddFirst(proposedProd);
                    break;
            }

            UpdateCity(city);
        }

        // queues a production at the given city if, it is valid
        private void QueueProduction(City city, string name)
        {
            ProductionType prodType = ProductionType.BUILDING;
            if (name.StartsWith("BUILDING_"))
                prodType = ProductionType.BUILDING;
            else if (name.StartsWith("UNIT_"))
                prodType = ProductionType.UNIT;
            else
                return;

            Production proposedProd;
            switch (prodType)
            {
                case ProductionType.UNIT:
                    proposedProd = new Production(Controllers.Data.Unit.GetUnit(name));
                    city.ProductionQueue.Enqueue(proposedProd);
                    break;
                case ProductionType.BUILDING:
                    // don't queue more than one of the same building
                    foreach (Production prod in city.ProductionQueue)
                        if (prod.Name == name)
                            return;
                    proposedProd = new Production(Controllers.Data.Building.GetBuilding(name));
                    city.ProductionQueue.Enqueue(proposedProd);
                    break;
            }

            UpdateCity(city);
        }

        // calculates the turns until a city's population will grow and assigns the value to city.TurnsUntilPopulationGrowth
        private void CalculateTurnsUntilPopulationGrowth(City city)
        {
            // if there is no food being generated, the city's population will never grow
            if (city.IncomeFood == 0)
            {
                city.TurnsUntilPopulationGrowth = -1;
                return;
            }

            // calculate the food required to grow the population
            int requiredTurns = 0;
            int requiredFood = (int)Math.Floor(15 + 6 * (city.Population - 1) + Math.Pow(city.Population - 1, 1.8));
            int foodIncome = city.IncomeFood - (city.Population - 1) * 2;
            requiredFood -= city.ExcessFood;
            // calculate the number of turns required to grow the population
            while (requiredFood > 0)
            {
                requiredFood -= foodIncome;
                requiredTurns++;
                if (requiredTurns > 999)
                {
                    city.TurnsUntilPopulationGrowth = -2;
                    return;
                }
            }
            // assign the value to the city
            city.TurnsUntilPopulationGrowth = requiredTurns;
        }

        // calculates the turns until a city's border will expand and assigns the value to city.TurnsUntilBorderGrowth
        private void CalculateTurnsUntilBorderGrowth(City city)
        {
            // if there is no culture being generated, the city's borders will never expand
            if (city.IncomeCulture == 0)
            {
                city.TurnsUntilBorderGrowth = -1;
                return;
            }

            // calculate the culture required to expand the city's borders
            int requiredTurns = 0;
            int requiredCulture = (int)Math.Round(20 + Math.Pow(10 * (Controllers.Board.GetCityTiles(city.InstanceID).Count - 1), 1.1));
            requiredCulture -= city.Culture;
            // calculate the number of turns required to expand the city's borders
            while (requiredCulture > 0)
            {
                requiredCulture -= city.IncomeCulture;
                requiredTurns++;
                if (requiredTurns > 999)
                {
                    city.TurnsUntilBorderGrowth = -2;
                    return;
                }
            }
            // assign the value to the city
            city.TurnsUntilBorderGrowth = requiredTurns;
        }

        // calculates the combat effectivness of a city
        private void CalculateDefense(City city)
        {
            int baseDefense = 10;
            city.Defense = baseDefense + city.Population;
            int moddedDef = (int)Math.Round(city.Defense * city.DefenseModifier);
            city.Defense = moddedDef;
        }

        // calculates the income the given city is to earn this turn
        private void CalculateIncome(City city)
        {
            // reset city income
            city.IncomeGold = 0;
            city.IncomeScience = 0;
            city.IncomeProduction = 0;
            city.IncomeFood = 0;
            city.IncomeCulture = 0;
            city.IncomeFaith = 0;
            city.IncomeTourism = 0;

            // building incomes
            int buildingIncomeGold = 0;
            int buildingIncomeScience = 0;
            int buildingIncomeProduction = 0;
            int buildingIncomeFood = 0;
            int buildingIncomeCulture = 0;
            int buildingIncomeFaith = 0;
            int buildingIncomeTourism = 0;

            // income modifiers
            float incomeGoldModifier = 1f;
            float incomeScienceModifier = 1f;
            float incomeProductionModifier = 1f;
            float incomeFoodModifier = 1f;
            float incomeCultureModifier = 1f;
            float incomeFaithModifier = 1f;
            float incomeTourismModifier = 1f;

            // yield income
            int yieldGold = 0;
            int yieldScience = 0;
            int yieldProduction = 0;
            int yieldFood = 0;
            int yieldCulture = 0;
            int yieldFaith = 0;
            int yieldTourism = 0;

            // yield modifiers
            float yieldGoldModifier = 1f;
            float yieldScienceModifier = 1f;
            float yieldProductionModifier = 1f;
            float yieldFoodModifier = 1f;
            float yieldCultureModifier = 1f;
            float yieldFaithModifier = 1f;
            float yieldTourismModifier = 1f;

            // add building income
            foreach (string building in city.Buildings)
            {
                // get building
                Building b = Controllers.Data.Building.GetBuilding(building);
                if (b == null)
                    continue;

                // track building income
                buildingIncomeGold += b.IncomeGold;
                buildingIncomeScience += b.IncomeScience;
                buildingIncomeProduction += b.IncomeProduction;
                buildingIncomeFood += b.IncomeFood;
                buildingIncomeCulture += b.IncomeCulture;
                buildingIncomeFaith += b.IncomeFaith;
                buildingIncomeTourism += b.IncomeTourism;

                // track income modifiers
                incomeGoldModifier *= b.IncomeGoldModifier;
                incomeScienceModifier *= b.IncomeScienceModifier;
                incomeProductionModifier *= b.IncomeProductionModifier;
                incomeFoodModifier *= b.IncomeFoodModifier;
                incomeCultureModifier *= b.IncomeCultureModifier;
                incomeFaithModifier *= b.IncomeFaithModifier;
                incomeTourismModifier *= b.IncomeTourismModifier;

                // track yield modifiers
                yieldGoldModifier *= b.YieldGoldModifier;
                yieldScienceModifier *= b.YieldScienceModifier;
                yieldProductionModifier *= b.YieldProductionModifier;
                yieldFoodModifier *= b.YieldFoodModifier;
                yieldCultureModifier *= b.YieldCultureModifier;
                yieldFaithModifier *= b.YieldFaithModifier;
                yieldTourismModifier *= b.YieldTourismModifier;
            }

            // add tile income
            foreach (Tile tile in Controllers.Board.GetCityTiles(city.InstanceID))
            {
                if (!city.CitizenLocations.Contains(tile.Location))
                    continue;
                int[] income = tile.GetIncome();
                yieldGold += income[(int)TileIncomeFormat.Gold];
                yieldScience += income[(int)TileIncomeFormat.Science];
                //city.IncomeHappiness += income[(int)TileIncomeFormat.Happiness];
                yieldProduction += income[(int)TileIncomeFormat.Production];
                yieldFood += income[(int)TileIncomeFormat.Food];
                yieldCulture += income[(int)TileIncomeFormat.Culture];
                yieldFaith += income[(int)TileIncomeFormat.Faith];
                yieldTourism += income[(int)TileIncomeFormat.Tourism];

                //city.Horses += income[(int)TileIncomeFormat.Horses];
                //city.Iron += income[(int)TileIncomeFormat.Iron];
                //city.Coal += income[(int)TileIncomeFormat.Coal];
                //city.Uranium += income[(int)TileIncomeFormat.Uranium];
                //city.Oil += income[(int)TileIncomeFormat.Oil];
            }

            // calculate final city income
            // add building income
            city.IncomeGold += (int)Math.Round(buildingIncomeGold * incomeGoldModifier);
            city.IncomeScience += (int)Math.Round(buildingIncomeScience * incomeScienceModifier);
            city.IncomeProduction += (int)Math.Round(buildingIncomeProduction * incomeProductionModifier);
            city.IncomeFood += (int)Math.Round(buildingIncomeFood * incomeFoodModifier);
            city.IncomeCulture += (int)Math.Round(buildingIncomeCulture * incomeCultureModifier);
            city.IncomeFaith += (int)Math.Round(buildingIncomeFaith * incomeFaithModifier);
            city.IncomeTourism += (int)Math.Round(buildingIncomeTourism * incomeTourismModifier);
            // add tile yield
            city.IncomeGold += (int)Math.Round(yieldGold * yieldGoldModifier);
            city.IncomeScience += (int)Math.Round(yieldScience * yieldScienceModifier);
            city.IncomeProduction += (int)Math.Round(yieldProduction * yieldProductionModifier);
            city.IncomeFood += (int)Math.Round(yieldFood * yieldFoodModifier);
            city.IncomeCulture += (int)Math.Round(yieldCulture * yieldCultureModifier);
            city.IncomeFaith += (int)Math.Round(yieldFaith * yieldFaithModifier);
            city.IncomeTourism += (int)Math.Round(yieldTourism * yieldTourismModifier);

            // track each city's culture production indiviually for border growth
            city.Culture += city.IncomeCulture;
        }

        // regenerates the city's hp by 20%
        private void RegenHP(City city)
        {
            if (city.IsBeingRaised) // don't regen the hp of a city that is being raised
                return;

            // make sure the city is aobve 1 hp
            if (city.HP < 1)
                city.HP = 1;

            // regen 20% of the city's max hp
            city.HP += (int)Math.Round(0.2 * city.MaxHP);

            // make sure the city's current hp doesn't go over its max hp
            if (city.HP > city.MaxHP)
                city.HP = city.MaxHP;
        }

        // exapnds the given city's borders if it has produced enough culture
        private void TryExpandBorders(City city)
        {
            if (city.IsBeingRaised) // don't expand the borders of a city that is being raised
                return;

            List<Tile> cityTiles = Controllers.Board.GetCityTiles(city.InstanceID);
            if (cityTiles.Count == 0) // if the city has no tiles, then give it all of its neighbour tiles and the tile it is located on
            {
                // add city's neighbour tiles to tiles we want to expand to
                List<Point> nLocs = Controllers.Board.GetTile(city.Location).GetNeighbourTileLocations();
                // add the city's location to the tiles we want to expand to
                nLocs.Add(city.Location);

                // get the tile for each valid location and mark it as belonging to the city
                // also keep track of each tile modified so we can trigger the CityBorderExpanded event later
                List<Tile> nTiles = new List<Tile>();
                foreach (Point loc in nLocs)
                {
                    Tile t = Controllers.Board.GetTile(loc);
                    if (t == null) // don't try and modify a til that doesn't exist
                        continue;
                    t.CityID = city.InstanceID; // mark the tile as belonging to the city
                    nTiles.Add(t); // keep track of the modified tiles
                }
                RelocateCitizens(city); // relocate citizens as they have new tiles they can possibly work
                OnCityBorderExpanded(new TileEventArgs(nTiles)); // trigger the CityBorderExpanded event
            }
            else if (cityTiles.Count == 91) // don't try and expand the borders of a city that has its maximum number of tiles already
            {
                return;
            }
            else // otherwise try expanding the city's border normally
            {
                // calculate the amount of culture required to expand the city's borders
                int requiredCulture = (int)Math.Round(20 + Math.Pow(10 * (cityTiles.Count - 1), 1.1));
                if (city.Culture <= requiredCulture) // if we don't have enough culture, then don't continue
                    return;
                city.Culture -= requiredCulture; // we have enough culture, so lets spend it
                
                int maxDist = 3; // the max dist is 3 at first
                if (cityTiles.Count >= 36) // but if we have every tile within a dist of 3, check every tile within 5 tiles
                    maxDist = 5;

                // Try find tiles at the predetermined max dist
                HashSet<TileScore> possibleExpansions = GetPossibleExpansions(city, maxDist);
                // If we didn't find any at 3 tiles....
                if (possibleExpansions.Count == 0 && maxDist == 3)
                    maxDist = 5;
                // ... try 5
                possibleExpansions = GetPossibleExpansions(city, maxDist);
                // Still nothing, so lets return
                if (possibleExpansions.Count == 0)
                    return;

                int bestScore = int.MinValue; // initialise the min socre with the lowest possible value
                foreach (TileScore ts in possibleExpansions) // get the best possible score
                {
                    if (ts.Score > bestScore)
                        bestScore = ts.Score;
                }

                // get a list of the tiles with the best possibel score
                List<TileScore> bestScoring = new List<TileScore>();
                foreach (TileScore ts in possibleExpansions)
                    if (ts.Score == bestScore)
                        bestScoring.Add(ts);
                // choose a tile from the best tiles at random
                int finalIndex = RandomHelper.Next(0, bestScoring.Count);

                bestScoring[finalIndex].Tile.CityID = city.InstanceID; // claim the tile
                Controllers.Board.UpdateTile(bestScoring[finalIndex].Tile); // update the tile
                RelocateCitizens(city); // relocate the city's citizens
                OnCityBorderExpanded(new TileEventArgs(new List<Tile>() { bestScoring[finalIndex].Tile })); // trigger the CityBorderExpanded event
            }
        }

        // gets a hash set of possible tile expansions and their scores for a given city with a given max distance
        private HashSet<TileScore> GetPossibleExpansions(City city, int maxDist)
        {
            // get every tile the city owns
            List<Tile> cityTiles = Controllers.Board.GetCityTiles(city.InstanceID);
            HashSet<TileScore> possibleExpansions = new HashSet<TileScore>();

            foreach (Tile t in cityTiles)
            {
                // loop through and find all the tiles that are neighbours of our city's tiles but we do not own
                foreach (Point nLoc in t.GetNeighbourTileLocations())
                {
                    // get the tile and check if it is within the maximum distance
                    Tile n = Controllers.Board.GetTile(nLoc);
                    if (n == null || Board.HexDistance(city.Location, n.Location) > maxDist)
                        continue;

                    // don't add the tile to the possible expansions if another city owns it
                    if (n.CityID != -1)
                        continue;

                    // calculate the tile's score
                    int score = 0;
                    switch (n.Resource)
                    {
                        case TileResource.Null:
                            break;
                        case TileResource.HORSES:
                            score += 5;
                            break;
                        case TileResource.IRON:
                            score += 6;
                            break;
                        case TileResource.COAL:
                            score += 7;
                            break;
                        case TileResource.URANIUM:
                            score += 9;
                            break;
                        case TileResource.OIL:
                            score += 8;
                            break;
                        default:
                            break;
                    }
                    switch (n.TerrainBase)
                    {
                        case TileTerrainBase.TUNDRA:
                            score -= 3;
                            break;
                        case TileTerrainBase.GRASSLAND:
                            score += 1;
                            break;
                        case TileTerrainBase.DESERT:
                            break;
                        case TileTerrainBase.SEA:
                            score -= 1;
                            break;
                        case TileTerrainBase.COAST:
                            score -= 1;
                            break;
                        case TileTerrainBase.SNOW:
                            score -= 4;
                            break;
                        default:
                            break;
                    }
                    switch (n.Improvement)
                    {
                        case TileImprovment.FOREST:
                        case TileImprovment.JUNGLE:
                            score += 1;
                            break;
                        case TileImprovment.MINE:
                        case TileImprovment.PLANTATION:
                        case TileImprovment.FARM:
                            score += 2;
                            break;
                    }
                    // prefer tiles that are closer to the city
                    score += (5 - Board.HexDistance(city.Location, nLoc));
                    // add the tile and its score to the possible expansions hash set
                    possibleExpansions.Add(new TileScore(n, score));
                }
            }

            return possibleExpansions;
        }

        // calculates a list of all the possible productions the given city can produce and assigns it to the city
        private void CalculateValidProductions(City city)
        {
            // results
            city.ValidProductions = new LinkedList<Production>();

            // get city info
            Tile cityTile = Controllers.Board.GetTile(city.Location);
            List<Point> cityNeighbourLocations = cityTile.GetNeighbourTileLocations();
            List<Tile> cityNeighbourTiles = new List<Tile>();
            foreach (Point p in cityNeighbourLocations)
                cityNeighbourTiles.Add(Controllers.Board.GetTile(p));
            List<Tile> cityTiles = Controllers.Board.GetCityTiles(city.InstanceID);
            Player cityOwner = Controllers.Player.GetPlayer(city.PlayerID);

            // get all valid buildings
            foreach (Building building in Controllers.Data.Building.GetAllBuildings())
            {
                // check if the building has already been constructed
                if (city.Buildings.Contains(building.ID))
                    continue;

                // control flag
                bool valid = true;

                // check if building is already in the production queue
                foreach (Production prod in city.ProductionQueue)
                {
                    if (prod.Name == building.ID)
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                    continue;

                // check if building prereqs are constructed
                foreach (string prereq in building.BuildingPrereq)
                {
                    if (!city.Buildings.Contains(prereq))
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                    continue;

                // check if tech prereqs are unlocked
                foreach (string prereq in building.TechPrereq)
                {
                    Technology prereqTech = cityOwner.TechTreeInstance.GetTech(prereq);
                    if (prereqTech != null && !prereqTech.Unlocked)
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                    continue;

                // check other properties
                if (building.Water)
                {
                    bool isOnCoast = false;
                    foreach (Tile tile in cityNeighbourTiles)
                    {
                        if (tile.TerrainBase == TileTerrainBase.COAST)
                        {
                            isOnCoast = true;
                            break;
                        }
                    }
                    if (!isOnCoast)
                        continue;
                }

                if (building.River)
                {
                    // NYI
                }

                if (building.FreshWater)
                {
                    // NYI
                }

                if (building.Mountain)
                {
                    bool hasMountainAccess = false;
                    foreach (Tile tile in cityTiles)
                    {
                        if (tile.TerrainFeature == TileTerrainFeature.MOUNTAIN)
                        {
                            hasMountainAccess = true;
                            break;
                        }
                    }
                    if (!hasMountainAccess)
                        continue;
                }

                if (building.Hill)
                {
                    if (cityTile.TerrainFeature != TileTerrainFeature.HILL)
                        continue;
                }

                if (building.Flat)
                {
                    if (cityTile.TerrainFeature == TileTerrainFeature.HILL)
                        continue;
                }

                if (building.Capital)
                {
                    if (!city.IsCapital)
                        continue;
                }

                if (building.NearbyTerrainRequired != TileTerrainBase.Null)
                {
                    bool hasCorrectNearbyTerrain = false;
                    foreach (Tile tile in cityNeighbourTiles)
                    {
                        if (tile.TerrainBase == building.NearbyTerrainRequired)
                        {
                            hasCorrectNearbyTerrain = true;
                            break;
                        }
                    }
                    if (!hasCorrectNearbyTerrain)
                        continue;
                }

                if (building.ProhibitedCityTerrain != TileTerrainBase.Null)
                {
                    if (cityTile.TerrainBase == building.ProhibitedCityTerrain)
                        continue;
                }

                // if the building is still valid, add it to the valid productions list
                city.ValidProductions.Enqueue(new Production(building));
            }

            // get all valid units
            foreach (Unit unit in Controllers.Data.Unit.GetAllUnits())
            {
                // control flag
                bool valid = true;

                // check if building prereqs are constructed
                foreach (string prereq in unit.BuildingPrereq)
                {
                    if (!city.Buildings.Contains(prereq))
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                    continue;

                // check if tech prereqs are unlocked
                foreach (string prereq in unit.TechPrereq)
                {
                    Technology prereqTech = cityOwner.TechTreeInstance.GetTech(prereq);
                    if (prereqTech != null && !prereqTech.Unlocked)
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                    continue;

                // if the unit is still valid, add it to the valid productions list
                city.ValidProductions.Enqueue(new Production(unit));
            }
        }

        //// calculate the happiness of the given city
        //private void CalculateHappiness(City city)
        //{
        //    if (city.IsBeingRaised) // dont't produce any happiness if the city is being raised
        //    {
        //        city.IncomeHappiness = 0;
        //        return;
        //    }

        //    int unhappiness = 3;
        //    unhappiness += city.Population;
        //    city.IncomeHappiness -= unhappiness;
        //}

        // calculate the population if the given city
        private void CalculatePopulation(City city)
        {
            if (city.IsBeingRaised) // don't grow the population of a city that is being raised
                return;

            // f(p) = 15 + 6(p - 1) + (p - 1)^1.8
            // f, food required to grow
            // p, current population
            int incomeFood = city.IncomeFood - ((city.Population - 1) * 2);
            int requiredFood = (int)Math.Floor(15 + 6 * (city.Population - 1) + Math.Pow(city.Population - 1, 1.8));
            city.ExcessFood += incomeFood;
            city.PopulationGorwthProgress = city.ExcessFood / (float)requiredFood; // keep track of propgress
            if (city.ExcessFood >= requiredFood)
            {
                city.Population++;
                city.ExcessFood -= requiredFood;
                RelocateCitizens(city);
            }
        }

        // calculate the production of the given city
        private void CalculateProduction(City city)
        {
            if (city.IsBeingRaised) // don't produce anything in a city that is being raised
                return;

            // DEBUG
            //city.ProductionOverflow = 999;
            // END DEBUG

            // don't try and process a production that doesn't exist
            if (city.ProductionQueue.First == null)
                return;
            
            Production currentProduction = city.ProductionQueue.First.Value; // get the current production
            currentProduction.Progress += city.ProductionOverflow; // add overflow

            float prodCost = float.MaxValue;
            float prodIncome = city.GetProductionIncome(currentProduction, Controllers.Data.Building, Controllers.Data.Unit);

            // get production cost
            switch (currentProduction.ProductionType)
            {
                case ProductionType.UNIT:
                    Unit u = Controllers.Data.Unit.GetUnit(currentProduction.Name);
                    prodCost = u.Cost;
                    break;
                case ProductionType.BUILDING:
                    Building b = Controllers.Data.Building.GetBuilding(currentProduction.Name);
                    prodCost = b.Cost;
                    break;
            }

            currentProduction.Progress += (int)Math.Round(prodIncome);

            // check if the production is done
            if (currentProduction.Progress >= prodCost)
            {
                // keep track of excess production
                city.ProductionOverflow = currentProduction.Progress - (int)Math.Round(prodCost);
                city.ProductionQueue.Dequeue(); // remove the production from the city's production queue
                switch (currentProduction.ProductionType)
                {
                    case ProductionType.BUILDING:
                        // add the produced building to the city's building list
                        city.Buildings.Add(currentProduction.Name);
                        CalculateIncome(city);
                        break;
                    case ProductionType.UNIT:
                        // add the produced unit to the unit controller
                        Controllers.Unit.AddUnit(currentProduction.Name, city.PlayerID, city.Location);
                        break;
                }
            }
        }

        // relocates citizens of the given city based on its production focus
        private void RelocateCitizens(City city)
        {
            switch (city.CitizenFocus)
            {
                case CityCitizenFocus.Default:
                    FocusCitizens(city, TileIncomeFormat.Food);
                    break;
                case CityCitizenFocus.Food:
                    FocusCitizens(city, TileIncomeFormat.Food);
                    break;
                case CityCitizenFocus.Production:
                    FocusCitizens(city, TileIncomeFormat.Production);
                    break;
                case CityCitizenFocus.Gold:
                    FocusCitizens(city, TileIncomeFormat.Gold);
                    break;
                case CityCitizenFocus.Science:
                    FocusCitizens(city, TileIncomeFormat.Science);
                    break;
                case CityCitizenFocus.Culture:
                    FocusCitizens(city, TileIncomeFormat.Culture);
                    break;
            }
        }

        // focus the given city's citizen on the given income format
        private void FocusCitizens(City city, TileIncomeFormat focus)
        {
            if (city.CitizenLocations == null)
                city.CitizenLocations = new List<Point>();
            else
                city.CitizenLocations.Clear();
            int[] tileinComeFormatValues = (int[])Enum.GetValues(typeof(TileIncomeFormat));
            int citizensLeft = city.Population;
            List<Tile> cityTiles = Controllers.Board.GetCityTiles(city.InstanceID);
            while (citizensLeft > 0)
            {
                TileScore best = null;
                foreach (Tile tile in cityTiles)
                {
                    int[] income = tile.GetIncome();
                    int score = 0;
                    score += income[(int)focus] * 5;
                    for (int i = 0; i < tileinComeFormatValues.Length; i++)
                        score += income[i];

                    if (best == null)
                    {
                        best = new TileScore(tile, score);
                        continue;
                    }

                    if (city.CitizenLocations.Contains(best.Tile.Location))
                        best = new TileScore(tile, score);

                    if (score > best.Score && !city.CitizenLocations.Contains(tile.Location))
                        best = new TileScore(tile, score);
                }

                city.CitizenLocations.Add(best.Tile.Location);
                citizensLeft--;
            }
        }

        /// <summary>
        /// Gets all the cities controlled by the city controller
        /// </summary>
        /// <returns></returns>
        public List<City> GetAllCities()
        {
            return cities;
        }

        /// <summary>
        /// Gets the city at the given location on the board
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public City GetCity(Point location)
        {
            return cities.Find(c => c.Location == location);
        }

        /// <summary>
        /// Gets the city with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public City GetCity(int id)
        {
            return cities.Find(c => c.InstanceID == id);
        }

        /// <summary>
        /// Gets a list of all the city's owned by the player with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<City> GetPlayerCities(int id)
        {
            return cities.FindAll(c => c.PlayerID == id);
        }

        /// <summary>
        /// Settles a city with the given settler unit
        /// </summary>
        /// <param name="settler"></param>
        /// <returns>whether a city was successfully settled</returns>
        public bool SettleCity(UnitInstance settler)
        {
            // check if the settler is too close to another city
            foreach (City c in cities)
            {
                if (Board.HexDistance(settler.Location, c.Location) < 4)
                {
                    ConsoleManager.Instance.WriteLine("City too close to another", MsgType.ServerInfo);
                    return false; // the city was not settled
                }
            }

            // get the player that is trying to settle the city
            Player player = Controllers.Player.GetPlayer(settler.PlayerID);
            // create a new city and give it a unique id
            City city = new City(player, Controllers.Data.Empire.GetEmpire(player.EmpireID).GetNextDefaultCityName(), settler.Location);
            city.InstanceID = lastCityID++;
            // check if this is the players first city
            if (GetPlayerCities(player.InstanceID).Count == 0)
            {
                city.IsCapital = true; // the player's first city is its capital
                city.Buildings.Add("BUILDING_PALACE"); // capital city's have the palace
            }
            cities.Add(city); // add the city to the city list
            TryExpandBorders(city); // expand the city's borders
            UpdateCity(city); // update city data
            //OnCitySettled(new CityEventArgs(city)); // trigger the CitySettled event
            OnCityUpdated(new CityEventArgs(city)); // trigger the CityUpdated event
            return true; // the city was settled
        }

        /// <summary>
        /// Captures a city for the player of the given id
        /// </summary>
        /// <param name="city"></param>
        /// <param name="playerID"></param>
        public void CaptureCity(City city, int playerID)
        {
            // transfer ownership
            city.PlayerID = playerID;
            city.EmpireID = Controllers.Player.GetPlayer(playerID).EmpireID;

            // prevent the city from being raised
            city.IsBeingRaised = false;

            // trigger CityCaptured event
            OnCityCaptured(new CityEventArgs(city));
        }

        /// <summary>
        /// Causes a city to take damage
        /// </summary>
        /// <param name="city"></param>
        /// <param name="damage"></param>
        public void DamageCity(City city, int damage)
        {
            // removethe damage done to the city from its current hp
            city.HP -= damage;
            // don't let the city fall below 1 hp
            if (city.HP < 1)
                city.HP = 1;
            // trigger the CityUpdated event
            OnCityUpdated(new CityEventArgs(city));
        }

        /// <summary>
        /// Marks the given city for raising, which will take a number of turns
        /// </summary>
        public void RaseCity(City city)
        {
            // TODO fully implement and test this feature
            city.IsBeingRaised = true;
        }
    }
}
