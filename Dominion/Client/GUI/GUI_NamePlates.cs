// Dominion - Copyright (C) Timothy Ings
// GUI_NamePlates.cs
// This file defines classes that manage the name plates gui elements

using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Dominion.Client.Renderers;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dominion.Client.GUI
{
    public class GUI_NamePlates : IGUIElement
    {
        private const int cityLabelWidth = 200;
        private const int cityLabelHeight = 30;
        private const int unitLabelHeight = 30;
        private const int unitLabelPadding = 5;

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
            cityLabelSprite = new Sprite("Graphics/Interface/Controls/Button");
            unitLabelSprite = new Sprite("Graphics/Interface/Controls/ScrollBox_Back");
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
            foreach (Unit unit in client.CachedUnits.ToArray())
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
                double hpPercent = unit.HP / (double)unit.Constants.MaxHP;
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

                // position the name plate
                Vector2 tileCentre = camera.ConvertWorldToScreen(boardRenderer.GetTileCentre(city.Location));
                Rectangle dest = new Rectangle((int)(tileCentre.X - cityLabelWidth / 2), (int)(tileCentre.Y - boardRenderer.TileHeight * camera.Zoom.Y), cityLabelWidth, cityLabelHeight);

                // format the text on the name plate
                RichText label;
                if (city.IsCapital)
                    label = $"$(population) {city.Population} - $(capital) {city.Name}".ToRichText();
                else
                    label = $"$(population) {city.Population} - {city.Name}".ToRichText();
                Vector2 labelMeasure = label.Measure();

                // draw the name plate
                cityLabelSprite.DrawNineCut(sb, dest);
                label.Draw(sb, new Vector2(dest.X + cityLabelWidth / 2 - labelMeasure.X / 2, dest.Y + labelMeasure.Y / 4));

                // draw the hp bar above the name plate
                double hpPercent = city.HP / (double)city.MaxHP;
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
    }
}
