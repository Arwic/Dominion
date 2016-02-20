// Dominion - Copyright (C) Timothy Ings
// Cursor.cs
// This file contains classes that define a win32 cursor object

using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ArwicEngine.Graphics
{
    public class Cursor
    {
        private System.Windows.Forms.Cursor cursor;
        private System.Windows.Forms.Form winForm;

        // exteral win32 cursor loading
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);
        
        /// <summary>
        /// Loads a cursor from file
        /// </summary>
        /// <param name="gw">the xna game winfow</param>
        /// <param name="path">path to the cur file</param>
        public Cursor(GameWindow gw, string path)
        {
             winForm = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(gw.Handle);
             cursor = LoadCustomCursor(path);
        }

        private System.Windows.Forms.Cursor LoadCustomCursor(string path)
        {
            IntPtr hCurs = LoadCursorFromFile(path);
            if (hCurs == IntPtr.Zero) throw new Win32Exception();
            System.Windows.Forms.Cursor curs = new System.Windows.Forms.Cursor(hCurs);
            // force the cursor to own the handle so it gets released properly
            FieldInfo fi = typeof(System.Windows.Forms.Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(curs, true);
            return curs;
        }

        /// <summary>
        /// Sets the gamewindow's cursor to be this cursor
        /// </summary>
        public void Enable()
        {
            winForm.Cursor = cursor;
        }
    }
}
