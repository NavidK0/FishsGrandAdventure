using FishsGrandAdventure.Game;
using GameNetcodeStuff;
using HarmonyLib;

namespace FishsGrandAdventure.Patches;

public static class PatchPlayerControllerB
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void SetPlayerMovementSpeed(ref float ___movementSpeed)
    {
        if (GameState.ForcePlayerMovementSpeed != null)
        {
            ___movementSpeed = GameState.ForcePlayerMovementSpeed.Value;
        }
    }
}