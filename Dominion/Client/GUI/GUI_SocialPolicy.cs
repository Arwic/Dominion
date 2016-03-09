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

namespace Dominion.Client.GUI
{
    public class GUI_SocialPolicy : IGUIElement
    {
        private class StringListItem : IListItem
        {
            public Button Button { get; set; }

            public RichText Text { get; set; }

            public ToolTip ToolTip { get; set; }

            public string String { get; set; }

            public StringListItem(string str)
            {
                String = str;
                Text = str.ToRichText();
            }

            public void OnDraw(object sender, DrawEventArgs e)
            {
                throw new NotImplementedException();
            }
        }

        private FormConfig formConfig;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        private int selectedIndex = 0;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }

        public GUI_SocialPolicy(Client client, SceneGame sceneGame, Canvas canvas)
        {
            // load the form config
            formConfig = FormConfig.FromStream(Engine.Instance.Content.GetAsset<Stream>("Core:XML/Interface/Game/SocialPolicy"));
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;
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
                form.CentreControl();
                form.Location = new Point(form.Location.X, 35);

                // get and setup the form elements
                ScrollBox sbSocialPolicyTrees = (ScrollBox)form.GetChildByName("sbSocialPolicyTrees");
                sbSocialPolicyTrees.SelectedIndex = selectedIndex;
                sbSocialPolicyTrees.Items = GetPolicyTreeListItems();
                sbSocialPolicyTrees.SelectedChanged += SbPolicyTrees_SelectedChanged;

                if (sbSocialPolicyTrees.Items.Count > 0)
                    GenerateTreeDisplay(((StringListItem)sbSocialPolicyTrees.Selected).String);
            }
        }

        private void SbPolicyTrees_SelectedChanged(object sender, ListItemEventArgs e)
        {
            Show();
        }

        // returns a list of all the social policy trees in the data manager as IListItems
        private List<IListItem> GetPolicyTreeListItems()
        {
            List<IListItem> res = new List<IListItem>();
            foreach (string tree in client.DataManager.SocialPolicy.GetAllSocialPolicyTrees())
                res.Add(new StringListItem(tree));
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
