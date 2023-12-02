using FishsGrandAdventure.Game;
using UnityEngine;

namespace FishsGrandAdventure.Network;

/// <summary>
/// The Broadcaster class is used to broadcast events to all clients except the host.
/// </summary>
public static class EventSyncer
{
    private const string SeaWorldSyncSignature = "NavidK0.SeaWorld.Sync";
    private const string GameEventSyncSignature = "NavidK0.GameEvent.Sync";
    private const string PlayersBlazedSync = "NavidK0.PlayersBlaze.Sync";
    private const string PlayersHeliumSync = "NavidK0.PlayersHelium.Sync";

    private const string SetLevelDataSignature = "NavidK0.SetLevelData.Sync";
    private const string SetWeatherSyncSignature = "NavidK0.SetWeather.Sync";
    private const string SetPlayerMovementSpeed = "NavidK0.SetPlayerMovementSpeed.Sync";

    static EventSyncer()
    {
        LC_API.ServerAPI.Networking.GetInt += GetInt;
        LC_API.ServerAPI.Networking.GetFloat += GetFloat;
        LC_API.ServerAPI.Networking.GetString += GetString;
    }

    /// <summary>
    /// Broadcasts the SeaWorld event.
    /// </summary>
    public static void SeaWorldSyncAll()
    {
        LC_API.ServerAPI.Networking.Broadcast(0, SeaWorldSyncSignature);
    }

    /// <summary>
    /// Broadcasts a GameEvent.
    /// </summary>
    /// <param name="gameEvent"></param>
    public static void GameEventSyncAll(GameEvent gameEvent)
    {
        LC_API.ServerAPI.Networking.Broadcast((int)gameEvent, GameEventSyncSignature);
    }

    /// <summary>
    /// Broadcasts a SelectableLevel.
    /// </summary>
    public static void SetLevelDataSyncAll(SelectableLevel level)
    {
        string levelJson = JsonUtility.ToJson(level);
        LC_API.ServerAPI.Networking.Broadcast(levelJson, GameEventSyncSignature);
    }

    public static void SetWeatherSyncAll(LevelWeatherType weatherType)
    {
        LC_API.ServerAPI.Networking.Broadcast((int)weatherType, SetWeatherSyncSignature);
    }

    public static void SetPlayerMovementSpeedSyncAll(float newSpeed)
    {
        LC_API.ServerAPI.Networking.Broadcast(newSpeed, SetPlayerMovementSpeed);
    }
    
    public static void PlayersBlazedSyncAll()
    {
        LC_API.ServerAPI.Networking.Broadcast(0, PlayersBlazedSync);
    }
    
    public static void PlayersHeliumSyncAll()
    {
        LC_API.ServerAPI.Networking.Broadcast(0, PlayersHeliumSync);
    }

    private static void GetInt(int data, string signature)
    {
        if (signature == SeaWorldSyncSignature)
        {
            EventManager.SetupSeaWorld();
        }

        if (signature == GameEventSyncSignature)
        {
            GameState.CurrentEvent = (GameEvent)data;
        }

        if (signature == SetWeatherSyncSignature)
        {
            EventManager.SetWeather((LevelWeatherType)data);
        }
        
        if (signature == PlayersBlazedSync)
        {
            EventManager.SetupBlazed();
        }
        
        if (signature == PlayersHeliumSync)
        {
            EventManager.SetupHelium();
        }
    }

    private static void GetFloat(float data, string signature)
    {
        if (signature == SetPlayerMovementSpeed)
        {
            EventManager.SetPlayerMovementSpeed(data);
        }
    }

    private static void GetString(string data, string signature)
    {
        if (signature == SetLevelDataSignature)
        {
            SelectableLevel selectableLevel = JsonUtility.FromJson<SelectableLevel>(data);
            EventManager.SetupLevelData(selectableLevel);
        }
    }
}