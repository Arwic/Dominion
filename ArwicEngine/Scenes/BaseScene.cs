// Dominion - Copyright (C) Timothy Ings
// BaseScene.cs
// This file contains classes that define a base scene for use with the scene manager

using ArwicEngine.Core;

namespace ArwicEngine.Scenes
{
    public abstract class BaseScene
    {
        /// <summary>
        /// Gets the name of the scene
        /// </summary>
        public string SceneName { get; }

        /// <summary>
        /// Gets the id of the scene
        /// This value should only be changed by the scene manager
        /// </summary>
        public int SceneID { get; set; } = -1;

        /// <summary>
        /// Creates a new scene
        /// </summary>
        /// <param name="engine"></param>
        public BaseScene()
        {
            SceneName = GetType().Name;
        }

        /// <summary>
        /// Occurs when the scene manager changes to this scene
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// Occurs when the scene manager changes away from this scene
        /// </summary>
        public abstract void Leave();

        /// <summary>
        /// Occurs when the engine updates
        /// </summary>
        /// <param name="delta"></param>
        public abstract void Update();

        /// <summary>
        /// Occurs when the engine draws
        /// </summary>
        /// <param name="delta"></param>
        public abstract void Draw();
    }
}
