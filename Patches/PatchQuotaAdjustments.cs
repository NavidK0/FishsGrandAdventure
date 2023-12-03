﻿using HarmonyLib;

namespace FishsGrandAdventure.Patches;

internal class PatchQuotaAdjustments
{
    [HarmonyPatch(typeof(TimeOfDay), "Start")]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void PatchQuotaVariables(TimeOfDay __instance)
    {
        __instance.quotaVariables.startingQuota = 1000;
        __instance.quotaVariables.startingCredits = 250;
        __instance.quotaVariables.baseIncrease = 500f;
        __instance.quotaVariables.randomizerMultiplier = 0f;
        __instance.quotaVariables.deadlineDaysAmount = 10;
    }
}