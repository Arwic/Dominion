using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Dominion.Common.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Client.GUI
{
    public class GUI_CityManagment : IGUIElement
    {
        private class ProductionListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public Production Production { get; set; }

            public ProductionListItem(Production prod, UnitFactory unitFactory, BuildingFactory buildingFactory, int turnsLeft)
            {
                Production = prod;
                string suffix = "";
                if (turnsLeft != -1)
                    suffix = $" - {turnsLeft} turns";
                switch (Production.ResultType)
                {
                    case ProductionResultType.Building:
                        Building building = buildingFactory.GetBuilding(Production.ResultID);
                        Text = $"{building.Name}{suffix}".ToRichText();
                        break;
                    case ProductionResultType.Unit:
                        UnitTemplate unit = unitFactory.GetUnit(Production.ResultID);
                        Text = $"{unit.Name}{suffix}".ToRichText();
                        break;
                }
            }
        }

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

        private Engine engine;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        //private FormConfig formConfig;
        private FormConfig frmFocusConfig;
        private FormConfig frmProductionConfig;
        private FormConfig frmStatsConfig;
        private FormConfig frmReturnBuyConfig;
        //private Form form;
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

        public GUI_CityManagment(Engine engine, Client client, SceneGame sceneGame, Canvas canvas)
        {
            frmFocusConfig = FormConfig.FromFile("Content/Interface/Game/City_FocusPane.xml");
            frmProductionConfig = FormConfig.FromFile("Content/Interface/Game/City_ProductionPane.xml");
            frmStatsConfig = FormConfig.FromFile("Content/Interface/Game/City_StatsPane.xml");
            frmReturnBuyConfig = FormConfig.FromFile("Content/Interface/Game/City_ReturnBuyPane.xml");
            this.engine = engine;
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;
            client.SelectedCityChnaged += (s, a) => Show();
            client.CityUpdated += (s, a) => Show();
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                sceneGame.HideForms(false);

                if (client.SelectedCity == null)
                    return;

                int yOffset = 40;

                canvas.RemoveChild(frmStats);
                frmStats = new Form(frmStatsConfig, canvas);
                frmStats.Location = new Point(0, yOffset);

                canvas.RemoveChild(frmProduction);
                frmProduction = new Form(frmProductionConfig, canvas);
                frmProduction.Location = new Point(0, 1080 - frmProduction.Size.Height);

                canvas.RemoveChild(frmFocus);
                frmFocus = new Form(frmFocusConfig, canvas);
                frmFocus.Location = new Point(1920 - frmFocus.Size.Width, yOffset);

                canvas.RemoveChild(frmReturnBuy);
                frmReturnBuy = new Form(frmReturnBuyConfig, canvas);
                frmReturnBuy.CentreControl();
                frmReturnBuy.Location = new Point(frmReturnBuy.Location.X, 1080 - 100 - frmReturnBuy.Size.Height);
                
                // Stats 
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

                // Production
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

                // Focus
                sbCitizenFocus = (ScrollBox)frmFocus.GetChildByName("sbCitizenFocus");
                sbCitizenFocus.Items = GetCitizenFocusListItems();
                sbCitizenFocus.SelectedIndex = (int)client.SelectedCity.CitizenFocus;
                sbCitizenFocus.SelectedChanged += SbCitizenFocus_SelectedChanged;
                sbBuildingList = (ScrollBox)frmFocus.GetChildByName("sbBuildingList");
                sbBuildingList.Items = GetBuildingList();
                sbBuildingList.SelectedIndex = 0;
                Button btnDemolish = (Button)frmFocus.GetChildByName("btnDemolish");
                btnDemolish.MouseClick += BtnDemolish_MouseClick;

                // Return/Buy
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
            client.CommandCity(new CityCommand(CityCommandID.ChangeCitizenFocus, client.SelectedCity, (CityCitizenFocus)((IntListItem)sbCitizenFocus.Selected).Int));
        }

        private void TbName_EnterPressed(object sender, EventArgs e)
        {
            TextBox tbName = (TextBox)sender;
            client.CommandCity(new CityCommand(CityCommandID.Rename, client.SelectedCity, tbName.Text));
        }

        private void BtnQueueProduction_MouseClick(object sender, MouseEventArgs e)
        {
            if (sbProductionList.Selected == null)
                return;
            client.CommandCity(new CityCommand(CityCommandID.QueueProduction, client.SelectedCity, ((ProductionListItem)sbProductionList.Selected).Production.ID));
        }

        private void BtnChangeProduction_MouseClick(object sender, MouseEventArgs e)
        {
            if (sbProductionList.Selected == null)
                return;
            client.CommandCity(new CityCommand(CityCommandID.ChangeProduction, client.SelectedCity, ((ProductionListItem)sbProductionList.Selected).Production.ID));
        }

        private void BtnMoveDown_MouseClick(object sender, MouseEventArgs e)
        {
            if (sbProductionQueue.SelectedIndex == client.SelectedCity.ProductionQueue.Count - 1 || client.SelectedCity.ProductionQueue.Count == 0 || client.SelectedCity.ProductionQueue.Count == 1)
                return;

            client.CommandCity(new CityCommand(CityCommandID.ReorderProductionMoveDown, client.SelectedCity, sbProductionQueue.SelectedIndex));

            sbProductionQueue.SelectedIndex++;
            if (sbProductionQueue.SelectedIndex >= sbProductionQueue.Items.Count)
                sbProductionQueue.SelectedIndex = sbProductionQueue.Items.Count - 1;
        }

        private void BtnMoveUp_MouseClick(object sender, MouseEventArgs e)
        {
            if (sbProductionQueue.SelectedIndex == 0 || client.SelectedCity.ProductionQueue.Count == 0 || client.SelectedCity.ProductionQueue.Count == 1)
                return;

            client.CommandCity(new CityCommand(CityCommandID.ReorderProductionMoveUp, client.SelectedCity, sbProductionQueue.SelectedIndex));

            sbProductionQueue.SelectedIndex--;
            if (sbProductionQueue.SelectedIndex < 0)
                sbProductionQueue.SelectedIndex = 0;
        }

        private void BtnCancelProduction_MouseClick(object sender, MouseEventArgs e)
        {
            client.CommandCity(new CityCommand(CityCommandID.CancelProduction, client.SelectedCity, sbProductionQueue.SelectedIndex));

            client.SelectedCity.ProductionQueue.Remove(sbProductionQueue.SelectedIndex);
            sbProductionQueue.SelectedIndex--;
            if (sbProductionQueue.SelectedIndex < 0)
                sbProductionQueue.SelectedIndex = 0;
        }

        private List<IListItem> GetProductionListListItems()
        {
            List<IListItem> items = new List<IListItem>();
            foreach (Production prod in client.SelectedCity.GetProductionList(client.ProductionFactory, client.Player, client.Player.TechTree))
                items.Add(new ProductionListItem(prod, client.UnitFactory, client.BuildingFactory, GetTurnsToProduce(client.SelectedCity, prod)));
            return items;
        }

        private List<IListItem> GetProductionQueueListItems()
        {
            List<IListItem> items = new List<IListItem>();
            foreach (Production prod in client.SelectedCity.ProductionQueue)
                items.Add(new ProductionListItem(prod, client.UnitFactory, client.BuildingFactory, GetTurnsToProduce(client.SelectedCity, prod)));
            return items;
        }

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

        private List<IListItem> GetBuildingList()
        {
            List<IListItem> items = new List<IListItem>();
            foreach (int buildingID in client.SelectedCity.Buildings)
            {
                Building building = client.BuildingFactory.GetBuilding(buildingID);
                items.Add(new IntListItem($"{building.Name}".ToRichText(), buildingID));
            }
            return items;
        }

        private int GetTurnsToProduce(City city, Production prod)
        {
            int prodIncome = city.IncomeProduction;
            int prodRequired = prod.ProductionCost;
            prodRequired -= prod.Progress;
            int requiredTurns = 0;
            while (prodRequired >= 0)
            {
                prodRequired -= prodIncome;
                requiredTurns++;
            }
            return requiredTurns;
        }

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
