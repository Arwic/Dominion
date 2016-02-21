using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Scenes;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

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
        private List<IListItem> cityListItems;

        public GUI_GameMenu(Client client, SceneGame sceneGame, Canvas canvas)
        {
            formConfig = FormConfig.FromFile("Content/Interface/Game/GameMenu.xml");
            this.client = client;
            this.sceneGame = sceneGame;
            this.canvas = canvas;
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                sceneGame.HideForms();
                canvas.RemoveChild(form);
                form = new Form(formConfig, canvas);
                form.CentreControl();

                Button btnSaveGame = (Button)form.GetChildByName("btnSaveGame");

                Button btnLoadGame = (Button)form.GetChildByName("btnLoadGame");

                Button btnOptions = (Button)form.GetChildByName("btnOptions");

                Button btnMainMenu = (Button)form.GetChildByName("btnMainMenu");
                btnMainMenu.MouseClick += (s, a) =>
                {
                    client.Dissconnect();
                    SceneManager.Instance.ChangeScene(0);
                };

                Button btnReturnToGame = (Button)form.GetChildByName("btnReturnToGame");
                btnReturnToGame.MouseClick += (s, a) =>
                {
                    Hide();
                };

                Label lblEmpire = (Label)form.GetChildByName("lblEmpireValue");
                lblEmpire.Text = $"{client.EmpireFactory.GetEmpire(client.Player.EmpireID).Name}".ToRichText();

                Label lblWorldType = (Label)form.GetChildByName("lblWorldTypeValue");
                lblWorldType.Text = $"{client.LobbyState.WorldType.GetName()}".ToRichText();

                Label lblWorldSize = (Label)form.GetChildByName("lblWorldSizeValue");
                lblWorldSize.Text = $"{client.LobbyState.WorldSize.GetName()}".ToRichText();

                Label lblDifficulty = (Label)form.GetChildByName("lblDifficultyValue");
                lblDifficulty.Text = $"NYI".ToRichText();

                Label lblGameSpeed = (Label)form.GetChildByName("lblGameSpeedValue");
                lblGameSpeed.Text = $"{client.LobbyState.GameSpeed.ToString()}".ToRichText();

                Label lblPlayerCount = (Label)form.GetChildByName("lblPlayerCountValue");
                lblPlayerCount.Text = $"NYI".ToRichText();

            }
        }

        public void Hide()
        {
            if (form != null)
                form.Visible = false;
        }
    }
}
