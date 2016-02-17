using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Dominion.Client.Renderers;
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
    public class GUI_NamePlates : IGUIElement
    {
        private const int cityLabelWidth = 200;
        private const int cityLabelHeight = 30;
        private const int unitLabelHeight = 30;
        private const int unitLabelPadding = 5;

        private Engine engine;
        private Client client;
        private Canvas canvas;
        private Camera2 camera;
        private BoardRenderer boardRenderer;
        private Form form;
        private Sprite cityLabelSprite;
        private Sprite unitLabelSprite;

        public GUI_NamePlates(Engine engine, Client client, Canvas canvas, Camera2 camera, BoardRenderer boardRenderer)
        {
            this.engine = engine;
            this.client = client;
            this.canvas = canvas;
            this.camera = camera;
            this.boardRenderer = boardRenderer;
            LoadResources();
            Show();
        }

        private void LoadResources()
        {
            cityLabelSprite = new Sprite(engine.Content, "Graphics/Interface/Controls/Button");
            unitLabelSprite = new Sprite(engine.Content, "Graphics/Interface/Controls/ScrollBox_Back");
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                form = new Form(new Rectangle(), canvas);
                form.Drawn += Form_Drawn;

                canvas.SendToBack(form);
            }
        }

        public void Hide()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                form.Visible = false;
            }
        }

        private void Form_Drawn(object sender, DrawEventArgs e)
        {
            lock (Client._lock_cacheUpdate)
            {
                DrawCityNamePlates(e.SpriteBatch);
                DrawUnitNamePlates(e.SpriteBatch);
            }
        }

        private void DrawUnitNamePlates(SpriteBatch sb)
        {
            foreach (Unit unit in client.CachedUnits.ToArray())
            {
                Vector2 tileCentre = camera.ConvertWorldToScreen(boardRenderer.GetTileCentre(unit.Location));
                RichText label;
                if (unit.PlayerID == client.Player.InstanceID)
                    label = $"$(flag) <[{Color.PaleGreen.ToRichFormat()}]{unit.Name}>".ToRichText();
                else
                    label = $"$(flag) <[{Color.IndianRed.ToRichFormat()}]{unit.Name}>".ToRichText();
                Vector2 labelMeasure = label.Measure();
                Rectangle dest = new Rectangle((int)(tileCentre.X - labelMeasure.X / 2 - unitLabelPadding), (int)(tileCentre.Y - boardRenderer.TileHeight * camera.Zoom.Y), (int)labelMeasure.X + unitLabelPadding * 2, unitLabelHeight);
                unitLabelSprite.DrawNineCut(sb, dest);
                label.Draw(sb, new Vector2(dest.X + (int)labelMeasure.X / 2 - labelMeasure.X / 2 + unitLabelPadding, dest.Y + labelMeasure.Y / 4));

                double hpPercent = unit.HP / (double)unit.Constants.MaxHP;
                if (hpPercent < 1)
                {
                    Color hpBarColor = Color.Green;
                    if (hpPercent <= 0.5)
                        hpBarColor = Color.Yellow;
                    else if (hpPercent <= 0.25)
                        hpBarColor = Color.Red;
                    Vector2 pos0 = new Vector2(dest.X, dest.Y);
                    Vector2 pos1 = new Vector2(dest.X + (int)(dest.Width * hpPercent), dest.Y);
                    GraphicsHelper.DrawLine(sb, pos0, pos1, 5, hpBarColor);
                }
            }
        }

        private void DrawCityNamePlates(SpriteBatch sb)
        {
            foreach (City city in client.Cities.ToArray())
            {
                if (client.GetCachedTile(city.Location) == null)
                    continue;

                Vector2 tileCentre = camera.ConvertWorldToScreen(boardRenderer.GetTileCentre(city.Location));
                Rectangle dest = new Rectangle((int)(tileCentre.X - cityLabelWidth / 2), (int)(tileCentre.Y - boardRenderer.TileHeight * camera.Zoom.Y), cityLabelWidth, cityLabelHeight);
                cityLabelSprite.DrawNineCut(sb, dest);
                RichText label;
                if (city.IsCapital)
                    label = $"$(population) {city.Population} - $(capital) {city.Name}".ToRichText();
                else
                    label = $"$(population) {city.Population} - {city.Name}".ToRichText();
                Vector2 labelMeasure = label.Measure();
                label.Draw(sb, new Vector2(dest.X + cityLabelWidth / 2 - labelMeasure.X / 2, dest.Y + labelMeasure.Y / 4));

                double hpPercent = city.HP / (double)city.MaxHP;
                if (hpPercent < 1)
                {
                    Color hpBarColor = Color.Green;
                    if (hpPercent <= 0.5)
                        hpBarColor = Color.Yellow;
                    else if (hpPercent <= 0.25)
                        hpBarColor = Color.Red;
                    Vector2 pos0 = new Vector2(dest.X, dest.Y);
                    Vector2 pos1 = new Vector2(dest.X + (int)(dest.Width * hpPercent), dest.Y);
                    GraphicsHelper.DrawLine(sb, pos0, pos1, 5, hpBarColor);
                }
            }
        }
    }
}
