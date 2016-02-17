using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.TypeConverters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ArwicInterfaceDesigner
{
    public class Game1 : Game
    {
        public Engine Engine { get; set; }
        public Scene1 Scene { get; set; }

        public Game1(IntPtr drawSurface)
        {
            Engine = new Engine(this, drawSurface);
            System.Windows.Forms.Form gameForm = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
            gameForm.Opacity = 0;
            gameForm.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            gameForm.Text = "ArwicEngine Renderer - This window has an opacity of 0, use the main form";
            Control.Content = Engine.Content;
            RichText.RichTextRules = new List<RichTextParseRule>()
            {
                new RichTextParseRule("population", ((char)FontSymbol.Person).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("food", ((char)FontSymbol.Apple).ToString(), Color.PaleGreen, RichText.SymbolFont),
                new RichTextParseRule("production", ((char)FontSymbol.Gavel).ToString(), Color.Orange, RichText.SymbolFont),
                new RichTextParseRule("gold", ((char)FontSymbol.Coin).ToString(), Color.Goldenrod, RichText.SymbolFont),
                new RichTextParseRule("science", ((char)FontSymbol.Flask).ToString(), Color.LightSkyBlue, RichText.SymbolFont),
                new RichTextParseRule("science_w", ((char)FontSymbol.Flask).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("culture", ((char)FontSymbol.BookOpen).ToString(), Color.MediumPurple, RichText.SymbolFont),
                new RichTextParseRule("capital", ((char)FontSymbol.StarBlack).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("strength", ((char)FontSymbol.Shield).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("cog", ((char)FontSymbol.Cog).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("book", ((char)FontSymbol.BookClosed).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("socialpolicy", ((char)FontSymbol.BookClosed).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("diplomacy", ((char)FontSymbol.GlobeEarth).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("happiness", ((char)FontSymbol.FaceHappy).ToString(), Color.Yellow, RichText.SymbolFont),
                new RichTextParseRule("house", ((char)FontSymbol.House).ToString(), Color.Yellow, RichText.SymbolFont),
                new RichTextParseRule("barchart", ((char)FontSymbol.BarChart).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("expand", ((char)FontSymbol.ResizeArrows).ToString(), Color.White, RichText.SymbolFont),

            };
            SpriteConverter.Content = Engine.Content;
            Scene = new Scene1(Engine);
            Engine.Scene.RegisterScene(Scene);
            Engine.Scene.ChangeScene(0);
        }

        protected override void Update(GameTime gameTime)
        {
            Engine.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Engine.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
