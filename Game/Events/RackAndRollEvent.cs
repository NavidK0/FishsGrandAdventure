using System;
using System.Collections.Generic;
using System.Linq;
using FishsGrandAdventure.Audio;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Game.Events;

public class RackAndRollEvent : BaseGameEvent
{
    public static Terminal Terminal;
    public static Item[] OriginalBuyableItems;

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
    private int gunTerminalTempIndex;

    public override string Description => "Rack and Roll";
    public override Color Color => new Color(0.97f, 0.99f, 1f);
    public override GameEventType GameEventType => GameEventType.RackAndRoll;

    public override void OnServerInitialize(SelectableLevel level)
    {
        PatchRackAndRoll.PlayedMusicOnFirstGunPickup = false;

        gunItem = StartOfRound.Instance.allItemsList.itemsList.Find(i => i.itemName.ToLower() == "shotgun");

        Terminal = Object.FindObjectOfType<Terminal>();
        OriginalBuyableItems = Terminal.buyableItemsList;
        Terminal.buyableItemsList = Terminal.buyableItemsList.Append(gunItem).ToArray();
        gunTerminalTempIndex = Terminal.buyableItemsList.Length - 1;
    }

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        for (var i = 0; i < GameNetworkManager.Instance.connectedPlayers; i++)
        {
            Terminal.orderedItemsFromTerminal.Add(gunTerminalTempIndex);
        }

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

    public override void OnPreFinishGeneratingLevel()
    {
        CommandListener.SendChatMessage(
            $"<color=#{new Color(0f, 0.99f, 1f).ToHex()}>Special delivery, wait for the drop ship!</color>"
        );
    }

    public override void Cleanup()
    {
        Terminal.buyableItemsList = OriginalBuyableItems;
    }
}

internal class PatchRackAndRoll
{
    public static bool PlayedMusicOnFirstGunPickup;

    [HarmonyPatch(typeof(ItemDropship), "ShipLeave")]
    [HarmonyPostfix]
    private static void ItemDropshipShipLeave()
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.RackAndRoll) return;

        RackAndRollEvent.Terminal.buyableItemsList = RackAndRollEvent.OriginalBuyableItems;
    }

    [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectServerRpc")]
    [HarmonyPostfix]
    private static void GrabObjectServerRpc(ref NetworkObjectReference grabbedObject)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.RackAndRoll) return;

        if (!PlayedMusicOnFirstGunPickup && grabbedObject.TryGet(out NetworkObject networkedObject))
        {
            GrabbableObject grabbableObject = networkedObject.GetComponent<GrabbableObject>();
            bool isShotgun = grabbableObject.itemProperties.itemName.ToLower() == "shotgun";

            if (isShotgun)
            {
                AudioManager.PlayMusic("doom_eternal", 0.5f);

                CommandListener.SendChatMessage("<color=red>Give 'em HELL!</color>");

                PlayedMusicOnFirstGunPickup = true;
            }
        }
    }

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