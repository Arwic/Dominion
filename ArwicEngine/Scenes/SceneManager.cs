// Dominion - Copyright (C) Timothy Ings
// SceneManager.cs
// This file contains classes that define the scene manager

using ArwicEngine.Core;
using System.Collections.Generic;

namespace ArwicEngine.Scenes
{
    public class SceneManager
    {
        // Singleton pattern
        private static object _lock_instance = new object();
        private static readonly SceneManager _instance = new SceneManager();
        public static SceneManager Instance
        {
            get
            {
                lock (_lock_instance)
                {
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Gets the name of the current scene
        /// </summary>
        public string CurrentSceneName => currentScene?.SceneName;

        private List<BaseScene> scenes = new List<BaseScene>();
        private BaseScene currentScene;

        private SceneManager() { }

        /// <summary>
        /// Registers the given scene so it can be switched to
        /// </summary>
        /// <param name="scene"></param>
        public void RegisterScene(BaseScene scene)
        {
            scene.SceneID = scenes.Count;
            scenes.Add(scene);
        }

        /// <summary>
        /// Changes to the scene with the given id
        /// </summary>
        /// <param name="id"></param>
        public void ChangeScene(int id)
        {
            if (id >= 0 && id < scenes.Count)
            {
                if (currentScene != null)
                {
                    // Call the current scene's leave method
                    currentScene.Leave();
                    ConsoleManager.Instance.WriteLine($"SceneManager: Changed scene '{currentScene.SceneName}' with '{scenes[id].SceneName}'", MsgType.Info);
                }
                else
                {
                    ConsoleManager.Instance.WriteLine($"SceneManager: Changed scene to '{scenes[id].SceneName}'", MsgType.Info);
                }
                // Call the new scene's enter method
                scenes[id].Enter();
                // update the current scene reference
                currentScene = scenes[id];
            }
        }

        /// <summary>
        /// Updates the current scene
        /// </summary>
        public void Update()
        {
            if (currentScene != null)
                currentScene.Update();
        }

        /// <summary>
        /// Draws the current scene
        /// </summary>
        public void Draw()
        {
            if (currentScene != null)
                currentScene.Draw();
        }
    }
}
