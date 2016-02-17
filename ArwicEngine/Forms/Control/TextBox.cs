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
    public class TextBox : Control
    {
        #region Defaults
        public static Sprite DefaultSprite;
        public static new Cursor DefaultCursor;
        public static new Font DefaultFont;

        public static new void InitDefaults(Engine e)
        {
            DefaultSprite = new Sprite(e.Content, CONTROL_TEXTBOX);
            DefaultCursor = new Cursor(e.Window, CURSOR_TEXT_PATH);
            DefaultFont = new Font(e.Content, FONT_CONSOLAS_PATH);
        }
        #endregion

        #region Properties & Accessors
        /// <summary>
        /// Gets or sets the text associated with this control
        /// </summary>
        public new string Text
        {
            get
            {
                if (_text == null)
                    _text = "";
                return _text;
            }
            set
            {
                string last = _text;
                _text = value;
                if (last != _text)
                    OnTextChanged(EventArgs.Empty);
            }
        }
        private string _text;
        /// <summary>
        /// Gets or sets the value indicating whether the text should be masked with MaskedCharacter
        /// </summary>
        public bool Masked
        {
            get
            {
                return _masked;
            }
            set
            {
                bool last = _masked;
                _masked = value;
                if (last != _masked)
                    OnMaskedChanged(EventArgs.Empty);
            }
        }
        private bool _masked;
        /// <summary>
        /// Gets or sets the character to use when masking text
        /// </summary>
        public char MaskedCharacter
        {
            get
            {
                return _maskedChar;
            }
            set
            {
                char last = _maskedChar;
                _maskedChar = value;
                if (last != _maskedChar)
                    OnMaskedCharacterChanged(EventArgs.Empty);
            }
        }
        private char _maskedChar;
        /// <summary>
        /// Gets or sets the maximum number of characters the can be entered by the text box
        /// </summary>
        public int MaxCharacters { get; set; }
        /// <summary>
        /// Gets or sets the sprite used to draw the text box
        /// </summary>
        [TypeConverter(typeof(SpriteConverter))]
        public Sprite Sprite { get; set; }
        /// <summary>
        /// Gets or sets the value indicating whether the text box is capturing input
        /// </summary>
        [Browsable(false)]
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                bool last = _selected;
                _selected = value;
                if (last != _selected)
                    OnSelectedChanged(EventArgs.Empty);
            }
        }
        private bool _selected;
        /// <summary>
        /// Gets or sets the offset used when drawing text
        /// </summary>
        [TypeConverter(typeof(PointConverter))]
        public Point TextOffset { get; set; }
        /// <summary>
        /// Gets the tiemr that controls how often the caret blinks
        /// </summary>
        public Timer CaretBlinkTimer { get; private set; }
        /// <summary>
        /// Gets or sets the value used as a color modifier when the text box is capturing input
        /// </summary>
        public float SelectedColorPercent { get; set; }
        private int caretIndex = 0;
        private int anchorIndex = -1;
        private bool displayCaret = true;
        private bool mouseSelecting = false;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the value of MaskedCharacter is changed
        /// </summary>
        public event EventHandler MaskedCharacterChanged;
        /// <summary>
        /// Occurs when the value of Selected is changed
        /// </summary>
        public event EventHandler SelectedChanged;
        /// <summary>
        /// Occurs when the value of Masked is changed
        /// </summary>
        public event EventHandler MaskedChanged;
        /// <summary>
        /// Occurs when the enter key is pressed while the text box is selected
        /// </summary>
        public event EventHandler EnterPressed;
        #endregion

        #region Event Handlers
        protected virtual void OnMaskedCharacterChanged(EventArgs args)
        {
            if (MaskedCharacterChanged != null)
                MaskedCharacterChanged(this, args);
        }
        protected virtual void OnSelectedChanged(EventArgs args)
        {
            if (SelectedChanged != null)
                SelectedChanged(this, args);
        }
        protected virtual void OnMaskedChanged(EventArgs args)
        {
            if (MaskedChanged != null)
                MaskedChanged(this, args);
        }
        protected virtual void OnEnterPressed(EventArgs args)
        {
            if (EnterPressed != null)
                EnterPressed(this, args);
        }

        #endregion

        public TextBox(Rectangle pos, Control parent = null)
            : base(pos, parent)
        {
            Initialize();
        }

        public TextBox(TextBoxConfig config, Control parent = null)
            : base(config, parent)
        {
            Initialize();
            Masked = config.Masked;
            MaskedCharacter = config.MaskedCharacter;
            MaxCharacters = config.MaxCharacters;
            Sprite = new Sprite(Content, config.SpritePath);
            TextOffset = config.TextOffset;
            CaretBlinkTimer = new Timer(config.CaretBlinkTimer);
        }

        private void Initialize()
        {
            Font = DefaultFont;
            Cursor = DefaultCursor;
            Sprite = DefaultSprite;
            Masked = false;
            MaxCharacters = int.MaxValue;
            TextOffset = new Point(10, 5);
            CaretBlinkTimer = new Timer(500);
            SelectedColorPercent = 0.8f;
            MouseDown += TextBox_MouseDown;
            MouseUp += TextBox_MouseUp;
            MouseMove += TextBox_MouseMove;
            MouseLeave += TextBox_MouseLeave;
            EventInput.CharEntered += EventInput_CharEntered;
            EventInput.KeyDown += EventInput_KeyDown;
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseSelecting = false;
        }
        private void TextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseSelecting)
            {
                Point mouseRelPos = e.Position - AbsoluteLocation;
                Point measureText = Font.MeasureString(Text).ToPoint();
                if (mouseRelPos.X > measureText.X)
                {
                    caretIndex = Text.Length;
                    return;
                }
                int avgCharWidth = measureText.X / Text.Length;
                caretIndex = mouseRelPos.X / avgCharWidth;
            }
        }
        private void TextBox_MouseUp(object sender, MouseEventArgs e)
        {
            mouseSelecting = false;
        }
        private void TextBox_MouseDown(object sender, MouseEventArgs e)
        {
            RequestCapture();
            Point mouseRelPos = e.Position - AbsoluteLocation;
            Point measureText = Font.MeasureString(Text).ToPoint();
            if (mouseRelPos.X > measureText.X)
            {
                anchorIndex = Text.Length;
            }
            else
            {
                int avgCharWidth = measureText.X / Text.Length;
                anchorIndex = mouseRelPos.X / avgCharWidth;
            }
            caretIndex = anchorIndex;
            mouseSelecting = true;
        }
        private void EventInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (Visible && Selected)
            {
                if (e.KeyCode == Microsoft.Xna.Framework.Input.Keys.Left && e.KeyCode == Microsoft.Xna.Framework.Input.Keys.Right)
                {
                    return;
                }
                else if (e.KeyCode == Microsoft.Xna.Framework.Input.Keys.Left)
                {
                    if (e.Control)
                        while ((caretIndex > 0 && Text[caretIndex - 1] != ' ') && (caretIndex > 1 && Text[caretIndex - 2] != ' '))
                            caretIndex--;

                    if (e.Shift && anchorIndex == -1)
                        anchorIndex = caretIndex;
                    else if (!e.Shift)
                        anchorIndex = -1;
                    caretIndex--;
                    if (caretIndex < 0)
                        caretIndex = 0;
                }
                else if (e.KeyCode == Microsoft.Xna.Framework.Input.Keys.Right)
                {
                    if (e.Control)
                        while ((caretIndex < Text.Length && Text[caretIndex] != ' ') && (caretIndex < Text.Length - 1 && Text[caretIndex + 1] != ' '))
                            caretIndex++;

                    if (e.Shift && anchorIndex == -1)
                        anchorIndex = caretIndex;
                    else if (!e.Shift)
                        anchorIndex = -1;
                    caretIndex++;
                    if (caretIndex > Text.Length)
                        caretIndex = Text.Length;
                }
            }
        }
        private void EventInput_CharEntered(object sender, CharacterEventArgs e)
        {
            if (Visible && Selected)
            {
                if (char.IsControl(e.Character))
                    RecieveCommandInput(e.Character);
                else
                    RecieveTextInput(e.Character);
            }
        }

        private void RequestCapture()
        {
            Control parent = Parent;
            while (parent != null)
            {
                Canvas canvas = parent as Canvas;
                if (canvas != null)
                {
                    canvas.CaptureTextBox(this);
                    break;
                }
                parent = parent.Parent;
            }
        }
        private void RequestUnCapture()
        {
            Control parent = Parent;
            while (parent != null)
            {
                Canvas canvas = parent as Canvas;
                if (canvas != null)
                {
                    canvas.UnCaptureTextBox(this);
                    break;
                }
                parent = parent.Parent;
            }
        }

        private bool CaretAndAnchorValidPos()
        {
            return anchorIndex >= 0 && anchorIndex < Text.Length + 1 && caretIndex >= 0 && caretIndex < Text.Length + 1;
        }
        private string GetSelected()
        {
            if (anchorIndex != -1)
            {
                int selectionLength = Math.Abs(caretIndex - anchorIndex);
                if (anchorIndex < caretIndex)
                    return Text.Substring(anchorIndex, caretIndex - anchorIndex);
                else
                    return Text.Substring(caretIndex, anchorIndex - caretIndex);
            }
            return "";
        }
        private bool DeleteSelected()
        {
            if (anchorIndex != -1)
            {
                int selectionLength = Math.Abs(caretIndex - anchorIndex);
                if (anchorIndex < caretIndex)
                    Text = Text.Remove(anchorIndex, caretIndex - anchorIndex);
                else
                    Text = Text.Remove(caretIndex, anchorIndex - caretIndex);
                caretIndex = Math.Min(caretIndex, anchorIndex);
                anchorIndex = -1;
                return true;
            }
            return false;
        }
        private void Backspace()
        {
            if (Text.Length > 0)
            {
                if (!DeleteSelected())
                {
                    string beforeCaret = Text.Substring(0, caretIndex);
                    if (beforeCaret.Length != 0)
                    {
                        string afterCaret = Text.Substring(caretIndex, Text.Length - caretIndex);
                        beforeCaret = beforeCaret.Substring(0, caretIndex - 1);
                        Text = beforeCaret + afterCaret;
                        caretIndex--;
                    }
                }
            }
        }
        private void Delete()
        {
            if (!DeleteSelected())
            {
                if (caretIndex != Text.Length)
                {
                    Text = Text.Remove(caretIndex, 1);
                }
            }
        }
        private void Escape()
        {
            if (anchorIndex != -1)
                anchorIndex = -1;
            else
                Selected = false;
        }
        private void Cut()
        {
            string selected = GetSelected();
            if (selected != "")
            {
                System.Windows.Forms.Clipboard.SetText(selected);
                DeleteSelected();
            }
        }
        private void Copy()
        {
            string selected = GetSelected();
            if (selected != "")
            {
                System.Windows.Forms.Clipboard.SetText(selected);
            }
        }
        private void Paste()
        {
            RecieveTextInput(System.Windows.Forms.Clipboard.GetText());
        }
        private void SelectAll()
        {
            caretIndex = Text.Length;
            anchorIndex = 0;
        }

        private void RecieveTextInput(string text)
        {
            DeleteSelected();
            int lastTextLength = Text.Length;
            Text = Text.Insert(caretIndex, text);
            if (Text.Length != lastTextLength)
                caretIndex += text.Length;
        }
        private void RecieveTextInput(char text)
        {
            DeleteSelected();
            int lastTextLength = Text.Length;
            Text = Text.Insert(caretIndex, text.ToString());
            if (Text.Length != lastTextLength)
                caretIndex += 1;
        }
        private void RecieveCommandInput(char cmd)
        {
           switch (cmd)
            {
                case '\u0001': // control + a
                    SelectAll();
                    break;
                case '\u0018': // control + x
                    Cut();
                    break;
                case '\u0003': // control + c
                    Copy();
                    break;
                case '\u0016': // control + v
                    Paste();
                    break;
                case '\u001b': // esc
                    Escape();
                    break;
                case (char)0x2E: // TODO make this work, delete
                    Delete();
                    break;
                case '\b': // backspace
                    Backspace();
                    break;
                case '\r': // enter
                    OnEnterPressed(EventArgs.Empty);
                    break;
                case '\t': // tab
                    break;
                default:
                    break;
            }
        }

        public override bool Update(InputManager input)
        {
            if (!AbsoluteBounds.Contains(input.MouseScreenPos()) && input.OnMouseDown(MouseButton.Left))
                RequestUnCapture();

            if (Text == null)
                Text = "";

            if (!Selected)
            {
                anchorIndex = -1;
                caretIndex = Text.Length;
            }

            if (caretIndex > Text.Length)
                caretIndex = Text.Length;
            if (caretIndex < 0)
                caretIndex = 0;
            if (Selected)
            {
                // Blink the caret
                if (CaretBlinkTimer.Expired())
                {
                    displayCaret = !displayCaret;
                    CaretBlinkTimer.Start();
                }

                if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) || input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                    displayCaret = true;
            }
            else
            {
                displayCaret = false;
            }

            return base.Update(input);
        }

        public override void Draw(SpriteBatch sb)
        {
            Color c = Color;
            if (Selected)
                c = Color.Multiply(c, SelectedColorPercent);

            if (anchorIndex != -1 && CaretAndAnchorValidPos())
            {
                int selectionLength = Math.Abs(caretIndex - anchorIndex);
                string contentBeforeCaret = Text.Substring(0, caretIndex);
                string contentBeforeAnchor = Text.Substring(0, anchorIndex);
                int caretPos = (int)Font.MeasureString(contentBeforeCaret).X + TextOffset.X;
                int anchorPos = (int)Font.MeasureString(contentBeforeAnchor).X + TextOffset.X;
                int paddingY = 5;
                Rectangle selectionPos = new Rectangle();
                if (anchorIndex < caretIndex)
                    selectionPos = new Rectangle(AbsoluteLocation.X + anchorPos, AbsoluteLocation.Y + paddingY, caretPos - anchorPos, Size.Height - 2 * paddingY);
                else
                    selectionPos = new Rectangle(AbsoluteLocation.X + caretPos, AbsoluteLocation.Y + paddingY, anchorPos - caretPos, Size.Height - 2 * paddingY);
                GraphicsHelper.DrawRectFill(sb, selectionPos, new Color(Color.Black, 255));
            }

            Sprite.DrawNineCut(sb, AbsoluteBounds, null, c);

            if (Masked)
            {
                string maskedText = "";
                for (int i = 0; i < Text.Length; i++)
                    maskedText += MaskedCharacter;
                Font.DrawString(sb, maskedText, (AbsoluteLocation + TextOffset).ToVector2(), Color);
            }
            else
            {
                Font.DrawString(sb, Text, (AbsoluteLocation + TextOffset).ToVector2(), Color);
            }

            if (displayCaret)
            {
                string contentBeforeCaret = Text.Substring(0, caretIndex);
                float caretPos = Font.MeasureString(contentBeforeCaret).X + TextOffset.X + 5;
                GraphicsHelper.DrawLine(sb, new Vector2(AbsoluteLocation.X + caretPos, AbsoluteLocation.Y + 5), new Vector2(AbsoluteLocation.X + caretPos, AbsoluteLocation.Y + Size.Height - 5), 2, Color.White);
            }
            base.Draw(sb);
        }
    }
}
