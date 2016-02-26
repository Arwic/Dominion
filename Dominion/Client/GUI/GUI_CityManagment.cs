// Dominion - Copyright (C) Timothy Ings
// GUI_CityManagment.cs
// This file defines classes that manage the city managment gui elements

using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Client.GUI
{
    public class GUI_CityManagment : IGUIElement
    {
        // defines list items that hold city productions
        private class ProductionListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public Production Production { get; set; }

            public ProductionListItem(Production prod, int turnsLeft)
            {
                Production = prod;

                // format the items text to show construction information
                string suffix = "";
                if (turnsLeft != -1)
                    suffix = $" - {turnsLeft} turns";
                Text = $"{Production.Name}{suffix}".ToRichText();
            }
        }

        // defines a list item that holds an int, used mainly for city focus enum
        private class IntListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public int Int { get; set; }

            public IntListItem(RichText text, int i)
            {
                Text = text;
                Int = i;
            }
        }

        // defines a list item that holds a string, used mainly for building list
        private class StringListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public string String { get; set; }

            public StringListItem(RichText text, string s)
            {
                Text = text;
                String = s;
            }
        }

        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private FormConfig frmFocusConfig;
        private FormConfig frmProductionConfig;
        private FormConfig frmStatsConfig;
        private FormConfig frmReturnBuyConfig;
        private Form frmFocus;
        private Form frmProduction;
        private Form frmStats;
        private Form frmReturnBuy;
        private ScrollBox sbProductionQueue;
        private ScrollBox sbProductionList;
        private ScrollBox sbCitizenFocus;
        private ScrollBox sbBuildingList;
        private int sbProductionQueueSelected;
        private int sbProductionListSelected;
        private bool buyingTiles;

        public GUI_CityManagment(Client client, SceneGame sceneGame, Canvas canvas)
        {
            // load configs from file
            frmFocusConfig = FormConfig.FromFile("Content/Interface/Game/City_FocusPane.xml");
            frmProductionConfig = FormConfig.FromFile("Content/Interface/Game/City_ProductionPane.xml");
            frmStatsConfig = FormConfig.FromFile("Content/Interface/Game/City_StatsPane.xml");
            frmReturnBuyConfig = FormConfig.FromFile("Content/Interface/Game/City_ReturnBuyPane.xml");
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;
            // register events
            client.SelectedCityChnaged += (s, a) => Show();
            client.CityUpdated += (s, a) => Show();
        }

        /// <summary>
        /// opens the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                sceneGame.HideForms(false); // hide other forms

                if (client.SelectedCity == null) // dont't continue if there is no selected city
                    return;

                int yOffset = 40;

                // setup the stats form
                canvas.RemoveChild(frmStats);
                frmStats = new Form(frmStatsConfig, canvas);
                frmStats.Location = new Point(0, yOffset);

                // setup the production list/selection form
                canvas.RemoveChild(frmProduction);
                frmProduction = new Form(frmProductionConfig, canvas);
                frmProduction.Location = new Point(0, 1080 - frmProduction.Size.Height);

                // setup the citizen focus form
                canvas.RemoveChild(frmFocus);
                frmFocus = new Form(frmFocusConfig, canvas);
                frmFocus.Location = new Point(1920 - frmFocus.Size.Width, yOffset);

                // setup the form in the bottom middle of the screen that allows the user to leave the city screen
                canvas.RemoveChild(frmReturnBuy);
                frmReturnBuy = new Form(frmReturnBuyConfig, canvas);
                frmReturnBuy.CentreControl();
                frmReturnBuy.Location = new Point(frmReturnBuy.Location.X, 1080 - 100 - frmReturnBuy.Size.Height);
                
                // get and format the stats form elements
                Label lblPopulationValue = (Label)frmStats.GetChildByName("lblPopulationValue");
                lblPopulationValue.Text = $"{client.SelectedCity.Population}".ToRichText();
                Label lblPopGrowthValue = (Label)frmStats.GetChildByName("lblPopGrowthValue");
                int turnsUntilGrowth = client.SelectedCity.TurnsUntilPopulationGrowth;
                if (turnsUntilGrowth == -1) lblPopGrowthValue.Text = $"Inf".ToRichText();
                else if (turnsUntilGrowth == -2) lblPopGrowthValue.Text = $"~".ToRichText();
                else lblPopGrowthValue.Text = $"{turnsUntilGrowth}".ToRichText();
                Label lblFoodValue = (Label)frmStats.GetChildByName("lblFoodValue");
                lblFoodValue.Text = $"+{client.SelectedCity.IncomeFood}".ToRichText();
                Label lblProductionValue = (Label)frmStats.GetChildByName("lblProductionValue");
                lblProductionValue.Text = $"+{client.SelectedCity.IncomeProduction}".ToRichText();
                Label lblGoldValue = (Label)frmStats.GetChildByName("lblGoldValue");
                lblGoldValue.Text = $"+{client.SelectedCity.IncomeGold}".ToRichText();
                Label lblScienceValue = (Label)frmStats.GetChildByName("lblScienceValue");
                lblScienceValue.Text = $"+{client.SelectedCity.IncomeScience}".ToRichText();
                Label lblFaithValue = (Label)frmStats.GetChildByName("lblFaithValue");
                lblFaithValue.Text = $"+{client.SelectedCity.IncomeFaith}".ToRichText();
                Label lblTourismValue = (Label)frmStats.GetChildByName("lblTourismValue");
                lblTourismValue.Text = $"+{client.SelectedCity.IncomeTourism}".ToRichText();
                Label lblCultureValue = (Label)frmStats.GetChildByName("lblCultureValue");
                lblCultureValue.Text = $"+{client.SelectedCity.IncomeCulture}".ToRichText();
                Label lblBorderGrowthValue = (Label)frmStats.GetChildByName("lblBorderGrowthValue");
                lblBorderGrowthValue.Text = $"+{client.SelectedCity.IncomeCulture}".ToRichText();
                int turnsUntilBorderGrowth = client.SelectedCity.TurnsUntilBorderGrowth;
                if (turnsUntilBorderGrowth == -1) lblBorderGrowthValue.Text = $"Inf".ToRichText();
                else if (turnsUntilBorderGrowth == -2) lblBorderGrowthValue.Text = $"~".ToRichText();
                else lblBorderGrowthValue.Text = $"{turnsUntilBorderGrowth}".ToRichText();
                TextBox tbName = (TextBox)frmStats.GetChildByName("tbName");
                tbName.Text = client.SelectedCity.Name;
                tbName.EnterPressed += TbName_EnterPressed;

                // get and setup the production form elements
                sbProductionQueue = (ScrollBox)frmProduction.GetChildByName("sbProductionQueue");
                sbProductionQueue.Items = GetProductionQueueListItems();
                sbProductionQueue.SelectedIndex = sbProductionQueueSelected;
                sbProductionQueue.SelectedChanged += (s, a) => sbProductionQueueSelected = sbProductionQueue.SelectedIndex;
                sbProductionList = (ScrollBox)frmProduction.GetChildByName("sbProductionList");
                sbProductionList.Items = GetProductionListListItems();
                sbProductionList.SelectedIndex = sbProductionListSelected;
                sbProductionList.SelectedChanged += (s, a) => sbProductionListSelected = sbProductionList.SelectedIndex;
                Button btnCancelProduction = (Button)frmProduction.GetChildByName("btnCancelProduction");
                btnCancelProduction.MouseClick += BtnCancelProduction_MouseClick;
                Button btnMoveUp = (Button)frmProduction.GetChildByName("btnMoveUp");
                btnMoveUp.MouseClick += BtnMoveUp_MouseClick;
                Button btnMoveDown = (Button)frmProduction.GetChildByName("btnMoveDown");
                btnMoveDown.MouseClick += BtnMoveDown_MouseClick;
                Button btnChangeProduction = (Button)frmProduction.GetChildByName("btnChangeProduction");
                btnChangeProduction.MouseClick += BtnChangeProduction_MouseClick;
                Button btnQueueProduction = (Button)frmProduction.GetChildByName("btnQueueProduction");
                btnQueueProduction.MouseClick += BtnQueueProduction_MouseClick;

                // get and setup the citizen focus form elements
                sbCitizenFocus = (ScrollBox)frmFocus.GetChildByName("sbCitizenFocus");
                sbCitizenFocus.Items = GetCitizenFocusListItems();
                sbCitizenFocus.SelectedIndex = (int)client.SelectedCity.CitizenFocus;
                sbCitizenFocus.SelectedChanged += SbCitizenFocus_SelectedChanged;
                sbBuildingList = (ScrollBox)frmFocus.GetChildByName("sbBuildingList");
                sbBuildingList.Items = GetBuildingList();
                sbBuildingList.SelectedIndex = 0;
                Button btnDemolish = (Button)frmFocus.GetChildByName("btnDemolish");
                btnDemolish.MouseClick += BtnDemolish_MouseClick;

                // get and setup the return/buy tiles form elements
                Button btnBuyTile = (Button)frmReturnBuy.GetChildByName("btnBuyTile");
                btnBuyTile.MouseClick += (s, a) => buyingTiles = true;
                Button btnReturnToMap = (Button)frmReturnBuy.GetChildByName("btnReturnToMap");
                btnReturnToMap.MouseClick += (s, a) => Hide();
            }
        }

        private void BtnDemolish_MouseClick(object sender, MouseEventArgs e)
        {
            // TODO city command to demolish the selected building
        }

        private void SbCitizenFocus_SelectedChanged(object sender, ListItemEventArgs e)
        {
            // tell the server to select a new citizen focus for this city
            client.CommandCity(new CityCommand(CityCommandID.ChangeCitizenFocus, client.SelectedCity, (CityCitizenFocus)((IntListItem)sbCitizenFocus.Selected).Int));
        }

        private void TbName_EnterPressed(object sender, EventArgs e)
        {
            // tell the server to rename this city
            TextBox tbName = (TextBox)sender;
            client.CommandCity(new CityCommand(CityCommandID.Rename, client.SelectedCity, tbName.Text));
        }

        private void BtnQueueProduction_MouseClick(object sender, MouseEventArgs e)
        {
            if (sbProductionList.Selected == null) // don't contact the server if no production is selected
                return;
            // tell the server to change this city's production
            client.CommandCity(new CityCommand(CityCommandID.QueueProduction, client.SelectedCity, ((ProductionListItem)sbProductionList.Selected).Production.Name));
        }

        private void BtnChangeProduction_MouseClick(object sender, MouseEventArgs e)
        {
            if (sbProductionList.Selected == null)// don't contact the server if no current production is selected
                return;
            // tell the server to cancel the selected production
            client.CommandCity(new CityCommand(CityCommandID.ChangeProduction, client.SelectedCity, ((ProductionListItem)sbProductionList.Selected).Production.Name));
        }

        private void BtnMoveDown_MouseClick(object sender, MouseEventArgs e)
        {
            // don't contact the server if no production is selected
            if (sbProductionQueue.SelectedIndex == client.SelectedCity.ProductionQueue.Count - 1 || client.SelectedCity.ProductionQueue.Count == 0 || client.SelectedCity.ProductionQueue.Count == 1)
                return;

            // tell the server to move the selected production down the queue
            client.CommandCity(new CityCommand(CityCommandID.ReorderProductionMoveDown, client.SelectedCity, sbProductionQueue.SelectedIndex));

            // keep the production we just moved selected
            sbProductionQueue.SelectedIndex++;
            if (sbProductionQueue.SelectedIndex >= sbProductionQueue.Items.Count)
                sbProductionQueue.SelectedIndex = sbProductionQueue.Items.Count - 1;
        }

        private void BtnMoveUp_MouseClick(object sender, MouseEventArgs e)
        {
            // don't contact the server if no production is selected
            if (sbProductionQueue.SelectedIndex == 0 || client.SelectedCity.ProductionQueue.Count == 0 || client.SelectedCity.ProductionQueue.Count == 1)
                return;

            // tell the server to move the selected production up the queue
            client.CommandCity(new CityCommand(CityCommandID.ReorderProductionMoveUp, client.SelectedCity, sbProductionQueue.SelectedIndex));

            // keep the production we just moved selected
            sbProductionQueue.SelectedIndex--;
            if (sbProductionQueue.SelectedIndex < 0)
                sbProductionQueue.SelectedIndex = 0;
        }

        private void BtnCancelProduction_MouseClick(object sender, MouseEventArgs e)
        {
            // don't contact the server if there is no selected production
            if (sbProductionQueue.SelectedIndex == -1)
                return;

            // tell the server to cancel the selected production
            client.CommandCity(new CityCommand(CityCommandID.CancelProduction, client.SelectedCity, sbProductionQueue.SelectedIndex));
        }

        // returns formatted list items from the selected city's production list
        private List<IListItem> GetProductionListListItems()
        {
            List<IListItem> items = new List<IListItem>();
            foreach (Production prod in client.SelectedCity.PossibleProductions)
                items.Add(new ProductionListItem(prod, GetTurnsToProduce(client.SelectedCity, prod)));
            return items;
        }

        // returns formatted list items from the selected city's production queue
        private List<IListItem> GetProductionQueueListItems()
        {
            List<IListItem> items = new List<IListItem>();
            foreach (Production prod in client.SelectedCity.ProductionQueue)
                items.Add(new ProductionListItem(prod, GetTurnsToProduce(client.SelectedCity, prod)));
            return items;
        }

        // returns formatted list items that are valid choices for citizen focus
        private List<IListItem> GetCitizenFocusListItems()
        {
            List<IListItem> items = new List<IListItem>();
            int i = 0;
            items.Add(new IntListItem("$(book) Default Focus".ToRichText(), i++));
            items.Add(new IntListItem("$(food) Food Focus".ToRichText(), i++));
            items.Add(new IntListItem("$(production) Production Focus".ToRichText(), i++));
            items.Add(new IntListItem("$(gold) Gold Focus".ToRichText(), i++));
            items.Add(new IntListItem("$(science) Science Focus".ToRichText(), i++));
            items.Add(new IntListItem("$(culture) Culture Focus".ToRichText(), i++));
            return items;
        }

        // returns formatted list items from the selected city's building list
        private List<IListItem> GetBuildingList()
        {
            List<IListItem> items = new List<IListItem>();
            foreach (string buildingID in client.SelectedCity.Buildings)
            {
                Building building = client.BuildingManager.GetBuilding(buildingID);
                items.Add(new StringListItem($"{building.Name}".ToRichText(), buildingID));
            }
            return items;
        }

        // returns the number of turns required to produce the given production at the given city
        private int GetTurnsToProduce(City city, Production prod)
        {
            Building b = client.BuildingManager.GetBuilding(prod.Name);

            int prodIncome = city.IncomeProduction;
            int prodRequired = b.Cost;

            prodRequired -= prod.Progress;
            int requiredTurns = 0;
            int maxLoops = 300;
            while (prodRequired >= 0)
            {
                prodRequired -= prodIncome;
                requiredTurns++;
                if (requiredTurns > maxLoops)
                    return -2;
            }
            return requiredTurns;
        }

        /// <summary>
        /// Closes the gui element
        /// </summary>
        public void Hide()
        {
            client.SelectedCity = null;
            if (frmFocus != null)
                canvas.RemoveChild(frmFocus);
            if (frmProduction != null)
                canvas.RemoveChild(frmProduction);
            if (frmStats != null)
                canvas.RemoveChild(frmStats);
            if (frmReturnBuy != null)
                canvas.RemoveChild(frmReturnBuy);
        }
    }
}
