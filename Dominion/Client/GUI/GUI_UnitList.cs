// Dominion - Copyright (C) Timothy Ings
// GUI_UnitList.cs
// This file defines classes that manage the unit list gui elements

using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Dominion.Client.GUI
{
    public class GUI_UnitList : IGUIElement
    {
        // defines a list item to hold a unit
        private class UnitListitem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }
            public Unit Unit { get; }

            public UnitListitem(Unit unit)
            {
                Unit = unit;
                Text = $"{unit.Name}".ToRichText();
            }
        }

        private FormConfig formConfig;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }
        private List<IListItem> unitListItems;

        public GUI_UnitList(Client client, SceneGame sceneGame, Canvas canvas)
        {
            // load the form config from file
            formConfig = FormConfig.FromFile("Content/Interface/Game/UnitList.xml");
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;
            client.UnitsUpdated += Client_UnitsUpdated;
            RebuildUnitList(client.GetMyUnits());
        }

        private void Client_UnitsUpdated(object sender, UnitListEventArgs e)
        {
            RebuildUnitList(client.GetMyUnits());
            if (form != null && form.Visible)
                Show();
        }

        // rebuilds the unit list
        private void RebuildUnitList(List<Unit> u)
        {
            unitListItems = new List<IListItem>();
            foreach (Unit unit in u)
                unitListItems.Add(new UnitListitem(unit));
        }

        /// <summary>
        /// Opens the gui elements
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                // setup the form
                sceneGame.HideForms();
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                form.Location = new Point(0, 200);

                // get and setup the form elements
                ScrollBox sbUnitList = (ScrollBox)form.GetChildByName("sbUnitList");
                sbUnitList.Items = unitListItems;
                sbUnitList.SelectedChanged += SbUnitList_SelectedChanged;
            }
        }

        private void SbUnitList_SelectedChanged(object sender, ListItemEventArgs e)
        {
            // select the clicked unit
            UnitListitem item = (UnitListitem)e.ListItem;
            client.SelectedUnit = item.Unit;
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
