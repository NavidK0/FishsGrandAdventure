using System.Threading.Tasks;
using FishsGrandAdventure.Behaviors;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace FishsGrandAdventure.Game;

public static class ClientHelper
{
    public static void SetWeather(LevelWeatherType weatherType)
    {
        SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
        currentLevel.currentWeather = weatherType;

        RoundManager roundManager = RoundManager.Instance;
        roundManager.currentLevel = currentLevel;
        roundManager.currentLevel.currentWeather = currentLevel.currentWeather;

        foreach (RandomWeatherWithVariables rw in roundManager.currentLevel.randomWeathers)
        {
            if (rw.weatherType == roundManager.currentLevel.currentWeather)
            {
                TimeOfDay.Instance.currentLevelWeather = rw.weatherType;
                TimeOfDay.Instance.currentWeatherVariable = rw.weatherVariable;
                TimeOfDay.Instance.currentWeatherVariable2 = rw.weatherVariable2;
            }
        }
    }

    public static void SetPlayerMoveSpeed(float newSpeed)
    {
        GameState.ForcePlayerMovementSpeed = newSpeed;
    }

    public static void SetupBlazedPlayer()
    {
        PlayerControllerB localPlayer = StartOfRound.Instance.localPlayerController;
        PlayerEffectBlazed effect = localPlayer.gameObject.AddComponent<PlayerEffectBlazed>();

        effect.PlayerController = localPlayer;
    }

    public static void SpawnExplosion(Vector3 position, float killRange = 1f, float damageRange = 4f)
    {
        Landmine.SpawnExplosion(position, true, killRange, damageRange);
    }

    public static void StrikeLightning(Vector3 packetPosition)
    {
        Vector3 transformPosition = packetPosition;
        transformPosition.y += 1f;

        StormyWeather stormy = Object.FindObjectOfType<StormyWeather>(true);
        stormy.gameObject.SetActive(true);

        stormy.LightningStrike(transformPosition, true);
    }

    public static void DestroyEffects()
    {
        // Remove effect components
        foreach (Effect effect in Object.FindObjectsOfType<Effect>())
        {
            Object.Destroy(effect);
        }
    }

    public static void ResetPlayerMoveSpeed()
    {
        GameState.ForcePlayerMovementSpeed = null;

        if (GameNetworkManager.Instance)
        {
            PlayerControllerB localController = GameNetworkManager.Instance.localPlayerController;
            if (localController != null)
            {
                GameNetworkManager.Instance.localPlayerController.movementSpeed = GameEventManager.DefaultMovementSpeed;
            }
        }
    }

    public static void TeleportPlayerIntoFactory(PlayerControllerB playerController, Vector3 pos)
    {
        if (Object.FindObjectOfType<AudioReverbPresets>())
        {
            Object.FindObjectOfType<AudioReverbPresets>().audioPresets[2].ChangeAudioReverbForPlayer(playerController);
        }

        playerController.DropAllHeldItems();

        playerController.isInElevator = false;
        playerController.isInHangarShipRoom = false;
        playerController.isInsideFactory = true;
        playerController.averageVelocity = 0f;
        playerController.velocityLastFrame = Vector3.zero;

        playerController.TeleportPlayer(pos);
        playerController.SyncBodyPositionWithClients();

        playerController.beamOutParticle.Play();

        if (playerController.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }
    }

    public static void TeleportNetworkObject(NetworkObject networkedObject, Vector3 position)
    {
        networkedObject.transform.position = position;
    }
}