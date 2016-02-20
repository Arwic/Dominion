// Dominion - Copyright (C) Timothy Ings
// ContentExtensions.cs
// This file defines extension methods for the xna content pipline

using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;

namespace ArwicEngine.Core
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Loads all content of type T in the given directory and returns a dictionaray containing it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contentManager"></param>
        /// <param name="contentFolder"></param>
        /// <returns></returns>
        public static Dictionary<string, T> LoadListContent<T>(this ContentManager contentManager, string contentFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(contentManager.RootDirectory + "/" + contentFolder);
            if (!dir.Exists)
                throw new DirectoryNotFoundException();
            Dictionary<string, T> result = new Dictionary<string, T>();

            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                try
                {
                    string key = Path.GetFileNameWithoutExtension(file.Name);
                    result[key] = contentManager.Load<T>($"{contentFolder}/{key}");
                }
                catch (System.Exception) { }
            }
            return result;
        }
    }
}