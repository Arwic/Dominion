using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArwicEngine.Forms
{
    public class SpinButton : Control
    {
        #region Properties & Accessors
        /// <summary>
        /// Gets the list of items in the spin button
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
        /// Adds an item to the spin button's items
        /// </summary>
        /// <param name="item">item to add</param>
        public void AddItem(IListItem item)
        {
            _items.Add(item);
            OnItemAdded(new ListItemEventArgs(item));
        }
        /// <summary>
        /// Removes an item to the spin button's items
        /// </summary>
        /// <param name="item">item to remove</param>
        public void RemoveItem(IListItem item)
        {
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
                return _selected;
            }
            set
            {
                int last = _selected;
                if (value >= 0 && value < Items.Length)
                    _selected = value;
                else
                    throw new IndexOutOfRangeException("SelectedIndex must be within range of Items");
                if (last != _selected)
                    OnSelectedChanged(new ListItemEventArgs(Selected));
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
                if (Items == null || SelectedIndex < 0 || SelectedIndex >= Items.Length)
                    return null;
                return Items[SelectedIndex];
            }
        }
        private Button btnForward;
        private Button btnBackward;
        private int buttonSep;
        private int buttonDim;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the selcted item changes
        /// </summary>
        public event EventHandler<ListItemEventArgs> SelectedChanged;
        /// <summary>
        /// Occurs when an item is added to the spin button
        /// </summary>
        public event EventHandler<ListItemEventArgs> ItemAdded;
        /// <summary>
        /// Occurs when an item is removed from the spin button
        /// </summary>
        public event EventHandler<ListItemEventArgs> ItemRemoved;
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
        #endregion

        public SpinButton(Rectangle pos, List<IListItem> items, Control parent = null)
            : base(pos, parent)
        {
            Initialize(items);
        }

        public SpinButton(ControlConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize(null);
        }

        private void Initialize(List<IListItem> items)
        {
            _items = items;

            buttonSep = 100;
            buttonDim = 50;

            btnBackward = new Button(new Rectangle(0, 0, buttonDim, buttonDim), this);
            btnBackward.Color = Color;
            btnBackward.Cursor = Cursor;
            btnBackward.Font = Font;
            btnBackward.Text = "<".ToRichText(); // TOD make this use a symbol font
            btnBackward.MouseClick += BtnBackward_MouseClick;

            btnForward = new Button(new Rectangle(buttonDim + buttonSep, 0, buttonDim, buttonDim), this);
            btnForward.Color = Color;
            btnForward.Cursor = Cursor;
            btnForward.Font = Font;
            btnForward.Text = ">".ToRichText(); // TOD make this use a symbol font
            btnForward.MouseClick += BtnForward_MouseClick;
        }

        private void BtnForward_MouseClick(object sender, MouseEventArgs e)
        {
            if (SelectedIndex + 1 < Items.Length)
                SelectedIndex++;
            else
                SelectedIndex = 0;
        }
        private void BtnBackward_MouseClick(object sender, MouseEventArgs e)
        {
            if (SelectedIndex - 1 >= 0)
                SelectedIndex--;
            else
                SelectedIndex = Items.Length - 1;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Visible)
            {
                if (Selected != null)
                {
                    Vector2 measuretext = Selected.Text.Measure();
                    Text.Draw(sb, new Vector2(AbsoluteLocation.X + buttonDim + buttonSep / 2 - measuretext.X / 2, AbsoluteLocation.Y + buttonDim / 2 - measuretext.Y / 2));
                }
            }
            base.Draw(sb);
        }
    }
}
