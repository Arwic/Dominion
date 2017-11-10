// Dominion - Copyright (C) Timothy Ings
// GUI_UnitList.cs
// This file defines classes that manage the unit list gui elements

using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using ArwicEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using ArwicEngine;

namespace Dominion.Client.GUI
{
    public class GUI_SocialPolicy : IGUIElement
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

        private class PolicyTreeListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public ToolTip ToolTip { get; set; }
            public SocialPolicyTree SocialPolicyTree { get; set; }
            public int Index { get; }

            public PolicyTreeListItem(SocialPolicyTree tree, int index)
            {
                Index = index;
                SocialPolicyTree = tree;
                Text = tree.Name.ToRichText();
                ToolTip = new ToolTip(tree.Description.ToRichText(), 400);
            }

            public void OnDraw(object sender, DrawEventArgs e)
            {
                // TODO: Add cutom draw with icon
                //Text.Draw(e.SpriteBatch, new Vector2(Button.AbsoluteLocation.X, Button.AbsoluteLocation.Y));
            }
        }

        private FormConfig formConfig;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        private Sprite policyLockedSprite;
        private Sprite policyUnlockedSprite;
        private Sprite policyAdoptable;
        private int selectedIndex = 0;
        private List<Line> lines;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }

        public GUI_SocialPolicy(Client client, SceneGame sceneGame, Canvas canvas)
        {
            // load the form config
            formConfig = FormConfig.FromStream(Engine.Instance.Content.GetAsset<Stream>("Core:XML/Interface/Game/SocialPolicy"));
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;

            client.PlayerUpdated += (s, a) =>
            {
                if (Visible)
                    Show();
            };

            // load sprites
            policyLockedSprite = Engine.Instance.Content.GetAsset<Sprite>("Core:Textures/Interface/Tech_Locked");
            policyUnlockedSprite = Engine.Instance.Content.GetAsset<Sprite>("Core:Textures/Interface/Tech_Unlocked");
            policyAdoptable = Engine.Instance.Content.GetAsset<Sprite>("Core:Textures/Interface/Tech_Selectedable");
        }

        /// <summary>
        /// Opens the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                // setup the form
                sceneGame.HideForms();
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                form.Draggable = false;
                form.Drawn += Form_Drawn;
                form.CentreControl();
                form.Location = new Point(form.Location.X, 35);

                // get and setup the form elements
                ScrollBox sbSocialPolicyTrees = (ScrollBox)form.GetChildByName("sbSocialPolicyTrees");
                sbSocialPolicyTrees.SelectedIndex = selectedIndex;
                sbSocialPolicyTrees.Items = GetPolicyTreeListItems();
                sbSocialPolicyTrees.SelectedChanged += SbPolicyTrees_SelectedChanged;
                PolicyTreeListItem selectedItem = (PolicyTreeListItem)sbSocialPolicyTrees.Selected;

                lines = new List<Line>();

                int offsetX = sbSocialPolicyTrees.AbsoluteBounds.Width + 10;
                int offsetY = 50;
                int itemWidth = 200;
                int itemHeight = 70;
                int sepX = 70;
                int sepY = 50;

                // build a button for every node in the social policy tree
                foreach (SocialPolicy policy in client.Player.SocialPolicyInstance.GetAllSocialPoliciesInTree(selectedItem.SocialPolicyTree.ID))
                {
                    Rectangle dest = new Rectangle(offsetX + policy.GridX * (itemWidth + sepX), offsetY + policy.GridY * (itemHeight + sepY), itemWidth, itemHeight);
                    Button b = new Button(dest, form);
                    string text = policy.Name;
                    // format the button text
                    b.Text = text.ToRichText();
                    // pick an appropriate sprite
                    bool unlockable = true;
                    foreach (string prereqID in policy.Prerequisites)
                    {
                        SocialPolicy prereq = client.Player.SocialPolicyInstance.GetSocialPolicy(prereqID);
                        if (prereq != null && !prereq.Unlocked)
                            unlockable = false;
                    }
                    if (policy.Unlocked)
                        b.Sprite = policyUnlockedSprite;
                    else if (unlockable)
                        b.Sprite = policyAdoptable;
                    else
                        b.Sprite = policyLockedSprite;

                    string policyID = policy.ID; // cache id because of closure
                    b.MouseClick += (s, a) =>
                    {
                        // only tell the server to select a new tech if all the prereqs are unlocked
                        SocialPolicy clicked = client.DataManager.SocialPolicy.GetSocialPolicy(policyID);
                        foreach (string prereqID in clicked.Prerequisites)
                        {
                            SocialPolicy prereq = client.Player.SocialPolicyInstance.GetSocialPolicy(prereqID);
                            if (prereq != null && !prereq.Unlocked)
                                return;
                        }

                        client.CommandPlayer(new PlayerCommand(PlayerCommandID.UnlockPolicy, policyID));
                    };
                    // add a tool tip with more info about the policy
                    b.ToolTip = new ToolTip(policy.Description.ToRichText(), 500);
                    b.ToolTip.FollowCurosr = true;

                    // add a line from the current policy to all its prereqs
                    foreach (string prereqID in policy.Prerequisites)
                    {
                        SocialPolicy prereq = client.DataManager.SocialPolicy.GetSocialPolicy(prereqID);
                        if (prereq == null)
                            continue;
                        Rectangle prereqRect = new Rectangle(offsetX + prereq.GridX * (itemWidth + sepX), offsetY + prereq.GridY * (itemHeight + sepY), itemWidth, itemHeight);
                        lines.Add(new Line(new Vector2(form.AbsoluteLocation.X + dest.X, dest.Y + itemHeight / 2 + offsetY), new Vector2(form.AbsoluteLocation.X + prereqRect.X + itemWidth, prereqRect.Y + itemHeight / 2 + offsetY)));
                    }
                }
            }
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

        private void SbPolicyTrees_SelectedChanged(object sender, ListItemEventArgs e)
        {
            // save the selected index
            PolicyTreeListItem ptli = (PolicyTreeListItem)e.ListItem;
            selectedIndex = ptli.Index;
            // draw the policy tree

            // show the form
            Show();
        }

        // returns a list of all the social policy trees in the data manager as IListItems
        private List<IListItem> GetPolicyTreeListItems()
        {
            List<IListItem> res = new List<IListItem>();
            List<SocialPolicyTree> trees = (List<SocialPolicyTree>)client.DataManager.SocialPolicy.GetAllSocialPolicyTrees();
            trees = trees.OrderBy(o => o.ListIndex).ToList();
            int index = 0;
            foreach (SocialPolicyTree tree in trees)
                res.Add(new PolicyTreeListItem(tree, index++));
            return res;
        }

        // generates the social policy tree on the form
        private void GenerateTreeDisplay(string treeID)
        {
            int offsetX = 450;
            int offsetY = 300;
            int sepX = 100;
            int sepY = 75;
            int buttonWidth = 200;
            int buttonHeight = 200;

            foreach (SocialPolicy policy in client.DataManager.SocialPolicy.GetAllSocialPoliciesInTree(treeID))
            {
                Button btn = new Button(new Rectangle(offsetX + sepX * buttonWidth * policy.GridX, offsetY + sepY * buttonHeight * policy.GridY, buttonWidth, buttonHeight), form);
            }
        }

        /// <summary>
        /// Closes the gui elements
        /// </summary>
        public void Hide()
        {
            if (form != null)
                form.Visible = false;
        }
    }
}
