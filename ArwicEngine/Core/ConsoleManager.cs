// Dominion - Copyright (C) Timothy Ings
// ConsoleManager.cs
// This file defines classes that manage the console

using ArwicEngine.Forms;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ArwicEngine.Constants;

namespace ArwicEngine.Core
{
    public enum MsgType
    {
        Info,
        Warning,
        Failed,
        Done,
        Input,
        Debug,
        Return,
        ServerInfo,
        ServerWarning,
        ServerFailed,
        ServerDone,
        ServerDebug
    }

    public class ConsoleManager
    {
        /// <summary>
        /// Reference to the engine
        /// </summary>
        public Engine Engine { get; set; }
        /// <summary>
        /// Gets the lines in the console output
        /// </summary>
        public List<string> Lines { get; }

        private Dictionary<string, Func<List<string>, int>> commands = new Dictionary<string, Func<List<string>, int>>();

        /// <summary>
        /// Occurs when a line is written to the console output
        /// </summary>
        public event EventHandler<TextLogLineEventArgs> LineWritten;

        /// <summary>
        /// Occurs when the console's output is cleared
        /// </summary>
        public event EventHandler ClearLines;

        protected virtual void OnLineWritten(TextLogLineEventArgs e)
        {
            if (LineWritten != null)
                LineWritten(this, e);
        }
        protected virtual void OnClearLines(EventArgs e)
        {
            if (ClearLines != null)
                ClearLines(this, e);
        }

        /// <summary>
        /// Creates a new console manager
        /// </summary>
        /// <param name="engine"></param>
        public ConsoleManager(Engine engine)
        {
            // init vars
            Engine = engine;
            Lines = new List<string>();

            // register default commands
            RegisterCommand("set", f_set);
            RegisterCommand("echo", f_get);
            RegisterCommand("get", f_get);
            RegisterCommand("gfx_apply", f_gfx_apply);
            RegisterCommand("aud_apply", f_aud_apply);
            RegisterCommand("quit", f_quit);
            RegisterCommand("exit", f_quit);
            RegisterCommand("config_defaults", f_config_defaults);
            RegisterCommand("clc", f_clear);
            RegisterCommand("cls", f_clear);
            RegisterCommand("clear", f_clear);
            RegisterCommand("config_save", f_config_save);
            RegisterCommand("print_details", f_printdetails);
        }

        /// <summary>
        /// Registers a command with the console
        /// </summary>
        /// <param name="call"></param>
        /// <param name="func"></param>
        public void RegisterCommand(string call, Func<List<string>, int> func)
        {
            commands.Add(call, func);
        }

        /// <summary>
        /// Unregisters a command from the console
        /// </summary>
        /// <param name="call"></param>
        /// <param name="func"></param>
        public void UnregisterCommand(string call, Func<List<string>, int> func)
        {
            commands.Remove(call);
        }

        // default commands
        private int f_set(List<string> args)
        {
            if (args.Count < 2)
                return EXIT_FAILURE;
            StringBuilder sb = new StringBuilder();
            foreach (string s in args)
                sb.Append(s);
            Engine.Config.SetVar(args[0], args[1]);
            return EXIT_SUCCESS; ;
        }
        private int f_get(List<string> args)
        {
            if (args.Count < 1)
                return EXIT_FAILURE;
            WriteLine($"{Engine.Config.GetVar(args[0])}", MsgType.Return);
            return EXIT_SUCCESS;
        }
        private int f_gfx_apply(List<string> args)
        {
            Engine.Graphics.Apply();
            return EXIT_SUCCESS;
        }
        private int f_aud_apply(List<string> args)
        {
            Engine.Audio.Apply();
            return 0;
        }
        private int f_quit(List<string> args)
        {
            int exitstatus = EXIT_SUCCESS;
            if (args.Count != 0)
                exitstatus = Convert.ToInt32(args[0]);
            Environment.Exit(exitstatus);
            return exitstatus;
        }
        private int f_config_defaults(List<string> args)
        {
            Engine.Config.SetDefaults();
            return EXIT_SUCCESS;
        }
        private int f_clear(List<string> args)
        {
            Lines.Clear();
            OnClearLines(EventArgs.Empty);
            return EXIT_SUCCESS;
        }
        private int f_config_save(List<string> args)
        {
            string path = CONFIG_PATH;
            if (args.Count > 0)
                path = args[0];
            Engine.Config.Write(path);
            return EXIT_SUCCESS;
        }
        private int f_printdetails(List<string> args)
        {
            WriteLine($"CurrentDirectory: {Environment.CurrentDirectory}");
            WriteLine($"Is64BitOperatingSystem: {Environment.Is64BitOperatingSystem}");
            WriteLine($"Is64BitProcess: {Environment.Is64BitProcess}");
            WriteLine($"MachineName: {Environment.MachineName}");
            WriteLine($"OSVersion: {Environment.OSVersion}");
            WriteLine($"ProcessorCount: {Environment.ProcessorCount}");
            WriteLine($"UserDomainName: {Environment.UserDomainName}");
            WriteLine($"UserName: {Environment.UserName}");
            WriteLine($"CLRVersion: {Environment.Version}");
            WriteLine($"WorkingSet: {Environment.WorkingSet * 1e-6}MB");
            return 0;
        }

        /// <summary>
        /// Formats and then writes a line to the console
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        public void WriteLine(string msg, MsgType type = MsgType.Info)
        {
            // create a prefix string
            string prefix = "";
            string module = "";
            // create a time string
            string time = $"[{DateTime.Now.ToString("HH:mm:ss")}] ";
            Color prefixColor = Color.White;
            Color moduleColor = Color.White;
            Color clientModuleColor = Color.CornflowerBlue;
            Color serverModuleColor = Color.MediumPurple;
            // determine what prefix and color is needed
            switch (type)
            {
                case MsgType.Info:
                    moduleColor = clientModuleColor;
                    prefixColor = Color.White;
                    prefix = "INFO: ";
                    module = "CLIENT: ";
                    break;
                case MsgType.Warning:
                    moduleColor = clientModuleColor;
                    prefixColor = Color.Gold;
                    prefix = "WARNING: ";
                    module = "CLIENT: ";
                    break;
                case MsgType.Failed:
                    moduleColor = clientModuleColor;
                    prefixColor = Color.Red;
                    prefix = "FAILED: ";
                    module = "CLIENT: ";
                    break;
                case MsgType.Done:
                    moduleColor = clientModuleColor;
                    prefixColor = Color.Green;
                    prefix = "DONE: ";
                    module = "CLIENT: ";
                    break;
                case MsgType.Input:
                    moduleColor = clientModuleColor;
                    prefixColor = Color.LightGray;
                    prefix = "> ";
                    module = "";
                    break;
                case MsgType.Debug:
                    moduleColor = clientModuleColor;
                    prefixColor = Color.Purple;
                    prefix = "DEBUG: ";
                    module = "CLIENT: ";
                    break;
                case MsgType.Return:
                    moduleColor = Color.White;
                    prefixColor = Color.White;
                    prefix = "";
                    module = "";
                    break;
                case MsgType.ServerInfo:
                    moduleColor = serverModuleColor;
                    prefixColor = Color.White;
                    prefix = "INFO: ";
                    module = "SERVER: ";
                    break;
                case MsgType.ServerWarning:
                    moduleColor = serverModuleColor;
                    prefixColor = Color.Yellow;
                    prefix = "WARNING: ";
                    module = "SERVER: ";
                    break;
                case MsgType.ServerFailed:
                    moduleColor = serverModuleColor;
                    prefixColor = Color.Red;
                    prefix = "FAILED: ";
                    module = "SERVER: ";
                    break;
                case MsgType.ServerDone:
                    moduleColor = serverModuleColor;
                    prefixColor = Color.Green;
                    prefix = "DONE: ";
                    module = "SERVER: ";
                    break;
                default:
                    break;
            }
            // write the fully formatted string to console
            string[] msgSplit = msg.Split('\n');
            foreach (string s in msgSplit)
            {
                string final = $"{time}{module}{prefix}{s}";
                Lines.Add(final);
                Console.Out.WriteLine(final);
                OnLineWritten(new TextLogLineEventArgs(new RichText(new RichTextSection(time, Color.LightGray), new RichTextSection(module, moduleColor), new RichTextSection(prefix, prefixColor), new RichTextSection(s, Color.Lerp(Color.White, prefixColor, 0.8f)))));
            }
        }

        /// <summary>
        /// Attempts to run a command
        /// </summary>
        /// <param name="input"></param>
        /// <param name="silent"></param>
        public void RunCommand(string input, bool silent = false)
        {
            string[] split = input.Split(' ');
            string cmd = split[0];
            List<string> args = new List<string>();
            for (int i = 1; i < split.Length; i++)
            {
                args.Add(split[i]);
            }
            if (!silent) WriteLine(input, MsgType.Input);
            bool commandExists = false;

            foreach (var c in commands)
            {
                if (c.Key == cmd)
                {
                    commandExists = true;
                    c.Value(args);
                }
            }
            if (!commandExists)
                WriteLine("Command does not exist", MsgType.Failed);
        }

        /// <summary>
        /// Runs a list of commands from a script file
        /// This should be used for loading user config on startup
        /// </summary>
        /// <param name="path"></param>
        public void RunScript(string path)
        {
            if (!File.Exists(path))
                return;
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line.Length > 2 && line[0] == '/' && line[1] == '/')
                    continue;
                RunCommand(line, true);
            }
        }
    }
}
