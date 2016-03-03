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
        private const string tempDir = "Content/_temp";

        private Dictionary<string, ZipArchive> loadedArchives = new Dictionary<string, ZipArchive>();

        public ContentPackManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory)
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        /// <summary>
        /// Loads all the assets in the given content pack
        /// </summary>
        /// <param name="path"></param>
        public void LoadPack(string path)
        {
            string fullPath = Path.Combine(RootDirectory, path);

            fullPath = fullPath.Replace('\\', '/');

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Could not find the pack file '{fullPath}'");

            string name = Path.GetFileNameWithoutExtension(fullPath);

            // don't keep the archive open after we are done loading assets from it
            using (ZipArchive archive = new ZipArchive(File.Open(fullPath, FileMode.Open), ZipArchiveMode.Read))
            {
                loadedArchives.Add(name, archive); // keep a reference to the archive

                // load every asset in the archive
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    try
                    {
                        if (entry.FullName.Last() == '/')
                            continue; // don't try and parse directories
                        string entryPath = $"{name}:{entry.FullName}";

                        string typeDir = entry.FullName.Split('/').First();
                        // Model, Effect, SpriteFont, Texture, Texture2D, and TextureCube
                        ConsoleManager.Instance.WriteLine($"Loading '{entryPath}'");
                        switch (typeDir)
                        {
                            case "Textures":
                                LoadSprite(entryPath);
                                break;
                            case "Cursors":
                                LoadCursor(entryPath);
                                break;
                            case "Fonts":
                                LoadFont(entryPath);
                                break;
                            case "Audio":
                                LoadAudio(entryPath);
                                break;
                            case "Shaders":
                                Load<Effect>(entryPath);
                                break;
                            case "XML":
                                LoadXml(entryPath);
                                break;
                            default:
                                ConsoleManager.Instance.WriteLine($"Error in content pack '{name}', unknown root directory '{typeDir}'", MsgType.Failed);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        ConsoleManager.Instance.WriteLine($"Error in content pack '{name}', item '{entry.FullName}'", MsgType.Failed);
                    }
                }

                loadedArchives.Remove(name); // dereference the archive
            }
        }

        // loads a sprite
        private void LoadSprite(string assetName)
        {
            // load texture and create new sprite
            Texture2D texture = Load<Texture2D>(assetName);
            Sprite sprite = new Sprite(texture);
            LoadedAssets.Remove(assetName); // remove texture's reference from loaded assets
            AddLoadedAsset(assetName, sprite); // add the sprite to the loaded assets
        }

        // loads a cursor
        private void LoadCursor(string assetName)
        {
            // standarise path seperators
            assetName = assetName.Replace('\\', '/');

            // check if the cursor is part of an asset pack
            if (assetName.Contains(':'))
            {
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
                        AddLoadedAsset(assetName, cursor);
                    }
                }
            }
            else
            {
                // load the cursor that isn't in a pack
                Cursor cursor = new Cursor(Path.Combine(RootDirectory, assetName));
                AddLoadedAsset(assetName, cursor);
            }
        }

        // loads a font
        private void LoadFont(string assetName)
        {
            // load spritefont and create a new font
            SpriteFont spriteFont = Load<SpriteFont>(assetName);
            Font font = new Font(spriteFont);
            LoadedAssets.Remove(assetName); // remove spritefont's reference from loaded assets
            AddLoadedAsset(assetName, font); // add the font to the loaded assets
        }

        // loads audio
        private void LoadAudio(string assetName)
        {
            SoundEffect soundEffect = Load<SoundEffect>(assetName);
            LoadedAssets.Remove(assetName); // remove the sound effect with key extension (pack:dir/dir/file.xnb)
            AddLoadedAsset(assetName, soundEffect); // add the sound effect without an extension (pack:dir/dir/file)
        }

        // loads xml files as streams
        private void LoadXml(string assetName)
        {
            // standardise
            assetName = assetName.Replace('\\', '/');
            
            // get xml type
            string xmlType = assetName.Split('/')[1];

            // get the asset pack and its relative path
            string[] assetNameParts = assetName.Split(':');
            string assetPack = assetNameParts[0];
            string assetPath = assetNameParts[1];

            // try getting the archive
            ZipArchive archive;
            if (!loadedArchives.TryGetValue(assetPack, out archive))
                throw new Exception($"Asset pack '{assetPack}' does not exist or has not been loaded");

            // find the file in the archive
            Stream stream = null;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName == assetPath)
                {
                    stream = entry.Open();
                    break;
                }
            }

            // parse xml file
            switch (xmlType)
            {
                case "AtlasDefinitions":
                    // load the atlas definition
                    SpriteAtlasDefinition atlasDef = SerializationHelper.XmlDeserialize<SpriteAtlasDefinition>(stream);
                    // ensure the sprite is loaded
                    //Sprite sprite = new Sprite(Load<Texture2D>(atlasDef.BaseTexturePath));
                    // instantiate and define a new atlas
                    SpriteAtlas atlas = new SpriteAtlas();
                    atlas.Define(atlasDef);
                    // add the atlas to the loaded assets dictionary
                    AddLoadedAsset(assetName, atlas);
                    break;
                default:
                    // memory stream is a better long term storage solution
                    MemoryStream ms = new MemoryStream();
                    stream.CopyTo(ms);
                    AddLoadedAsset(assetName, ms);
                    break;
            }
        }

        // adds an asset to the loaded assets dictionary with the correctally formatted key
        private void AddLoadedAsset(string assetName, object asset)
        {
            string ext = Path.GetExtension(assetName);
            string key = assetName.Remove(assetName.Length - ext.Length);
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

        /// <summary>
        /// Gets the asset with the given name
        /// names are to be fornmatted as such:
        /// pack:dir/subdir/file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetAsset<T>(string name)
        {
            object asset;
            if (LoadedAssets.TryGetValue(name, out asset))
            {
                if (typeof(T) == typeof(Stream))
                    ((Stream)asset).Position = 0;
                return (T)asset;
            }
            ConsoleManager.Instance.WriteLine($"Attempting to access an asset that does not exist '{name}'");
            return default(T);
        }
    }
}
