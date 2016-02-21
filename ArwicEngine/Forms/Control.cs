// Dominion - Copyright (C) Timothy Ings
// Control.cs
// This file contains classes that define an abstract base control

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class BrowsableRectangle
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public BrowsableRectangle(Rectangle rect)
        {
            Width = rect.Width;
            Height = rect.Height;
        }
    }

    public class BrowsablePoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public BrowsablePoint(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
    }

    public class ControlEventArgs : EventArgs
    {
        public Control Control { get; }

        public ControlEventArgs(Control control)
        {
            Control = control;
        }
    }

    public class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates whether the event invloved the left mouse button
        /// </summary>
        public bool Left { get; }
        /// <summary>
        /// Indicates whether the event invloved the right mouse button
        /// </summary>
        public bool Right { get; }
        /// <summary>
        /// Indicates whether the event invloved the middle mouse button
        /// </summary>
        public bool Middle { get; }
        /// <summary>
        /// Change in the scroll wheel value
        /// </summary>
        public int Delta { get; }
        /// <summary>
        /// Position of the cursor
        /// </summary>
        public Point Position { get; }
        /// <summary>
        /// Whether the even has been handled
        /// </summary>
        public bool Handled { get; set; }

        public MouseEventArgs(bool left, bool right, bool middle, Point position, int delta)
        {
            Left = left;
            Right = right;
            Middle = middle;
            Position = position;
            Delta = delta;
        }
    }

    public class ListItemEventArgs : EventArgs
    {
        /// <summary>
        /// List item that was effected
        /// </summary>
        public IListItem ListItem { get; }

        public ListItemEventArgs(IListItem listItem)
        {
            ListItem = listItem;
        }
    }

    public class StringEventArgs : EventArgs
    {
        /// <summary>
        /// String that was effected
        /// </summary>
        public string String { get; }

        public StringEventArgs(string s)
        {
            String = s;
        }
    }

    public class TextLogLineEventArgs : EventArgs
    {
        /// <summary>
        /// TextLogLine that was effected
        /// </summary>
        public RichText TextLogLine { get; }

        public TextLogLineEventArgs(RichText line)
        {
            TextLogLine = line;
        }
    }

    public class KeyEventArgs : EventArgs
    {
        public bool Alt { get; }
        public bool Control { get; }
        public Keys KeyCode { get; }
        //public Keys KeyData { get; } // key pressed OR'd with modifiers
        //public int KeyValue { get; } // int, char number?
        //public bool Modifiers { get; }
        public bool Shift { get; }
        //public bool SuppressKeyPress { get; }

        public KeyEventArgs(Keys key, bool alt = false, bool control = false, bool shift = false)
        {
            KeyCode = key;
            Alt = alt;
            Control = control;
            Shift = shift;
        }
    }

    public class DrawEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }

        public DrawEventArgs(SpriteBatch sb)
        {
            SpriteBatch = sb;
        }
    }

    public abstract class Control
    {
        #region Defaults
        /// <summary>
        /// Gets the default color of the control
        /// </summary>
        public static Color DefaultColor;
        /// <summary>
        /// Gets the default font of the control
        /// </summary>
        public static Font DefaultFont;
        /// <summary>
        /// Gets the default cursor of the control
        /// </summary>
        public static Cursor DefaultCursor;

        public static void InitDefaults()
        {
            DefaultColor = Color.White;
            DefaultCursor = new Cursor(CURSOR_NORMAL_PATH);
            DefaultFont = new Font(FONT_ARIAL_PATH);
        }
        #endregion

        private static object _childrenModifyLock = new object();

        #region Properties & Accessors
        [Browsable(false)]
        public bool EditorSelected { get { return _editorSelected; } set { _editorSelected = value; } }
        private bool _editorSelected;
        /// <summary>
        /// Gets or sets a value indicating whether the control will respond to user interaction
        /// </summary>
        [Browsable(false)]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                bool last = _enabled;
                _enabled = value;
                if (last != _enabled)
                    OnEnabledChanged(EventArgs.Empty);
            }
        }
        private bool _enabled;
        /// <summary>
        /// Gets or sets a value indicating whether the control and all its child controls are displayed
        /// </summary>
        [Browsable(false)]
        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                bool last = _visible;
                _visible = value;
                if (last != _visible)
                    OnVisibleChanged(EventArgs.Empty);
            }
        }
        private bool _visible;
        /// <summary>
        /// Gets or sets the name of the control
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                string last = _name;
                _name = value;
                if (last != _name)
                    OnNameChanged(EventArgs.Empty);
            }
        }
        private string _name;
        /// <summary>
        /// Gets or sets the text associated with this control
        /// </summary>
        [TypeConverter(typeof(RichTextConverter))]
        public virtual RichText Text
        {
            get
            {
                return _text;
            }
            set
            {
                RichText last = _text;
                _text = value;
                if (last != _text)
                    OnTextChanged(EventArgs.Empty);
            }
        }
        private RichText _text;
        /// <summary>
        /// Gets or sets the parent container of the control
        /// </summary>
        [Browsable(false)]
        public Control Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                Control last = _parent;
                _parent = value;
                if (last != _parent)
                    OnParentChanged(new ControlEventArgs(_parent));
            }
        }
        private Control _parent;
        /// <summary>
        /// Gets an array of the control's direct children
        /// </summary>
        [Browsable(false)]
        public Control[] Children
        {
            get
            {
                lock (_childrenModifyLock)
                {
                    Control[] children = new Control[_children.Count];
                    LinkedListNode<Control> child = _children.First;
                    for (int i = 0; i < _children.Count; i++)
                    {
                        children[i] = child.Value;
                        child = child.Next;
                    }
                    return children;
                }
            }
            set
            {
                lock (_childrenModifyLock)
                {
                    _children = new LinkedList<Control>();
                    foreach (Control child in value)
                    {
                        _children.Enqueue(child);
                    }
                }
            }
        }
        private LinkedList<Control> _children;
        /// <summary>
        /// Gets an array of all the control's children and thier children etc.
        /// </summary>
        [Browsable(false)]
        public Control[] ChildrenAndSubChildren
        {
            get
            {
                lock (_childrenModifyLock)
                {
                    List<Control> children = new List<Control>();
                    foreach (Control directChild in Children)
                    {
                        children.Add(directChild);
                        foreach (Control subChild in directChild.ChildrenAndSubChildren)
                            children.Add(subChild);
                    }
                    return children.ToArray();
                }
            }
        }
        /// <summary>
        /// Adds the control as the control's last child, same as AddChildLast()
        /// </summary>
        /// <param name="control">control to add</param>
        public void AddChild(Control control)
        {
            AddChildFirst(control);
        }
        /// <summary>
        /// Adds the control before the control at the given index
        /// </summary>
        /// <param name="control">control to add</param>
        /// <param name="index">index of the control</param>
        public void AddChildBefore(Control control, int index)
        {
            lock (_childrenModifyLock)
            {
                LinkedListNode<Control> before = _children.First;
                int counter = 0;
                while (counter < index)
                {
                    before = before.Next;
                    index++;
                }
                _children.AddBefore(before, control);
                control.Parent = this;
                OnChildAdded(new ControlEventArgs(control));
            }
        }
        /// <summary>
        /// Adds the control after the control at the given index
        /// </summary>
        /// <param name="control">control to add</param>
        /// <param name="index">index of the control</param>
        public void AddChildAfter(Control control, int index)
        {
            lock (_childrenModifyLock)
            {
                LinkedListNode<Control> before = _children.First;
                int counter = 0;
                while (counter < index)
                {
                    before = before.Next;
                    index++;
                }
                _children.AddAfter(before, control);
                control.Parent = this;
                OnChildAdded(new ControlEventArgs(control));
            }
        }
        /// <summary>
        /// Adds the control as the control's first child
        /// </summary>
        /// <param name="control"></param>
        public void AddChildFirst(Control control)
        {
            lock (_childrenModifyLock)
            {
                _children.AddFirst(control);
                control.Parent = this;
                OnChildAdded(new ControlEventArgs(control));
            }
        }
        /// <summary>
        /// Adds the control as the control's last child
        /// </summary>
        /// <param name="control"></param>
        public void AddChildLast(Control control)
        {
            lock (_childrenModifyLock)
            {
                _children.AddLast(control);
                control.Parent = this;
                OnChildAdded(new ControlEventArgs(control));
            }
        }
        /// <summary>
        /// Removes the control from the control's children, if the parent's parent becomes the child's new parent
        /// </summary>
        /// <param name="control">control to remove</param>
        public void RemoveChild(Control control)
        {
            lock (_childrenModifyLock)
            {
                if (control == null)
                    return;
                while (_children.Contains(control))
                    _children.Remove(control);
                control.Parent = Parent;
                OnChildRemoved(new ControlEventArgs(control));
            }
        }
        /// <summary>
        /// Gets a value indicating whether the control contains one or more child controls
        /// </summary>
        [Browsable(false)]
        public bool HasChildren
        {
            get
            {
                lock (_childrenModifyLock)
                {
                    return _children.Count != 0;
                }
            }
        }
        /// <summary>
        /// Gets or sets the shortcut menu associated with the control
        /// </summary>
        [Browsable(false)]
        public virtual ContextMenu ContextMenu
        {
            get
            {
                return _contextMenu;
            }
            set
            {
                ContextMenu last = _contextMenu;
                _contextMenu = value;
                if (last != _contextMenu)
                    OnContextMenuChanged(EventArgs.Empty);
            }
        }
        private ContextMenu _contextMenu;
        /// <summary>
        /// Gets or sets the cursor that is displayed when the mouse pointer is over the control
        /// </summary>
        [Browsable(false)]
        public virtual Cursor Cursor
        {
            get
            {
                return _cursor;
            }
            set
            {
                Cursor last = _cursor;
                _cursor = value;
                if (last != _cursor)
                    OnCursorChanged(EventArgs.Empty);
            }
        }
        private Cursor _cursor;
        /// <summary>
        /// Gets or sets the font of the text displayed by the control
        /// </summary>
        [Browsable(false)]
        public virtual Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                Font last = _font;
                _font = value;
                if (last != _font)
                    OnFontChanged(EventArgs.Empty);
            }
        }
        private Font _font;
        /// <summary>
        /// Gets or sets the color of the control
        /// </summary>
        [TypeConverter(typeof(ColorConverter))]
        public virtual Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                Color last = _color;
                _color = value;
                if (last != _color)
                    OnColorChanged(EventArgs.Empty);
            }
        }
        private Color _color;
        /// <summary>
        /// Gets or sets the coordinates of the upper-left corner of the control relative to the upper-left corner of its container
        /// </summary>
        [TypeConverter(typeof(PointConverter))]
        public virtual Point Location
        {
            get
            {
                return _location;
            }
            set
            {
                Point last = _location;
                _location = value;
                if (last != _location)
                    OnSizeChanged(EventArgs.Empty);
            }
        }
        private Point _location;
        /// <summary>
        /// Gets or sets the height and width of the control
        /// </summary>
        [TypeConverter(typeof(RectangleDimConverter))]
        public virtual Rectangle Size
        {
            get
            {
                return _size;
            }
            set
            {
                Rectangle last = _size;
                _size = value;
                if (last != _size)
                    OnSizeChanged(EventArgs.Empty);
            }
        }
        private Rectangle _size;
        /// <summary>
        /// Gets or sets the size and location of the control, in pixels, relative to the parent control
        /// </summary>
        [Browsable(false)]
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(Location, new Point(Size.Width, Size.Height));
            }
            set
            {
                Location = new Point(value.X, value.Y);
                Size = new Rectangle(0, 0, value.Width, value.Height);
            }
        }
        /// <summary>
        /// Gets or sets the size and location of the control, in pixels, relative to the window
        /// </summary>
        [Browsable(false)]
        public Rectangle AbsoluteBounds
        {
            get
            {
                return new Rectangle(AbsoluteLocation, new Point(Size.Width, Size.Height));
            }
            set
            {
                AbsoluteLocation = new Point(value.X, value.Y);
                Size = new Rectangle(0, 0, value.Width, value.Height);
            }
        }
        /// <summary>
        /// Gets or sets the location of the control, in pixels, relative to the window
        /// </summary>
        [Browsable(false)]
        public Point AbsoluteLocation
        {
            get
            {
                Point parentLocation = new Point(0, 0);
                if (Parent != null)
                    parentLocation = Parent.AbsoluteLocation;
                return Location + parentLocation;
            }
            set
            {
                Point parentLocation = new Point(0, 0);
                if (Parent != null)
                    parentLocation = Parent.AbsoluteLocation;
                Location = value - parentLocation;
            }
        }
        /// <summary>
        /// Gets or sets the tooltip that is displayed on mouse over
        /// </summary>
        [Browsable(false)]
        public ToolTip ToolTip
        {
            get
            {
                return _toolTip;
            }
            set
            {
                _toolTip = value;
                AddChild(_toolTip);
            }
        }
        private ToolTip _toolTip;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a child is added
        /// </summary>
        public event EventHandler<ControlEventArgs> ChildAdded;
        /// <summary>
        /// Occurs when a child is removed
        /// </summary>
        public event EventHandler<ControlEventArgs> ChildRemoved;
        /// <summary>
        /// Occurs when the value of the ContextMenu property changes 
        /// </summary>
        public event EventHandler ContextMenuChanged;
        /// <summary>
        /// Occurs when the value of the Cursor property changes
        /// </summary>
        public event EventHandler CursorChanged;
        /// <summary>
        /// Occurs when the Enabled property value has changed
        /// </summary>
        public event EventHandler EnabledChanged;
        /// <summary>
        /// Occurs when the Font property value has changed
        /// </summary>
        public event EventHandler FontChanged;
        /// <summary>
        /// Occurs when the Color property value has changed
        /// </summary>
        public event EventHandler ColorChanged;
        /// <summary>
        /// Occurs when the Location property value has changed
        /// </summary>
        public event EventHandler LocationChanged;
        /// <summary>
        /// Occurs when the Size property value has changed
        /// </summary>
        public event EventHandler SizeChanged;
        /// <summary>
        /// Occurs when the control is clicked by the mouse
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseClick;
        /// <summary>
        /// (NYI) Occurs when the control is double clicked by the mouse
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseDoubleClick;
        /// <summary>
        /// Occurs when the mouse pointer is over the control and a mouse button is pressed
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseDown;
        /// <summary>
        /// Occurs when the mouse pointer is over the control and a mouse button is released
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseUp;
        /// <summary>
        /// Occurs when the mouse pointer is moved over the control
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseMove;
        /// <summary>
        /// Occurs when the mouse wheel moves while the mouse is over the control
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseWheel;
        /// <summary>
        /// Occurs when the mouse pointer enters the control
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseEnter;
        /// <summary>
        /// Occurs when the mouse pointer leaves the control
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseLeave;
        /// <summary>
        /// Occurs when the mouse pointer rests on the control
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseHover;
        /// <summary>
        /// Occurs when the Parent property value changes
        /// </summary>
        public event EventHandler<ControlEventArgs> ParentChanged;
        /// <summary>
        /// Occurs when the Text property value changes
        /// </summary>
        public event EventHandler TextChanged;
        /// <summary>
        /// Occurs when the Visible property value changes
        /// </summary>
        public event EventHandler VisibleChanged;
        /// <summary>
        /// Occurs when the Name property value changes
        /// </summary>
        public event EventHandler NameChanged;
        /// <summary>
        /// Occurs when the control is updated
        /// </summary>
        public event EventHandler Updated;
        /// <summary>
        /// Occurs when the control is redrawn
        /// </summary>
        public event EventHandler<DrawEventArgs> Drawn;
        #endregion

        #region Event Handlers
        protected virtual void OnChildAdded(ControlEventArgs e)
        {
            if (ChildAdded != null)
                ChildAdded(this, e);
        }
        protected virtual void OnChildRemoved(ControlEventArgs e)
        {
            if (ChildRemoved != null)
                ChildRemoved(this, e);
        }
        protected virtual void OnContextMenuChanged(EventArgs e)
        {
            if (ContextMenuChanged != null)
                ContextMenuChanged(this, e);
        }
        protected virtual void OnCursorChanged(EventArgs e)
        {
            if (CursorChanged != null)
                CursorChanged(this, e);
        }
        protected virtual void OnEnabledChanged(EventArgs e)
        {
            if (EnabledChanged != null)
                EnabledChanged(this, e);
        }
        protected virtual void OnFontChanged(EventArgs e)
        {
            if (FontChanged != null)
                FontChanged(this, e);
        }
        protected virtual void OnColorChanged(EventArgs e)
        {
            if (ColorChanged != null)
                ColorChanged(this, e);
        }
        protected virtual void OnLocationChanged(EventArgs e)
        {
            if (LocationChanged != null)
                LocationChanged(this, e);
        }
        protected virtual void OnSizeChanged(EventArgs e)
        {
            if (SizeChanged != null)
                SizeChanged(this, e);
        }
        protected virtual void OnMouseClick(MouseEventArgs e)
        {
            if (MouseClick != null)
                MouseClick(this, e);
        }
        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (MouseDoubleClick != null)
                MouseDoubleClick(this, e);
        }
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null)
                MouseDown(this, e);
        }
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null)
                MouseUp(this, e);
        }
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null)
                MouseMove(this, e);
        }
        protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            if (MouseWheel != null)
                MouseWheel(this, e);
        }
        protected virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (MouseEnter != null)
                MouseEnter(this, e);
        }
        protected virtual void OnMouseLeave(MouseEventArgs e)
        {
            if (MouseLeave != null)
                MouseLeave(this, e);
        }
        protected virtual void OnMouseHover(MouseEventArgs e)
        {
            if (MouseHover != null)
                MouseHover(this, e);
        }
        protected virtual void OnParentChanged(ControlEventArgs e)
        {
            if (ParentChanged != null)
                ParentChanged(this, e);
        }
        protected virtual void OnTextChanged(EventArgs e)
        {
            if (TextChanged != null)
                TextChanged(this, e);
        }
        protected virtual void OnVisibleChanged(EventArgs e)
        {
            if (VisibleChanged != null)
                VisibleChanged(this, e);
        }
        protected virtual void OnNameChanged(EventArgs e)
        {
            if (NameChanged != null)
                NameChanged(this, e);
        }
        protected virtual void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
        protected virtual void OnDrawn(DrawEventArgs e)
        {
            if (Drawn != null)
                Drawn(this, e);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the Control class with default settings
        /// </summary>
        /// <param name="pos">Position of the Control</param>
        public Control(Rectangle pos, Control parent = null)
        {
            Initialize(pos, parent);
        }

        public Control(ControlConfig config, Control parent = null)
        {
            Initialize(new Rectangle(config.LocationX, config.LocationY, config.SizeWidth, config.SizeHeight), parent);
            Name = config.Name;
            Color = config.Color;
            Text = RichText.ParseText(config.Text, Color, Font);
            foreach (dynamic child in config.Children)
            {
                Assembly assembly = typeof(Control).Assembly;
                Type controlType = assembly.GetType(child.TypeName);
                ConstructorInfo[] ctors = controlType.GetConstructors();
                ctors[1].Invoke(new object[] { child, this });
            }
        }

        public void ParseText(List<RichTextParseRule> rules, Color? generalColor = null, Font generalFont = null)
        {
            if (generalColor == null) generalColor = Color;
            if (generalFont == null) generalFont = Font;
            Text = RichText.ParseText(Text.ToString(), generalColor.Value, generalFont, rules);
        }

        private void Initialize(Rectangle pos, Control parent)
        {
            _name = GetType().ToString();
            parent?.AddChild(this);
            _enabled = true;
            _visible = true;
            _text = "".ToRichText();
            _children = new LinkedList<Control>();
            _contextMenu = null;
            _cursor = DefaultCursor;
            _font = DefaultFont;
            _color = DefaultColor;
            _location = new Point(pos.X, pos.Y);
            _size = new Rectangle(0, 0, pos.Width, pos.Height);

            MouseEnter += Control_MouseEnter;
            MouseLeave += Control_MouseLeave;
            MouseMove += Control_MouseMove;
        }
        
        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Enable();
        }
        private void Control_MouseLeave(object sender, MouseEventArgs e)
        {
            DefaultCursor.Enable();
        }
        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor.Enable();
        }

        public Control GetChildByName(string name)
        {
            foreach (Control child in Children)
            {
                if (child.Name == name)
                    return child;
            }
            throw new InvalidOperationException($"There is no child with name {name}");
        }

        public void CentreControl()
        {
            if (Parent != null)
                Location = new Point(Parent.Size.Width / 2 - Size.Width / 2, Parent.Size.Height / 2 - Size.Height / 2);
            else
                Location = new Point(1920 / 2 - Size.Width / 2, 1080 / 2 - Size.Height / 2);
        }

        /// <summary>
        /// Updates the control and its events
        /// </summary>
        public virtual bool Update()
        {
            bool interacted = false;
            if (Visible && Enabled)
            {
                foreach (Control child in Children)
                {
                    child.UpdateEnterLeave();
                }
                foreach (Control child in Children)
                {
                    interacted = child.Update();
                    if (interacted)
                        break;
                }
                if (ToolTip != null)
                    ToolTip.Update();
                
                if (!interacted)
                {
                    Point mouseScreenPos = InputManager.Instance.MouseScreenPos();
                    bool containsMouse = AbsoluteBounds.Contains(mouseScreenPos);

                    if (containsMouse)
                    {
                        bool leftMouseDown = InputManager.Instance.IsMouseDown(MouseButton.Left);
                        bool rightMouseDown = InputManager.Instance.IsMouseDown(MouseButton.Right);
                        bool middleMouseDown = InputManager.Instance.IsMouseDown(MouseButton.Middle);
                        bool onLeftMouseDown = InputManager.Instance.OnMouseDown(MouseButton.Left);
                        bool onRightMouseDown = InputManager.Instance.OnMouseDown(MouseButton.Right);
                        bool onMiddleMouseDown = InputManager.Instance.OnMouseDown(MouseButton.Middle);
                        bool onLeftMouseUp = InputManager.Instance.OnMouseUp(MouseButton.Left);
                        bool onRightMouseUp = InputManager.Instance.OnMouseUp(MouseButton.Right);
                        bool onMiddleMouseUp = InputManager.Instance.OnMouseUp(MouseButton.Middle);

                        Point lastMouseScreenPos = InputManager.Instance.LastMouseScreenPos();
                        int mouseScrollWheelValue = InputManager.Instance.CurrentMouseScrollWheelValue();
                        int lastMouseScrollWheelValue = InputManager.Instance.LastMouseScrollWheelValue();
                        int mouseScrollWheelDelta = lastMouseScrollWheelValue - mouseScrollWheelValue;
                        bool lastContainsMouse = AbsoluteBounds.Contains(lastMouseScreenPos);

                        if (onLeftMouseDown || onRightMouseDown || onMiddleMouseDown)
                        {
                            OnMouseDown(new MouseEventArgs(onLeftMouseDown, onRightMouseDown, onMiddleMouseDown, mouseScreenPos, mouseScrollWheelDelta));
                            //input.Engine.Console.WriteLine($"OnMouseDown {onLeftMouseDown}:{onRightMouseDown}:{onMiddleMouseDown}");
                        }

                        if (onLeftMouseUp || onRightMouseUp || onMiddleMouseUp)
                        {
                            OnMouseClick(new MouseEventArgs(onLeftMouseUp, onRightMouseUp, onMiddleMouseUp, mouseScreenPos, mouseScrollWheelDelta));
                            //input.Engine.Console.WriteLine($"OnMouseClick {onLeftMouseUp}:{onRightMouseUp}:{onMiddleMouseUp}");
                            OnMouseUp(new MouseEventArgs(onLeftMouseUp, onRightMouseUp, onMiddleMouseUp, mouseScreenPos, mouseScrollWheelDelta));
                            //input.Engine.Console.WriteLine($"OnMouseUp {onLeftMouseUp}:{onRightMouseUp}:{onMiddleMouseUp}");
                        }

                        if (mouseScreenPos == lastMouseScreenPos)
                        {
                            OnMouseHover(new MouseEventArgs(leftMouseDown, middleMouseDown, rightMouseDown, mouseScreenPos, mouseScrollWheelDelta));
                            //input.Engine.Console.WriteLine("OnMouseHover");
                        }
                        else if (mouseScreenPos != lastMouseScreenPos)
                        {
                            OnMouseMove(new MouseEventArgs(leftMouseDown, middleMouseDown, rightMouseDown, mouseScreenPos, mouseScrollWheelDelta));
                            //input.Engine.Console.WriteLine("OnMouseMove");
                        }

                        if (mouseScrollWheelDelta != 0)
                        {
                            OnMouseWheel(new MouseEventArgs(leftMouseDown, middleMouseDown, rightMouseDown, mouseScreenPos, mouseScrollWheelDelta));
                            //input.Engine.Console.WriteLine($"OnMouseWheel {mouseScrollWheelDelta}");
                        }

                        interacted = true;
                    }
                }
            }
            OnUpdated(EventArgs.Empty);
            return interacted;
        }

        private void UpdateEnterLeave()
        {
            bool leftMouseDown = InputManager.Instance.IsMouseDown(MouseButton.Left);
            bool rightMouseDown = InputManager.Instance.IsMouseDown(MouseButton.Right);
            bool middleMouseDown = InputManager.Instance.IsMouseDown(MouseButton.Middle);
            bool onLeftMouseDown = InputManager.Instance.OnMouseDown(MouseButton.Left);

            Point mouseScreenPos = InputManager.Instance.MouseScreenPos();
            Point lastMouseScreenPos = InputManager.Instance.LastMouseScreenPos();
            int mouseScrollWheelValue = InputManager.Instance.CurrentMouseScrollWheelValue();
            int lastMouseScrollWheelValue = InputManager.Instance.LastMouseScrollWheelValue();
            int mouseScrollWheelDelta = lastMouseScrollWheelValue - mouseScrollWheelValue;
            bool containsMouse = AbsoluteBounds.Contains(mouseScreenPos);
            bool lastContainsMouse = AbsoluteBounds.Contains(lastMouseScreenPos);

            if (containsMouse && !lastContainsMouse)
            {
                OnMouseEnter(new MouseEventArgs(leftMouseDown, rightMouseDown, middleMouseDown, mouseScreenPos, mouseScrollWheelDelta));
                //input.Engine.Console.WriteLine("OnMouseEnter");
            }
            else if (!containsMouse && lastContainsMouse)
            {
                OnMouseLeave(new MouseEventArgs(leftMouseDown, rightMouseDown, middleMouseDown, mouseScreenPos, mouseScrollWheelDelta));
                //input.Engine.Console.WriteLine("OnMouseLeave");
            }
        }

        /// <summary>
        /// Draws the control
        /// </summary>
        public virtual void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                if (EditorSelected)
                    GraphicsHelper.DrawRect(sb, AbsoluteBounds, 2, Color.Red);
                for (int i = Children.Length - 1; i >= 0; i--)
                {
                    if (Children[i] is ToolTip)
                        continue;
                    Children[i].Draw(sb);
                }
                OnDrawn(new DrawEventArgs(sb));
            }
        }
    }
}
