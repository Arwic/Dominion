// Dominion - Copyright (C) Timothy Ings
// ConsoleForm.cs
// This file contains classes that define a standard console form

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class ConsoleForm : Form
    {
        public ConsoleManager ConsoleManager { get; set; }

        private TextLog messageLog;
        private TextBox commandBox;
        private List<string> commandHistory = new List<string>();
        private int historyIndex = 0;

        public ConsoleForm(ConsoleManager cm, Control parent = null)
            : base(new Rectangle(0, 0, cm.Engine.Graphics.Viewport.Width, Math.Min(500, cm.Engine.Graphics.Viewport.Height)), parent)
        {
            ConsoleManager = cm;

            Text = $"{ENGINE_NAME} - {ConsoleManager.Engine.Version}".ToRichText();
            Draggable = false;
            Visible = false;
            DrawTitlebar = false;
            CloseButtonEnabled = false;
            HotKey = '`';
            if (parent != null && parent as Canvas != null)
                (parent as Canvas).AlwayOnTop = this;

            Font = new Font(ConsoleManager.Engine.Content, FONT_CONSOLAS_PATH);

            int commandBoxHeight = 30;
            commandBox = new TextBox(new Rectangle(0, Bounds.Height - commandBoxHeight, Bounds.Width, commandBoxHeight), this);
            commandBox.Font = Font;
            commandBox.EnterPressed += CommandBox_EnterPressed;

            messageLog = new TextLog(new Rectangle(0, 0, Bounds.Width, Bounds.Height - commandBox.Bounds.Height), this);
            messageLog.Font = Font;
            foreach (string line in cm.Lines.ToArray())
                messageLog.WriteLine(new RichText(line, Color, Font));
            messageLog.WriteLine(new RichText(new RichTextSection("---- End of backlog ----", Color.Red)));

            EventInput.KeyUp += EventInput_KeyUp;
            VisibleChanged += Form_VisibleChanged;
            ConsoleManager.LineWritten += ConsoleManager_LineWritten;
            ConsoleManager.ClearLines += ConsoleManager_ClearLines;
        }

        private void Form_VisibleChanged(object sender, EventArgs e)
        {
            commandBox.Selected = false;
        }
        private void EventInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (Visible)
            {
                if (e.KeyCode == Keys.Down)
                {
                    if (commandHistory.Count > 0)
                    {
                        historyIndex++;
                        if (historyIndex >= commandHistory.Count)
                            historyIndex = 0;
                        commandBox.Text = commandHistory[historyIndex];
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    historyIndex--;
                    if (historyIndex < 0)
                    {
                        historyIndex = commandHistory.Count - 1;
                        if (historyIndex == -1)
                            historyIndex = 0;
                    }
                    try { commandBox.Text = commandHistory[historyIndex]; }
                    catch { }
                }
            }
        }
        private void CommandBox_EnterPressed(object sender, EventArgs e)
        {
            if (Visible && commandBox.Text.Trim() != "")
            {
                commandHistory.Add(commandBox.Text);
                ConsoleManager.RunCommand(commandBox.Text);
                commandBox.Text = "";
                historyIndex = 0;
            }
        }

        private void ConsoleManager_ClearLines(object sender, EventArgs e)
        {
            for (int i = 0; i < messageLog.Lines.Length; i++)
            {
                messageLog.RemoveLine(i);
            }
        }

        private void ConsoleManager_LineWritten(object sender, TextLogLineEventArgs e)
        {
            foreach (RichTextSection section in e.TextLogLine.Sections)
                section.Font = Font;
            messageLog.WriteLine(e.TextLogLine);
        }

        public void Update(float delta)
        {
            commandBox.Selected = Visible;
        }
    }
}
