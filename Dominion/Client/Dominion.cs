using ArwicEngine.Core;
using ArwicEngine.Forms;
using Dominion.Client.Scenes;
using Dominion.Server;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Dominion.Client
{
    public enum Scene
    {
        Menu,
        Game
    }

    public class Dominion : Game
    {
        private Engine engine;

        public Dominion()
        {
            engine = new Engine(this);
            Control.Content = engine.Content;

            RichText.RichTextRules = new List<RichTextParseRule>()
            {
                new RichTextParseRule("population", ((char)FontSymbol.Users).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("food", ((char)FontSymbol.Apple).ToString(), Color.PaleGreen, RichText.SymbolFont),
                new RichTextParseRule("production", ((char)FontSymbol.Gavel).ToString(), Color.Orange, RichText.SymbolFont),
                new RichTextParseRule("gold", ((char)FontSymbol.Coin).ToString(), Color.Goldenrod, RichText.SymbolFont),
                new RichTextParseRule("science", ((char)FontSymbol.Flask).ToString(), Color.LightSkyBlue, RichText.SymbolFont),
                new RichTextParseRule("science_w", ((char)FontSymbol.Flask).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("culture", ((char)FontSymbol.BookOpen).ToString(), Color.MediumPurple, RichText.SymbolFont),
                new RichTextParseRule("tourism", ((char)FontSymbol.Suitcase).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("faith", ((char)FontSymbol.Bolt).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("capital", ((char)FontSymbol.StarBlack).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("city", ((char)FontSymbol.Flag).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("strength", ((char)FontSymbol.Shield).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("cog", ((char)FontSymbol.Cog).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("book", ((char)FontSymbol.BookClosed).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("socialpolicy", ((char)FontSymbol.BookClosed).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("diplomacy", ((char)FontSymbol.GlobeEarth).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("happiness", ((char)FontSymbol.FaceHappy).ToString(), Color.Yellow, RichText.SymbolFont),
                new RichTextParseRule("house", ((char)FontSymbol.House).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("barchart", ((char)FontSymbol.BarChart).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("expand", ((char)FontSymbol.ResizeArrows).ToString(), Color.White, RichText.SymbolFont),
                new RichTextParseRule("flag", ((char)FontSymbol.Flag).ToString(), Color.White, RichText.SymbolFont),

            };

            //string[] symbols = Enum.GetNames(typeof(FontSymbol));
            //for (int i = 0; i < symbols.Length; i++)
            //{
            //    FontSymbol fs = (FontSymbol)i;
            //    char cfs = (char)fs;
            //    RichText.RichTextRules.Add(new RichTextParseRule(symbols[i], cfs.ToString(), Color.White, RichText.SymbolFont));
            //}

            GameManager manager = new GameManager();
            manager.Client = new Client(engine);
            manager.Server = new Server.Server(engine);
            engine.Scene.RegisterScene(new SceneMenu(engine, manager));
            engine.Scene.RegisterScene(new SceneGame(engine, manager));
            engine.Scene.ChangeScene(0);
        }

        protected override void Update(GameTime gameTime)
        {
            engine.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            engine.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
