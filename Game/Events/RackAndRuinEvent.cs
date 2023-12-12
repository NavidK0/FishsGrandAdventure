using System;
using System.Collections.Generic;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Game.Events;

public class RackAndRuinEvent : IGameEvent
{
    private Item gunItem;

    public string Description => "Rack and Ruin";
    public Color Color => new Color(0.97f, 0.99f, 1f);
    public GameEventType GameEventType => GameEventType.ThespianSociety;

    public void OnServerInitialize(SelectableLevel level)
    {
        gunItem = StartOfRound.Instance.allItemsList.itemsList.Find(i => i.itemName.ToLower() == "shotgun");
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(NutcrackerEnemyAI) });


        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            NutcrackerEnemyAI enemy =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<NutcrackerEnemyAI>();

            spawnableEnemyWithRarity.rarity = enemy != null ? 999 : 0;
        }
    }

    public void OnFinishGeneratingLevel()
    {
        foreach (PlayerControllerB player in RoundManager.Instance.playersManager.allPlayerScripts)
        {
            player.DropAllHeldItems();

            if (RoundManager.Instance.IsServer)
            {
                GameObject newItem = Object.Instantiate(gunItem.spawnPrefab,
                    GameNetworkManager.Instance.localPlayerController.transform.position,
                    Quaternion.identity);
                GrabbableObject grabbable = newItem.GetComponent<GrabbableObject>();
                grabbable.fallTime = 0f;
                NetworkObject networkObject = newItem.GetComponent<NetworkObject>();
                networkObject.SpawnWithOwnership(player.actualClientId);

                grabbable.GrabItemOnClient();
            }
        }

        NetworkUtils.BroadcastAll(new PacketPlayMusic
        {
            Name = "doom_eternal"
        });
    }

    public void Cleanup()
    {
    }
}