// Dominion - Copyright (C) Timothy Ings
// PacketHeader.cs
// This file defines packet headers

namespace Dominion.Common
{
    public enum PacketHeader
    {
        Null = 0,
        // Lobby
        LobbyInit,
        LobbyEmpire,
        LobbyStateSync,
        LobbyKick,
        LobbyBan,
        LobbyStartGame,

        // Game data
        TurnState,
        TurnData,
        PlayerUpdate,
        TileUpdate,
        UnitUpdate,
        UnitAdded,
        UnitRemoved,
        UnitCommand,
        CityUpdate,
        CityAdded,
        CityRemoved,
        CityCommand,
        PlayerCommand,
        GameOver
    }
}
