// Dominion - Copyright (C) Timothy Ings
// GUI_NamePlates.cs
// This file defines classes that manage the name plates gui elements

using ArwicEngine;
using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Dominion.Client.Renderers;
using Dominion.Client.Scenes;
using Dominion.Common.Data;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Dominion.Client.GUI
{
    public class GUI_NamePlates : IGUIElement
    {
        private const int cityLabelWidth = 200;
        private const int cityLabelHeight = 35;
        private const int unitLabelHeight = 30;
        private const int unitLabelPadding = 5;

        private const int cityDefensePlateWidth = 55;
        private const int cityDefensePlateHeight = 30;
        private const int cityPadding = 2;
        private const int cityPopBarPosX = 30;
        private const int cityPopBarWidth = 3;
        private const int cityProdBarWidth = 3;
        private const float citySubTextScale = 0.75f;

        private Client client;
        private Canvas canvas;
        private Camera2 camera;
        private BoardRenderer boardRenderer;
        private Form form;
        private Sprite cityLabelSprite;
        private Sprite unitLabelSprite;

        public GUI_NamePlates(Client client, Canvas canvas, Camera2 camera, BoardRenderer boardRenderer)
        {
            this.client = client;
            this.canvas = canvas;
            this.camera = camera;
            this.boardRenderer = boardRenderer;
            LoadResources();
            Show();
        }

        private void LoadResources()
        {
            // load sprites
            cityLabelSprite = Engine.Instance.Content.GetAsset<Sprite>(Constants.ASSET_CONTROL_BUTTON);
            unitLabelSprite = Engine.Instance.Content.GetAsset<Sprite>(Constants.ASSET_CONTROL_SCROLLBOX_BACK);
        }

        /// <summary>
        /// Opens the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                form = new Form(new Rectangle(), canvas);
                form.Drawn += Form_Drawn;

                canvas.SendToBack(form);
            }
        }

        /// <summary>
        /// Closes the gui element
        /// </summary>
        public void Hide()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                form.Visible = false;
            }
        }

        // occurs when the form is drawn
        private void Form_Drawn(object sender, DrawEventArgs e)
        {
            lock (Client._lock_cacheUpdate)
            {
                // draw the name plates
                DrawCityNamePlates(e.SpriteBatch);
                DrawUnitNamePlates(e.SpriteBatch);
            }
        }

        // draws unit name plates
        private void DrawUnitNamePlates(SpriteBatch sb)
        {
            // draw a name plate for every unit the client can see
            foreach (UnitInstance unit in client.CachedUnits.ToArray())
            {
                // darken the nameplate of the player's units if they have no moves/actions left
                Color color = Color.White;
                if (unit.PlayerID == client.Player.InstanceID && unit.Movement == 0)
                    color = Color.Lerp(color, Color.Black, 0.8f);

                // format the text on the name plate
                RichText label;
                if (unit.PlayerID == client.Player.InstanceID)
                    label = $"$(flag) <[{Color.PaleGreen.ToRichFormat()}]{unit.Name}>".ToRichText();
                else
                    label = $"$(flag) <[{Color.IndianRed.ToRichFormat()}]{unit.Name}>".ToRichText();
                Vector2 labelMeasure = label.Measure();

                // position the name plate
                Vector2 tileCentre = camera.ConvertWorldToScreen(boardRenderer.GetTileCentre(unit.Location));
                Rectangle dest = new Rectangle((int)(tileCentre.X - labelMeasure.X / 2 - unitLabelPadding), (int)(tileCentre.Y - boardRenderer.TileHeight * camera.Zoom.Y), (int)labelMeasure.X + unitLabelPadding * 2, unitLabelHeight);
                
                // draw the name plate
                unitLabelSprite.DrawNineCut(sb, dest, null, color);
                label.Draw(sb, new Vector2(dest.X + (int)labelMeasure.X / 2 - labelMeasure.X / 2 + unitLabelPadding, dest.Y + labelMeasure.Y / 4));

                // draw the hp bar above the nameplate
                double hpPercent = unit.HP / (double)unit.BaseUnit.MaxHP;
                if (hpPercent < 1)
                {
                    // colour the bar based on current hp
                    Color hpBarColor = Color.Green;
                    if (hpPercent <= 0.5)
                        hpBarColor = Color.Yellow;
                    else if (hpPercent <= 0.25)
                        hpBarColor = Color.Red;
                    // calculate the bar pos/length based on current hp
                    Vector2 pos0 = new Vector2(dest.X, dest.Y);
                    Vector2 pos1 = new Vector2(dest.X + (int)(dest.Width * hpPercent), dest.Y);
                    // draw the hp bar
                    GraphicsHelper.DrawLine(sb, pos0, pos1, 5, hpBarColor);
                }
            }
        }

        // draws city name plates
        private void DrawCityNamePlates(SpriteBatch sb)
        {
            // draw a nameplate for every city the client can see
            foreach (City city in client.Cities.ToArray())
            {
                // if the city is not on a chached tile, dont draw its name plate
                if (client.GetCachedTile(city.Location) == null)
                    continue;
                
                // get the city name
                RichText lblCityName;
                if (city.IsCapital)
                    lblCityName = $"$(capital) {city.Name}".ToRichText();
                else
                    lblCityName = $"{city.Name}".ToRichText();
                Vector2 lblCityNameMeasure = lblCityName.Measure();

                // position the name plate
                Vector2 tileCentre = camera.ConvertWorldToScreen(boardRenderer.GetTileCentre(city.Location));
                Rectangle dest = new Rectangle((int)(tileCentre.X - cityLabelWidth / 2), (int)(tileCentre.Y - boardRenderer.TileHeight * camera.Zoom.Y), (int)(cityLabelWidth + lblCityNameMeasure.X / 2), cityLabelHeight);
                Rectangle defDest = new Rectangle(dest.X + dest.Width / 2 - cityDefensePlateWidth / 2, dest.Y - cityDefensePlateHeight, cityDefensePlateWidth, cityDefensePlateHeight);

                // draw the name plate
                cityLabelSprite.DrawNineCut(sb, dest);
                // draw the defense plate
                cityLabelSprite.DrawNineCut(sb, defDest);

                // draw population value
                RichText lblPop = $"{city.Population}".ToRichText();
                Vector2 lblPopMeasure = lblPop.Measure();
                lblPop.Draw(sb, new Vector2(dest.X + cityPadding + 5, dest.Y + cityPadding + lblPopMeasure.Y / 4));

                // draw population bar
                float popProgress = city.PopulationGorwthProgress;
                // clamp progress between 0% and 100%
                if (popProgress > 1f)
                    popProgress = 1f;
                if (popProgress < 0f)
                    popProgress = 0f;
                int popProgBarHeight = (int)Math.Round(dest.Height * popProgress) - cityPadding * 2;
                Rectangle popProgBar = new Rectangle(dest.X + cityPopBarPosX, dest.Y + dest.Height - cityPadding - popProgBarHeight, cityPopBarWidth, popProgBarHeight);
                // draw bar back and bar
                GraphicsHelper.DrawRectFill(sb, new Rectangle(popProgBar.X, dest.Y + cityPadding, popProgBar.Width, dest.Height - cityPadding * 2), Color.DarkGray);
                GraphicsHelper.DrawRectFill(sb, popProgBar, Color.Green);

                // draw turns until pop growth value
                string lblTurnsUntilPopGrowthString = $"{city.TurnsUntilPopulationGrowth}";
                if (city.TurnsUntilPopulationGrowth == -2) // -2 means indefinite
                    lblTurnsUntilPopGrowthString = "-";
                RichText lblTurnsUntilPopGrowth = lblTurnsUntilPopGrowthString.ToRichText();
                lblTurnsUntilPopGrowth.Sections.First().Scale = citySubTextScale;
                lblTurnsUntilPopGrowth.Draw(sb, new Vector2(popProgBar.X + popProgBar.Width, popProgBar.Y + popProgBar.Height - lblTurnsUntilPopGrowth.Measure().Y * citySubTextScale - cityPadding));

                // draw the city name
                lblCityName.Draw(sb, new Vector2(dest.X + dest.Width / 2 - lblCityNameMeasure.X / 2, dest.Y + lblCityNameMeasure.Y / 4));

                // draw the city defense value
                RichText lblDefense = $"$(strength) {city.Defense}".ToRichText();
                Vector2 lblDefenseMeasure = lblDefense.Measure();
                lblDefense.Draw(sb, new Vector2(defDest.X + defDest.Width / 2 - lblDefenseMeasure.X / 2, defDest.Y + lblDefenseMeasure.Y / 4));

                if (city.ProductionQueue.Count > 0)
                {
                    Production curProd = city.ProductionQueue.First.Value;

                    // draw current production icon
                    Sprite sprite = GetProductionIcon(curProd);
                    Rectangle prodIconDest = new Rectangle(dest.X + dest.Width - dest.Height, dest.Y, dest.Height, dest.Height);
                    sprite.Draw(sb, prodIconDest);

                    // draw current production progress bar
                    float prodProgress = city.PopulationGorwthProgress;
                    // clamp progress between 0% and 100%
                    if (prodProgress > 1f)
                        prodProgress = 1f;
                    if (prodProgress < 0f)
                        prodProgress = 0f;
                    int prodProgBarHeight = (int)Math.Round(dest.Height * prodProgress) - cityPadding * 2;

                    Rectangle prodProgBar = new Rectangle(prodIconDest.X - cityProdBarWidth, dest.Y + dest.Height - cityPadding - prodProgBarHeight, 3, prodProgBarHeight);
                    // draw bar back and bar
                    GraphicsHelper.DrawRectFill(sb, new Rectangle(prodProgBar.X, dest.Y + cityPadding, prodProgBar.Width, dest.Height - cityPadding * 2), Color.DarkGray);
                    GraphicsHelper.DrawRectFill(sb, prodProgBar, Color.Orange);

                    // draw turns until prod done value
                    string lblTurnsUntilProdDoneString = $"{GetTurnsToProduce(city, curProd)}";
                    if (city.TurnsUntilPopulationGrowth == -2) // -2 means indefinite
                        lblTurnsUntilPopGrowthString = "-";
                    RichText lblTurnsUntilProdDone = lblTurnsUntilProdDoneString.ToRichText();
                    lblTurnsUntilProdDone.Sections.First().Scale = citySubTextScale;
                    Vector2 lblTurnsUntilProdDoneMeasure = lblTurnsUntilProdDone.Measure();
                    lblTurnsUntilProdDone.Draw(sb, new Vector2(prodProgBar.X - prodProgBar.Width - lblTurnsUntilProdDoneMeasure.X + cityPadding, prodProgBar.Y + prodProgBar.Height - lblTurnsUntilProdDoneMeasure.Y * citySubTextScale - cityPadding));
                }

                // draw the hp bar above the name plate
                double hpPercent = city.HP / (double)city.MaxHP;
                if (true || hpPercent < 1)
                {
                    int barThickness = 4;
                    // colour the bar based on current hp
                    Color hpBarColor = Color.Green;
                    if (hpPercent <= 0.5)
                        hpBarColor = Color.Yellow;
                    else if (hpPercent <= 0.25)
                        hpBarColor = Color.Red;
                    // calculate the bar dest based on current hp
                    Rectangle hpBarBackDest = new Rectangle(dest.X, dest.Y - barThickness / 2, dest.Width, barThickness);
                    Rectangle hpBarDest = new Rectangle(dest.X, dest.Y - barThickness / 2, (int)(dest.Width * hpPercent), barThickness);
                    // draw the hp bar
                    GraphicsHelper.DrawRectFill(sb, hpBarBackDest, Color.DarkGray);
                    GraphicsHelper.DrawRectFill(sb, hpBarDest, hpBarColor);
                }
            }
        }

        // returns the number of turns required to produce the given production at the given city
        private int GetTurnsToProduce(City city, Production prod)
        {
            int prodIncome = city.GetProductionIncome(prod, client.DataManager.Building, client.DataManager.Unit);
            int prodRequired = -1;
            switch (prod.ProductionType)
            {
                case ProductionType.BUILDING:
                    Building b = client.DataManager.Building.GetBuilding(prod.Name);
                    prodRequired = b.Cost;
                    break;
                case ProductionType.UNIT:
                    Unit u = client.DataManager.Unit.GetUnit(prod.Name);
                    prodRequired = u.Cost;
                    break;
            }

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
            //ConsoleManager.Instance.WriteLine($"Production: {prod.Name} progress {prod.Progress}/{prodRequired + prodIncome * requiredTurns} (+{prodIncome}) done in {requiredTurns} turns");
            return requiredTurns;
        }

        // returns the sprite for the given production
        private Sprite GetProductionIcon(Production prod)
        {
            switch (prod.ProductionType)
            {
                case ProductionType.UNIT:
                    Unit unit = client.DataManager.Unit.GetUnit(prod.Name);
                    SpriteAtlas unitAtlas = Engine.Instance.Content.GetAsset<SpriteAtlas>(unit.IconAtlas);
                    return new Sprite(unitAtlas, unit.IconKey);
                case ProductionType.BUILDING:
                    Building building = client.DataManager.Building.GetBuilding(prod.Name);
                    SpriteAtlas buildingAtlas = Engine.Instance.Content.GetAsset<SpriteAtlas>(building.IconAtlas);
                    return new Sprite(buildingAtlas, building.IconKey);
            }
            return null;
        }
    }
}
