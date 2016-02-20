using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Microsoft.Xna.Framework;
using System;

namespace Dominion.Client.GUI
{
    public class GUI_StatusBar : IGUIElement
    {
        private Engine engine;
        private Canvas canvas;
        private Form form;
        private FormConfig formConfig;
        private Label lblResources;
        private SceneGame sceneGame;
        private Client client;

        public GUI_StatusBar(Engine engine, Client client, SceneGame sceneGame, Canvas canvas)
        {
            this.engine = engine;
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;

            formConfig = FormConfig.FromFile("Content/Interface/Game/StatusBar.xml");

            client.PlayerUpdated += (s, a) => UpdateResourceLabel();

            Show();
        }

        public void Hide()
        {
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                canvas.AlwayOnTop = form;

                int toolTipWidth = 250;

                Button btnMenu = (Button)form.GetChildByName("btnMenu");
                btnMenu.ToolTip = new ToolTip("Opens the game menu", toolTipWidth);
                btnMenu.MouseClick += (s, a) =>
                {
                    if (sceneGame.GameMenu.Visible)
                        sceneGame.GameMenu.Hide();
                    else
                        sceneGame.GameMenu.Show();
                };

                Button btnTech = (Button)form.GetChildByName("btnTech");
                btnTech.ToolTip = new ToolTip("Opens the technology tree interface", toolTipWidth);
                btnTech.MouseClick += (s, a) =>
                {
                    if (sceneGame.TechTree.Visible)
                        sceneGame.TechTree.Hide();
                    else
                        sceneGame.TechTree.Show();
                };

                Button btnSocialPolicy = (Button)form.GetChildByName("btnSocialPolicy");
                btnSocialPolicy.ToolTip = new ToolTip("Opens the social policy interface", toolTipWidth);

                Button btnDiplomacy = (Button)form.GetChildByName("btnDiplomacy");
                btnDiplomacy.ToolTip = new ToolTip("Opens the diplomacy interface", toolTipWidth);

                Button btnCityList = (Button)form.GetChildByName("btnCityList");
                btnCityList.ToolTip = new ToolTip("Opens a list of your empire's controlled cities", toolTipWidth);
                btnCityList.MouseClick += (s, a) =>
                {
                    if (sceneGame.CityList.Visible)
                        sceneGame.CityList.Hide();
                    else
                        sceneGame.CityList.Show();
                };

                Button btnUnitList = (Button)form.GetChildByName("btnUnitList");
                btnUnitList.ToolTip = new ToolTip("Opens a list of your empire's controlled units", toolTipWidth);
                btnUnitList.MouseClick += (s, a) =>
                {
                    if (sceneGame.UnitList.Visible)
                        sceneGame.UnitList.Hide();
                    else
                        sceneGame.UnitList.Show();
                };

                lblResources = (Label)form.GetChildByName("lblResources");
                UpdateResourceLabel();
            }
        }

        private void UpdateResourceLabel()
        {
            if (lblResources == null)
                return;

            string rawText = "";

            rawText += $"$(science) +{client.Player.IncomeScience}  ";
            rawText += $"$(gold) {client.Player.Gold} (+{client.Player.IncomeGold})   ";
            rawText += $"$(happiness) {client.Player.Happiness}   ";
            rawText += $"$(culture) {client.Player.Culture}/{"NYI"} (+{client.Player.IncomeCulture})   ";
            rawText += $"$(tourism) +{client.Player.IncomeTourism}   ";
            rawText += $"$(faith) {client.Player.Faith} (+{client.Player.IncomeFaith})   ";

            if (client.Player.Iron > 0)
                rawText += $"$(res_iron) <[{Color.Green.ToRichFormat()}]{client.Player.Iron}>";
            else if (client.Player.Iron < 0)
                rawText += $"$(res_iron) <[{Color.Red.ToRichFormat()}]{client.Player.Iron}>";

            if (client.Player.Horses > 0)
                rawText += $"$(res_horses) <[{Color.Green.ToRichFormat()}]{client.Player.Horses}>";
            else if (client.Player.Horses < 0)
                rawText += $"$(res_horses) <[{Color.Red.ToRichFormat()}]{client.Player.Horses}>";

            if (client.Player.Coal > 0)
                rawText += $"$(res_coal) <[{Color.Green.ToRichFormat()}]{client.Player.Coal}>";
            else if (client.Player.Coal < 0)
                rawText += $"$(res_coal) <[{Color.Red.ToRichFormat()}]{client.Player.Coal}>";

            if (client.Player.Oil > 0)
                rawText += $"$(res_oil) <[{Color.Green.ToRichFormat()}]{client.Player.Oil}>";
            else if (client.Player.Oil < 0)
                rawText += $"$(res_oil) <[{Color.Red.ToRichFormat()}]{client.Player.Oil}>";

            if (client.Player.Aluminium > 0)
                rawText += $"$(res_aluminium) <[{Color.Green.ToRichFormat()}]{client.Player.Aluminium}>";
            else if (client.Player.Aluminium < 0)
                rawText += $"$(res_aluminium) <[{Color.Red.ToRichFormat()}]{client.Player.Aluminium}>";

            if (client.Player.Uranium > 0)
                rawText += $"$(res_uranium) <[{Color.Green.ToRichFormat()}]{client.Player.Uranium}>";
            else if (client.Player.Uranium < 0)
                rawText += $"$(res_uranium) <[{Color.Red.ToRichFormat()}]{client.Player.Uranium}>";

            lblResources.Text = rawText.ToRichText();
        }

        private void Form_Updated(object sender, EventArgs e)
        {
        }
    }
}
