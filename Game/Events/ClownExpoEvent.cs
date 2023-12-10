using System;
using System.Collections.Generic;
using System.Linq;
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
    public const int MaxOutdoorJesters = 4;

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
                    "clown horn" => 50,
                    "airhorn" => 50,
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
    public static double LastInputTime;

    [HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void OnActivateItemServerRpc(GrabbableObject __instance, bool onOff, bool buttonDown)
    {
        if (GameState.CurrentGameEventType != GameEventType.ClownExpo) return;
        if (!buttonDown) return;

        // debounce
        if (LastInputTime + 0.5f > Time.timeAsDouble) return;

        bool isClownHorn = __instance.itemProperties.itemName.ToLower().Contains("clown horn");
        bool isAirHorn = __instance.itemProperties.itemName.ToLower().Contains("airhorn");

        PlayerControllerB playerControllerB = __instance.playerHeldBy;

        if (playerControllerB.isInHangarShipRoom) return;

        if (isClownHorn)
        {
            if (RoundManager.Instance.SpawnedEnemies.Count(se =>
                    se.enemyType.enemyPrefab.GetComponent<JesterAI>() != null) < ClownExpoEvent.MaxOutdoorJesters)
            {
                HUDManager.Instance.AddTextToChatOnServer(
                    $"{playerControllerB.playerUsername} impressed everyone! The expo is now outdoors!"
                );

                for (var i = 0; i < ClownExpoEvent.MaxOutdoorJesters; i++)
                {
                    ModUtils.SpawnEnemyOutside(typeof(JesterAI), RoundManager.Instance.currentLevel, true);
                }
            }
        }

        if (isAirHorn)
        {
            HUDManager.Instance.AddTextToChatOnServer(
                $"{playerControllerB.playerUsername} impressed a Jester! {ClownExpoEvent.CongratsTexts.GetRandomElement()}"
            );

            ModUtils.SpawnEnemy(
                typeof(JesterAI),
                RoundManager.Instance.currentLevel,
                playerControllerB.transform.position,
                playerControllerB
            );
        }

        LastInputTime = Time.timeAsDouble;
    }

    [HarmonyPatch(typeof(JesterAI), "Update")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void JesterAI(JesterAI __instance)
    {
        if (GameState.CurrentGameEventType != GameEventType.ClownExpo ||
            !RoundManager.Instance.IsServer ||
            __instance.isEnemyDead
           ) return;

        bool outdoorsJester = __instance.isOutside;

        if (outdoorsJester)
        {
            __instance.agent.speed = 5f;
        }

        if (__instance.currentBehaviourStateIndex == 2)
        {
            Timing.CallDelayed(.5f, () =>
            {
                NetworkUtils.BroadcastAll(new PacketSpawnExplosion
                {
                    Position = __instance.transform.position,
                    DamageRange = 10f,
                    KillRange = 5f
                });

                __instance.agent.speed = 0f;
                __instance.isEnemyDead = true;

                __instance.GetComponentInChildren<NetworkObject>().Despawn();
            });
        }
    }
}