using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Graphics;
using ArwicEngine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static ArwicEngine.Constants;
using System;

namespace ArwicInterfaceDesigner
{
    public class Scene1 : BaseScene
    {
        private class StringListItem : IListItem
        {
            public Button Button { get; set; }
            public RichText Text { get; set; }

            public StringListItem(string s)
            {
                Text = s.ToRichText();
            }
        }

        private const int START_POS_X = 100;
        private const int START_POS_Y = 100;

        public Form Form { get; set; }
        public Form DefaultForm => new Form(new Rectangle(0, 0, 400, 600));
        private SpriteBatch sb;

        public Scene1()
            : base()
        {
        }

        public void AddControl(string control)
        {
            switch (control)
            {
                case "button":
                    Button button = new Button(new Rectangle(START_POS_X, START_POS_Y, PROP_BUTTON_WIDTH, PROP_BUTTON_HEIGHT), Form);
                    button.Text = "New Button".ToRichText();
                    break;
                case "checkBox":
                    CheckBox checkBox = new CheckBox(new Rectangle(START_POS_X, START_POS_Y, PROP_CHECKBOX_DIM, PROP_CHECKBOX_DIM), Form);
                    checkBox.Text = "New CheckBox".ToRichText();
                    checkBox.Value = true;
                    break;
                case "comboBox":
                    ComboBox comboBox = new ComboBox(new Rectangle(START_POS_X, START_POS_Y, PROP_BUTTON_WIDTH, PROP_BUTTON_HEIGHT), null, Form);
                    comboBox.Text = "New ComboBox".ToRichText();
                    break;
                case "image":
                    Image image = new Image(new Rectangle(START_POS_X, START_POS_Y, 200, 200), new Sprite(GraphicsHelper.PixelTexture), null, Form);
                    break;
                case "label":
                    Label label = new Label(new Rectangle(START_POS_X, START_POS_Y, 0, 0), "New Label".ToRichText(), Form);
                    break;
                case "progressBar":
                    ProgressBar progressBar = new ProgressBar(new Rectangle(START_POS_X, START_POS_Y, PROP_BUTTON_WIDTH, PROP_BUTTON_HEIGHT), 1f, 0.25f, Form);
                    break;
                case "scrollBox":
                    ScrollBox scrollBox = new ScrollBox(new Rectangle(START_POS_X, START_POS_Y, PROP_BUTTON_WIDTH, 500), null, Form);
                    break;
                case "spinButton":
                    SpinButton spinButton = new SpinButton(new Rectangle(START_POS_X, START_POS_Y, 100, 100), null, Form);
                    break;
                case "textBox":
                    TextBox textBox = new TextBox(new Rectangle(START_POS_X, START_POS_Y, PROP_BUTTON_WIDTH, PROP_BUTTON_HEIGHT), Form);
                    break;
                case "textLog":
                    TextLog textLog = new TextLog(new Rectangle(START_POS_X, START_POS_Y, 500, 300), Form);
                    break;
                case "toolTip":
                    break;
            }
        }

        private List<IListItem> GetListItems(int count)
        {
            List<IListItem> items = new List<IListItem>(count);
            for (int i = 0; i < count; i++)
            {
                items.Add(new StringListItem($"Item {i}"));
            }
            return items;
        }

        public override void Enter()
        {
            sb = new SpriteBatch(GraphicsManager.Instance.Device);
            Form = DefaultForm;
        }

        public override void Leave()
        {
        }

        public override void Update()
        {
            if (Form != null)
            {
                Form.Visible = true;
                Form.Location = new Point(0, 0);
                Form.Update();
            }
        }

        public override void Draw()
        {
            sb.Begin();
            if (Form != null)
                Form.Draw(sb);
            sb.End();
        }
    }
}
