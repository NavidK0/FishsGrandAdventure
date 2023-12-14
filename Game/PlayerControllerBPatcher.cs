using GameNetcodeStuff;
using HarmonyLib;

namespace FishsGrandAdventure.Game;

public static class PlayerControllerBPatcher
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    [HarmonyPostfix]
    private static void SetPlayerMovementSpeed(ref float ___movementSpeed)
    {
        if (GameState.ForcePlayerMovementSpeed != null)
        {
            ___movementSpeed = GameState.ForcePlayerMovementSpeed.Value;
        }
    }
}