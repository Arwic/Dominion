// Dominion - Copyright (C) Timothy Ings
// SceneManager.cs
// This file contains classes that define the scene manager

using ArwicEngine.Core;
using System.Collections.Generic;

namespace ArwicEngine.Scenes
{
    public class SceneManager
    {
        /// <summary>
        /// Reference to the engine
        /// </summary>
        public Engine Engine { get; }

        /// <summary>
        /// Gets the name of the current scene
        /// </summary>
        public string CurrentSceneName => currentScene?.SceneName;

        private List<BaseScene> scenes;
        private BaseScene currentScene;

        /// <summary>
        /// Creates a new scene manager
        /// </summary>
        /// <param name="engine"></param>
        public SceneManager(Engine engine)
        {
            Engine = engine;
            scenes = new List<BaseScene>();
        }

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
                    Engine.Console.WriteLine($"SceneManager: Changed scene '{currentScene.SceneName}' with '{scenes[id].SceneName}'", MsgType.Info);
                }
                else
                {
                    Engine.Console.WriteLine($"SceneManager: Changed scene to '{scenes[id].SceneName}'", MsgType.Info);
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
        /// <param name="delta"></param>
        public void Update(float delta)
        {
            if (currentScene != null)
                currentScene.Update(delta);
        }

        /// <summary>
        /// Draws the current scene
        /// </summary>
        /// <param name="delta"></param>
        public void Draw(float delta)
        {
            if (currentScene != null)
                currentScene.Draw(delta);
        }
    }
}
