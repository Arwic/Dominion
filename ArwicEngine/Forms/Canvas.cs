// Dominion - Copyright (C) Timothy Ings
// Canvas.cs
// This file contains classes that define a canvas that is used to manage forms

using ArwicEngine.Core;
using ArwicEngine.Graphics;
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
        /// <returns></returns>
        public override bool Update()
        {
            bool interacted = false;
            foreach (Control child in Children)
            {
                if (child is Form)
                {
                    if (child.Visible && child.Enabled)
                    {
                        Form form = child as Form;
                        if (form.AbsoluteBounds.Contains(InputManager.Instance.MouseScreenPos()) && InputManager.Instance.OnMouseUp(MouseButton.Left))
                            BringToFront(form);

                        if (form.Draggable && formDragging == null && InputManager.Instance.OnMouseDown(MouseButton.Left))
                        {
                            Rectangle titleBar = new Rectangle(form.AbsoluteLocation.X, form.AbsoluteLocation.Y, form.Size.Width - (FORM_CLOSEBUTTON_DIM + FORM_CLOSEBUTTON_PADDING), FORM_CLOSEBUTTON_DIM + FORM_CLOSEBUTTON_PADDING);
                            if (titleBar.Contains(InputManager.Instance.MouseScreenPos()))
                            {
                                formDragOffset = InputManager.Instance.MouseScreenPos() - form.AbsoluteLocation;
                                formDragging = form;
                                BringToFront(formDragging);
                            }
                        }
                        if (formDragging != null && !InputManager.Instance.IsMouseDown(MouseButton.Left))
                            formDragging = null;
                        if (formDragging != null)
                            formDragging.AbsoluteLocation = InputManager.Instance.MouseScreenPos() - formDragOffset;
                    }
                }

                interacted = child.Update();
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
                    tt.Visible = tt.Parent.AbsoluteBounds.Contains(InputManager.Instance.MouseScreenPos());
            }

            if (!GraphicsManager.Instance.Viewport.Bounds.Contains(InputManager.Instance.MouseScreenPos()))
                interacted = true;
            if (!interacted)
                DefaultCursor.Enable();
            return interacted;
        }
    }
}
