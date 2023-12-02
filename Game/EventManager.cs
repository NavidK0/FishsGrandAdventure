using FishsGrandAdventure.Effects;
using GameNetcodeStuff;
using UnityEngine;

namespace FishsGrandAdventure.Game;

public static class EventManager
{
    /// <summary>
    /// Sets up the SeaWorld event on the local client.
    /// </summary>
    public static void SetupSeaWorld()
    {
        SelectableLevel level = Object.Instantiate(RoundManager.Instance.currentLevel);

        level.overrideWeather = true;
        level.overrideWeatherType = LevelWeatherType.Flooded;
        level.currentWeather = LevelWeatherType.Flooded;
    }

    /// <summary>
    /// Sets up level data on the local client.
    /// </summary>
    /// <param name="level"></param>
    public static void SetupLevelData(SelectableLevel level)
    {
        StartOfRound.Instance.currentLevel = level;
        StartOfRound.Instance.currentLevelID = level.levelID;
        StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    /// <summary>
    /// Sets up the SeaWorld event on the local client.
    /// </summary>
    public static void SetWeather(LevelWeatherType weatherType)
    {
        SelectableLevel level = RoundManager.Instance.currentLevel;

        level.overrideWeather = true;
        level.overrideWeatherType = weatherType;
        level.currentWeather = weatherType;
    }

    public static void SetPlayerMovementSpeed(float newSpeed)
    {
        Plugin.Log.LogInfo("PREVIOUS SPEED: " + GameNetworkManager.Instance.localPlayerController.movementSpeed);

        StartOfRound.Instance.localPlayerController.movementSpeed = newSpeed;
    }

    public static void SetupBlazed()
    {
        PlayerControllerB localPlayer = StartOfRound.Instance.localPlayerController;
        PlayerEffectBlazed effect = localPlayer.gameObject.AddComponent<PlayerEffectBlazed>();

        effect.PlayerController = localPlayer;
    }

    public static void SetupHelium()
    {
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            PlayerEffectHelium effect = player.gameObject.AddComponent<PlayerEffectHelium>();
            effect.PlayerController = player;
        }
    }
}