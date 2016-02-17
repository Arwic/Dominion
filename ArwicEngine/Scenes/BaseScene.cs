using ArwicEngine.Core;

namespace ArwicEngine.Scenes
{
    public abstract class BaseScene : IEngineComponent
    {
        public Engine Engine { get; }
        public string SceneName { get; }
        public int SceneID { get; set; } = -1;

        public BaseScene(Engine engine)
        {
            Engine = engine;
            SceneName = GetType().Name;
        }

        public abstract void Enter();
        public abstract void Leave();
        public abstract void Update(float delta);
        public abstract void Draw(float delta);
    }
}
