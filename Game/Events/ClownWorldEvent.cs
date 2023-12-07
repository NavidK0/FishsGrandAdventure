using System.Threading.Tasks;
using FishsGrandAdventure.Network;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ClownWorldEvent : IGameEvent
{
    public string Description => "Clown World";
    public Color Color => new Color32(252, 126, 0, 255);
    public GameEventType GameEventType => GameEventType.ClownWorld;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        // Replace all items with horns :)
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap)
        {
            spawnableItemWithRarity.rarity =
                spawnableItemWithRarity.spawnableItem.itemName.ToLower() switch
                {
                    "clown horn" => 999,
                    "airhorn" => 999,
                    _ => 0
                };
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
        ClownWorld.Clownified = false;
    }
}

public static class ClownWorld
{
    public static bool Clownified;

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
            if (!Clownified)
            {
                Clownified = true;
                HUDManager.Instance.AddTextToChatOnServer(
                    $"{playerControllerB.playerUsername} is a clown! The world is now clownified!");
            }

            _ = SpawnLightning(playerControllerB);
        }

        if (isAirHorn)
        {
            _ = SpawnExplosion(playerControllerB);
        }
    }

    private static async Task SpawnLightning(PlayerControllerB playerControllerB)
    {
        Vector3 transformPosition = playerControllerB.transform.position;

        await Task.Delay(1500);

        NetworkUtils.BroadcastAll(new PacketStrikeLightning
        {
            Position = transformPosition
        });
    }

    private static async Task SpawnExplosion(PlayerControllerB playerControllerB)
    {
        Vector3 transformPosition = playerControllerB.transform.position;

        await Task.Delay(2000);

        NetworkUtils.BroadcastAll(new PacketSpawnExplosion
        {
            Position = transformPosition
        });
    }
}