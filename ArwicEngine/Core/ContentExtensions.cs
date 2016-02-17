﻿using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;

namespace ArwicEngine.Core
{
    public static class ContentExtensions
    {
        public static Dictionary<string, T> LoadListContent<T>(this ContentManager contentManager, string contentFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(contentManager.RootDirectory + "/" + contentFolder);
            if (!dir.Exists)
                throw new DirectoryNotFoundException();
            Dictionary<string, T> result = new Dictionary<string, T>();

            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);
                result[key] = contentManager.Load<T>($"{contentFolder}/{key}");
            }
            return result;
        }
    }
}