using ArwicEngine.Core;
using ArwicEngine.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace ArwicInterfaceDesigner
{
    public partial class MainForm : Form
    {
        public static List<ArwicEngine.Forms.RichTextParseRule> RichTextRules = new List<ArwicEngine.Forms.RichTextParseRule>()
        {
            new ArwicEngine.Forms.RichTextParseRule("population", ((char)ArwicEngine.Forms.FontSymbol.Person).ToString(), Microsoft.Xna.Framework.Color.White, ArwicEngine.Forms.RichText.SymbolFont),
            new ArwicEngine.Forms.RichTextParseRule("food", ((char)ArwicEngine.Forms.FontSymbol.Apple).ToString(), Microsoft.Xna.Framework.Color.PaleGreen, ArwicEngine.Forms.RichText.SymbolFont),
            new ArwicEngine.Forms.RichTextParseRule("production", ((char)ArwicEngine.Forms.FontSymbol.Gavel).ToString(), Microsoft.Xna.Framework.Color.Orange, ArwicEngine.Forms.RichText.SymbolFont),
            new ArwicEngine.Forms.RichTextParseRule("gold", ((char)ArwicEngine.Forms.FontSymbol.Coin).ToString(), Microsoft.Xna.Framework.Color.Goldenrod, ArwicEngine.Forms.RichText.SymbolFont),
            new ArwicEngine.Forms.RichTextParseRule("science", ((char)ArwicEngine.Forms.FontSymbol.Flask).ToString(), Microsoft.Xna.Framework.Color.LightSkyBlue, ArwicEngine.Forms.RichText.SymbolFont),
            new ArwicEngine.Forms.RichTextParseRule("culture", ((char)ArwicEngine.Forms.FontSymbol.BookOpen).ToString(), Microsoft.Xna.Framework.Color.MediumPurple, ArwicEngine.Forms.RichText.SymbolFont)
        };

        public Game1 Game { get; set; }
        private ArwicEngine.Forms.Control selectedControl;
        private bool draggingControl;
        private bool resizingControl_TopLeft;
        private bool resizingControl_TopRight;
        private bool resizingControl_BottomLeft;
        private bool resizingControl_BottomRight;
        private Microsoft.Xna.Framework.Point dragOffset;
        private Microsoft.Xna.Framework.Point lastMousePos;

        public MainForm()
        {
            InitializeComponent();
        }

        public IntPtr GetDrawSurface()
        {
            return pb_drawSurface.Handle;
        }
        
        public Point GetMousePos()
        {
            return new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y - 30);
        }

        private void SelectControl(ArwicEngine.Forms.Control control)
        {
            if (selectedControl != null)
                selectedControl.EditorSelected = false;
            selectedControl = control;
            if (selectedControl != null)
                selectedControl.EditorSelected = true;
            pg_properties.SelectedObject = selectedControl;
        }

        private void MainForm_Close(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            pb_drawSurface.Size = new Size(10, 10);
        }

        private void pb_drawSurface_Click(object sender, EventArgs e)
        {
            bool madeSelection = false;
                SelectControl(null);
            foreach (ArwicEngine.Forms.Control control in Game.Scene.Form.Children)
            {
                if (control == Game.Scene.Form.CloseButton)
                    continue;
                if (control.AbsoluteBounds.Contains(GetMousePos().ToXNAPoint()))
                {
                    SelectControl(control);
                    madeSelection = true;
                    break;
                }
            }
            if (!madeSelection)
            {
                if (Game.Scene.Form.AbsoluteBounds.Contains(GetMousePos().ToXNAPoint()))
                    SelectControl(Game.Scene.Form);
            }
        }

        private void pb_drawSurface_MouseDown(object sender, MouseEventArgs e)
        {
            if (selectedControl != null)
            {
                int resizeHandleDim = 10;
                Microsoft.Xna.Framework.Point mousePos = GetMousePos().ToXNAPoint();
                if (new Microsoft.Xna.Framework.Rectangle(
                    selectedControl.AbsoluteLocation.X - resizeHandleDim,
                    selectedControl.AbsoluteLocation.Y - resizeHandleDim,
                    resizeHandleDim * 2, resizeHandleDim * 2).Contains(mousePos))
                        resizingControl_TopLeft = true;
                else if (new Microsoft.Xna.Framework.Rectangle(
                    selectedControl.AbsoluteLocation.X + selectedControl.Size.Width - resizeHandleDim,
                    selectedControl.AbsoluteLocation.Y - resizeHandleDim,
                    resizeHandleDim * 2, resizeHandleDim * 2).Contains(mousePos))
                        resizingControl_TopRight = true;
                else if (new Microsoft.Xna.Framework.Rectangle(
                    selectedControl.AbsoluteLocation.X - resizeHandleDim,
                    selectedControl.AbsoluteLocation.Y + selectedControl.Size.Height - resizeHandleDim,
                    resizeHandleDim * 2, resizeHandleDim * 2).Contains(mousePos))
                        resizingControl_BottomLeft = true;
                else if (new Microsoft.Xna.Framework.Rectangle(
                    selectedControl.AbsoluteLocation.X + selectedControl.Size.Width - resizeHandleDim,
                    selectedControl.AbsoluteLocation.Y + selectedControl.Size.Height - resizeHandleDim,
                    resizeHandleDim * 2, resizeHandleDim * 2).Contains(mousePos))
                        resizingControl_BottomRight = true;
                else if (selectedControl.AbsoluteBounds.Contains(mousePos))
                {
                    draggingControl = true;
                    dragOffset = GetMousePos().ToXNAPoint() - selectedControl.AbsoluteLocation;
                }
            }
        }

        private void pb_drawSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedControl != null)
            {
                if (resizingControl_TopLeft)
                {
                    if (selectedControl is ArwicEngine.Forms.Form)
                        return;
                    int x = selectedControl.AbsoluteBounds.X + (GetMousePos().ToXNAPoint().X - lastMousePos.X);
                    int y = selectedControl.AbsoluteBounds.Y + (GetMousePos().ToXNAPoint().Y - lastMousePos.Y);
                    int w = selectedControl.AbsoluteBounds.Width - (GetMousePos().ToXNAPoint().X - lastMousePos.X);
                    int h = selectedControl.AbsoluteBounds.Height - (GetMousePos().ToXNAPoint().Y - lastMousePos.Y);
                    selectedControl.AbsoluteBounds = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
                }
                else if (resizingControl_TopRight)
                {
                    if (selectedControl is ArwicEngine.Forms.Form)
                        return;
                    int x = selectedControl.AbsoluteBounds.X;
                    int y = selectedControl.AbsoluteBounds.Y + (GetMousePos().ToXNAPoint().Y - lastMousePos.Y);
                    int w = selectedControl.AbsoluteBounds.Width + (GetMousePos().ToXNAPoint().X - lastMousePos.X);
                    int h = selectedControl.AbsoluteBounds.Height - (GetMousePos().ToXNAPoint().Y - lastMousePos.Y);
                    selectedControl.AbsoluteBounds = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
                }
                else if (resizingControl_BottomLeft)
                {
                    if (selectedControl is ArwicEngine.Forms.Form)
                        return;
                    int x = selectedControl.AbsoluteBounds.X + (GetMousePos().ToXNAPoint().X - lastMousePos.X);
                    int y = selectedControl.AbsoluteBounds.Y;
                    int w = selectedControl.AbsoluteBounds.Width - (GetMousePos().ToXNAPoint().X - lastMousePos.X);
                    int h = selectedControl.AbsoluteBounds.Height + (GetMousePos().ToXNAPoint().Y - lastMousePos.Y);
                    selectedControl.AbsoluteBounds = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
                }
                else if (resizingControl_BottomRight)
                {
                    int x = selectedControl.AbsoluteBounds.X;
                    int y = selectedControl.AbsoluteBounds.Y;
                    int w = selectedControl.AbsoluteBounds.Width + (GetMousePos().ToXNAPoint().X - lastMousePos.X);
                    int h = selectedControl.AbsoluteBounds.Height + (GetMousePos().ToXNAPoint().Y - lastMousePos.Y);
                    selectedControl.AbsoluteBounds = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
                }
                else if (draggingControl)
                {
                    if (selectedControl is ArwicEngine.Forms.Form)
                        return;
                    selectedControl.AbsoluteLocation = GetMousePos().ToXNAPoint() - dragOffset;
                }
            }
            lastMousePos = GetMousePos().ToXNAPoint();
        }

        private void pb_drawSurface_MouseUp(object sender, MouseEventArgs e)
        {
            draggingControl = false;
            resizingControl_TopLeft = false;
            resizingControl_TopRight = false;
            resizingControl_BottomLeft = false;
            resizingControl_BottomRight = false;
        }
        
        private void btn_new_Click(object sender, EventArgs e)
        {
            Game.Scene.Form = Game.Scene.DefaultForm;
            SelectControl(null);
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog();
        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ArwicEngine.Forms.FormConfig formConfig = SerializationHelper.XmlDeserialize<ArwicEngine.Forms.FormConfig>(openFileDialog.FileName);
                Game.Scene.Form = new ArwicEngine.Forms.Form(formConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, "Error opening file");
            }
        }

        private void saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Game.Scene.Form.RemoveChild(Game.Scene.Form.CloseButton);
            ArwicEngine.Forms.FormConfig formConfig = new ArwicEngine.Forms.FormConfig(Game.Scene.Form);
            Game.Scene.Form.AddChild(Game.Scene.Form.CloseButton);
            try
            {
                SerializationHelper.XmlSerialize<ArwicEngine.Forms.FormConfig>(saveFileDialog.FileName, formConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, "Error saving form");
            }
        }

        private void tv_toolBox_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string control = e.Node.Name.Substring(5, e.Node.Name.Length - 5);
            Game.Scene.AddControl(control);
        }

        private void pb_drawSurface_SizeChanged(object sender, EventArgs e)
        {
            GraphicsManager.Instance.DeviceManager.PreferredBackBufferWidth = pb_drawSurface.Size.Width;
            GraphicsManager.Instance.DeviceManager.PreferredBackBufferHeight = pb_drawSurface.Size.Height;
            GraphicsManager.Instance.DeviceManager.ApplyChanges();
        }

        private void btn_deleteSelected_Click(object sender, EventArgs e)
        {
            if (selectedControl == null || selectedControl == Game.Scene.Form)
                return;
            if(MessageBox.Show($"Are you sure you want to delete '{selectedControl.Name}'?", "Delete Confirmation", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Game.Scene.Form.RemoveChild(selectedControl);
                SelectControl(null);
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine(e.KeyChar);
            if (selectedControl != null && selectedControl != Game.Scene.Form)
            {
                switch (e.KeyChar)
                {
                    case 'W':
                        selectedControl.Location = new Microsoft.Xna.Framework.Point(selectedControl.Location.X, selectedControl.Location.Y - 1);
                        break;
                    case 'A':
                        selectedControl.Location = new Microsoft.Xna.Framework.Point(selectedControl.Location.X - 1, selectedControl.Location.Y);
                        break;
                    case 'S':
                        selectedControl.Location = new Microsoft.Xna.Framework.Point(selectedControl.Location.X, selectedControl.Location.Y + 1);
                        break;
                    case 'D':
                        selectedControl.Location = new Microsoft.Xna.Framework.Point(selectedControl.Location.X + 1, selectedControl.Location.Y);
                        break;
                    default:
                        break;
                }
            }
            //switch (e.KeyChar)
            //{
            //    case 'd':
            //        btn_deleteSelected_Click(btn_deleteSelected, EventArgs.Empty);
            //        break;
            //    case 'b':
            //        Game.Scene.AddControl("button");
            //        break;
            //    case 'c':
            //        Game.Scene.AddControl("checkBox");
            //        break;
            //    case 'C':
            //        Game.Scene.AddControl("comboBox");
            //        break;
            //    case 'i':
            //        Game.Scene.AddControl("image");
            //        break;
            //    case 'l':
            //        Game.Scene.AddControl("label");
            //        break;
            //    case 'p':
            //        Game.Scene.AddControl("progressBar");
            //        break;
            //    case 's':
            //        Game.Scene.AddControl("scrollBox");
            //        break;
            //    case 'S':
            //        Game.Scene.AddControl("spinButton");
            //        break;
            //    case 't':
            //        Game.Scene.AddControl("textBox");
            //        break;
            //    default:
            //        break;
            //}
        }
    }
}
