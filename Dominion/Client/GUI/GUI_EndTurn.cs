// Dominion - Copyright (C) Timothy Ings
// GUI_EndTurn.cs
// This file defines classes that manage the end turn gui element

using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Microsoft.Xna.Framework;

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
            // format the button text based on the current turn state
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

        /// <summary>
        /// Closes the form
        /// </summary>
        public void Hide()
        {
            form.Visible = false;
        }

        /// <summary>
        /// Opens the form
        /// </summary>
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
            // try to advance the turn
            client.AdvanceTurn();
        }
    }
}
