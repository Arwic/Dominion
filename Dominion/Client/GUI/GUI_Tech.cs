using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Client.GUI
{
    public class GUI_Tech : IGUIElement
    {
        private class Line
        {
            private Vector2 pos1, pos2;

            public Line(Vector2 p1, Vector2 p2)
            {
                pos1 = p1;
                pos2 = p2;
            }

            public void Draw(SpriteBatch sb, Color c)
            {
                GraphicsHelper.DrawLine(sb, pos1, pos2, 2, c);
            }
        }

        private Engine engine;
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

        public GUI_Tech(Engine engine, Client client, SceneGame sceneGame, Canvas canvas)
        {
            this.engine = engine;
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;

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

            foreach (TechNode node in client.Player.TechTree.Nodes)
                if (node.GridX > maxScrollIndex)
                    maxScrollIndex = node.GridX;

            techLockedSprite = new Sprite(engine.Content, "Graphics/Interface/Controls/ScrollBox_Back");
            techUnlockedSprite = new Sprite(engine.Content, "Graphics/Interface/Controls/ScrollBox_Button");
            techSelectedSprite = new Sprite(engine.Content, "Graphics/Interface/Controls/Form_Back");
            techSelectable = new Sprite(engine.Content, "Graphics/Interface/Controls/ScrollBox_Back");
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
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
                        if (turnsLeft == -2)
                            text = $"{tn.Name} - ~ turns";
                        else if (turnsLeft != -1)
                            text = $"{tn.Name} - {turnsLeft} turns";
                    }
                    b.Text = text.ToRichText();
                    if (tn.Unlocked)
                        b.Sprite = techUnlockedSprite;
                    else if (client.Player.SelectedTechNodeID == i)
                        b.Sprite = techSelectedSprite;
                    else
                        b.Sprite = techLockedSprite;

                    int locali = i;
                    b.MouseClick += (s, a) =>
                    {
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
                    b.MouseWheel += Form_MouseWheel;
                    b.ToolTip = new ToolTip(tn.Description, 500);
                    b.ToolTip.FollowCurosr = true;

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
                if (turnsLeft > 999)
                    return -2;
            }

            return turnsLeft;
        }

        private void Form_Drawn(object sender, DrawEventArgs e)
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                foreach (Line line in lines)
                    line.Draw(e.SpriteBatch, Color.White);
            }
        }

        private void Form_MouseWheel(object sender, MouseEventArgs e)
        {
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

            Show();
        }

        public void Hide()
        {
            if (form != null)
                form.Visible = false;
        }
    }
}
