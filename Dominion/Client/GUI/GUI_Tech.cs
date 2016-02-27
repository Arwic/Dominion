// Dominion - Copyright (C) Timothy Ings
// GUI_Tech.cs
// This file defines classes that manage the tech tree gui elements

using ArwicEngine;
using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Dominion.Client.GUI
{
    public class GUI_Tech : IGUIElement
    {
        // defines a line that can be drawn
        private class Line
        {
            private Vector2 pos1, pos2;

            public Line(Vector2 p1, Vector2 p2)
            {
                pos1 = p1;
                pos2 = p2;
            }

            /// <summary>
            /// Draws the line
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="c"></param>
            public void Draw(SpriteBatch sb, Color c)
            {
                GraphicsHelper.DrawLine(sb, pos1, pos2, 2, c);
            }
        }

        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        private Sprite techLockedSprite;
        private Sprite techUnlockedSprite;
        private Sprite techSelectedSprite;
        private Sprite techSelectable;
        private int scrollIndex;
        private int maxScrollIndex;
        private List<Line> lines;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }

        public GUI_Tech(Client client, SceneGame sceneGame, Canvas canvas)
        {
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;

            // register events
            // only update the form if it is already open, so it doesn't open all the time
            client.PlayerUpdated += (s, a) =>
            {
                if (Visible)
                    Show();
            };
            client.CityUpdated += (s, a) =>
            {
                if (Visible)
                    Show();
            };
            client.CityAdded += (s, a) =>
            {
                if (Visible)
                    Show();
            };
            client.CityRemoved += (s, a) =>
            {
                if (Visible)
                    Show();
            };

            // calculate the max scroll index
            foreach (TechNode node in client.Player.TechTree.Nodes)
                if (node.GridX > maxScrollIndex)
                    maxScrollIndex = node.GridX;

            // load sprites
            techLockedSprite = Engine.Instance.Content.GetAsset<Sprite>(Constants.CONTROL_SCROLLBOX_BACK);
            techUnlockedSprite = Engine.Instance.Content.GetAsset<Sprite>(Constants.CONTROL_SCROLLBOX_BUTTON);
            techSelectedSprite = Engine.Instance.Content.GetAsset<Sprite>(Constants.CONTROL_FORM_BACK);
            techSelectable = Engine.Instance.Content.GetAsset<Sprite>(Constants.CONTROL_SCROLLBOX_BACK);
        }

        /// <summary>
        /// Opens the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                // programatically build the form
                sceneGame.HideForms();
                canvas.RemoveChild(form);
                form = new Form(new Rectangle(0, 40, 1920, 650), canvas);
                form.DrawTitlebar = false;
                form.Draggable = false;
                form.MouseWheel += Form_MouseWheel;
                form.Drawn += Form_Drawn;

                lines = new List<Line>();

                int offsetX = 10;
                int offsetY = 50;
                int itemWidth = 250;
                int itemHeight = 50;
                int sepX = 30;
                int sepY = 10;

                // build a button for every node in the tech tree
                for (int i = 0; i < client.Player.TechTree.Nodes.Count; i++)
                {
                    TechNode tn = client.Player.TechTree.GetNode(i);
                    if (tn == null)
                        continue;

                    Rectangle dest = new Rectangle(offsetX + tn.GridX * (itemWidth + sepX) + scrollIndex * (itemWidth + tn.GridX + sepX), offsetY + tn.GridY * (itemHeight + sepY), itemWidth, itemHeight);

                    Button b = new Button(dest, form);
                    int turnsLeft = GetTurnsUntilTech(tn);
                    string text = tn.Name;
                    if (!tn.Unlocked)
                    {
                        // -2 indicates the tech will take a very long time to research
                        if (turnsLeft == -2)
                            text = $"{tn.Name} - ~ turns";
                        else if (turnsLeft != -1)
                            text = $"{tn.Name} - {turnsLeft} turns";
                    }
                    // format the button text
                    b.Text = text.ToRichText();
                    // pick an appropriate sprite
                    if (tn.Unlocked)
                        b.Sprite = techUnlockedSprite;
                    else if (client.Player.SelectedTechNodeID == i)
                        b.Sprite = techSelectedSprite;
                    else
                        b.Sprite = techLockedSprite;

                    int locali = i; // cache i because of closure
                    b.MouseClick += (s, a) =>
                    {
                        // only tell the server to select a new tech if all the prereqs are unlocked
                        TechNode clicked = client.Player.TechTree.GetNode(locali);
                        foreach (int prereqID in clicked.Prerequisites)
                        {
                            TechNode prereq = client.Player.TechTree.GetNode(prereqID);
                            if (prereq != null && !prereq.Unlocked)
                            {
                                return;
                            }
                        }

                        client.CommandPlayer(new PlayerCommand(PlayerCommandID.SelectTech, locali));
                    };
                    // register events
                    b.MouseWheel += Form_MouseWheel;
                    // add a tool tip with more info about the tech
                    b.ToolTip = new ToolTip(tn.Description, 500);
                    b.ToolTip.FollowCurosr = true;

                    // add a line from the current tech to all its prereqs
                    for (int j = 0; j < tn.Prerequisites.Count; j++)
                    {
                        TechNode prereq = client.Player.TechTree.GetNode(tn.Prerequisites[j]);
                        if (prereq == null)
                            continue;
                        Rectangle prereqRect = new Rectangle(offsetX + prereq.GridX * (itemWidth + sepX) + scrollIndex * (itemWidth + prereq.GridX), offsetY + prereq.GridY * (itemHeight + sepY), itemWidth, itemHeight);
                        lines.Add(new Line(new Vector2(dest.X, dest.Y + itemHeight / 2 + offsetY), new Vector2(prereqRect.X + itemWidth + sepX * scrollIndex, prereqRect.Y + itemHeight / 2 + offsetY)));
                    }
                }
            }
        }

        // returns the number of turns required to research the given node
        private int GetTurnsUntilTech(TechNode node)
        {
            if (client.Player.IncomeScience == 0)
                return -1;

            int turnsLeft = -1;
            int currentProg = node.ResearchCost;
            currentProg -= node.Progress;
            while (currentProg >= 0)
            {
                currentProg -= client.Player.IncomeScience;
                turnsLeft++;
                if (turnsLeft > 999) // don't bother trying for too long
                    return -2;
            }

            return turnsLeft;
        }

        private void Form_Drawn(object sender, DrawEventArgs e)
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                // draw the lines
                foreach (Line line in lines)
                    line.Draw(e.SpriteBatch, Color.White);
            }
        }

        private void Form_MouseWheel(object sender, MouseEventArgs e)
        {
            // modify the scroll index
            if (e.Delta < 0)
            {
                scrollIndex--;
                if (scrollIndex < 0)
                    scrollIndex = 0;
            }
            else if (e.Delta > 0)
            {
                scrollIndex++;
                if (scrollIndex > maxScrollIndex)
                    scrollIndex = maxScrollIndex;
            }
            // refresh the form
            Show();
        }

        /// <summary>
        /// Closes the gui element
        /// </summary>
        public void Hide()
        {
            if (form != null)
                form.Visible = false;
        }
    }
}
