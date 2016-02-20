using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Client.GUI
{
    public class GUI_UnitList : IGUIElement
    {
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

        public Engine Engine { get; }
        private FormConfig formConfig;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }
        private List<IListItem> unitListItems;

        public GUI_UnitList(Engine engine, Client client, SceneGame sceneGame, Canvas canvas)
        {
            formConfig = FormConfig.FromFile("Content/Interface/Game/UnitList.xml");
            Engine = engine;
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

        private void RebuildUnitList(List<Unit> u)
        {
            unitListItems = new List<IListItem>();
            foreach (Unit unit in u)
                unitListItems.Add(new UnitListitem(unit));
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                sceneGame.HideForms();
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                form.Location = new Point(0, 200);

                ScrollBox sbUnitList = (ScrollBox)form.GetChildByName("sbUnitList");
                sbUnitList.Items = unitListItems;
                sbUnitList.SelectedChanged += SbUnitList_SelectedChanged;
            }
        }

        private void SbUnitList_SelectedChanged(object sender, ListItemEventArgs e)
        {
            UnitListitem item = (UnitListitem)e.ListItem;
            client.SelectedUnit = item.Unit;
        }

        public void Hide()
        {
            if (form != null)
                form.Visible = false;
        }
    }
}
