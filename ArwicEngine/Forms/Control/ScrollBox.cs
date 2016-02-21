// Dominion - Copyright (C) Timothy Ings
// ScrollBox.cs
// This file contains classes that define a scroll box

using ArwicEngine.Core;
using ArwicEngine.Graphics;
using ArwicEngine.Input;
using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static ArwicEngine.Constants;

namespace ArwicEngine.Forms
{
    public class ScrollBox : Control
    {
        #region Defaults
        public static Sprite DefaultBackgroundSprite;
        public static Sprite DefaultSelectedItemSprite;
        public static Sprite DefaultListItemSprite;
        public static Sprite DefaultScrubberSprite;

        public static new void InitDefaults()
        {
            DefaultBackgroundSprite = new Sprite(CONTROL_SCROLLBOX_BACK);
            DefaultSelectedItemSprite = new Sprite(CONTROL_SCROLLBOX_SELECTED);
            DefaultScrubberSprite = new Sprite(CONTROL_SCROLLBOX_SCRUBBER);
            DefaultListItemSprite = new Sprite(CONTROL_SCROLLBOX_BUTTON);
        }
        #endregion

        #region Properties & Accessors
        /// <summary>
        /// Gets the list of items in the scroll box
        /// </summary>
        [Browsable(false)]
        public List<IListItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                List<IListItem> last = _items;
                _items = value;
                if (last != _items)
                    OnItemsChanged(EventArgs.Empty);
            }
        }
        private List<IListItem> _items;
        /// <summary>
        /// Adds an item to the scroll box's items
        /// </summary>
        /// <param name="item">item to add</param>
        public void AddItem(IListItem item)
        {
            if (Items == null)
                Items = new List<IListItem>();
            Items.Add(item);
            OnItemAdded(new ListItemEventArgs(item));
        }
        /// <summary>
        /// Removes an item to the scroll box's items
        /// </summary>
        /// <param name="item">item to remove</param>
        public void RemoveItem(IListItem item)
        {
            if (Items == null)
                return;
            Items.Remove(item);
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
                return _selected;
            }
            set
            {
                int last = _selected;
                _selected = value;
                if (last != _selected)
                    OnSelectedChanged(new ListItemEventArgs(Selected));
                else
                    UpdateItems();
            }
        }
        private int _selected;
        /// <summary>
        /// Gets the item with index SelectedIndex
        /// </summary>
        [Browsable(false)]
        public IListItem Selected
        {
            get
            {
                if (Items == null || SelectedIndex < 0 || SelectedIndex >= Items.Count)
                    return null;
                return Items[SelectedIndex];
            }
        }
        /// <summary>
        /// Gets or sets the sprite of the background
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite BackgroundSprite
        {
            get
            {
                return _backgroundSprite;
            }
            set
            {
                _backgroundSprite = value;
            }
        }
        private Sprite _backgroundSprite;
        /// <summary>
        /// Gets or sets the sprite of the selected item
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite SelectedItemSprite
        {
            get
            {
                return _selectedItemSprite;
            }
            set
            {
                _selectedItemSprite = value;
                if (Selected != null)
                    Selected.Button.Sprite = SelectedItemSprite;
            }
        }
        private Sprite _selectedItemSprite;
        /// <summary>
        /// Gets or sets the sprite of the list buttons
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite ListItemSprite
        {
            get
            {
                return _listItemSprite;
            }
            set
            {
                _listItemSprite = value;
                if (Items == null)
                    return;
                foreach (IListItem item in Items)
                    item.Button.Sprite = ListItemSprite;
                if (Selected == null)
                    return;
                Selected.Button.Sprite = SelectedItemSprite;
            }
        }
        private Sprite _listItemSprite;
        /// <summary>
        /// Gets or sets the sprite of the scrubber
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite ScrubberSprite
        {
            get
            {
                return _scrubberSprite;
            }
            set
            {
                _scrubberSprite = value;
            }
        }
        private Sprite _scrubberSprite;
        /// <summary>
        /// Gets or sets the index of the item to be drawn at the top of the scroll box
        /// </summary>
        [Browsable(false)]
        public int ScrollIndex
        {
            get
            {
                return _scrollIndex;
            }
            set
            {
                int itemCount = 0;
                if (Items != null)
                    itemCount = Items.Count;
                int last = _scrollIndex;
                int maxIndex = itemCount - (int)Math.Floor((float)Size.Height / ItemHeight);
                _scrollIndex = value;
                if (_scrollIndex < 0)
                    _scrollIndex = 0;
                if (_scrollIndex > maxIndex)
                    _scrollIndex = maxIndex;
                if (_scrollIndex > itemCount)
                    _scrollIndex = itemCount;
                if (last != _scrollIndex)
                    OnScrollIndexChanged(EventArgs.Empty);
            }
        }
        private int _scrollIndex;
        /// <summary>
        /// Gets or sets the width of the scroll bar and its scrubber
        /// </summary>
        public int ScrollBarWidth
        {
            get
            {
                return _scrollBarWidth;
            }
            set
            {
                int last = _scrollBarWidth;
                _scrollBarWidth = value;
                if (last != _scrollBarWidth)
                    OnScrollBarWidthChanged(EventArgs.Empty);
            }
        }
        private int _scrollBarWidth;
        /// <summary>
        /// Gets or sets the height of each list item
        /// </summary>
        public int ItemHeight
        {
            get
            {
                return _itemHeight;
            }
            set
            {
                int last = _itemHeight;
                _itemHeight = value;
                if (last != _itemHeight)
                    OnItemHeightChanged(EventArgs.Empty);
            }
        }
        private int _itemHeight;
        /// <summary>
        /// Gets or sets the context menu of the list items
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
                    foreach (IListItem item in Items)
                        item.Button.ContextMenu = ListContextMenu;
            }
        }
        private ContextMenu _listContextMenu;
        private Rectangle scrubberPos;
        private Point scrubberDragOffset;
        private bool scrubberDragging;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the selected item changes
        /// </summary>
        public event EventHandler<ListItemEventArgs> SelectedChanged;
        /// <summary>
        /// Occurs when an item is added to the scroll box
        /// </summary>
        public event EventHandler<ListItemEventArgs> ItemAdded;
        /// <summary>
        /// Occurs when an item is removed from the scroll box
        /// </summary>
        public event EventHandler<ListItemEventArgs> ItemRemoved;
        /// <summary>
        /// Occurs when the value of the scroll index property is changed
        /// </summary>
        public event EventHandler ScrollIndexChanged;
        /// <summary>
        /// Occurs when the value of the scoll bar width property is changed
        /// </summary>
        public event EventHandler ScrollBarWidthChanged;
        /// <summary>
        /// Occurs when the value of the item height property is changed
        /// </summary>
        public event EventHandler ItemHeightChanged;
        /// <summary>
        /// Occurs when the value of the items property is changed
        /// </summary>
        public event EventHandler ItemsChanged;
        #endregion

        #region Event Handlers
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
        protected virtual void OnScrollIndexChanged(EventArgs e)
        {
            if (ScrollIndexChanged != null)
                ScrollIndexChanged(this, e);
        }
        protected virtual void OnScrollBarWidthChanged(EventArgs e)
        {
            if (ScrollBarWidthChanged != null)
                ScrollBarWidthChanged(this, e);
        }
        protected virtual void OnItemHeightChanged(EventArgs e)
        {
            if (ItemHeightChanged != null)
                ItemHeightChanged(this, e);
        }
        protected virtual void OnItemsChanged(EventArgs e)
        {
            if (ItemsChanged != null)
                ItemsChanged(this, e);
        }
        #endregion

        public ScrollBox(Rectangle pos, List<IListItem> items, Control parent = null)
            : base(pos, parent)
        {
            Initialize(items);
        }

        public ScrollBox(ScrollBoxConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize(null);
            BackgroundSprite = new Sprite(config.BackgroundSpritePath);
            SelectedItemSprite = new Sprite(config.SelectedItemSpritePath);
            ListItemSprite = new Sprite(config.ListItemSpritePath);
            ScrubberSprite = new Sprite(config.ScrubberSpritePath);
            ScrollBarWidth = config.ScrollBarWidth;
            ItemHeight = config.ItemHeight;
        }

        private void Initialize(List<IListItem> items)
        {
            Items = items;
            _itemHeight = 30;
            _selected = 0;
            _backgroundSprite = DefaultBackgroundSprite;
            _selectedItemSprite = DefaultSelectedItemSprite;
            _listItemSprite = DefaultListItemSprite;
            _scrubberSprite = DefaultScrubberSprite;
            _scrollIndex = -1;
            _scrollBarWidth = 15;
            UpdateItems();
            Font = DefaultFont;

            ScrollIndexChanged += ScrollBox_ScrollIndexChanged;
            SelectedChanged += ScrollBox_SelectedChanged;
            ScrollBarWidthChanged += ScrollBox_ScrollBarWidthChanged;
            MouseWheel += ScrollBox_MouseWheel;
            ItemsChanged += ScrollBox_ItemsChanged;
        }

        private void ScrollBox_ItemsChanged(object sender, EventArgs e)
        {
            UpdateItems();
        }
        private void ScrollBox_ScrollIndexChanged(object sender, EventArgs e)
        {
            UpdateItems();
        }
        private void ScrollBox_MouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta > 0 ? 1 : -1; 
            ScrollIndex += delta;
        }
        private void ScrollBox_ScrollBarWidthChanged(object sender, EventArgs e)
        {
            UpdateItems();
        }
        private void ScrollBox_SelectedChanged(object sender, ListItemEventArgs e)
        {
            if (Items == null)
                return;
            foreach (IListItem item in Items)
            {
                if (item.Button != null)
                    item.Button.Sprite = ListItemSprite;
            }
            if (Selected != null && Selected.Button != null)
                Selected.Button.Sprite = SelectedItemSprite;
        }

        private void UpdateItems()
        {
            if (Items == null)
                return;
            foreach (Control child in Children)
                RemoveChild(child);
            int maxIndex = Math.Min(ScrollIndex + (int)Math.Floor((float)Size.Height / ItemHeight), Items.Count);
            if (_scrollIndex < 0)
                _scrollIndex = 0;
            for (int i = ScrollIndex; i < maxIndex; i++)
			{
                if (i > Items.Count || i < 0)
                    continue;
                Rectangle pos = new Rectangle(0, (i - ScrollIndex) * ItemHeight, Size.Width - ScrollBarWidth, ItemHeight);
                Items[i].Button = new Button(pos, this);
                Items[i].Button.Text = Items[i].Text;
                Items[i].Button.Cursor = Cursor;
                Items[i].Button.ContextMenu = ListContextMenu;
                Items[i].Button.Sprite = ListItemSprite;
                Items[i].Button.Font = Font;
                Items[i].Button.Visible = true;
                int locali = i;
                Items[i].Button.MouseClick += (s, a) =>
                {
                    SelectedIndex = locali;
                };
                Items[i].Button.MouseWheel += ScrollBox_MouseWheel;
            }
            if (Selected != null && Selected.Button != null)
                Selected.Button.Sprite = SelectedItemSprite;
        }

        public override bool Update()
        {
            if (scrubberPos.Contains(InputManager.Instance.MouseScreenPos()) && InputManager.Instance.OnMouseDown(MouseButton.Left))
            {
                scrubberDragOffset = InputManager.Instance.MouseScreenPos() - new Point(scrubberPos.X, scrubberPos.Y);
                scrubberDragging = true;
            }

            if (scrubberDragging && InputManager.Instance.OnMouseUp(MouseButton.Left))
                scrubberDragging = false;

            if (scrubberDragging)
            {
                int mousePos = InputManager.Instance.MouseScreenPos().Y - scrubberDragOffset.Y - AbsoluteLocation.Y;
                if (Items == null)
                    ScrollIndex = 0;
                else
                    ScrollIndex = (int)((float)mousePos / Size.Height * (Items.Count == 0 ? 1f : Items.Count));
            }

            int itemsDisplayed = (int)Math.Floor((float)Size.Height / ItemHeight);
            int scrubberOffset = 0;
            int scrubberHeight = 0;
            if (Items != null)
            {
                scrubberOffset = (int)((ScrollIndex / (Items.Count == 0 ? 1f : Items.Count)) * Size.Height);
                scrubberHeight = (int)(Size.Height * (itemsDisplayed / (float)Items.Count));
            }
            if (scrubberHeight > Size.Height)
                scrubberHeight = Size.Height;
            scrubberPos = new Rectangle(
                AbsoluteBounds.X + (Size.Width - ScrollBarWidth),
                AbsoluteBounds.Y + scrubberOffset,
                ScrollBarWidth,
                scrubberHeight);

            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                BackgroundSprite.DrawNineCut(sb, AbsoluteBounds, null, Color);
                ScrubberSprite.DrawNineCut(sb, scrubberPos, null, Color, 5);
            }
            base.Draw(sb);
        }
    }
}
