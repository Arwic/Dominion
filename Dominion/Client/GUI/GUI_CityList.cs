using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Dominion.Client.GUI
{
    public class GUI_CityList : IGUIElement
    {
        private class CityListitem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public City City { get; }

            public CityListitem(City city)
            {
                City = city;
                if (City.IsCapital)
                    Text = $"$(capital) {city.Population} - {city.Name}".ToRichText();
                else
                    Text = $"{city.Population} - {city.Name}".ToRichText();
            }
        }

        public Engine Engine { get; }
        private FormConfig formConfig;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }
        private List<IListItem> cityListItems;

        public GUI_CityList(Engine engine, Client client, SceneGame sceneGame, Canvas canvas)
        {
            formConfig = FormConfig.FromFile("Content/Interface/Game/CityList.xml");
            Engine = engine;
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;
            client.CitiesUpdated += Client_CitiesUpdated;
            client.CityUpdated += Client_CityUpdated;
            client.CityAdded += Client_CityUpdated;
            client.CityRemoved += Client_CityUpdated;
            RebuildCityList(client.GetMyCities());
        }

        private void Client_CityUpdated(object sender, CityEventArgs e)
        {
            RebuildCityList(client.GetMyCities());
            if (form != null && form.Visible)
                Show();
        }

        private void Client_CitiesUpdated(object sender, CityListEventArgs e)
        {
            RebuildCityList(client.GetMyCities());
            if (form != null && form.Visible)
                Show();
        }

        private void RebuildCityList(List<City> c)
        {
            cityListItems = new List<IListItem>();
            foreach (City city in c)
                cityListItems.Add(new CityListitem(city));
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                sceneGame.HideForms();
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                form.Location = new Point(0, 200);

                ScrollBox sbCities = (ScrollBox)form.GetChildByName("sbCityList");
                sbCities.Items = cityListItems;
                sbCities.SelectedIndex = -1;
                sbCities.SelectedChanged += SbCities_SelectedChanged;
            }
        }

        private void SbCities_SelectedChanged(object sender, ListItemEventArgs e)
        {
            ScrollBox sb = (ScrollBox)sender;
            if (sb.SelectedIndex == -1)
                return;
            CityListitem item = (CityListitem)e.ListItem;
            client.SelectedCity = item.City;
            sb.SelectedIndex = -1;
        }

        public void Hide()
        {
            if (form != null)
                form.Visible = false;
        }
    }
}
