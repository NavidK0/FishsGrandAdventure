using System;
using System.Threading.Tasks;
using FishsGrandAdventure.Game;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Patches;

public static class PatchClownWorld
{
    [HarmonyPatch(typeof(GrabbableObject), "ActivateItemServerRpc")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void OnItemActivate(NoisemakerProp __instance, bool onOff, bool buttonDown)
    {
        if (GameState.CurrentEvent != GameEvent.ClownWorld) return;
        if (!buttonDown) return;

        bool isClownHorn = __instance.itemProperties.itemName.ToLower().Contains("clown horn");
        bool isAirHorn = __instance.itemProperties.itemName.ToLower().Contains("airhorn");

        if (isClownHorn)
        {
            async Task SpawnLightning()
            {
                await Task.Delay(1500);

                if (__instance.playerHeldBy && !__instance.isInFactory)
                {
                    Object.FindObjectOfType<StormyWeather>(true)
                        .LightningStrike(__instance.playerHeldBy.transform.position, true);
                }
            }

            _ = SpawnLightning();
        }

        if (isAirHorn)
        {
            async Task SpawnExplosion()
            {
                await Task.Delay(1500);

                if (__instance.playerHeldBy)
                {
                    Landmine.SpawnExplosion(__instance.playerHeldBy.transform.position, true, 1f, 4f);
                }
            }

            _ = SpawnExplosion();
        }
    }
}