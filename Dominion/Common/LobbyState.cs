// Dominion - Copyright (C) Timothy Ings
// LobbyState.cs
// This file defines classes that defines a lobby state

using Dominion.Common.Entities;
using System;
using System.Collections.Generic;

namespace Dominion.Common
{
    public enum TurnEndReason
    {
        TimeOut,
        PlayersEnded,
        HostForced
    }

    public enum GameSpeed
    {
        Fast,
        Normal,
        Slow,
        Marathon
    }

    public enum VictoryType
    {
        Domination,
        Science,
        Culture,
        Diplomatic
    }

    public enum LobbyOtherOption
    {
        MaxTurns,
        DisableStartBias,
        NoAncientRuins,
        NoBarbarians,
        NoCityRazing,
        NoEspionage,
        OneCityChallenge,
        QuickCombat,
        QuickMovement,
        RagingBarbarians
    }
    
    [Serializable()]
    public class LobbyState
    {
        /// <summary>
        /// A list of players in the game
        /// </summary>
        public List<BasicPlayer> Players { get; set; }

        /// <summary>
        /// The selected world size
        /// </summary>
        public WorldSize WorldSize { get; set; }

        /// <summary>
        /// The selected world type
        /// </summary>
        public WorldType WorldType { get; set; }

        /// <summary>
        /// The selected game speed
        /// </summary>
        public GameSpeed GameSpeed { get; set; }

        /// <summary>
        /// An array of bools representing the state of victory types
        /// </summary>
        public bool[] VictoryTypes { get; set; }

        /// <summary>
        /// An array of bools representing the state of other options
        /// </summary>
        public bool[] OtherOptions { get; set; }

        public LobbyState()
        {
            Players = new List<BasicPlayer>();
            VictoryTypes = new bool[Enum.GetNames(typeof(VictoryType)).Length];
            OtherOptions = new bool[Enum.GetNames(typeof(LobbyOtherOption)).Length];
            Defaults();
        }

        /// <summary>
        /// Resets the lobby state to its defaults
        /// </summary>
        public void Defaults()
        {
            WorldSize = WorldSize.Medium;
            WorldType = WorldType.Pangea;
            GameSpeed = GameSpeed.Normal;

            VictoryTypes[(int)VictoryType.Culture] = true;
            VictoryTypes[(int)VictoryType.Diplomatic] = true;
            VictoryTypes[(int)VictoryType.Domination] = true;
            VictoryTypes[(int)VictoryType.Science] = true;

            OtherOptions[(int)LobbyOtherOption.DisableStartBias] = false;
            OtherOptions[(int)LobbyOtherOption.MaxTurns] = false;
            OtherOptions[(int)LobbyOtherOption.NoAncientRuins] = false;
            OtherOptions[(int)LobbyOtherOption.NoBarbarians] = false;
            OtherOptions[(int)LobbyOtherOption.NoCityRazing] = false;
            OtherOptions[(int)LobbyOtherOption.NoEspionage] = false;
            OtherOptions[(int)LobbyOtherOption.OneCityChallenge] = false;
            OtherOptions[(int)LobbyOtherOption.QuickCombat] = true;
            OtherOptions[(int)LobbyOtherOption.QuickMovement] = true;
            OtherOptions[(int)LobbyOtherOption.RagingBarbarians] = false;
        }
    }
}
