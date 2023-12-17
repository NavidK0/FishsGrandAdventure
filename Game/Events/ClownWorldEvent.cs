using System.Collections.Generic;
using System.Threading.Tasks;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ClownWorldEvent : BaseGameEvent
{
    public override string Name => "Clown World";
    public override string Description => "Clowning is an art, and I am a master artist.";
    public override Color Color => new Color32(252, 126, 0, 255);
    public override GameEventType GameEventType => GameEventType.ClownWorld;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificItemsForEvent(level, new List<string> { "airhorn", "clown horn" });

        // Replace all items with horns :)
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap)
        {
            spawnableItemWithRarity.rarity =
                spawnableItemWithRarity.spawnableItem.itemName.ToLower() switch
                {
                    "clown horn" => 100,
                    "airhorn" => 100,
                    _ => 0
                };
        }
    }

    public override void CleanupClient()
    {
        PatchClownWorld.Clownified = false;

        StormyWeather stormy = Object.FindObjectOfType<StormyWeather>(true);
        stormy.gameObject.SetActive(false);
    }
}

public static class PatchClownWorld
{
    public static bool Clownified;

    private static double LastInputTime;

    [HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
    [HarmonyPostfix]
    private static void OnActivateItemServerRpc(GrabbableObject __instance, bool onOff, bool buttonDown)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.ClownWorld) return;
        if (!buttonDown) return;

        // debounce
        if (LastInputTime + 0.5f > Time.timeAsDouble) return;

        bool isClownHorn = __instance.itemProperties.itemName.ToLower().Contains("clown horn");
        bool isAirHorn = __instance.itemProperties.itemName.ToLower().Contains("airhorn");

        PlayerControllerB playerControllerB = __instance.playerHeldBy;

        if (playerControllerB == null || playerControllerB.isInHangarShipRoom) return;

        if (isClownHorn)
        {
            if (!Clownified)
            {
                Clownified = true;
                HUDManager.Instance.AddTextToChatOnServer(
                    $"{playerControllerB.playerUsername} is a clown! The world is now clownified!");
            }

            Timing.RunCoroutine(SpawnLightning((playerControllerB)));
        }

        if (isAirHorn)
        {
            Timing.RunCoroutine(SpawnExplosion(playerControllerB));
        }

        LastInputTime = Time.timeAsDouble;
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