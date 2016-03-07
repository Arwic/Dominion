// Dominion - Copyright (C) Timothy Ings
// ConfigManager.cs
// This file defines classes that manage config variables

using System;
using System.Collections.Generic;
using System.IO;
using static ArwicEngine.Constants;

namespace ArwicEngine.Core
{
    /// <summary>
    /// Manages a list of variables
    /// </summary>
    public sealed class ConfigManager
    {
        private class Variable
        {
            public string Var { get; set; }
            public string Val { get; set; }

            public Variable(string var, string val)
            {
                Var = var;
                Val = val;
            }
        }

        // Singleton pattern
        private static object _lock_instance = new object();
        private static readonly ConfigManager _instance = new ConfigManager();
        public static ConfigManager Instance
        {
            get
            {
                lock (_lock_instance)
                {
                    return _instance;
                }
            }
        }

        //private List<string> vars = new List<string>();
        //private List<string> vals = new List<string>();
        private List<Variable> vars = new List<Variable>();

        /// <summary>
        /// Creates a new config manager
        /// </summary>
        private ConfigManager() { }

        /// <summary>
        /// Returns a variable's value
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public string GetVar(string var)
        {
            bool exists = false;
            int i;
            // loop through all variables
            for (i = 0; i < vars.Count; i++)
            {
                if (vars[i].Var == var)
                {
                    // break when we have found the var
                    exists = true;
                    break;
                }
            }
            // if we have found the var, return its value
            if (exists)
                return vars[i].Val;
            // else return null
            return "NULL";
        }

        /// <summary>
        /// Sets a variable's value
        /// </summary>
        /// <param name="var"></param>
        /// <param name="val"></param>
        public void SetVar(string var, string val)
        {
            // check if the var exists
            bool exists = false;
            int i;
            for (i = 0; i < vars.Count; i++)
            {
                if (vars[i].Var == var)
                {
                    exists = true;
                    break;
                }
            }
            // if it does, set the var to val
            if (exists)
            {
                vars[i].Val = val;
                return;
            }
            // else, add var = val as a new variable
            else
            {
                vars.Add(new Variable(var, val));
            }
        }

        /// <summary>
        /// Reset the config to its defaults
        /// </summary>
        public void SetDefaults()
        {
            ConsoleManager.Instance.WriteLine("Setting defaults", MsgType.Info);

            // clear/reset vars and vals
            vars = new List<Variable>();

            // set default variables
            SetVar(CONFIG_GFX_RESOLUTION, "1920x1080");
            SetVar(CONFIG_GFX_VSYNC, "0");
            SetVar(CONFIG_GFX_DISPLAY_MODE, "0");
            SetVar(CONFIG_AUD_MUSIC_ENABLED, "1");
            SetVar(CONFIG_AUD_MUSIC, "1");
            SetVar(CONFIG_AUD_SFX_ENABLED, "1");
            SetVar(CONFIG_AUD_SFX, "1");
            SetVar(CONFIG_NET_SERVER_PORT, "7894");
            SetVar(CONFIG_NET_SERVER_TIMEOUT, "2000");
            SetVar(CONFIG_NET_CLIENT_PORT, "7894");
            SetVar(CONFIG_NET_CLIENT_TIMEOUT, "2000");
            SetVar(CONFIG_NET_CLIENT_ADDRESS, "localhost");
            SetVar(CONFIG_GAME_AUTO_WORKER_DONT_REPLACE_IMPROVEMENTS, "0");
            SetVar(CONFIG_GAME_AUTO_WORKER_DONT_REMOVE_FEATURES, "0");
            SetVar(CONFIG_GAME_SHOW_REWARD_POPUPS, "1");
            SetVar(CONFIG_GAME_SHOW_TILE_RECOMMENDATIONS, "1");
            SetVar(CONFIG_GAME_AUTO_UNIT_CYCLE, "0");
            SetVar(CONFIG_GAME_SP_AUTO_END_TURN, "0");
            SetVar(CONFIG_GAME_MP_AUTO_END_TURN, "0");
            SetVar(CONFIG_GAME_SP_QUICK_COMBAT, "0");
            SetVar(CONFIG_GAME_MP_QUICK_COMBAT, "0");
            SetVar(CONFIG_GAME_SP_QUICK_MOVEMENT, "0");
            SetVar(CONFIG_GAME_MP_QUICK_MOVEMENT, "0");
            SetVar(CONFIG_GAME_SHOW_ALL_POLICY_INFO, "0");
            SetVar(CONFIG_GAME_SP_SCORE_LIST, "1");
            SetVar(CONFIG_GAME_MP_SCORE_LIST, "1");
            SetVar(CONFIG_GAME_AUTOSAVE_ENABLED, "1");
            SetVar(CONFIG_GAME_TURNS_BETWEEN_AUTOSAVES, "10");
            SetVar(CONFIG_GAME_AUTOSAVES_TO_KEEP, "10");
        }

        /// <summary>
        /// Writes the config to a text file
        /// </summary>
        /// <param name="path"></param>
        public void Write(string path)
        {
            List<string> file = new List<string>();
            if (File.Exists(path))
            {
                string[] fileArray = File.ReadAllLines(CONFIG_PATH);
                for (int i = 0; i < fileArray.Length; i++)
                    file.Add(fileArray[i]);
            }

            for (int vi = 0; vi < vars.Count; vi++)
            {
                bool found = false;
                for (int fi = 0; fi < file.Count; fi++)
                {
                    string fileVar = file[fi].Split(' ')?[1];
                    if (string.Compare(fileVar, vars[vi].Var) == 0)
                    {
                        file[fi] = $"set {vars[vi].Var} {vars[vi].Val}";
                        found = true;
                        break;
                    }
                }
                if (!found)
                    file.Add($"set {vars[vi].Var} {vars[vi].Val}");
            }

            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllLines(path, file);
            ConsoleManager.Instance.WriteLine($"Saved config vars to '{path}'");
        }
    }
}
