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
        public List<BasicPlayer> Players { get; set; }
        public WorldSize WorldSize { get; set; }
        public WorldType WorldType { get; set; }
        public GameSpeed GameSpeed { get; set; }
        public bool[] VictoryTypes { get; set; }
        public bool[] OtherOptions { get; set; }

        public LobbyState()
        {
            Players = new List<BasicPlayer>();
            VictoryTypes = new bool[Enum.GetNames(typeof(VictoryType)).Length];
            OtherOptions = new bool[Enum.GetNames(typeof(LobbyOtherOption)).Length];
            Defaults();
        }

        public void Defaults()
        {
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
