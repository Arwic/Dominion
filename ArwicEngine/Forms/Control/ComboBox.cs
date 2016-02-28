// Dominion - Copyright (C) Timothy Ings
// ComboBox.cs
// This file contains classes that define a combo box

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class ComboBox : Control
    {
        #region Defaults
        public static Sprite DefaultButtonSprite;

        public static new void InitDefaults()
        {
            DefaultButtonSprite = Engine.Instance.Content.GetAsset<Sprite>(ASSET_CONTROL_COMBOBOX_BUTTON);
        }
        #endregion

        #region Porperties & Accessors
        /// <summary>
        /// Gets the button that is used to open and close the combo box
        /// </summary>
        [Browsable(false)]
        public Button HeadButton { get; private set; }
        /// <summary>
        /// Gets the list of items in the combo box
        /// </summary>
        [Browsable(false)]
        public IListItem[] Items
        {
            get
            {
                if (_items == null)
                    return null;
                return _items.ToArray();
            }
        }
        private List<IListItem> _items;
        /// <summary>
        /// Adds an item to the combo box's items
        /// </summary>
        /// <param name="item">item to add</param>
        public void AddItem(IListItem item)
        {
            if (_items == null)
                _items = new List<IListItem>();
            _items.Add(item);
            OnItemAdded(new ListItemEventArgs(item));
        }
        /// <summary>
        /// Removes an item to the combo box's items
        /// </summary>
        /// <param name="item">item to remove</param>
        public void RemoveItem(IListItem item)
        {
            if (_items == null)
                return;
            _items.Remove(item);
            OnItemRemoved(new ListItemEventArgs(item));
        }
        /// <summary>
        /// Gets or sets the index of the selected item
        /// </summary>
        [Browsable(false)]
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                int last = _selectedIndex;
                _selectedIndex = value;
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
                if (Items != null && _selectedIndex >= Items.Length)
                    _selectedIndex = Items.Length;
                if (Items != null)
                    HeadButton.Text = Text + Selected.Text;
                if (last != _selectedIndex)
                    OnSelectedChanged(new ListItemEventArgs(Selected));
            }
        }
        private int _selectedIndex;
        /// <summary>
        /// Gets the item with index SelectedIndex
        /// </summary>
        [Browsable(false)]
        public IListItem Selected
        {
            get
            {
                if (Items == null || SelectedIndex < 0 || SelectedIndex > Items.Length)
                    return null;
                return Items[SelectedIndex];
            }
        }
        /// <summary>
        /// Gets or sets the state of the combo box
        /// </summary>
        [Browsable(false)]
        public bool Open
        {
            get
            {
                return _open;
            }
            set
            {
                bool last = _open;
                _open = value;
                if (last != _open)
                    OnOpenChanged(EventArgs.Empty);
            }
        }
        private bool _open;
        /// <summary>
        /// Gets or sets the sprite of the header button
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite HeadButtonSprite
        {
            get
            {
                return _headButtonSprite;
            }
            set
            {
                _headButtonSprite = value;
                HeadButton.Sprite = _headButtonSprite;
            }
        }
        private Sprite _headButtonSprite;
        /// <summary>
        /// Gets or sets the sprite of the list buttons
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite ListButtonSprite
        {
            get
            {
                return _listButtonSprite;
            }
            set
            {
                _listButtonSprite = value;
                UpdateItems();
            }
        }
        private Sprite _listButtonSprite;
        /// <summary>
        /// Gets or sets the context menu of the items
        /// </summary>
        [Browsable(false)]
        public ContextMenu ListContextMenu
        {
            get
            {
                return _listContextMenu;
            }
            set
            {
                ContextMenu last = _listContextMenu;
                _listContextMenu = value;
                if (last != _listContextMenu)
                {
                    foreach (IListItem item in Items)
                    {
                        if (item != null && item.Button != null)
                            item.Button.ContextMenu = ListContextMenu;
                    }
                }
            }
        }
        private ContextMenu _listContextMenu;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the value of the open property is changed
        /// </summary>
        public event EventHandler OpenChanged;
        /// <summary>
        /// Occurs when the value of the selected item is changed
        /// </summary>
        public event EventHandler<ListItemEventArgs> SelectedChanged;
        /// <summary>
        /// Occurs when an item is added to the ComboBox
        /// </summary>
        public event EventHandler<ListItemEventArgs> ItemAdded;
        /// <summary>
        /// Occurs when an item is removed from the ComboBox
        /// </summary>
        public event EventHandler<ListItemEventArgs> ItemRemoved;
        #endregion

        #region Event Handlers
        protected virtual void OnOpenChanged(EventArgs e)
        {
            if (OpenChanged != null)
                OpenChanged(this, e);
        }
        protected virtual void OnSelectedChanged(ListItemEventArgs e)
        {
            if (SelectedChanged != null)
                SelectedChanged(this, e);
        }
        protected virtual void OnItemAdded(ListItemEventArgs e)
        {
            if (ItemAdded != null)
                ItemAdded(this, e);
        }
        protected virtual void OnItemRemoved(ListItemEventArgs e)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, e);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the ComboBox class with default settings and optional parent
        /// </summary>
        /// <param name="pos">Position of the ComboBox</param>
        /// <param name="parent">Optional parent</param>
        public ComboBox(Rectangle pos, List<IListItem> items, Control parent = null)
            : base (pos, parent)
        {
            Initialize(items);
        }

        public ComboBox(ComboBoxConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize(new List<IListItem>());
            HeadButtonSprite = DefaultButtonSprite;
            ListButtonSprite = DefaultButtonSprite;
        }

        private void Initialize(List<IListItem> items)
        {
            _selectedIndex = 0;
            _items = items;
            _open = false;
            HeadButton = new Button(Size, this);
            HeadButton.Text = Text;
            HeadButton.Sprite = DefaultButtonSprite;
            _headButtonSprite = DefaultButtonSprite;
            _listButtonSprite = DefaultButtonSprite;
            _listContextMenu = new ContextMenu();
            UpdateItems();

            HeadButton.MouseClick += HeadButton_MouseClick;
            OpenChanged += ComboBox_OpenChanged;
            VisibleChanged += ComboBox_VisibleChanged;
            ItemAdded += ComboBox_ItemAdded;
            ItemRemoved += ComboBox_ItemRemoved;
            TextChanged += ComboBox_TextChanged;
            SizeChanged += ComboBox_SizeChanged;

            if (_items != null)
                foreach (IListItem item in _items)
                    item.Button.Visible = Open;
            SelectedIndex = 0;
        }

        private void ComboBox_SizeChanged(object sender, EventArgs e)
        {
            HeadButton.Size = Size;
        }

        private void ComboBox_TextChanged(object sender, EventArgs e)
        {
            HeadButton.Text = Text;
        }
        private void ComboBox_ItemAdded(object sender, ListItemEventArgs e)
        {
            UpdateItems();
        }
        private void ComboBox_ItemRemoved(object sender, ListItemEventArgs e)
        {
            UpdateItems();
        }
        private void ComboBox_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible == false)
                Open = false;
        }
        private void ComboBox_OpenChanged(object sender, EventArgs e)
        {
            if (_items == null)
                return;
            foreach (IListItem item in _items)
            {
                item.Button.Visible = Open;
            }
        }
        private void HeadButton_MouseClick(object sender, MouseEventArgs e)
        {
            Open = !Open;
        }

        private void UpdateItems()
        {
            if (Items != null)
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    Items[i].Button = new Button(new Rectangle(0, (i + 1) * HeadButton.Size.Height, HeadButton.Size.Width, HeadButton.Size.Height), HeadButton);
                    Items[i].Button.Text = Items[i].Text;
                    Items[i].Button.Cursor = Cursor;
                    Items[i].Button.ContextMenu = ListContextMenu;
                    Items[i].Button.Sprite = ListButtonSprite;
                    Items[i].Button.Font = Font;
                    int locali = i;
                    EventHandler<MouseEventArgs> handler = (sender, args) =>
                    {
                        SelectedIndex = locali;
                        Open = false;
                        HeadButton.Text = Text + Selected.Text;
                    };
                    Items[i].Button.MouseClick += handler;
                }
            }
        }
    }
}
