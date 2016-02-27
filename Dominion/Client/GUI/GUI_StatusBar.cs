// Dominion - Copyright (C) Timothy Ings
// GUI_StatusBar.cs
// This file defines classes that manage the status bar gui elements

using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Microsoft.Xna.Framework;
using System.IO;
using System.Text;

namespace Dominion.Client.GUI
{
    public class GUI_StatusBar : IGUIElement
    {
        private Canvas canvas;
        private Form form;
        private FormConfig formConfig;
        private Label lblResources;
        private SceneGame sceneGame;
        private Client client;

        public GUI_StatusBar(Client client, SceneGame sceneGame, Canvas canvas)
        {
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;

            // get form config
            formConfig = FormConfig.FromStream(Engine.Instance.Content.GetAsset<Stream>("Core:XML/Interface/Game/StatusBar"));

            // register events
            client.PlayerUpdated += (s, a) => UpdateResourceLabel();

            Show();
        }

        /// <summary>
        /// Clsoes the gui element
        /// </summary>
        public void Hide()
        {
            form.Visible = false;
        }

        /// <summary>
        /// Opens the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                // setup the form
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                canvas.AlwayOnTop = form; // the status bar should always be on top

                int toolTipWidth = 250;

                // setup form elements
                Button btnMenu = (Button)form.GetChildByName("btnMenu");
                btnMenu.ToolTip = new ToolTip("Opens the game menu", toolTipWidth);
                btnMenu.MouseClick += (s, a) =>
                {
                    // toggle game menu visibility
                    if (sceneGame.GameMenu.Visible)
                        sceneGame.GameMenu.Hide();
                    else
                        sceneGame.GameMenu.Show();
                };

                Button btnTech = (Button)form.GetChildByName("btnTech");
                btnTech.ToolTip = new ToolTip("Opens the technology tree interface", toolTipWidth);
                btnTech.MouseClick += (s, a) =>
                {
                    // toggle tech tree visibility
                    if (sceneGame.TechTree.Visible)
                        sceneGame.TechTree.Hide();
                    else
                        sceneGame.TechTree.Show();
                };

                Button btnSocialPolicy = (Button)form.GetChildByName("btnSocialPolicy");
                btnSocialPolicy.ToolTip = new ToolTip("Opens the social policy interface", toolTipWidth);
                btnSocialPolicy.MouseClick += (s, a) =>
                {
                    // NYI
                };

                Button btnDiplomacy = (Button)form.GetChildByName("btnDiplomacy");
                btnDiplomacy.ToolTip = new ToolTip("Opens the diplomacy interface", toolTipWidth);
                btnDiplomacy.MouseClick += (s, a) =>
                {
                    // NYI
                };

                Button btnCityList = (Button)form.GetChildByName("btnCityList");
                btnCityList.ToolTip = new ToolTip("Opens a list of your empire's controlled cities", toolTipWidth);
                btnCityList.MouseClick += (s, a) =>
                {
                    // toggle city list visibility
                    if (sceneGame.CityList.Visible)
                        sceneGame.CityList.Hide();
                    else
                        sceneGame.CityList.Show();
                };

                Button btnUnitList = (Button)form.GetChildByName("btnUnitList");
                btnUnitList.ToolTip = new ToolTip("Opens a list of your empire's controlled units", toolTipWidth);
                btnUnitList.MouseClick += (s, a) =>
                {
                    // toggle unit list visibility
                    if (sceneGame.UnitList.Visible)
                        sceneGame.UnitList.Hide();
                    else
                        sceneGame.UnitList.Show();
                };

                lblResources = (Label)form.GetChildByName("lblResources");
                UpdateResourceLabel();
            }
        }

        // updates the resource labels
        private void UpdateResourceLabel()
        {
            if (lblResources == null)
                return;

            // build the label string
            StringBuilder sb = new StringBuilder();
            sb.Append($"$(science) +{client.Player.IncomeScience}  ");
            sb.Append($"$(gold) {client.Player.Gold} (+{client.Player.IncomeGold})   ");
            sb.Append($"$(happiness) {client.Player.Happiness}   ");
            sb.Append($"$(culture) {client.Player.Culture}/{"NYI"} (+{client.Player.IncomeCulture})   ");
            sb.Append($"$(tourism) +{client.Player.IncomeTourism}   ");
            sb.Append($"$(faith) {client.Player.Faith} (+{client.Player.IncomeFaith})   ");

            // these resources only appear if the player has some or is in need on some
            // this might cause issues when the player is using all of one resource
            //if (client.Player.Iron > 0)
            //    sb.Append($"$(res_iron) <[{Color.Green.ToRichFormat()}]{client.Player.Iron}>");
            //else if (client.Player.Iron < 0)
            //    sb.Append($"$(res_iron) <[{Color.Red.ToRichFormat()}]{client.Player.Iron}>");

            //if (client.Player.Horses > 0)
            //    sb.Append($"$(res_horses) <[{Color.Green.ToRichFormat()}]{client.Player.Horses}>");
            //else if (client.Player.Horses < 0)
            //    sb.Append($"$(res_horses) <[{Color.Red.ToRichFormat()}]{client.Player.Horses}>");

            //if (client.Player.Coal > 0)
            //    sb.Append($"$(res_coal) <[{Color.Green.ToRichFormat()}]{client.Player.Coal}>");
            //else if (client.Player.Coal < 0)
            //    sb.Append($"$(res_coal) <[{Color.Red.ToRichFormat()}]{client.Player.Coal}>");

            //if (client.Player.Oil > 0)
            //    sb.Append($"$(res_oil) <[{Color.Green.ToRichFormat()}]{client.Player.Oil}>");
            //else if (client.Player.Oil < 0)
            //    sb.Append($"$(res_oil) <[{Color.Red.ToRichFormat()}]{client.Player.Oil}>");

            //if (client.Player.Aluminium > 0)
            //    sb.Append($"$(res_aluminium) <[{Color.Green.ToRichFormat()}]{client.Player.Aluminium}>");
            //else if (client.Player.Aluminium < 0)
            //    sb.Append($"$(res_aluminium) <[{Color.Red.ToRichFormat()}]{client.Player.Aluminium}>");

            //if (client.Player.Uranium > 0)
            //    sb.Append($"$(res_uranium) <[{Color.Green.ToRichFormat()}]{client.Player.Uranium}>");
            //else if (client.Player.Uranium < 0)
            //    sb.Append($"$(res_uranium) <[{Color.Red.ToRichFormat()}]{client.Player.Uranium}>");

            // format the string
            lblResources.Text = sb.ToString().ToRichText();
        }
    }
}
