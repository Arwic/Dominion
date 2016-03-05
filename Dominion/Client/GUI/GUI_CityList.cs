// Dominion - Copyright (C) Timothy Ings
// GUI_CityList.cs
// This file defines classes that manage the city list gui element

using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Dominion.Client.GUI
{
    public class GUI_CityList : IGUIElement
    {
        // class that represents a list item
        private class CityListitem : IListItem
        {
            public Button Button { get; set; }
            public ToolTip ToolTip { get; set; }
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

            public void OnDraw(object sender, DrawEventArgs e)
            {
            }
        }

        private FormConfig formConfig;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }
        private List<IListItem> cityListItems;

        public GUI_CityList(Client client, SceneGame sceneGame, Canvas canvas)
        {
            // load the form configs
            formConfig = FormConfig.FromStream(Engine.Instance.Content.GetAsset<Stream>("Core:XML/Interface/Game/CityList"));
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

        /// <summary>
        /// Opens the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                sceneGame.HideForms(); // hide other forms
                canvas.RemoveChild(form); // remove this form from the canvas
                form = new Form(formConfig, canvas); // re build the form from the config
                form.Location = new Point(0, 200); // position the form

                // retrieve form components and set them up
                ScrollBox sbCities = (ScrollBox)form.GetChildByName("sbCityList");
                sbCities.Items = cityListItems;
                sbCities.SelectedIndex = -1;
                sbCities.SelectedChanged += SbCities_SelectedChanged;
            }
        }

        private void SbCities_SelectedChanged(object sender, ListItemEventArgs e)
        {
            // when a list item is clicked in the scroll tell the client to select that city
            ScrollBox sb = (ScrollBox)sender;
            if (sb.SelectedIndex == -1)
                return;
            CityListitem item = (CityListitem)e.ListItem;
            client.SelectedCity = item.City;
            sb.SelectedIndex = -1; // remove the selection from the scroll box to create desired behaivour
        }

        /// <summary>
        /// Closes the gui element
        /// </summary>
        public void Hide()
        {
            if (form != null)
                form.Visible = false;
        }
    }
}
