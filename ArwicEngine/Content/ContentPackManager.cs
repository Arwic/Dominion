using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.IO.Compression;
using ArwicEngine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using ArwicEngine.Graphics;
using System.Xml.Serialization;

namespace ArwicEngine.Content
{
    public class ContentPackManager : ContentManager
    {
        private Dictionary<string, ZipArchive> loadedArchives = new Dictionary<string, ZipArchive>();

        private Dictionary<string, ContentPack> 

        public ContentPackManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory)
        {

        }

        /// <summary>
        /// Loads
        /// </summary>
        /// <param name="path"></param>
        public void LoadPack(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            string fullPath = Path.Combine(RootDirectory, path);

            fullPath = fullPath.Replace('\\', '/');

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Could not find the pack file '{fullPath}'");

            string name = Path.GetFileNameWithoutExtension(fullPath);

            ZipArchive archive = new ZipArchive(File.Open(fullPath, FileMode.Open), ZipArchiveMode.Read);
            loadedArchives.Add(name, archive);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                try
                {
                    if (entry.FullName == "TOC.xml")
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ContentToc));
                        return ()serializer.Deserialize(new StreamReader(file));
                    }

                    if (entry.FullName.Last() == '/')
                        continue; // don't try and load directories as files
                    string entryPath = name + ':' + entry.FullName;

                    string root = entry.FullName.Split('/').First();
                    // Model, Effect, SpriteFont, Texture, Texture2D, and TextureCube
                    ConsoleManager.Instance.WriteLine($"Loading '{entryPath}'");
                    switch (root)
                    {
                        case "Sprite": // Represents a 2D grid of texels.
                            LoadSprite(entryPath);
                            break;
                        case "Cursor":
                            LoadCursor(entryPath);
                            break;
                        case "Font": // Represents a font texture.
                            LoadFont(entryPath);
                            break;
                        case "Audio":
                            LoadAudio(entryPath);
                            break;
                        case "Shader": // Used to set and query effects, and to choose techniques.
                            Load<Effect>(entryPath);
                            break;
                        case "XML":
                            LoadXml(entryPath);
                            break;
                        default:
                            ConsoleManager.Instance.WriteLine($"Error in content pack '{name}', unknown root directory '{root}'", MsgType.Failed);
                            break;
                    }
                }
                catch (Exception)
                {
                    ConsoleManager.Instance.WriteLine($"Error in content pack '{name}', item '{entry.FullName}'", MsgType.Failed);
                }
            }
        }

        private void LoadSprite(string assetName)
        {
            Texture2D texture = Load<Texture2D>(assetName);
            Sprite sprite = new Sprite(texture);
            AddLoadedAsset(assetName, sprite);
        }

        private void LoadCursor(string assetName)
        {
            // standarise path seperators
            assetName = assetName.Replace('\\', '/');

            // check if the cursor is part of an asset pack
            if (assetName.Contains(':'))
            {
                string tempDir = "Content/_temp";
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                // get the asset pack and its relative path
                string[] assetNameParts = assetName.Split(':');
                string assetPack = assetNameParts[0];
                string assetPath = assetNameParts[1];

                // try getting the archive
                ZipArchive archive;
                if (!loadedArchives.TryGetValue(assetPack, out archive))
                    throw new Exception($"Asset pack '{assetPack}' does not exist or has not been loaded");

                // find the file in the archive
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName == assetPath)
                    {
                        // generate a new guid
                        Guid guid = Guid.NewGuid();
                        string finalPath = Path.Combine(tempDir, guid.ToString());
                        // create a temp file and save the entry into it
                        using (FileStream fs = File.Create(finalPath))
                            entry.Open().CopyTo(fs);
                        // load the temp file as a cursor
                        Cursor cursor = new Cursor(finalPath);
                        // add the cursor to the loaded assets
                        LoadedAssets.Add(assetName, cursor);
                    }
                }
            }
            else
            {
                // load the cursor that isn't in a pack
                Cursor cursor = new Cursor(Path.Combine(RootDirectory, assetName));
            }
        }

        private void LoadFont(string assetName)
        {
            SpriteFont spriteFont = Load<SpriteFont>(assetName);
            Font font = new Font(spriteFont);
            AddLoadedAsset(assetName, font);
        }

        private void LoadAudio(string assetName)
        {
            SoundEffect soundEffect = Load<SoundEffect>(assetName);
            AddLoadedAsset(assetName, soundEffect);
        }

        private void LoadXml(string assetName)
        {
            // Core:XML/Graphics/*.xml
            Stream stream = null;
            try
            {
                // standarise path seperators
                assetName = assetName.Replace('\\', '/');

                // check if the asset is part of an asset pack
                if (assetName.Contains(':'))
                {
                    // get the asset pack and its relative path
                    string[] assetNameParts = assetName.Split(':');
                    string assetPack = assetNameParts[0];
                    string assetPath = assetNameParts[1];

                    // try getting the archive
                    ZipArchive archive;
                    if (!loadedArchives.TryGetValue(assetPack, out archive))
                        throw new Exception($"Asset pack '{assetPack}' does not exist or has not been loaded");

                    // find the file in the archive
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName == assetPath)
                        {
                            stream = entry.Open();
                            break;
                        }
                    }
                }
                else
                {
                    // load an asset that isn't in a pack
                    var path = Path.Combine(RootDirectory, assetName) + ".xnb";
                    stream = File.Open(path, FileMode.Open);
                }
            }
            catch (FileNotFoundException fileNotFound)
            {
                throw new ContentLoadException("The content file was not found.", fileNotFound);
            }
            catch (DirectoryNotFoundException directoryNotFound)
            {
                throw new ContentLoadException("The directory was not found.", directoryNotFound);
            }
            catch (Exception exception)
            {
                throw new ContentLoadException("Opening stream error.", exception);
            }

            if (stream != null)
                AddLoadedAsset(assetName, stream);
        }

        private void AddLoadedAsset(string assetName, object asset)
        {
            string key = assetName.Remove(assetName.Length - 4);
            LoadedAssets.Add(key, asset);
        }

        protected override Stream OpenStream(string assetName)
        {
            try
            {
                // standarise path seperators
                assetName = assetName.Replace('\\', '/');

                // check if the asset is part of an asset pack
                if (assetName.Contains(':'))
                {
                    // get the asset pack and its relative path
                    string[] assetNameParts = assetName.Split(':');
                    string assetPack = assetNameParts[0];
                    string assetPath = assetNameParts[1];
                    
                    // try getting the archive
                    ZipArchive archive;
                    if (!loadedArchives.TryGetValue(assetPack, out archive))
                        throw new Exception($"Asset pack '{assetPack}' does not exist or has not been loaded");

                    // find the file in the archive
                    foreach (ZipArchiveEntry entry in archive.Entries)
                        if (entry.FullName == assetPath)
                            return entry.Open();
                }
                else
                {
                    // load an asset that isn't in a pack
                    var path = Path.Combine(RootDirectory, assetName) + ".xnb";
                    return File.Open(path, FileMode.Open);
                }
            }
            catch (FileNotFoundException fileNotFound)
            {
                throw new ContentLoadException("The content file was not found.", fileNotFound);
            }
            catch (DirectoryNotFoundException directoryNotFound)
            {
                throw new ContentLoadException("The directory was not found.", directoryNotFound);
            }
            catch (Exception exception)
            {
                throw new ContentLoadException("Opening stream error.", exception);
            }
            return null;
        }

        public T GetAsset<T>(string name)
        {
            object asset;
            if (LoadedAssets.TryGetValue(name, out asset))
                return (T)asset;
            ConsoleManager.Instance.WriteLine($"Attempting to access an asset that does not exist '{name}'");
            return default(T);
        }
    }
}
