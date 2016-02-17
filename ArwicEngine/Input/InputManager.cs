using ArwicEngine.Core;
using ArwicEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ArwicEngine.Input
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public class InputManager : IEngineComponent
    {
        public Engine Engine { get; }
        public KeyboardState LastKeyboardState { get; private set; }
        public MouseState LastMouseState { get; private set; }

        private float yFix { get { return (Engine.Graphics.DeviceManager.IsFullScreen ? 1f : 1.025f); } }

        public InputManager(Engine engine)
        {
            Engine = engine;
        }

        public void Update()
        {
            LastKeyboardState = Keyboard.GetState();
            LastMouseState = Mouse.GetState();
        }

        public bool IsMouseDown(MouseButton button)
        {
            if (!Engine.WindowActive)
                return false;

            switch (button)
            {
                case MouseButton.Left:
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case MouseButton.Right:
                    if (Mouse.GetState().RightButton == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case MouseButton.Middle:
                    if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
                        return true;
                    else
                        return false;
            }
            return false;
        }

        public bool OnMouseUp(MouseButton button)
        {
            if (!Engine.WindowActive)
                return false;

            switch (button)
            {
                case MouseButton.Left:
                    if (Mouse.GetState().LeftButton == ButtonState.Released && LastMouseState.LeftButton == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case MouseButton.Right:
                    if (Mouse.GetState().RightButton == ButtonState.Released && LastMouseState.RightButton == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case MouseButton.Middle:
                    if (Mouse.GetState().MiddleButton == ButtonState.Released && LastMouseState.MiddleButton == ButtonState.Pressed)
                        return true;
                    else
                        return false;
            }
            return false;
        }

        public bool OnMouseDown(MouseButton button)
        {
            if (!Engine.WindowActive)
                return false;

            switch (button)
            {
                case MouseButton.Left:
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released)
                        return true;
                    else
                        return false;
                case MouseButton.Right:
                    if (Mouse.GetState().RightButton == ButtonState.Pressed && LastMouseState.RightButton == ButtonState.Released)
                        return true;
                    else
                        return false;
                case MouseButton.Middle:
                    if (Mouse.GetState().MiddleButton == ButtonState.Pressed && LastMouseState.MiddleButton == ButtonState.Released)
                        return true;
                    else
                        return false;
            }
            return false;
        }

        public bool ScrolledUp()
        {
            if (!Engine.WindowActive)
                return false;

            if (LastMouseState.ScrollWheelValue < Mouse.GetState().ScrollWheelValue)
                return true;
            return false;
        }

        public bool ScrolledDown()
        {
            if (!Engine.WindowActive)
                return false;

            if (LastMouseState.ScrollWheelValue > Mouse.GetState().ScrollWheelValue)
                return true;
            return false;
        }

        public int CurrentMouseScrollWheelValue()
        {
            return Mouse.GetState().ScrollWheelValue;
        }

        public int LastMouseScrollWheelValue()
        {
            return LastMouseState.ScrollWheelValue;
        }

        public bool IsKeyDown(Keys key)
        {
            if (!Engine.WindowActive)
                return false;

            switch (key)
            {
                case Keys.CapsLock:
                    if (System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock))
                        return true;
                    return false;
                case Keys.NumLock:
                    if (System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.NumLock))
                        return true;
                    return false;
                case Keys.Scroll:
                    if (System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.Scroll))
                        return true;
                    return false;
                default:
                    if (Keyboard.GetState().IsKeyDown(key))
                        if (Keyboard.GetState().IsKeyUp(key))
                            return true;
                    return false;
            }
        }

        public bool WasKeyDown(Keys key)
        {
            if (!Engine.WindowActive)
                return false;

            if (LastKeyboardState.IsKeyDown(key))
                if (Keyboard.GetState().IsKeyUp(key))
                    return true;
            return false;
        }

        public Keys[] GetPressedKeys()
        {
            return Keyboard.GetState().GetPressedKeys();
        }

        public Keys[] GetLastPressedKeys()
        {
            return LastKeyboardState.GetPressedKeys();
        }

        public Point MouseScreenPos()
        {
            Point pos = Mouse.GetState().Position;
            return new Point((int)(pos.X * Engine.Graphics.Scale), (int)(pos.Y * Engine.Graphics.Scale * yFix));
        }

        public Vector2 MouseWorldPos(Camera2 camera)
        {
            return camera.ConvertScreenToWorld(MouseScreenPos().ToVector2());
        }

        public Point LastMouseScreenPos()
        {
            Point pos = LastMouseState.Position;
            return new Point((int)(pos.X * Engine.Graphics.Scale), (int)(pos.Y * Engine.Graphics.Scale * yFix));
        }

        public Vector2 LastMouseWorldPos(Camera2 camera)
        {
            return camera.ConvertScreenToWorld(LastMouseScreenPos().ToVector2());
        }
    }
}
