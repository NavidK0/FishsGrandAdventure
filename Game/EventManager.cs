using FishsGrandAdventure.Effects;
using GameNetcodeStuff;
using UnityEngine;

namespace FishsGrandAdventure.Game;

public static class EventManager
{
    /// <summary>
    /// Sets up level data on the local client.
    /// </summary>
    /// <param name="level"></param>
    public static void SetupLevelDataClient(SelectableLevel level)
    {
        GameState.ForceLoadLevel = level;

        StartOfRound.Instance.currentLevel = level;
        StartOfRound.Instance.currentLevelID = level.levelID;
        StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    /// <summary>
    /// Sets up the SeaWorld event on the local client.
    /// </summary>
    public static void SetWeatherClient(LevelWeatherType weatherType)
    {
        SelectableLevel level = RoundManager.Instance.currentLevel;

        level.currentWeather = weatherType;

        GameState.ForceLoadLevel = level;
    }

    public static void SetPlayerMovementSpeedClient(float newSpeed)
    {
        GameState.ForcePlayerMovementSpeed = newSpeed;
    }

    public static void SetupBlazedClient()
    {
        PlayerControllerB localPlayer = StartOfRound.Instance.localPlayerController;
        PlayerEffectBlazed effect = localPlayer.gameObject.AddComponent<PlayerEffectBlazed>();

        effect.PlayerController = localPlayer;
    }

    public static void SpawnExplosionClient(Vector3 position)
    {
        Landmine.SpawnExplosion(position, true, 1f, 4f);
    }
}