using ArwicEngine.Core;
using ArwicEngine.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class Canvas : Control
    {
        /// <summary>
        /// Gets or sets the form that is always on top of the other forms, if null no form is always on top
        /// </summary>
        public Control AlwayOnTop
        {
            get
            {
                return _alwaysOnTop;
            }
            set
            {
                _alwaysOnTop = value;
                if (_alwaysOnTop != null)
                    BringToFront(_alwaysOnTop);
            }
        }
        private Control _alwaysOnTop;

        private TextBox selectedTextBox;
        private ToolTip selectedToolTip;
        private Form formDragging;
        private Point formDragOffset;

        public Canvas(Rectangle pos)
            : base (pos)
        {
        }

        public void AddForm(FormConfig formConfig)
        {
            Form form = new Form(formConfig, this);
        }

        public void BringToFront(Control control)
        {
            RemoveChild(control);
            AddChildFirst(control);
            if (AlwayOnTop != null)
            {
                RemoveChild(AlwayOnTop);
                AddChildFirst(AlwayOnTop);
            }
        }

        public void SendToBack(Control control)
        {
            if (control != AlwayOnTop)
            {
                RemoveChild(control);
                AddChildLast(control);
            }
        }

        public void CaptureTextBox(TextBox tb)
        {
            selectedTextBox = tb;
            selectedTextBox.Selected = true;
        }

        public void UnCaptureTextBox(TextBox tb)
        {
            if (selectedTextBox == null)
                return;
            selectedTextBox.Selected = false;
            selectedTextBox = null;
        }

        public bool IsCapturingText()
        {
            return selectedTextBox != null;
        }

        /// <summary>
        /// Updates the canvas, returns true if a ui element is capturing the mouse
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool Update(InputManager input)
        {
            bool interacted = false;
            foreach (Control child in Children)
            {
                if (child is Form)
                {
                    if (child.Visible && child.Enabled)
                    {
                        Form form = child as Form;
                        if (form.AbsoluteBounds.Contains(input.MouseScreenPos()) && input.OnMouseUp(MouseButton.Left))
                            BringToFront(form);

                        if (form.Draggable && formDragging == null && input.OnMouseDown(MouseButton.Left))
                        {
                            Rectangle titleBar = new Rectangle(form.AbsoluteLocation.X, form.AbsoluteLocation.Y, form.Size.Width - (FORM_CLOSEBUTTON_DIM + FORM_CLOSEBUTTON_PADDING), FORM_CLOSEBUTTON_DIM + FORM_CLOSEBUTTON_PADDING);
                            if (titleBar.Contains(input.MouseScreenPos()))
                            {
                                formDragOffset = input.MouseScreenPos() - form.AbsoluteLocation;
                                formDragging = form;
                                BringToFront(formDragging);
                            }
                        }
                        if (formDragging != null && !input.IsMouseDown(MouseButton.Left))
                            formDragging = null;
                        if (formDragging != null)
                            formDragging.AbsoluteLocation = input.MouseScreenPos() - formDragOffset;
                    }
                }

                interacted = child.Update(input);
                if (interacted)
                    break;
            }

            foreach (Control control in ChildrenAndSubChildren)
            {
                TextBox tb = control as TextBox;
                if (tb != null && tb != selectedTextBox)
                {
                    tb.Selected = false;
                    continue;
                }

                ToolTip tt = control as ToolTip;
                if (tt != null)
                    tt.Visible = tt.Parent.AbsoluteBounds.Contains(input.MouseScreenPos());
            }

            if (!input.Engine.Graphics.Viewport.Bounds.Contains(input.MouseScreenPos()))
                interacted = true;
            if (!interacted)
                DefaultCursor.Enable();
            return interacted;
        }
    }
}
