// Dominion - Copyright (C) Timothy Ings
// GUI_GameMenu.cs
// This file defines classes that manage the game menu gui elements

using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Scenes;
using Dominion.Client.Scenes;

namespace Dominion.Client.GUI
{
    public class GUI_GameMenu : IGUIElement
    {
        private FormConfig formConfig;
        private Client client;
        private SceneGame sceneGame;
        private Canvas canvas;
        private Form form;
        public bool Visible { get { if (form != null) return form.Visible; else return false; } }

        public GUI_GameMenu(Client client, SceneGame sceneGame, Canvas canvas)
        {
            // load the form from file
            formConfig = FormConfig.FromFile("Content/Interface/Game/GameMenu.xml");
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;
        }

        /// <summary>
        /// Shows the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                // setup the form and centre it
                sceneGame.HideForms();
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                form.CentreControl();

                // get and setup the for elements
                Button btnSaveGame = (Button)form.GetChildByName("btnSaveGame"); // NYI

                Button btnLoadGame = (Button)form.GetChildByName("btnLoadGame"); // NYI

                Button btnOptions = (Button)form.GetChildByName("btnOptions"); // NYI

                Button btnMainMenu = (Button)form.GetChildByName("btnMainMenu");
                btnMainMenu.MouseClick += (s, a) =>
                {
                    // TODO add a confirmation dialogue
                    client.Dissconnect();
                    SceneManager.Instance.ChangeScene(0);
                };

                Button btnReturnToGame = (Button)form.GetChildByName("btnReturnToGame");
                btnReturnToGame.MouseClick += (s, a) =>
                {
                    Hide();
                };

                Label lblEmpire = (Label)form.GetChildByName("lblEmpireValue");
                lblEmpire.Text = client.Player.EmpireID.ToRichText();

                Label lblWorldType = (Label)form.GetChildByName("lblWorldTypeValue");
                lblWorldType.Text = client.LobbyState.WorldType.GetName().ToRichText();

                Label lblWorldSize = (Label)form.GetChildByName("lblWorldSizeValue");
                lblWorldSize.Text = client.LobbyState.WorldSize.GetName().ToRichText();

                Label lblDifficulty = (Label)form.GetChildByName("lblDifficultyValue");
                lblDifficulty.Text = "NYI".ToRichText();

                Label lblGameSpeed = (Label)form.GetChildByName("lblGameSpeedValue");
                lblGameSpeed.Text = client.LobbyState.GameSpeed.ToString().ToRichText();

                Label lblPlayerCount = (Label)form.GetChildByName("lblPlayerCountValue");
                lblPlayerCount.Text = "NYI".ToRichText();

            }
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
