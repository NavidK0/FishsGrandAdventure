using System.Threading.Tasks;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Network;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Patches;

public static class PatchClownWorld
{
    [HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void OnItemActivate(GrabbableObject __instance, bool onOff, bool buttonDown)
    {
        Plugin.Log.LogInfo("Current Event: " + GameState.CurrentEvent);
        Plugin.Log.LogInfo("Button Down: " + buttonDown);

        if (GameState.CurrentEvent != GameEvent.ClownWorld) return;
        if (!buttonDown) return;

        bool isClownHorn = __instance.itemProperties.itemName.ToLower().Contains("clown horn");
        bool isAirHorn = __instance.itemProperties.itemName.ToLower().Contains("airhorn");

        if (isClownHorn)
        {
            async Task SpawnLightning()
            {
                await Task.Delay(1500);

                if (__instance.playerHeldBy)
                {
                    Vector3 transformPosition = __instance.playerHeldBy.transform.position;

                    StormyWeather stormy = Object.FindObjectOfType<StormyWeather>(true);
                    if (stormy != null)
                    {
                        stormy.LightningStrike(transformPosition, true);
                        RoundManager.Instance.LightningStrikeClientRpc(transformPosition);
                    }

                    EventManager.SpawnExplosionClient(transformPosition);
                    EventSyncer.SpawnExplosionSyncAll(transformPosition);
                }
            }

            _ = SpawnLightning();
        }

        if (isAirHorn)
        {
            async Task SpawnExplosion()
            {
                await Task.Delay(1000);

                if (__instance.playerHeldBy)
                {
                    Vector3 transformPosition = __instance.playerHeldBy.transform.position;

                    EventManager.SpawnExplosionClient(transformPosition);
                    EventSyncer.SpawnExplosionSyncAll(transformPosition);
                }
            }

            _ = SpawnExplosion();
        }
    }
}