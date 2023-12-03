using FishsGrandAdventure.Game;
using UnityEngine;

namespace FishsGrandAdventure.Network;

/// <summary>
/// The Broadcaster class is used to broadcast events to all clients except the host.
/// </summary>
public static class EventSyncer
{
    private const string GameEventSyncSignature = "NavidK0.GameEvent.Sync";
    private const string PlayersBlazedSignature = "NavidK0.PlayersBlaze.Sync";

    private const string SetLevelDataSignature = "NavidK0.SetLevelData.Sync";
    private const string SetWeatherSyncSignature = "NavidK0.SetWeather.Sync";
    private const string SetPlayerMovementSpeedSignature = "NavidK0.SetPlayerMovementSpeed.Sync";

    private const string SpawnExplosionSignature = "NavidK0.SpawnExplosion.Sync";

    static EventSyncer()
    {
        LC_API.ServerAPI.Networking.GetInt += GetInt;
        LC_API.ServerAPI.Networking.GetFloat += GetFloat;
        LC_API.ServerAPI.Networking.GetString += GetString;
        LC_API.ServerAPI.Networking.GetVector3 += GetVector3;
        
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
        LC_API.ServerAPI.Networking.Broadcast(newSpeed, SetPlayerMovementSpeedSignature);
    }

    public static void PlayersBlazedSyncAll()
    {
        LC_API.ServerAPI.Networking.Broadcast(0, PlayersBlazedSignature);
    }

    public static void SpawnExplosionSyncAll(Vector3 position)
    {
        LC_API.ServerAPI.Networking.Broadcast(position, SpawnExplosionSignature);
    }

    private static void GetInt(int data, string signature)
    {
        if (signature == GameEventSyncSignature)
        {
            GameState.CurrentEvent = (GameEvent)data;
        }

        if (signature == SetWeatherSyncSignature)
        {
            EventManager.SetWeatherClient((LevelWeatherType)data);
        }

        if (signature == PlayersBlazedSignature)
        {
            EventManager.SetupBlazedClient();
        }
    }

    private static void GetFloat(float data, string signature)
    {
        if (signature == SetPlayerMovementSpeedSignature)
        {
            EventManager.SetPlayerMovementSpeedClient(data);
        }
    }

    private static void GetString(string data, string signature)
    {
        if (signature == SetLevelDataSignature)
        {
            SelectableLevel selectableLevel = JsonUtility.FromJson<SelectableLevel>(data);
            EventManager.SetupLevelDataClient(selectableLevel);
        }
    }

    private static void GetVector3(Vector3 data, string signature)
    {
        if (signature == SpawnExplosionSignature)
        {
            EventManager.SpawnExplosionClient(data);
        }
    }
}