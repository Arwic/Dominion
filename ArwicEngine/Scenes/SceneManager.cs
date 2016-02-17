using ArwicEngine.Core;
using System.Collections.Generic;

namespace ArwicEngine.Scenes
{
    public class SceneManager : IEngineComponent
    {
        public Engine Engine { get; }
        public string CurrentSceneName => currentScene?.SceneName;

        private List<BaseScene> scenes;
        private BaseScene currentScene;

        public SceneManager(Engine engine)
        {
            Engine = engine;
            scenes = new List<BaseScene>();
        }

        public void RegisterScene(BaseScene scene)
        {
            scene.SceneID = scenes.Count;
            scenes.Add(scene);
        }

        public void ChangeScene(int id)
        {
            if (id >= 0 && id < scenes.Count)
            {
                if (currentScene != null)
                {
                    currentScene.Leave();
                    Engine.Console.WriteLine($"SceneManager: Changed scene '{currentScene.SceneName}' with '{scenes[id].SceneName}'", MsgType.Info);
                }
                else
                {
                    Engine.Console.WriteLine($"SceneManager: Changed scene to '{scenes[id].SceneName}'", MsgType.Info);
                }
                scenes[id].Enter();
                currentScene = scenes[id];
            }
        }

        public void Update(float delta)
        {
            if (currentScene != null)
                currentScene.Update(delta);
        }

        public void Draw(float delta)
        {
            if (currentScene != null)
                currentScene.Draw(delta);
        }
    }
}
