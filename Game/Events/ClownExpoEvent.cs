using System;
using System.Collections.Generic;
using System.Linq;
using FishsGrandAdventure.Effects;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using MEC;
using Unity.Netcode;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ClownExpoEvent : IGameEvent
{
    public const int MaxOutdoorJesters = 16;

    public static readonly List<string> CongratsTexts = new List<string>
    {
        "Congrats!",
        "Nice!",
        "Wow!",
        "Amazing!",
        "Awesome!",
        "Great!",
        "Cool!",
        "Incredible!",
        "Fantastic!",
        "Frytastic!",
        "Frytacular!",
        "French!",
        "Baguette!",
    };

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
                    "cash register" => 999,
                    "clown horn" => 200,
                    "airhorn" => 200,
                    _ => 0
                };
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            JesterAI jesterAi = spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<JesterAI>();
            if (jesterAi != null)
            {
                spawnableEnemyWithRarity.rarity = 9999;
            }
            else if (!spawnableEnemyWithRarity.enemyType.isOutsideEnemy)
            {
                spawnableEnemyWithRarity.rarity = 0;
            }
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
    public static double LastInputTime;
    public static double LastInputTimeGrabItem;

    [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectServerRpc")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void GrabObjectServerRpc(ref NetworkObjectReference grabbedObject)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.ClownExpo) return;

        // debounce
        if (LastInputTimeGrabItem + 0.5f > Time.timeAsDouble) return;

        if (grabbedObject.TryGet(out NetworkObject networkedObject))
        {
            GrabbableObject grabbableObject = networkedObject.GetComponent<GrabbableObject>();
            bool isCashRegister = grabbableObject.itemProperties.itemName.ToLower().Contains("cash register");

            PlayerControllerB playerControllerB = grabbableObject.playerHeldBy;

            if (playerControllerB == null || playerControllerB.isInHangarShipRoom) return;

            if (isCashRegister)
            {
                var packetSpawnEnemy = new PacketSpawnEnemy
                {
                    EnemyType = typeof(JesterAI),
                    LevelId = RoundManager.Instance.currentLevel.levelID,
                    ClientId = playerControllerB.actualClientId,
                    Position = playerControllerB.transform.position,
                    IsInside = playerControllerB.isInsideFactory,
                    ComponentsToAttach = new List<Type>
                    {
                        typeof(ExplodingJester)
                    }
                };

                NetworkUtils.BroadcastAll(packetSpawnEnemy);
            }

            LastInputTimeGrabItem = Time.timeAsDouble;
        }
    }

    [HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void OnActivateItemServerRpc(GrabbableObject __instance, bool onOff, bool buttonDown)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.ClownExpo) return;
        if (!buttonDown) return;

        // debounce
        if (LastInputTime + 0.5f > Time.timeAsDouble) return;

        bool isClownHorn = __instance.itemProperties.itemName.ToLower().Contains("clown horn");
        bool isAirHorn = __instance.itemProperties.itemName.ToLower().Contains("airhorn");

        PlayerControllerB playerControllerB = __instance.playerHeldBy;

        if (playerControllerB == null || playerControllerB.isInHangarShipRoom) return;

        if (isClownHorn)
        {
            int jesterCount = RoundManager.Instance.SpawnedEnemies.Count(se =>
                se.enemyType.enemyPrefab.GetComponent<JesterAI>() != null);

            if (jesterCount < ClownExpoEvent.MaxOutdoorJesters)
            {
                HUDManager.Instance.AddTextToChatOnServer(
                    $"{playerControllerB.playerUsername} impressed everyone! Party outside!"
                );

                for (int i = jesterCount; i < ClownExpoEvent.MaxOutdoorJesters; i++)
                {
                    NetworkUtils.BroadcastAll(new PacketSpawnEnemyOutside
                    {
                        EnemyType = typeof(JesterAI),
                        LevelId = RoundManager.Instance.currentLevel.levelID,
                        ForceOutside = true,
                        ComponentsToAttach = new List<Type>
                        {
                            typeof(ExplodingJester)
                        }
                    });
                }
            }
        }

        if (isAirHorn)
        {
            HUDManager.Instance.AddTextToChatOnServer(
                $"{playerControllerB.playerUsername} impressed a Jester! {ClownExpoEvent.CongratsTexts.GetRandomElement()}"
            );

            var packetSpawnEnemy = new PacketSpawnEnemy
            {
                EnemyType = typeof(JesterAI),
                LevelId = RoundManager.Instance.currentLevel.levelID,
                ClientId = playerControllerB.actualClientId,
                Position = playerControllerB.transform.position,
                IsInside = playerControllerB.isInsideFactory,
                ComponentsToAttach = new List<Type>
                {
                    typeof(ExplodingJester)
                }
            };

            NetworkUtils.BroadcastAll(packetSpawnEnemy);
        }

        LastInputTime = Time.timeAsDouble;
    }
}