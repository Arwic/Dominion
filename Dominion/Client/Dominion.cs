// Dominion - Copyright (C) Timothy Ings
// Dominion.cs
// This file defines classes that define the engines entry point

using ArwicEngine.Core;
using ArwicEngine.Forms;
using ArwicEngine.Scenes;
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
        public Dominion()
        {
            // initialise the engine
            Engine.Init(this);

            // add some rich text rules
            // TODO this probabbly isn't a good way of doing it
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

            // initialise the game manager
            GameManager manager = new GameManager();
            manager.Client = new Client();
            manager.Server = new Server.Server();

            // register scenes
            SceneManager.Instance.RegisterScene(new SceneMenu(manager));
            SceneManager.Instance.RegisterScene(new SceneGame(manager));
            SceneManager.Instance.ChangeScene((int)Scene.Menu);
        }

        protected override void Update(GameTime gameTime)
        {
            Engine.Instance.Update(gameTime); // update the engine
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Engine.Instance.Draw(gameTime); // draw the engine
            base.Draw(gameTime);
        }
    }
}
