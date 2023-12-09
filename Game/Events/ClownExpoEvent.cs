using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using MEC;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ClownExpoEvent : IGameEvent
{
    public string Description => $"Welcome to Clown Expo {DateTime.Today.Year}!";
    public Color Color => new Color32(252, 10, 0, 255);
    public GameEventType GameEventType => GameEventType.ClownExpo;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificItemsForEvent(level, new List<string> { "cash register" });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(JesterAI) });

        // Replace all items with cash registers :)
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap)
        {
            spawnableItemWithRarity.rarity =
                spawnableItemWithRarity.spawnableItem.itemName.ToLower() switch
                {
                    "cash register" => 100,
                    "clown horn" => 100,
                    "airhorn" => 100,
                    _ => 0
                };
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            JesterAI jesterAi =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<JesterAI>();
            spawnableEnemyWithRarity.rarity = jesterAi != null ? 100 : 0;
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}

public static class PatchClownExpo
{
    public static bool InvokedJesters;

    [HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void OnActivateItemServerRpc(GrabbableObject __instance, bool onOff, bool buttonDown)
    {
        if (GameState.CurrentGameEventType != GameEventType.ClownWorld) return;
        if (!buttonDown) return;

        bool isClownHorn = __instance.itemProperties.itemName.ToLower().Contains("clown horn");
        bool isAirHorn = __instance.itemProperties.itemName.ToLower().Contains("airhorn");

        PlayerControllerB playerControllerB = __instance.playerHeldBy;

        if (playerControllerB.isInHangarShipRoom) return;

        if (isClownHorn)
        {
            if (!InvokedJesters)
            {
                InvokedJesters = true;
                HUDManager.Instance.AddTextToChatOnServer(
                    $"{playerControllerB.playerUsername} is a clown! The expo is now outdoors!");

                for (var i = 0; i < 4; i++)
                {
                    ModUtils.SpawnEnemyOutside(typeof(JesterAI), RoundManager.Instance.currentLevel, true);
                }
            }

            Timing.RunCoroutine(SpawnLightning(playerControllerB));
        }

        if (isAirHorn)
        {
            Timing.RunCoroutine(SpawnExplosion(playerControllerB));
        }
    }

    private static IEnumerator<float> SpawnLightning(PlayerControllerB playerControllerB)
    {
        Vector3 transformPosition = playerControllerB.transform.position;

        yield return Timing.WaitForSeconds(1.5f);

        NetworkUtils.BroadcastAll(new PacketStrikeLightning
        {
            Position = transformPosition
        });
    }

    private static IEnumerator<float> SpawnExplosion(PlayerControllerB playerControllerB)
    {
        Vector3 transformPosition = playerControllerB.transform.position;

        yield return Timing.WaitForSeconds(2f);

        NetworkUtils.BroadcastAll(new PacketSpawnExplosion
        {
            Position = transformPosition
        });
    }
}