// Dominion - Copyright (C) Timothy Ings
// GUI_UnitActions.cs
// This file defines classes that manage the unit actions gui elements

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
        private List<UnitCommandID> commandIds;
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
            // show the form when the user selects a unit
            Show();
        }

        /// <summary>
        /// Shows the gui element
        /// </summary>
        public void Show()
        {
            lock (SceneGame._lock_guiDrawCall)
            {
                // hide other forms
                sceneGame.HideForms(true, false);

                // don't show try showing the form if no unit is selected
                if (client.SelectedUnit == null)
                {
                    Hide();
                    return;
                }

                // programatically build the form
                int height = 250;
                int width = 400;
                canvas.RemoveChild(form);
                form = new Form(new Rectangle(0, canvas.Bounds.Height - height, width, height), canvas);
                form.Draggable = false;
                form.CloseButtonEnabled = true;
                // format the form text
                form.Text = $"Unit - {client.EmpireManager.GetEmpire(client.Player.EmpireID).Adjective} {client.SelectedUnit.Name}".ToRichText();
                form.CloseButton.MouseClick += (s, a) =>
                {
                    // when the form is closed, deselect the selected unit
                    client.SelectedUnit = null;
                };

                // construct the command buttons based on the selected unit's abilities
                int xOffset = 0;
                int yOffset = 40;
                int btnWidth = 50;
                int btnHeight = 50;
                int maxPerLine = 5;
                int index = 0;
                commandIds = client.SelectedUnit.Commands;
                if (commandIds == null) // if the unit has no commands, don't try build any buttons
                    return;
                for (int i = 0; i < commandIds.Count; i++)
                {
                    // move the buttons to the next line if appropriate
                    if (i != 0 && i % maxPerLine == 0)
                    {
                        index = 0;
                        yOffset = 40 + btnHeight;
                    }
                    // build button
                    Button btnCmd = new Button(new Rectangle((xOffset + btnWidth) * index, yOffset, btnWidth, btnHeight), form);
                    btnCmd.Text = UnitCommand.GetCommandIcon(commandIds[i]);
                    btnCmd.ToolTip = new ToolTip(UnitCommand.FormatName((commandIds[i]).ToString()), 200);
                    int locali = i; // closure means we can't just use i
                    btnCmd.MouseClick += (s, a) =>
                    {
                        // execute the unit command
                        UnitCommandID cmdID = commandIds[locali];
                        ConsoleManager.Instance.WriteLine($"Select a new command, {cmdID}");
                        // if the command is instant, cast is instantly
                        if (UnitCommand.GetTargetType(cmdID) == UnitCommandTargetType.Instant)
                        {
                            // command the unit
                            if (client.SelectedUnit != null)
                                client.CommandUnit(new UnitCommand(cmdID, client.SelectedUnit, null));
                            if (cmdID == UnitCommandID.UNITCMD_DISBAND || cmdID == UnitCommandID.UNITCMD_SETTLE)
                            {
                                // disbanding and settling cause the unit to be removed, to close the form and deselect the unit
                                client.SelectedUnit = null;
                                Hide();
                            }
                        }
                        // otherwise select it
                        else
                        {
                            client.SelectedCommand = cmdID;
                        }
                    };
                    index++;
                }
            }
        }

        /// <summary>
        /// Clsoes the gui element
        /// </summary>
        public void Hide()
        {
            // deselect the selected unit
            client.SelectedUnit = null;

            // close th form
            if (form != null)
                form.Visible = false;
        }
    }
}
