// Dominion - Copyright (C) Timothy Ings
// UnitRenderer.cs
// This file defines classes that render units

using ArwicEngine.Graphics;
using ArwicEngine.Input;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Dominion.Client.Renderers
{
    class UnitRenderer
    {
        private object _lock_updateMoveTargets = new object();

        private BoardRenderer boardRenderer;
        private int playerID;
        private Client client;
        private Camera2 camera;
        private Color pathColor;

        // Resources
        private Sprite[] unitSprites;
        private Sprite tileHighlightSprite;

        // Cache
        private List<Tile> moveTargets;

        public UnitRenderer(BoardRenderer boardRenderer, Camera2 camera, Client client)
        {
            this.client = client;
            this.camera = camera;
            playerID = client.Player.InstanceID;
            this.boardRenderer = boardRenderer;
            pathColor = Color.White;
            moveTargets = new List<Tile>();
            // register events and load resources
            client.SelectedCommandChanged += Client_SelectedCommandChanged;
            LoadResources();
        }

        private void Client_SelectedCommandChanged(object sender, UnitCommandIDEventArgs e)
        {
            // recalc move targets if the selected command changes
            CalculateMoveTargets(e.UnitCommandID);
        }

        // calculates the tiles to which the select unit can move to
        private void CalculateMoveTargets(UnitCommandID cmd)
        {
            if (cmd != UnitCommandID.Move)
                return; // don't bother if the select command isn't move

            lock (_lock_updateMoveTargets)
            {
                moveTargets.Clear(); // clear the current move targets

                // Init values
                if (client.SelectedUnit == null)
                    return;
                Point start = client.SelectedUnit.Location;
                int movement = client.SelectedUnit.Movement;

                // Init collections
                HashSet<Tile> checkedMoves = new HashSet<Tile>();
                HashSet<Tile> possibleMoves = new HashSet<Tile>();
                Queue<Tile> uncheckedMoves = new Queue<Tile>(client.GetCachedTile(start).GetNeighbourTiles(client.CachedBoard));

                // Process data
                while (uncheckedMoves.Count > 0)
                {
                    // Get the next unchecked tile
                    Tile uncheckedMove = uncheckedMoves.Dequeue();

                    // Check if we have checked it
                    if (checkedMoves.Contains(uncheckedMove)) continue; // Yes we have, lets not check it again
                    else checkedMoves.Add(uncheckedMove); // No we haven't, lets mark ti as check so we don't check it again

                    // Get the path to the tile
                    LinkedList<Point> path = Board.FindPath(uncheckedMove.Location, start, client.Board);
                    // Check if it is possible this turn
                    if (path != null && path.Count <= movement && uncheckedMove.GetMovementCost() != -1)
                    {
                        possibleMoves.Add(uncheckedMove); // Yes it is, add it to the possible moves

                        // Try adding the unchecked tile's neighbours to the queue
                        List<Tile> ns = uncheckedMove.GetNeighbourTiles(client.CachedBoard);
                        foreach (Tile n in ns)
                        {
                            // Don't bother checking a tile we will never be able to move onto
                            if (n.GetMovementCost() == -1)
                                continue;
                            // Add the neighbour to the queue if we haven't checked it yet
                            if (!checkedMoves.Contains(n))
                                uncheckedMoves.Enqueue(n);
                        }
                    }
                }
                moveTargets.AddRange(possibleMoves);
            }
        }

        // load unit resources
        private void LoadResources()
        {
            tileHighlightSprite = new Sprite("Graphics/Game/Tiles/TileOverlay");

            int count = Enum.GetNames(typeof(UnitGraphicID)).Length;
            unitSprites = new Sprite[count];
            for (int i = 0; i < count; i++)
                unitSprites[i] = new Sprite($"Graphics/Game/Units/{i}");
        }

        // draw all units to client can see
        public void Draw(SpriteBatch sb)
        {
            lock (Client._lock_cacheUpdate)
            {
                foreach (Unit unit in client.CachedUnits.ToArray())
                {
                    if (unit.PlayerID == playerID)
                        DrawMovementPath(sb, unit);
                    DrawGraphic(sb, unit);
                }
                DrawSelectedMarker(sb);
                DrawCommandTarget(sb);
            }
        }

        // draw the given unit's graphic
        private void DrawGraphic(SpriteBatch sb, Unit unit)
        {
            if (unit == null)
                return; // don't render anything if the unit is null
            // calculate the units position
            Vector2 tileCentre = boardRenderer.GetTileCentre(unit.Location);
            Rectangle dest = new Rectangle(
                        (int)tileCentre.X - boardRenderer.TileSize / 2,
                        (int)tileCentre.Y - boardRenderer.TileSize / 2,
                        boardRenderer.TileSize,
                        boardRenderer.TileSize);
            // draw the correct sprite at the unit's position
            unitSprites[unit.Template.GraphicID].Draw(sb, dest);
        }

        // draws the given unit's movement path
        private void DrawMovementPath(SpriteBatch sb, Unit unit)
        {
            if (unit.MovementQueue == null || unit.MovementQueue.Count <= 0)
                return;
            DrawPath(sb, unit.Location, unit.MovementQueue, pathColor);
        }

        // draws a line following the given path with the given starting point
        private void DrawPath(SpriteBatch sb, Point start, LinkedList<Point> path, Color color)
        {
            if (path == null)
                return;
            // draw a line from the last tile centre to the next tile centre
            int i = 0;
            Vector2[] movePath = new Vector2[path.Count + 1];
            movePath[i++] = boardRenderer.GetTileCentre(start);
            foreach (Point point in path)
                movePath[i++] = boardRenderer.GetTileCentre(point);
            for (i = 0; i < movePath.Length - 1; i++)
                GraphicsHelper.DrawLine(sb, movePath[i], movePath[i + 1], 2, color);
        }

        // draws the highlight sprite over the selected unit
        private void DrawSelectedMarker(SpriteBatch sb)
        {
            if (client.SelectedUnit != null)
                tileHighlightSprite.Draw(sb, boardRenderer.GetTileRenderRect(client.SelectedUnit.Location), null, Color.White);
        }

        // draws all the valid tiles the selected command can to used on
        private void DrawCommandTarget(SpriteBatch sb)
        {
            if (client.SelectedUnit == null)
                return; // don't continue if there is no selected unit

            if (UnitCommand.GetTargetType(client.SelectedCommand) != UnitCommandTargetType.Tile)
                return; // don't continue if the selected command is not of target type tile

            switch (client.SelectedCommand)
            {
                case UnitCommandID.Move: // Draw possible move locations
                    lock (_lock_updateMoveTargets)
                    {
                        if (moveTargets == null)
                            break;
                        foreach (Tile tileLoc in moveTargets)
                            tileHighlightSprite.Draw(sb, boardRenderer.GetTileRenderRect(tileLoc), null, Color.CornflowerBlue);
                        Tile tileUnderMouse = boardRenderer.GetTileAtPoint(InputManager.Instance.MouseWorldPos(camera));
                        if (tileUnderMouse != null && client.GetCachedTile(tileUnderMouse.Location) != null)
                            DrawPath(sb, client.SelectedUnit.Location, Board.FindPath(client.SelectedUnit.Location, tileUnderMouse.Location, client.Board), Color.Yellow);
                    }
                    break;
                case UnitCommandID.Settle: // Draw recommended city locations
                    break;
                case UnitCommandID.MeleeAttack: // Draw adjacent tiles
                    foreach (Point nLoc in client.GetCachedTile(client.SelectedUnit.Location).GetNeighbourTileLocations())
                    {
                        tileHighlightSprite.Draw(sb, boardRenderer.GetTileRenderRect(nLoc), null, Color.Red);
                    }
                    break;
                case UnitCommandID.RangedAttack: // Draw all tiles in range
                    break;
            }
        }
    }
}
