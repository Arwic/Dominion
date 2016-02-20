// Dominion - Copyright (C) Timothy Ings
// InputManager.cs
// This file contains classes that define the input manager

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

    public class InputManager
    {
        /// <summary>
        /// Reference to the engine
        /// </summary>
        public Engine Engine { get; }

        /// <summary>
        /// Gets or sets the state of the keyboard last frame
        /// </summary>
        public KeyboardState LastKeyboardState { get; private set; }

        /// <summary>
        /// Gets or sets the state of the mouse last frame
        /// </summary>
        public MouseState LastMouseState { get; private set; }

        // is this still needed?
        private float yFix { get { return (Engine.Graphics.DeviceManager.IsFullScreen ? 1f : /*1.025f*/ 1f ); } }

        /// <summary>
        /// Create a new input manager
        /// </summary>
        /// <param name="engine"></param>
        public InputManager(Engine engine)
        {
            Engine = engine;
        }

        /// <summary>
        /// Updates the input manager
        /// This should be called at the end of update
        /// </summary>
        public void Update()
        {
            LastKeyboardState = Keyboard.GetState();
            LastMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Returns true if the given mouse button is curently pressed
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns true when the mouse button is released
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns true when the mouse button is first pressed
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns true when the scroll wheel is scrolled up
        /// </summary>
        /// <returns></returns>
        public bool ScrolledUp()
        {
            if (!Engine.WindowActive)
                return false;

            if (LastMouseState.ScrollWheelValue < Mouse.GetState().ScrollWheelValue)
                return true;
            return false;
        }

        /// <summary>
        /// Returns true when the scroll wheel is scrolled down
        /// </summary>
        /// <returns></returns>
        public bool ScrolledDown()
        {
            if (!Engine.WindowActive)
                return false;

            if (LastMouseState.ScrollWheelValue > Mouse.GetState().ScrollWheelValue)
                return true;
            return false;
        }

        /// <summary>
        /// Gets the current mouse scroll wheel value
        /// </summary>
        /// <returns></returns>
        public int CurrentMouseScrollWheelValue()
        {
            return Mouse.GetState().ScrollWheelValue;
        }

        /// <summary>
        /// Gets the value of the mosue scroll wheel last frame
        /// </summary>
        /// <returns></returns>
        public int LastMouseScrollWheelValue()
        {
            return LastMouseState.ScrollWheelValue;
        }

        /// <summary>
        /// Returns true if the given key is pressed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns true is the given key was down last frame but isn't this frame
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool WasKeyDown(Keys key)
        {
            if (!Engine.WindowActive)
                return false;

            if (LastKeyboardState.IsKeyDown(key))
                if (Keyboard.GetState().IsKeyUp(key))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns an array of the keys pressed this frame
        /// </summary>
        /// <returns></returns>
        public Keys[] GetPressedKeys()
        {
            return Keyboard.GetState().GetPressedKeys();
        }

        /// <summary>
        /// Returns an array of the keys pressed last frame
        /// </summary>
        /// <returns></returns>
        public Keys[] GetLastPressedKeys()
        {
            return LastKeyboardState.GetPressedKeys();
        }

        /// <summary>
        /// Gets the position of the mouse in screen space
        /// </summary>
        /// <returns></returns>
        public Point MouseScreenPos()
        {
            Point pos = Mouse.GetState().Position;
            return new Point((int)(pos.X * Engine.Graphics.Scale), (int)(pos.Y * Engine.Graphics.Scale * yFix));
        }

        /// <summary>
        /// Gets the position of the mouse in world space
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public Vector2 MouseWorldPos(Camera2 camera)
        {
            return camera.ConvertScreenToWorld(MouseScreenPos().ToVector2());
        }

        /// <summary>
        /// Gets the position of the mouse in screen space last frame
        /// </summary>
        /// <returns></returns>
        public Point LastMouseScreenPos()
        {
            Point pos = LastMouseState.Position;
            return new Point((int)(pos.X * Engine.Graphics.Scale), (int)(pos.Y * Engine.Graphics.Scale * yFix));
        }

        /// <summary>
        /// Gets the position of the mouse in world space last frame
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public Vector2 LastMouseWorldPos(Camera2 camera)
        {
            return camera.ConvertScreenToWorld(LastMouseScreenPos().ToVector2());
        }
    }
}
