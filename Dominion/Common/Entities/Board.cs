using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dominion.Common.Entities
{
    public enum WorldSize
    {
        Tiny,
        Small,
        Medium,
        Large,
        //Massive
    }

    public enum WorldType
    {
        Pangea,
        Archipelago,
        Continents,
        Fractal,
        GreatPlains,
        Highlands,
        InlandSea,
        Lakes
    }

    [Serializable()]
    public class Board
    {
        public Tile[][] Tiles { get; private set; }
        public int DimX { get; private set; }
        public int DimY { get; private set; }

        public Board(WorldType worldType, WorldSize worldSize)
        {
            switch (worldSize)
            {
                case WorldSize.Tiny:
                    DimX = 15;
                    DimY = 10;
                    break;
                case WorldSize.Small:
                    DimX = 40;
                    DimY = 30;
                    break;
                case WorldSize.Medium:
                    DimX = 60;
                    DimY = 40;
                    break;
                case WorldSize.Large:
                    DimX = 80;
                    DimY = 50;
                    break;
            }
            switch (worldType)
            {
                case WorldType.Pangea:
                    GeneratePangea();
                    break;
                case WorldType.Archipelago:
                    GenerateArchipelago();
                    break;
                case WorldType.Continents:
                    GenerateContinents();
                    break;
                case WorldType.Fractal:
                    GenerateFractal();
                    break;
                case WorldType.GreatPlains:
                    GenerateGreatPlains();
                    break;
                case WorldType.Highlands:
                    GenerateHighlands();
                    break;
                case WorldType.InlandSea:
                    GenerateInlandSea();
                    break;
                case WorldType.Lakes:
                    GenerateLakes();
                    break;
                default:
                    break;
            }
        }

        public Board()
        {

        }

        // "Memset" all tiles to given settings
        private void InstantiateTiles(int dimx, int dimy, TileResource tres, TileTerrainBase tbase, TileTerrainFeature tfeature, TileImprovment timp)
        {
            Tiles = new Tile[dimy][];
            for (int y = 0; y < Tiles.Length; y++)
            {
                Tiles[y] = new Tile[dimx];
                for (int x = 0; x < Tiles[y].Length; x++)
                {
                    Tiles[y][x] = new Tile(new Point(x, y), tres, tbase, tfeature, timp);
                }
            }
        }

        public static LinkedList<Point> FindPath(Point start, Point dest, Board board)
        {
            if (start == dest)
                return new LinkedList<Point>();

            Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
            Dictionary<Point, int> costSoFar = new Dictionary<Point, int>();

            PriorityQueue<Point> frontier = new PriorityQueue<Point>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                Tile current = board.GetTile(frontier.Dequeue());

                if (current.Location == dest)
                    break;

                foreach (Point nextLoc in current.GetNeighbourTileLocations())
                {
                    Tile next = board.GetTile(nextLoc);
                    if (next == null || next.GetMovementCost() == -1)
                        continue;
                    int newCost = costSoFar[current.Location] + current.GetMovementCost();
                    if (!costSoFar.ContainsKey(next.Location) || newCost < costSoFar[next.Location])
                    {
                        costSoFar[next.Location] = newCost;
                        int priority = newCost;// + HexDistance(next.Location, goal);
                        frontier.Enqueue(next.Location, priority);
                        cameFrom[next.Location] = current.Location;
                    }
                }
            }

            List<Point> inverseMoveQueue = new List<Point>();
            inverseMoveQueue.Add(dest);
            Point from;
            try { from = cameFrom[dest]; }
            catch (Exception) { return null; }
            while (from != start)
            {
                inverseMoveQueue.Add(from);
                from = cameFrom[from];
            }
            inverseMoveQueue.Add(start);
            inverseMoveQueue.Reverse();

            LinkedList<Point> movementQueue = new LinkedList<Point>();
            for (int i = 1; i < inverseMoveQueue.Count; i++)
                movementQueue.Enqueue(inverseMoveQueue[i]);
            //foreach (Point point in inverseMoveQueue)
            //    movementQueue.Enqueue(point);

            return movementQueue;
        }

        public static int HexDistance(Point p1, Point p2)
        {
            int y1 = p1.X;
            int x1 = p1.Y;
            int y2 = p2.X;
            int x2 = p2.Y;
            int du = x2 - x1;
            int dv = (y2 + (int)Math.Floor(x2 / 2f)) - (y1 + (int)Math.Floor(x1 / 2f));

            int adu = Math.Abs(du);
            int adv = Math.Abs(dv);

            if ((du >= 0 && dv >= 0) || (du < 0 && dv < 0))
                return Math.Max(adu, adv);
            return adu + adv;
        }

        // Generates map features, to be used after generating the rough map of sea & plains. Setting a param to -1 disables the process
        private void GenerateFeatures(double lakeSeedChance, double lakeSpreadChance, int minimunWaterSize, int minimunLandSize, double hillSeedChance, double hillDirChangeChance, double hillSpreadChance, double mountainSeedChance, double mountainDirChangeChance, double mountainSpreadChance, double forestSeedChance, double forestSpreadChance, double desertSeedChance, double desertSpreadChance, int minimumDesertSize, double resourceSeedChance, int snowLevel, int tundraLevel, bool defineCoast)
        {
            Stopwatch sw = new Stopwatch();

            if (lakeSeedChance != -1 && lakeSpreadChance != -1)
            {
                ConsoleManager.Instance.WriteLine("Seeding lakes", MsgType.ServerInfo);
                sw.Start();
                SeedLakes(lakeSeedChance, lakeSpreadChance);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (minimunLandSize != -1)
            {
                ConsoleManager.Instance.WriteLine("Removing small islands", MsgType.ServerInfo);
                sw.Start();
                RemoveSmallLand(minimunLandSize);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (minimunWaterSize != -1)
            {
                ConsoleManager.Instance.WriteLine("Removing small bodies of water", MsgType.ServerInfo);
                sw.Start();
                RemoveSmallWater(minimunWaterSize);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (hillSeedChance != -1 && hillDirChangeChance != -1 && hillSpreadChance != -1)
            {
                ConsoleManager.Instance.WriteLine("Seeding hills", MsgType.ServerInfo);
                sw.Start();
                SeedHillRange(hillSeedChance, hillDirChangeChance, hillSpreadChance);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (mountainSeedChance != -1 && mountainDirChangeChance != -1 && mountainSpreadChance != -1)
            {
                ConsoleManager.Instance.WriteLine("Seeding mountains", MsgType.ServerInfo);
                sw.Start();
                SeedMountainRange(mountainSeedChance, mountainDirChangeChance, mountainSpreadChance);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (forestSeedChance != -1 && forestSpreadChance != -1)
            {
                ConsoleManager.Instance.WriteLine("Seeding forests", MsgType.ServerInfo);
                sw.Start();
                SeedImprovment(forestSeedChance, forestSpreadChance, TileImprovment.Forest, true);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (desertSeedChance != -1 && desertSpreadChance != -1)
            {
                ConsoleManager.Instance.WriteLine("Seeding deserts", MsgType.ServerInfo);
                sw.Start();
                SeedDesert(desertSeedChance, desertSpreadChance);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (minimumDesertSize != -1)
            {
                ConsoleManager.Instance.WriteLine("Removing small deserts", MsgType.ServerInfo);
                sw.Start();
                ReplaceSmallTerrain(minimumDesertSize, TileTerrainBase.Desert, TileTerrainBase.Grassland, true);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (resourceSeedChance != -1)
            {
                ConsoleManager.Instance.WriteLine("Seeding resources", MsgType.ServerInfo);
                sw.Start();
                SeedResources(resourceSeedChance);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (tundraLevel != -1)
            {
                ConsoleManager.Instance.WriteLine("Defining tundra", MsgType.ServerInfo);
                sw.Start();
                DefineTundra(tundraLevel);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (snowLevel != -1)
            {
                ConsoleManager.Instance.WriteLine("Defining snow", MsgType.ServerInfo);
                sw.Start();
                DefineSnow(snowLevel);
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            if (defineCoast)
            {
                ConsoleManager.Instance.WriteLine("Defining coast", MsgType.ServerInfo);
                sw.Start();
                DefineCoast();
                sw.Stop();
                ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            }

            ConsoleManager.Instance.WriteLine("Fixing errors", MsgType.ServerInfo);
            sw.Start();
            FixErrors();
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
        }

        // Generate a pangea map
        private void GeneratePangea()
        {
            // Properties
            int landSeedOffset = (int)(DimX * 0.1);
            double landSpreadChance = 0.99;
            int landSpreadItterations = (int)(DimY / 2.5);

            double lakeSeedChance = 0.02;
            double lakeSpreadChance = 0.8;
            int minimunWaterSize = 20;
            int minimunLandSize = -1;
            double hillSeedChance = 0.1;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.05;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.95;
            double forestSeedChance = 0.025;
            double forestSpreadChance = 0.9;
            double desertSeedChance = 0.025;
            double desertSpreadChance = 0.8;
            int minimumDesertSize = 8;
            double resourceSeedChance = 0.01;
            int snowLevel = (int)(0.02 * DimY);
            int tundraLevel = (int)(0.05 * DimY);
            bool defineCoast = true;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Sea, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            ConsoleManager.Instance.WriteLine("Generating land", MsgType.ServerInfo);
            sw.Start();
            Tile centre = GetTile(DimX / 2, DimY / 2);
            for (int i = 0; i < landSpreadItterations; i++)
                SpreadTerrainRecursive(centre, landSpreadChance, TileTerrainBase.Grassland);
            Tile centreLeft = GetTile(DimX / 2 - landSeedOffset, DimY / 2);
            for (int i = 0; i < landSpreadItterations; i++)
                SpreadTerrainRecursive(centreLeft, landSpreadChance, TileTerrainBase.Grassland);
            Tile centreRight = GetTile(DimX / 2 + landSeedOffset, DimY / 2);
            for (int i = 0; i < landSpreadItterations; i++)
                SpreadTerrainRecursive(centreRight, landSpreadChance, TileTerrainBase.Grassland);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);
        }

        // Generate a archipelago map
        private void GenerateArchipelago()
        {
            // Properties
            double landSeedChance = 0.98;
            double landSpreadChance = 0.7;
            int landSpreadItterations = (int)(DimY / 2.5);

            double lakeSeedChance = -1;
            double lakeSpreadChance = -1;
            int minimunWaterSize = -1;
            int minimunLandSize = 9;
            double hillSeedChance = 0.05;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.01;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.95;
            double forestSeedChance = 0.025;
            double forestSpreadChance = 0.9;
            double desertSeedChance = -1;
            double desertSpreadChance = -1;
            int minimumDesertSize = -1;
            double resourceSeedChance = 0.01;
            int snowLevel = (int)(0.02 * DimY);
            int tundraLevel = (int)(0.05 * DimY);
            bool defineCoast = true;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Sea, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            ConsoleManager.Instance.WriteLine("Generating land", MsgType.ServerInfo);
            sw.Start();
            foreach (Tile tile in GetAllTiles())
            {
                if (RandomHelper.Roll(landSeedChance))
                    continue;
                SpreadTerrainRecursive(tile, landSpreadChance, TileTerrainBase.Grassland);
            }
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);
        }

        // Generate a continents map
        private void GenerateContinents()
        {
            // Properties
            int landSeedOffset = (int)(DimX * 0.25);
            double landSpreadChance = 0.99;
            int landSpreadItterations = DimY / 2;

            double lakeSeedChance = 0.01;
            double lakeSpreadChance = 0.8;
            int minimunWaterSize = 20;
            int minimunLandSize = -1;
            double hillSeedChance = 0.1;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.05;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.95;
            double forestSeedChance = 0.025;
            double forestSpreadChance = 0.9;
            double desertSeedChance = 0.025;
            double desertSpreadChance = 0.8;
            int minimumDesertSize = 8;
            double resourceSeedChance = 0.01;
            int snowLevel = (int)(0.02 * DimY);
            int tundraLevel = (int)(0.05 * DimY);
            bool defineCoast = true;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Sea, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            ConsoleManager.Instance.WriteLine("Generating land", MsgType.ServerInfo);
            sw.Start();
            Tile centreLeft = GetTile(DimX / 2 - landSeedOffset, DimY / 2);// - (int)(DimY * 0.2));
            for (int i = 0; i < landSpreadItterations; i++)
                SpreadTerrainRecursive(centreLeft, landSpreadChance, TileTerrainBase.Grassland);
            Tile centreRight = GetTile(DimX / 2 + landSeedOffset, DimY / 2);// - (int)(DimY * 0.2));
            for (int i = 0; i < landSpreadItterations; i++)
                SpreadTerrainRecursive(centreRight, landSpreadChance, TileTerrainBase.Grassland);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);
        }

        // Generate a fractal map
        private void GenerateFractal()
        {
            // Properties
            int perlinOctaveCount = 100;
            double landChanceModifier = 0.9;

            double lakeSeedChance = 0.02;
            double lakeSpreadChance = 0.8;
            int minimunWaterSize = 20;
            int minimunLandSize = 50;
            double hillSeedChance = 0.1;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.05;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.99;
            double forestSeedChance = 0.04;
            double forestSpreadChance = 0.9;
            double desertSeedChance = 0.025;
            double desertSpreadChance = 0.8;
            int minimumDesertSize = 8;
            double resourceSeedChance = 0.01;
            int snowLevel = (int)(DimY * 0.02);
            int tundraLevel = (int)(DimY * 0.05);
            bool defineCoast = false;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Sea, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            ConsoleManager.Instance.WriteLine("Generating land", MsgType.ServerInfo);
            sw.Start();
            float[][] perlinNoise = GeneratePerlinNoise(GenerateWhiteNoise(DimX, DimY), perlinOctaveCount);
            for (int y = 0; y < perlinNoise.Length; y++)
                for (int x = 0; x < perlinNoise[y].Length; x++)
                    if (RandomHelper.Roll(perlinNoise[y][x] * landChanceModifier))
                        GetTile(x, y).TerrainBase = TileTerrainBase.Grassland;
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
            
            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);

            ConsoleManager.Instance.WriteLine("Spreading land", MsgType.ServerInfo);
            sw.Start();
            HashSet<Tile> checkedTiles = new HashSet<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.TerrainBase == TileTerrainBase.Grassland && !checkedTiles.Contains(tile))
                {
                    checkedTiles.Add(tile);
                    foreach (Point loc in tile.GetNeighbourTileLocations())
                    {
                        Tile n = GetTile(loc);
                        if (n == null)
                            continue;
                        if (!checkedTiles.Contains(n))
                        {
                            n.TerrainBase = TileTerrainBase.Grassland;
                            checkedTiles.Add(n);
                        }
                    }
                }
            }
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            ConsoleManager.Instance.WriteLine("Defining coast", MsgType.ServerInfo);
            sw.Start();
            DefineCoast();
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);
        }

        // Generate a great plains map
        private void GenerateGreatPlains()
        {
            // Porperties
            int seaSpreadIntterations = DimY / 2;
            int seaSeedOffsets = (int)(DimX * 0.1);
            double seaSpreadChance = 0.99;

            double lakeSeedChance = 0.001;
            double lakeSpreadChance = 0.6;
            int minimunWaterSize = 20;
            int minimunLandSize = -1;
            double hillSeedChance = 0.01;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.005;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.99;
            double forestSeedChance = -1;
            double forestSpreadChance = -1;
            double desertSeedChance = 0.25;
            double desertSpreadChance = 0.9;
            int minimumDesertSize = 8;
            double resourceSeedChance = 0.01;
            int snowLevel = -1;
            int tundraLevel = -1;
            bool defineCoast = true;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Grassland, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            ConsoleManager.Instance.WriteLine("Generating sea", MsgType.ServerInfo);
            sw.Start();
            Tile bottomRight = GetTile(DimX - 3, DimY - 3);
            for (int i = 0; i < DimY / seaSpreadIntterations; i++)
                SpreadTerrainRecursive(bottomRight, seaSpreadChance, TileTerrainBase.Sea);
            Tile bottomLessRight = GetTile(DimX - seaSeedOffsets, DimY - 3);
            for (int i = 0; i < DimY / seaSpreadIntterations; i++)
                SpreadTerrainRecursive(bottomLessRight, seaSpreadChance, TileTerrainBase.Sea);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);
        }

        // Generate a great plains map
        private void GenerateHighlands()
        {
            // Properties
            double lakeSeedChance = 0.02;
            double lakeSpreadChance = 0.8;
            int minimunWaterSize = 20;
            int minimunLandSize = -1;
            double hillSeedChance = 0.1;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.15;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.95;
            double forestSeedChance = 0.025;
            double forestSpreadChance = 0.8;
            double desertSeedChance = -1;
            double desertSpreadChance = -1;
            int minimumDesertSize = -1;
            double resourceSeedChance = 0.01;
            int snowLevel = (int)(0.02 * DimY);
            int tundraLevel = (int)(0.05 * DimY);
            bool defineCoast = true;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Grassland, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);
        }

        // Generate an inland sea map
        private void GenerateInlandSea()
        {
            // Properties
            int seaSpreadIntterations = DimY / 2;
            int seaSeedOffsets = (int)(DimX * 0.1);
            double terrainSpreadChance = 0.95;

            double lakeSeedChance = 0.01;
            double lakeSpreadChance = 0.3;
            int minimunWaterSize = 20;
            int minimunLandSize = -1;
            double hillSeedChance = 0.1;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.05;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.99;
            double forestSeedChance = 0.025;
            double forestSpreadChance = 0.8;
            double desertSeedChance = 0.025;
            double desertSpreadChance = 0.8;
            int minimumDesertSize = 8;
            double resourceSeedChance = 0.01;
            int snowLevel = (int)(DimY * 0.02);
            int tundraLevel = (int)(DimY * 0.05);
            bool defineCoast = true;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Grassland, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            ConsoleManager.Instance.WriteLine("Generating Sea", MsgType.ServerInfo);
            sw.Start();
            Tile centre = GetTile(DimX / 2, DimY / 2);
            for (int i = 0; i < seaSpreadIntterations; i++)
                SpreadTerrainRecursive(centre, terrainSpreadChance, TileTerrainBase.Sea);
            Tile centreRight = GetTile(DimX / 2 + seaSeedOffsets, DimY / 2);
            for (int i = 0; i < seaSpreadIntterations; i++)
                SpreadTerrainRecursive(centreRight, terrainSpreadChance, TileTerrainBase.Sea);
            Tile centreLeft = GetTile(DimX / 2 - seaSeedOffsets, DimY / 2);
            for (int i = 0; i < seaSpreadIntterations; i++)
                SpreadTerrainRecursive(centreLeft, terrainSpreadChance, TileTerrainBase.Sea);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);
        }

        // Generate a lakes map
        private void GenerateLakes()
        {
            // Porperties
            double lakeSeedChance = 0.05;
            double lakeSpreadChance = 0.9;
            int minimunWaterSize = 9;
            int minimunLandSize = -1;
            double hillSeedChance = 0.1;
            double hillDirChangeChance = 0.5;
            double hillSpreadChance = 0.99;
            double mountainSeedChance = 0.05;
            double mountainDirChangeChance = 0.5;
            double mountainSpreadChance = 0.99;
            double forestSeedChance = 0.025;
            double forestSpreadChance = 0.8;
            double desertSeedChance = -1;
            double desertSpreadChance = -1;
            int minimumDesertSize = -1;
            double resourceSeedChance = 0.01;
            int snowLevel = (int)(DimY * 0.02);
            int tundraLevel = (int)(DimY * 0.05);
            bool defineCoast = true;

            Stopwatch sw = new Stopwatch();

            ConsoleManager.Instance.WriteLine("Initializing board", MsgType.ServerInfo);
            sw.Start();
            InstantiateTiles(DimX, DimY, TileResource.None, TileTerrainBase.Grassland, TileTerrainFeature.Open, TileImprovment.None);
            sw.Stop();
            ConsoleManager.Instance.WriteLine($"Time taken, {sw.ElapsedMilliseconds}ms", MsgType.ServerInfo);

            GenerateFeatures(lakeSeedChance, lakeSpreadChance, minimunWaterSize, minimunLandSize, hillSeedChance, hillDirChangeChance, hillSpreadChance, mountainSeedChance, mountainDirChangeChance, mountainSpreadChance, forestSeedChance, forestSpreadChance, desertSeedChance, desertSpreadChance, minimumDesertSize, resourceSeedChance, snowLevel, tundraLevel, defineCoast);
        }

        // Fix inconsistencies/errors
        private void FixErrors()
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (!tile.Land)
                    tile.Improvement = TileImprovment.None;

                if (tile.TerrainBase == TileTerrainBase.Desert)
                    tile.Improvement = TileImprovment.None;

                if (tile.TerrainBase == TileTerrainBase.Snow)
                    tile.Improvement = TileImprovment.None;

                if (tile.TerrainBase == TileTerrainBase.Tundra && tile.Improvement == TileImprovment.Jungle)
                    tile.Improvement = TileImprovment.None;

                if (tile.TerrainFeature == TileTerrainFeature.Mountain)
                    tile.Improvement = TileImprovment.None;
            }
        }

        // Tries to place random resources on passable land tiles with a chance of success
        private void SeedResources(double seedChance)
        {
            string[] resources = Enum.GetNames(typeof(TileResource));
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land && tile.Passable && RandomHelper.Roll(seedChance))
                {
                    int resIndex = RandomHelper.Next(0, resources.Length);
                    tile.Resource = (TileResource)resIndex;
                }
            }
        }

        // Tries to create a lake on tiles with a chance of success, and then has a chance to spreads the lake
        private void SeedLakes(double lakeSeedChance, double lakeSpreadChance)
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land)
                {
                    while (RandomHelper.Roll(lakeSeedChance))
                    {
                        SpreadTerrainRecursive(tile, lakeSpreadChance, TileTerrainBase.Coast, true);
                    }
                }
            }
        }

        // Tries to create a feature on all tiles with a chance of success, and then has a chance to spreads the feature
        private void SeedFeature(double seedChance, double spreadChance, TileTerrainFeature feature, bool land)
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land == land)
                {
                    if (RandomHelper.Roll(seedChance))
                        SpreadFeatureRecursive(tile, spreadChance, feature, land);
                }
            }
        }

        // Tries to create an improvment on all tiles with a chance of success, and then has a chance to spreads the improvment
        private void SeedImprovment(double seedChance, double spreadChance, TileImprovment imp, bool land)
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land == land)
                {
                    if (RandomHelper.Roll(seedChance))
                        SpreadImprovmentRecursive(tile, spreadChance, imp, land);
                }
            }
        }
        
        // Sets all neighbours of every land tile to land
        private void SpreadLandToNeighbours()
        {
            HashSet<Tile> tilesChecked = new HashSet<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land && !tilesChecked.Contains(tile))
                {
                    foreach (Point loc in tile.GetNeighbourTileLocations())
                    {
                        Tile n = GetTile(loc);
                        n.TerrainBase = TileTerrainBase.Grassland;
                        tilesChecked.Add(n);
                    }
                    tilesChecked.Add(tile);
                }
            }
        }

        // Tries to create mountain ranges on all tiles with a chance of success, and then has a chance to spread the range
        private void SeedMountainRange(double seedChance, double dirChangeChance, double spreadChance)
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land)
                {
                    if (RandomHelper.Roll(seedChance))
                        CreateMountainRangeRecursive(tile, dirChangeChance, spreadChance);
                }
            }
        }

        // Spreads a mountain range
        private void CreateMountainRangeRecursive(Tile tile, double spreadChance, double dirChangeChance, TileDirection dir = TileDirection.Null)
        {
            if (dir == TileDirection.Null)
            {
                int dirCount = Enum.GetNames(typeof(TileDirection)).Length;
                dir = (TileDirection)RandomHelper.Next(0, dirCount - 1);
            }

            if (tile == null || !tile.Land)
                return;

            tile.TerrainFeature = TileTerrainFeature.Mountain;

            if (RandomHelper.Roll(spreadChance))
            {
                if (RandomHelper.Roll(dirChangeChance))
                {
                    bool cw = RandomHelper.Roll(0.5);
                    switch (dir)
                    {
                        case TileDirection.NE:
                            if (cw) dir = TileDirection.E;
                            else dir = TileDirection.NW;
                            break;
                        case TileDirection.E:
                            if (cw) dir = TileDirection.SE;
                            else dir = TileDirection.NE;
                            break;
                        case TileDirection.SE:
                            if (cw) dir = TileDirection.SW;
                            else dir = TileDirection.E;
                            break;
                        case TileDirection.SW:
                            if (cw) dir = TileDirection.W;
                            else dir = TileDirection.SE;
                            break;
                        case TileDirection.W:
                            if (cw) dir = TileDirection.NW;
                            else dir = TileDirection.SW;
                            break;
                        case TileDirection.NW:
                            if (cw) dir = TileDirection.NE;
                            else dir = TileDirection.W;
                            break;
                    }
                }
                Tile next = GetTile(tile.GetNeighbour(dir));
                CreateMountainRangeRecursive(next, spreadChance, dirChangeChance, dir);
            }
        }

        // Tries to create hill ranges on all tiles with a chance of success, and then has a chance to spread the range
        private void SeedHillRange(double seedChance, double dirChangeChance, double spreadChance)
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land)
                {
                    if (RandomHelper.Roll(seedChance))
                        CreateHillRangeRecursive(tile, dirChangeChance, spreadChance);
                }
            }
        }

        // Tries to create a desert on all tiles with a chance of success, and then has a chance to spread
        private void SeedDesert(double seedChance, double spreadChance)
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (tile.Land)
                {
                    if (RandomHelper.Roll(seedChance))
                        SpreadTerrainRecursive(tile, spreadChance, TileTerrainBase.Desert, true);
                }
            }
        }

        // Spreads a hill range
        private void CreateHillRangeRecursive(Tile tile, double spreadChance, double dirChangeChance, TileDirection dir = TileDirection.Null)
        {
            if (dir == TileDirection.Null)
            {
                int dirCount = Enum.GetNames(typeof(TileDirection)).Length;
                dir = (TileDirection)RandomHelper.Next(0, dirCount - 1);
            }

            if (tile == null || !tile.Land)
                return;

            tile.TerrainFeature = TileTerrainFeature.Hill;

            if (RandomHelper.Roll(spreadChance))
            {
                if (RandomHelper.Roll(dirChangeChance))
                {
                    bool cw = RandomHelper.Roll(0.5);
                    switch (dir)
                    {
                        case TileDirection.NE:
                            if (cw) dir = TileDirection.E;
                            else dir = TileDirection.NW;
                            break;
                        case TileDirection.E:
                            if (cw) dir = TileDirection.SE;
                            else dir = TileDirection.NE;
                            break;
                        case TileDirection.SE:
                            if (cw) dir = TileDirection.SW;
                            else dir = TileDirection.E;
                            break;
                        case TileDirection.SW:
                            if (cw) dir = TileDirection.W;
                            else dir = TileDirection.SE;
                            break;
                        case TileDirection.W:
                            if (cw) dir = TileDirection.NW;
                            else dir = TileDirection.SW;
                            break;
                        case TileDirection.NW:
                            if (cw) dir = TileDirection.NE;
                            else dir = TileDirection.W;
                            break;
                    }
                }
                Tile next = GetTile(tile.GetNeighbour(dir));
                CreateHillRangeRecursive(next, spreadChance, dirChangeChance, dir);
            }
        }

        // Remove contigious bodies of water that are below the minimum size
        private void RemoveSmallWater(int minWaterBodySize)
        {
            HashSet<Tile> checkedWaterTiles = new HashSet<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (checkedWaterTiles.Contains(tile))
                    continue;
                HashSet<Tile> water = ContigiousWater(tile);
                if (water.Count < minWaterBodySize)
                {
                    //ConsoleManager.Instance.WriteLine($"Removing water body of size {water.Count}", MsgType.ServerInfo);
                    foreach (Tile waterTile in water)
                    {
                        waterTile.TerrainBase = TileTerrainBase.Grassland;
                    }
                }
                else
                {
                    foreach (Tile waterTile in water)
                    {
                        checkedWaterTiles.Add(waterTile);
                    }
                }
            }
        }

        // Remove contigious landmasses that are below the minimum size
        private void RemoveSmallLand(int minLandSize)
        {
            HashSet<Tile> checkedLandTiles = new HashSet<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (checkedLandTiles.Contains(tile))
                    continue;
                HashSet<Tile> land = ContigiousLand(tile);
                if (land.Count < minLandSize)
                {
                    //ConsoleManager.Instance.WriteLine($"Removing island of size {land.Count}", MsgType.ServerInfo);
                    foreach (Tile landTile in land)
                    {
                        landTile.TerrainBase = TileTerrainBase.Sea;
                    }
                }
                else
                {
                    foreach (Tile landTile in land)
                    {
                        checkedLandTiles.Add(landTile);
                    }
                }
            }
        }

        // Replace contigious terrain that in below the minimum size with replacement
        private void ReplaceSmallTerrain(int minTerrainSize, TileTerrainBase tBase, TileTerrainBase replacement, bool land)
        {
            HashSet<Tile> checkedTiles = new HashSet<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (checkedTiles.Contains(tile))
                    continue;
                HashSet<Tile> tiles = ContigiousTerrain(tile, tBase, land);
                if (tiles.Count < minTerrainSize)
                {
                    //ConsoleManager.Instance.WriteLine($"Removing island of size {land.Count}", MsgType.ServerInfo);
                    foreach (Tile baseTile in tiles)
                    {
                        if (baseTile.Land == land)
                            baseTile.TerrainBase = replacement;
                    }
                }
                else
                {
                    foreach (Tile baseTile in tiles)
                    {
                        checkedTiles.Add(baseTile);
                    }
                }
            }
        }

        // Replace contigious improvments that are below the minimum size with replacement
        private void ReplaceSmallImprovment(int minImpSize, TileImprovment imp, TileImprovment replacement)
        {
            HashSet<Tile> checkedLandTiles = new HashSet<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (checkedLandTiles.Contains(tile))
                    continue;
                HashSet<Tile> land = ContigiousImprovment(tile, imp);
                if (land.Count < minImpSize)
                {
                    //ConsoleManager.Instance.WriteLine($"Removing island of size {land.Count}", MsgType.ServerInfo);
                    foreach (Tile impTile in land)
                    {
                        impTile.Improvement = replacement;
                    }
                }
                else
                {
                    foreach (Tile impTile in land)
                    {
                        checkedLandTiles.Add(impTile);
                    }
                }
            }
        }

        // Replace contigious features that are below the minimum size with replacement
        private void ReplaceSmallFeature(int minImpSize, TileTerrainFeature tFeature, TileTerrainFeature replacement)
        {
            HashSet<Tile> checkedLandTiles = new HashSet<Tile>();
            foreach (Tile tile in GetAllTiles())
            {
                if (checkedLandTiles.Contains(tile))
                    continue;
                HashSet<Tile> feature = ContigiousFeature(tile, tFeature);
                if (feature.Count < minImpSize)
                {
                    //ConsoleManager.Instance.WriteLine($"Removing island of size {land.Count}", MsgType.ServerInfo);
                    foreach (Tile featureTile in feature)
                    {
                        featureTile.TerrainFeature = replacement;
                    }
                }
                else
                {
                    foreach (Tile featureTile in feature)
                    {
                        checkedLandTiles.Add(featureTile);
                    }
                }
            }
        }

        // Gets a set of land that is connected
        public HashSet<Tile> ContigiousLand(Tile tile, HashSet<Tile> checkedTiles = null)
        {
            if (checkedTiles == null)
                checkedTiles = new HashSet<Tile>();
            checkedTiles.Add(tile);
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n != null && n.Land && !checkedTiles.Contains(n))
                    ContigiousLand(n, checkedTiles);
            }
            return checkedTiles;
        }

        // Gets a set of water that is connected
        public HashSet<Tile> ContigiousWater(Tile tile, HashSet<Tile> checkedTiles = null)
        {
            if (checkedTiles == null)
                checkedTiles = new HashSet<Tile>();
            checkedTiles.Add(tile);
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n != null && !n.Land && !checkedTiles.Contains(n))
                    ContigiousWater(n, checkedTiles);
            }
            return checkedTiles;
        }

        // Gets a set of tiles with the same imporvments that are connected
        public HashSet<Tile> ContigiousImprovment(Tile tile, TileImprovment imp, HashSet<Tile> checkedTiles = null)
        {
            if (checkedTiles == null)
                checkedTiles = new HashSet<Tile>();
            checkedTiles.Add(tile);
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n.Improvement == imp && !checkedTiles.Contains(n))
                    ContigiousImprovment(n, imp, checkedTiles);
            }
            return checkedTiles;
        }

        // Gets a set of tiles with the same base terrain that are connected
        public HashSet<Tile> ContigiousTerrain(Tile tile, TileTerrainBase tBase, bool land, HashSet<Tile> checkedTiles = null)
        {
            if (checkedTiles == null)
                checkedTiles = new HashSet<Tile>();
            checkedTiles.Add(tile);
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n != null && n.TerrainBase == tBase && n.Land == land && !checkedTiles.Contains(n))
                    ContigiousTerrain(n, tBase, land, checkedTiles);
            }
            return checkedTiles;
        }

        // Gets a set of tiles with the same features that are connected
        public HashSet<Tile> ContigiousFeature(Tile tile, TileTerrainFeature feature, HashSet<Tile> checkedTiles = null)
        {
            if (checkedTiles == null)
                checkedTiles = new HashSet<Tile>();
            checkedTiles.Add(tile);
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n.TerrainFeature == feature && !checkedTiles.Contains(n))
                    ContigiousFeature(n, feature, checkedTiles);
            }
            return checkedTiles;
        }

        // Spreads land from a seed tile, with a given chance to continue spreading
        private void SpreadTerrainRecursive(Tile seed, double spreadChance, TileTerrainBase terrain)
        {
            Tile next = null;
            int nIndex = RandomHelper.Next(0, 7);
            int i = 0;
            foreach (Point loc in seed.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (i == nIndex)
                {
                    next = n;
                    break;
                }
                i++;
            }
            if (next == null) // Edge of the map might cause issues, so try again if we didn't get a tile
            {
                SpreadTerrainRecursive(seed, spreadChance, terrain);
                return;
            }

            SetNeightbourTerrain(next, terrain);
            //seed.TerrainBase = terrain;
            next.TerrainBase = terrain;

            // 10% chance of stopping
            if (RandomHelper.Roll(spreadChance))
                SpreadTerrainRecursive(next, spreadChance, terrain);
        }

        // Spreads terrain from a seed tile, with a given chance to continue spreading to only land/water
        private void SpreadTerrainRecursive(Tile seed, double spreadChance, TileTerrainBase terrain, bool land)
        {
            Tile next = null;
            int nIndex = RandomHelper.Next(0, 7);
            int i = 0;
            foreach (Point loc in seed.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (i == nIndex)
                {
                    next = n;
                    break;
                }
                i++;
            }
            if (next == null) // Edge of the map might cause issues, so try again if we didn't get a tile
            {
                SpreadTerrainRecursive(seed, spreadChance, terrain, land);
                return;
            }

            if (next.Land != land)
                return;

            SetNeightbourTerrain(next, terrain, land);
            //seed.TerrainBase = terrain;
            next.TerrainBase = terrain;

            // 10% chance of stopping
            if (RandomHelper.Roll(spreadChance))
                SpreadTerrainRecursive(next, spreadChance, terrain, land);
        }

        // Spreads improvments from a seed tile, with a given chance to continue spreading
        private void SpreadImprovmentRecursive(Tile seed, double spreadChance, TileImprovment imp, bool land)
        {
            Tile next = null;
            int nIndex = RandomHelper.Next(0, 7);
            int i = 0;
            foreach (Point loc in seed.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (i == nIndex)
                {
                    next = n;
                    break;
                }
                i++;
            }
            if (next == null) // Edge of the map might cause issues, so try again if we didn't get a tile
            {
                SpreadImprovmentRecursive(seed, spreadChance, imp, land);
                return;
            }

            if (next.Land == land)
            {
                next.Improvement = imp;
                SetNeightbourImprovment(next, imp, land);
            }
            // 10% chance of stopping
            if (RandomHelper.Roll(spreadChance))
                SpreadImprovmentRecursive(next, spreadChance, imp, land);
        }

        // Spreads features from a seed tile, with a given chance to continue spreading
        private void SpreadFeatureRecursive(Tile seed, double spreadChance, TileTerrainFeature feature, bool land)
        {
            Tile next = null;
            int nIndex = RandomHelper.Next(0, 7);
            int i = 0;
            foreach (Point loc in seed.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (i == nIndex)
                {
                    next = n;
                    break;
                }
                i++;
            }
            if (next == null) // Edge of the map might cause issues, so try again if we didn't get a tile
            {
                SpreadFeatureRecursive(seed, spreadChance, feature, land);
                return;
            }

            if (next.Land == land)
            {
                next.TerrainFeature = feature;
                SetNeightbourFeature(next, feature, land);
            }

            // 10% chance of stopping
            if (RandomHelper.Roll(spreadChance))
                SpreadFeatureRecursive(next, spreadChance, feature, land);
        }

        // Sets all neightbours of the given tile to the given terrain
        private void SetNeightbourTerrain(Tile tile, TileTerrainBase terrain)
        {
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n != null)
                    n.TerrainBase = terrain;
            }
        }

        // Sets all neightbours of the given tile with the given land value to the given terrain
        private void SetNeightbourTerrain(Tile tile, TileTerrainBase terrain, bool land)
        {
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n != null && n.Land)
                    n.TerrainBase = terrain;
            }
        }

        // Sets all neightbours of the given tile to the given improvment
        private void SetNeightbourImprovment(Tile tile, TileImprovment imp, bool land)
        {
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n != null && n.Land == land)
                    n.Improvement = imp;
            }
        }

        // Sets all neightbours of the given tile to the given feature
        private void SetNeightbourFeature(Tile tile, TileTerrainFeature feature, bool land)
        {
            foreach (Point loc in tile.GetNeighbourTileLocations())
            {
                Tile n = GetTile(loc);
                if (n.Land == land)
                    n.TerrainFeature = feature;
            }
        }

        // Sets sea tiles that are next to land to coast
        private void DefineCoast()
        {
            foreach (Tile tile in GetAllTiles())
            {
                if (!tile.Land)
                {
                    foreach (Point loc in tile.GetNeighbourTileLocations())
                    {
                        Tile n = GetTile(loc);
                        if (n != null && n.Land)
                            tile.TerrainBase = TileTerrainBase.Coast;
                    }
                }
            }
        }

        // Add snow to the north and south of the board, up to the given snowLevel
        private void DefineSnow(int snowLevel)
        {
            // Snow top
            for (int y = 0; y < snowLevel; y++)
            {
                for (int x = 0; x < DimX; x++)
                {
                    Tile tile = GetTile(x, y);
                    if (tile.Land)
                        SpreadTerrainRecursive(tile, 0.05, TileTerrainBase.Snow, true);
                }
            }
            // Snow bottom
            for (int y = DimY - 1; y >= DimY - snowLevel; y--)
            {
                for (int x = 0; x < DimX; x++)
                {
                    Tile tile = GetTile(x, y);
                    if (tile.Land)
                        SpreadTerrainRecursive(tile, 0.05, TileTerrainBase.Snow, true);
                }
            }
        }

        // Add tundra to the north and south of the board, up to the given tundraLevel
        private void DefineTundra(int tundraLevel)
        {
            // Tundra top
            for (int y = 0; y < tundraLevel; y++)
            {
                for (int x = 0; x < DimX; x++)
                {
                    Tile tile = GetTile(x, y);
                    if (tile.Land)
                        SpreadTerrainRecursive(tile, 0.5, TileTerrainBase.Tundra, true);
                }
            }
            // Tundra bottom
            for (int y = DimY - 1; y >= DimY - tundraLevel; y--)
            {
                for (int x = 0; x < DimX; x++)
                {
                    Tile tile = GetTile(x, y);
                    if (tile.Land)
                        SpreadTerrainRecursive(tile, 0.5, TileTerrainBase.Tundra, true);
                }
            }
        }

        #region Noise generation
        private float Interpolate(float x0, float x1, float alpha)
        {
            return x0 * (1 - alpha) + alpha * x1;
        }

        private float[][] GenerateWhiteNoise(int width, int height)
        {
            float[][] noise = new float[height][];
            for (int y = 0; y < height; y++)
            {
                noise[y] = new float[width];
                for (int x = 0; x < width; x++)
                {
                    noise[y][x] = (float)RandomHelper.NextDouble() % 1;
                }
            }

            return noise;
        }

        private float[][] GenerateSmoothNoise(float[][] baseNoise, int octave)
        {
            int height = baseNoise.Length;
            int width = baseNoise[0].Length;


            int samplePeriod = 1 << octave; // calculates 2 ^ k
            float sampleFrequency = 1.0f / samplePeriod;

            float[][] smoothNoise = new float[height][];
            for (int y = 0; y < height; y++)
            {
                smoothNoise[y] = new float[width];

                //calculate the horizontal sampling indices
                int sample_y0 = Math.Min(Math.Abs((y / samplePeriod) * samplePeriod), height - 1);
                int sample_y1 = Math.Min(Math.Abs((sample_y0 + samplePeriod) % width), height - 1); //wrap around
                float horizontal_blend = (y - sample_y0) * sampleFrequency;

                for (int x = 0; x < width; x++)
                {
                    //calculate the vertical sampling indices
                    int sample_x0 = Math.Abs(Math.Min((x / samplePeriod) * samplePeriod, width - 1));
                    int sample_x1 = Math.Abs(Math.Min((sample_x0 + samplePeriod) % height, width - 1)); //wrap around
                    float vertical_blend = (x - sample_x0) * sampleFrequency;

                    //blend the top two corners
                    float top = Interpolate(baseNoise[sample_y0][sample_x0],
                       baseNoise[sample_y1][sample_x1], horizontal_blend);

                    //blend the bottom two corners
                    float bottom = Interpolate(baseNoise[sample_y0][sample_x0],
                       baseNoise[sample_y1][sample_x1], horizontal_blend);

                    //final blend
                    smoothNoise[y][x] = Interpolate(top, bottom, vertical_blend);
                }
            }

            return smoothNoise;
        }

        private float[][] GeneratePerlinNoise(float[][] baseNoise, int octaveCount)
        {
            int height = baseNoise.Length;
            int width = baseNoise[0].Length;

            float[][][] smoothNoise = new float[octaveCount][][]; //an array of 2D arrays containing

            float persistance = 0.5f;

            //generate smooth noise
            for (int i = 0; i < octaveCount; i++)
                smoothNoise[i] = GenerateSmoothNoise(baseNoise, i);

            float[][] perlinNoise = new float[height][];
            for (int y = 0; y < perlinNoise.Length; y++)
                perlinNoise[y] = new float[width];

            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;

            //blend noise together
            for (int octave = octaveCount - 1; octave >= 0; octave--)
            {
                amplitude *= persistance;
                totalAmplitude += amplitude;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        perlinNoise[y][x] += smoothNoise[octave][y][x] * amplitude;
                    }
                }
            }

            //normalisation
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    perlinNoise[y][x] /= totalAmplitude;
                }
            }

            return perlinNoise;
        }
        #endregion

        // Gets every tile on the board, row by row
        public List<Tile> GetAllTiles()
        {
            List<Tile> tiles = new List<Tile>();
            for (int y = 0; y < DimY; y++)
            {
                for (int x = 0; x < DimX; x++)
                {
                    tiles.Add(Tiles[y][x]);
                }
            }
            return tiles;
        }

        // Gets a tile at the given coords
        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= DimX || y >= DimY)
                return null;
            return Tiles[y][x];
        }

        // Gets a tile at the given coords
        public Tile GetTile(Point location)
        {
            return GetTile(location.X, location.Y);
        }

        // Updates the board with the newer version of the given tiles
        public void UpdateTiles(List<Tile> updatedTiles)
        {
            foreach (Tile tile in updatedTiles)
            {
                UpdateTile(tile);
            }
        }

        // Updates the board with the newer version of the given tile
        public void UpdateTile(Tile tile)
        {
            Tiles[tile.Location.Y][tile.Location.X] = tile;
        }
    }
}
