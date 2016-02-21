using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Client.GUI
{
    class GUI_EndTurn : IGUIElement
    {
        private Canvas canvas;
        private Client client;
        private TurnState turnState;
        private Form form;
        private Button btnEndTurn;

        public GUI_EndTurn(Client client, Canvas canvas)
        {
            this.canvas = canvas;
            this.client = client;
            turnState = TurnState.Begin;
            client.TurnStateChanged += Client_TurnStateChanged;
            Show();
        }

        private void Client_TurnStateChanged(object sender, TurnStateEventArgs e)
        {
            turnState = e.TurnState;
            switch (turnState)
            {
                case TurnState.Begin:
                    btnEndTurn.Text = "End Turn".ToRichText();
                    break;
                case TurnState.ChooseResearch:
                    btnEndTurn.Text = "Choose Research".ToRichText();
                    break;
                case TurnState.ChooseSocialPolicy:
                    btnEndTurn.Text = "Choose Social Policy".ToRichText();
                    break;
                case TurnState.ChooseProduction:
                    btnEndTurn.Text = "Choose Production".ToRichText();
                    break;
                case TurnState.UnitOrders:
                    btnEndTurn.Text = "Order Unit".ToRichText();
                    break;
                case TurnState.WaitingForPlayers:
                    btnEndTurn.Text = "Waiting for players".ToRichText();
                    break;
                default:
                    break;
            }
        }

        public void Hide()
        {
            form.Visible = false;
        }

        public void Show()
        {
            lock (Scenes.SceneGame._lock_guiDrawCall)
            {
                canvas.RemoveChild(form);
                int width = 350;
                int height = 50;
                int xOffset = -20;
                int yOffset = -220;
                form = new Form(new Rectangle(GraphicsManager.Instance.Viewport.Bounds.Width - width + xOffset, GraphicsManager.Instance.Viewport.Bounds.Height - height + yOffset, width, height), canvas);
                form.CloseButtonEnabled = false;
                form.DrawTitlebar = false;
                form.Draggable = false;
                form.Visible = true;

                btnEndTurn = new Button(new Rectangle(0, 0, form.Size.Width, form.Size.Height), form);
                btnEndTurn.Text = "End Turn".ToRichText();
                btnEndTurn.MouseClick += BtnEndTurn_MouseClick;
            }
        }

        private void BtnEndTurn_MouseClick(object sender, MouseEventArgs e)
        {
            client.AdvanceTurn();
        }
    }
}
