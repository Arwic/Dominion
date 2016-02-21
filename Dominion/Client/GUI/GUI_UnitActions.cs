using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using Dominion.Client.Renderers;
using Dominion.Client.Scenes;
using Dominion.Common.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Dominion.Client.GUI
{
    public class GUI_UnitActions : IGUIElement
    {
        private Form form;
        private Canvas canvas;
        private SceneGame sceneGame;
        private Client client;
        private List<int> commandIds;
        private Camera2 camera;
        private BoardRenderer boardRenderer;

        public GUI_UnitActions(Client client, SceneGame sceneGame, Camera2 camera, BoardRenderer boardRenderer, Canvas canvas)
        {
            this.client = client;
            this.sceneGame = sceneGame;
            this.camera = camera;
            this.canvas = canvas;
            this.boardRenderer = boardRenderer;
            client.SelectedUnitChnaged += Client_SelectedUnitChnaged;
        }

        private void Client_SelectedUnitChnaged(object sender, UnitEventArgs e)
        {
            Show();
        }

        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                sceneGame.HideForms(true, false);

                if (client.SelectedUnit == null)
                {
                    Hide();
                    return;
                }

                int height = 250;
                int width = 400;
                canvas.RemoveChild(form);
                form = new Form(new Rectangle(0, canvas.Bounds.Height - height, width, height), canvas);
                form.Draggable = false;
                form.CloseButtonEnabled = true;
                form.Text = $"Unit - {client.EmpireFactory.GetEmpire(client.Player.EmpireID).Adjective} {client.SelectedUnit.Name}".ToRichText();
                form.CloseButton.MouseClick += (s, a) =>
                {
                    client.SelectedUnit = null;
                };

                int xOffset = 0;
                int yOffset = 40;
                int btnWidth = 50;
                int btnHeight = 50;
                int index = 0;
                commandIds = client.SelectedUnit.Commands;
                if (commandIds == null)
                    return;
                for (int i = 0; i < commandIds.Count; i++)
                {
                    if (i == 5)
                    {
                        index = 0;
                        yOffset = 40 + btnHeight;
                    }
                    Button btnCmd = new Button(new Rectangle((xOffset + btnWidth) * index, yOffset, btnWidth, btnHeight), form);
                    btnCmd.Text = UnitCommand.GetCommandIcon((UnitCommandID)commandIds[i]);
                    btnCmd.ToolTip = new ToolTip(((UnitCommandID)commandIds[i]).ToString(), 200);
                    int locali = i;
                    btnCmd.MouseClick += (s, a) =>
                    {
                        UnitCommandID cmdID = (UnitCommandID)commandIds[locali];
                        ConsoleManager.Instance.WriteLine($"Select a new command, {cmdID}");
                        if (UnitCommand.GetTargetType(cmdID) == UnitCommandTargetType.Instant)
                        {
                            if (client.SelectedUnit != null)
                                client.CommandUnit(new UnitCommand(cmdID, client.SelectedUnit, null));
                            if (cmdID == UnitCommandID.Disband || cmdID == UnitCommandID.Settle)
                            {
                                client.SelectedUnit = null;
                                Hide();
                            }
                        }
                        else
                        {
                            client.SelectedCommand = cmdID;
                        }
                    };
                    index++;
                }
            }
        }

        public void Hide()
        {
            client.SelectedUnit = null;
            if (form != null)
                form.Visible = false;
        }
    }
}
