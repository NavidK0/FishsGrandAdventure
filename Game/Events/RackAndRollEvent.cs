using System;
using System.Collections.Generic;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Game.Events;

public class RackAndRollEvent : IGameEvent
{
    private static readonly List<Type> AllowedEnemyTypes = new List<Type>
    {
        typeof(BaboonBirdAI),
        typeof(CentipedeAI),
        typeof(CrawlerAI),
        typeof(FlowermanAI),
        typeof(HoarderBugAI),
        typeof(LassoManAI),
        typeof(MaskedPlayerEnemy),
        typeof(MouthDogAI),
        typeof(NutcrackerEnemyAI),
        typeof(SandSpiderAI)
    };

    private Item gunItem;

    public string Description => "Rack and Roll";
    public Color Color => new Color(0.97f, 0.99f, 1f);
    public GameEventType GameEventType => GameEventType.RackAndRoll;

    public void OnServerInitialize(SelectableLevel level)
    {
        gunItem = StartOfRound.Instance.allItemsList.itemsList.Find(i => i.itemName.ToLower() == "shotgun");
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(NutcrackerEnemyAI) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            NutcrackerEnemyAI nutcrackerAI =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<NutcrackerEnemyAI>();

            if (nutcrackerAI)
            {
                spawnableEnemyWithRarity.rarity = 300;
                continue;
            }

            EnemyAI enemyAI = spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<EnemyAI>();

            if (!AllowedEnemyTypes.Contains(enemyAI.GetType()))
            {
                spawnableEnemyWithRarity.rarity = 0;
            }
        }
    }

    public void OnFinishGeneratingLevel()
    {
        CommandListener.SendChatMessage("<color=red>Don't pick anything up yet.</color>");
        foreach (PlayerControllerB player in RoundManager.Instance.playersManager.allPlayerScripts)
        {
            if (player.isPlayerDead || !player.isPlayerControlled) break;

            player.DropAllHeldItems();
        }

        Timing.CallDelayed(16f, () =>
        {
            foreach (PlayerControllerB player in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                if (player.isPlayerDead || !player.isPlayerControlled) break;

                Vector3 playerLocation = player.NetworkObject.transform.position;

                if (player.IsServer)
                {
                    GameObject gunGo = Object.Instantiate(gunItem.spawnPrefab,
                        playerLocation,
                        Quaternion.identity
                    );

                    ShotgunItem grabbable = gunGo.GetComponent<ShotgunItem>();
                    grabbable.fallTime = 0f;
                    grabbable.shellsLoaded = 2;
                    grabbable.SetScrapValue(UnityEngine.Random.Range(30, 90));

                    NetworkObject gunNetObject = gunGo.GetComponent<NetworkObject>();
                    gunNetObject.Spawn();

                    NetworkUtils.BroadcastAll(new PacketPlayMusic
                    {
                        Name = "doom_eternal",
                        Volume = 0.5f
                    });

                    // NetworkUtils.BroadcastAll(new PacketGrabItem
                    // {
                    //     ClientId = playerController.actualClientId,
                    //     NetworkObjectId = networkObject.NetworkObjectId
                    // });
                }
            }

            CommandListener.SendChatMessage("<color=red>Don't forget your gun. Ask your host. Good luck.</color>");
        });
    }

    public void Cleanup()
    {
    }
}

internal class PatchRackAndRoll
{
    [HarmonyPatch(typeof(ShotgunItem), "ItemActivate")]
    [HarmonyPostfix]
    public static void PatchItemActivate(ShotgunItem __instance)
    {
        if (GameState.CurrentGameEvent?.GameEventType == GameEventType.RackAndRoll)
        {
            __instance.shellsLoaded = 4;
        }
    }

    [HarmonyPatch(typeof(ShotgunItem), "ShootGun")]
    [HarmonyPostfix]
    public static void PatchShootGun(ShotgunItem __instance)
    {
        if (GameState.CurrentGameEvent?.GameEventType == GameEventType.RackAndRoll)
        {
            __instance.shellsLoaded = 4;
        }
    }
}