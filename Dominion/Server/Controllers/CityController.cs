using ArwicEngine.Core;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
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

        public event EventHandler<CityEventArgs> CitySettled;
        public event EventHandler<CityEventArgs> CityCaptured;
        public event EventHandler<CityEventArgs> CityUpdated;
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

        public override void ProcessTurn()
        {
            foreach (City city in cities)
            {
                RegenHP(city);
                CalculateIncome(city);
                CalculateHappiness(city);
                CalculatePopulation(city);
                CalculateProduction(city);
                TryExpandBorders(city);
                CalculateTurnsUntilPopulationGrowth(city);
                CalculateTurnsUntilBorderGrowth(city);
            }
        }

        public void CommandCity(CityCommand cmd)
        {
            City city = GetCity(cmd.CityID);
            if (city == null)
                return;

            try
            {
                switch (cmd.CommandID)
                {
                    case CityCommandID.Rename:
                        city.Name = (string)cmd.Arguments[0];
                        break;
                    case CityCommandID.ChangeProduction:
                        city.ProductionQueue.AddFirst(Controllers.Factory.Production.GetProduction((int)cmd.Arguments[0]));
                        break;
                    case CityCommandID.QueueProduction:
                        city.ProductionQueue.Enqueue(Controllers.Factory.Production.GetProduction((int)cmd.Arguments[0]));
                        break;
                    case CityCommandID.CancelProduction:
                        city.ProductionQueue.Remove((int)cmd.Arguments[0]);
                        break;
                    case CityCommandID.ReorderProductionMoveUp:
                        LinkedListNode<Production> selectedNode = city.ProductionQueue.GetNode((int)cmd.Arguments[0]);
                        city.ProductionQueue.Swap(selectedNode, selectedNode.Previous);
                        break;
                    case CityCommandID.ReorderProductionMoveDown:
                        selectedNode = city.ProductionQueue.GetNode((int)cmd.Arguments[0]);
                        city.ProductionQueue.Swap(selectedNode, selectedNode.Next);
                        break;
                    case CityCommandID.BuyProduction:
                        // TODO implement buying producibles with gold
                        break;
                    case CityCommandID.ChangeCitizenFocus:
                        city.CitizenFocus = (CityCitizenFocus)cmd.Arguments[0];
                        RelocateCitizens(city);
                        CalculateIncome(city);
                        break;
                }
            }
            catch (Exception)
            {
                //Engine.Console.WriteLine($"CityCommand.{cmd.CommandID.ToString()} Error: malformed data", MsgType.ServerWarning);
            }

            OnCityUpdated(new CityEventArgs(city));
        }

        private void CalculateTurnsUntilPopulationGrowth(City city)
        {
            if (city.IncomeFood == 0)
            {
                city.TurnsUntilPopulationGrowth = -1;
                return;
            }
            int requiredTurns = 0;
            int requiredFood = (int)Math.Floor(15 + 6 * (city.Population - 1) + Math.Pow(city.Population - 1, 1.8));
            int foodIncome = city.IncomeFood - (city.Population - 1) * 2;
            requiredFood -= city.ExcessFood;
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
            city.TurnsUntilPopulationGrowth = requiredTurns;
        }

        private void CalculateTurnsUntilBorderGrowth(City city)
        {
            if (city.IncomeCulture == 0)
            {
                city.TurnsUntilBorderGrowth = -1;
                return;
            }
            int requiredTurns = 0;
            int requiredCulture = (int)Math.Round(20 + Math.Pow(10 * (Controllers.Board.GetCityTiles(city.InstanceID).Count - 1), 1.1));
            requiredCulture -= city.Culture;
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
            city.TurnsUntilBorderGrowth = requiredTurns;
        }

        private void CalculateIncome(City city)
        {
            city.IncomeGold = 0;
            city.IncomeScience = 0;
            city.IncomeHappiness = 0;
            city.IncomeProduction = 0;
            city.IncomeFood = 0;
            city.IncomeCulture = 0;

            city.Horses = 0;
            city.Iron = 0;
            city.Coal = 0;
            city.Uranium = 0;
            city.Oil = 0;

            foreach (int buildingID in city.Buildings)
            {
                Building b = Controllers.Factory.Building.GetBuilding(buildingID);
                city.IncomeGold += b.IncomeGold;
                city.IncomeScience += b.IncomeScience;
                city.IncomeHappiness += b.IncomeHappiness;
                city.IncomeProduction += b.IncomeProduction;
                city.IncomeFood += b.IncomeFood;
                city.IncomeCulture += b.IncomeCulture;
            }

            foreach (Tile tile in Controllers.Board.GetCityTiles(city.InstanceID))
            {
                if (!city.CitizenLocations.Contains(tile.Location))
                    continue;
                int[] income = tile.GetIncome();
                city.IncomeGold += income[(int)TileIncomeFormat.Gold];
                city.IncomeScience += income[(int)TileIncomeFormat.Science];
                city.IncomeHappiness += income[(int)TileIncomeFormat.Happiness];
                city.IncomeProduction += income[(int)TileIncomeFormat.Production];
                city.IncomeFood += income[(int)TileIncomeFormat.Food];
                city.IncomeCulture += income[(int)TileIncomeFormat.Culture];
                city.Horses += income[(int)TileIncomeFormat.Horses];
                city.Iron += income[(int)TileIncomeFormat.Iron];
                city.Coal += income[(int)TileIncomeFormat.Coal];
                city.Uranium += income[(int)TileIncomeFormat.Uranium];
                city.Oil += income[(int)TileIncomeFormat.Oil];
            }

            city.Culture += city.IncomeCulture;
        }

        private void RegenHP(City city)
        {
            if (city.HP < 1)
                city.HP = 1;

            city.HP += (int)Math.Round(0.2 * city.MaxHP);

            if (city.HP > city.MaxHP)
                city.HP = city.MaxHP;
        }

        private void TryExpandBorders(City city)
        {
            List<Tile> cityTiles = Controllers.Board.GetCityTiles(city.InstanceID);
            if (cityTiles.Count == 0)
            {
                List<Point> nLocs = Controllers.Board.GetTile(city.Location).GetNeighbourTileLocations();
                nLocs.Add(city.Location);
                List<Tile> nTiles = new List<Tile>();
                foreach (Point loc in nLocs)
                {
                    Tile t = Controllers.Board.GetTile(loc);
                    if (t == null)
                        continue;
                    t.CityID = city.InstanceID;
                    nTiles.Add(t);
                }
                RelocateCitizens(city);
                OnCityBorderExpanded(new TileEventArgs(nTiles));
            }
            else if (cityTiles.Count == 91)
            {
                return;
            }
            else
            {
                int requiredCulture = (int)Math.Round(20 + Math.Pow(10 * (cityTiles.Count - 1), 1.1));
                if (city.Culture <= requiredCulture)
                    return;
                city.Culture -= requiredCulture;

                int maxDist = 3;
                if (cityTiles.Count >= 36)
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

                int bestScore = int.MinValue;
                foreach (TileScore ts in possibleExpansions)
                {
                    if (ts.Score > bestScore)
                        bestScore = ts.Score;
                }

                List<TileScore> bestScoring = new List<TileScore>();
                foreach (TileScore ts in possibleExpansions)
                    if (ts.Score == bestScore)
                        bestScoring.Add(ts);
                int finalIndex = RandomHelper.Next(0, bestScoring.Count);
                List<Tile> result = new List<Tile>();
                bestScoring[finalIndex].Tile.CityID = city.InstanceID;
                Controllers.Board.UpdateTile(bestScoring[finalIndex].Tile);
                RelocateCitizens(city);
                OnCityBorderExpanded(new TileEventArgs(new List<Tile>() { bestScoring[finalIndex].Tile }));
            }
        }

        private HashSet<TileScore> GetPossibleExpansions(City city, int maxDist)
        {
            List<Tile> cityTiles = Controllers.Board.GetCityTiles(city.InstanceID);
            HashSet<TileScore> possibleExpansions = new HashSet<TileScore>();
            int bestScore = int.MinValue;

            foreach (Tile t in cityTiles)
            {
                foreach (Point nLoc in t.GetNeighbourTileLocations())
                {
                    Tile n = Controllers.Board.GetTile(nLoc);
                    if (n == null || Board.HexDistance(city.Location, n.Location) > maxDist)
                        continue;

                    if (n.CityID == -1)
                    {
                        int score = 0;
                        switch (n.Resource)
                        {
                            case TileResource.None:
                                break;
                            case TileResource.Horses:
                                score += 5;
                                break;
                            case TileResource.Iron:
                                score += 6;
                                break;
                            case TileResource.Coal:
                                score += 7;
                                break;
                            case TileResource.Uranium:
                                score += 9;
                                break;
                            case TileResource.Oil:
                                score += 8;
                                break;
                            default:
                                break;
                        }
                        switch (n.TerrainBase)
                        {
                            case TileTerrainBase.Tundra:
                                score -= 3;
                                break;
                            case TileTerrainBase.Grassland:
                                score += 1;
                                break;
                            case TileTerrainBase.Desert:
                                break;
                            case TileTerrainBase.Sea:
                                score -= 1;
                                break;
                            case TileTerrainBase.Coast:
                                score -= 1;
                                break;
                            case TileTerrainBase.Snow:
                                score -= 4;
                                break;
                            default:
                                break;
                        }
                        switch (n.Improvement)
                        {
                            case TileImprovment.Forest:
                            case TileImprovment.Jungle:
                                score += 1;
                                break;
                            case TileImprovment.Mine:
                            case TileImprovment.Plantation:
                            case TileImprovment.Farm:
                                score += 2;
                                break;
                        }
                        score += (5 - Board.HexDistance(city.Location, nLoc));

                        if (score > bestScore)
                            bestScore = score;
                        possibleExpansions.Add(new TileScore(n, score));
                    }
                }
            }

            return possibleExpansions;
        }

        private void CalculateHappiness(City city)
        {
            int unhappiness = 3;
            unhappiness += city.Population;
            city.IncomeHappiness -= unhappiness;
        }

        private void CalculatePopulation(City city)
        {
            // f(p) = 15 + 6(p - 1) + (p - 1)^1.8
            // f, food required to grow
            // p, current population
            int incomeFood = city.IncomeFood - ((city.Population - 1) * 2);
            int requiredFood = (int)Math.Floor(15 + 6 * (city.Population - 1) + Math.Pow(city.Population - 1, 1.8));
            city.ExcessFood += incomeFood;
            if (city.ExcessFood >= requiredFood)
            {
                city.Population++;
                city.ExcessFood -= requiredFood;
                RelocateCitizens(city);
            }
        }

        private void CalculateProduction(City city)
        {
            // TODO this is temp
            city.ProductionOverflow = 999;

            if (city.ProductionQueue.First == null)
                return;
            Production currentProduction = city.ProductionQueue.First.Value;
            currentProduction.Progress += city.ProductionOverflow;
            currentProduction.Progress += city.IncomeProduction;
            if (currentProduction.Progress >= currentProduction.ProductionCost)
            {
                city.ProductionOverflow = currentProduction.Progress - currentProduction.ProductionCost;
                city.ProductionQueue.Dequeue();
                switch (currentProduction.ResultType)
                {
                    case ProductionResultType.Building:
                        city.Buildings.Add(currentProduction.ResultID);
                        break;
                    case ProductionResultType.Unit:
                        Controllers.Unit.AddUnit(currentProduction.ResultID, city.PlayerID, city.Location);
                        break;
                }
            }
        }

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

        public List<City> GetAllCities()
        {
            return cities;
        }

        public City GetCity(Point location)
        {
            return cities.Find(c => c.Location == location);
        }

        public City GetCity(int id)
        {
            return cities.Find(c => c.InstanceID == id);
        }

        public List<City> GetPlayerCities(int id)
        {
            return cities.FindAll(c => c.PlayerID == id);
        }

        public bool SettleCity(Unit settler)
        {
            foreach (City c in cities)
            {
                if (Board.HexDistance(settler.Location, c.Location) < 4)
                {
                    Console.WriteLine("City too close to another");
                    return false;
                }
            }

            Player player = Controllers.Player.GetPlayer(settler.PlayerID);
            City city = new City(player, Controllers.Factory.Empire.GetNextDefaultName(player.EmpireID), settler.Location);
            city.InstanceID = cities.Count;
            if (GetPlayerCities(player.InstanceID).Count == 0)
            {
                city.IsCapital = true;
                city.Buildings.Add(0);
            }
            cities.Add(city);
            TryExpandBorders(city);
            CalculateIncome(city);
            OnCitySettled(new CityEventArgs(city));
            return true;
        }

        public void CaptureCity(City city, int playerID)
        {
            city.PlayerID = playerID;
            city.EmpireID = Controllers.Player.GetPlayer(playerID).EmpireID;

            OnCityCaptured(new CityEventArgs(city));
        }

        public void DamageCity(City city, int damage)
        {
            city.HP -= damage;
            if (city.HP < 1)
                city.HP = 1;

            OnCityUpdated(new CityEventArgs(city));
        }

        public void RaseCity()
        {
            throw new NotImplementedException();
        }
    }
}
