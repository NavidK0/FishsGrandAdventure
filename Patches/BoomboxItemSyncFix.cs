using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace FishsGrandAdventure.Patches;

public static class BoomboxItemSyncFix
{
    private static readonly Dictionary<BoomboxItem, bool> seedSyncDictionary = new Dictionary<BoomboxItem, bool>();

    private static readonly FieldInfo playersManagerField = AccessTools.Field(typeof(BoomboxItem), "playersManager");

    [HarmonyPatch(typeof(BoomboxItem), "StartMusic")]
    [HarmonyPrefix]
    public static void StartMusicPatch(BoomboxItem __instance)
    {
        var val = (StartOfRound)playersManagerField.GetValue(__instance);

        if ((!seedSyncDictionary.TryGetValue(__instance, out bool value) || !value) && val != null &&
            val.randomMapSeed > 0)
        {
            int num = val.randomMapSeed - 10;
            __instance.musicRandomizer = new Random(num);

            Plugin.Log.LogInfo(
                $"musicRandomizer variable has been synced with seed: {num}"
            );
            seedSyncDictionary[__instance] = true;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc")]
    [HarmonyPrefix]
    public static void OnPlayerConnectedPatch(StartOfRound __instance)
    {
        Plugin.Log.LogInfo(
            (object)"Another client joined -- forcing everyone to reinitialize musicRandomizer."
        );
        forceReinitialize(seedSyncDictionary);
    }

    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
    [HarmonyPrefix]
    public static void openingDoorsSequencePatch(StartOfRound __instance)
    {
        Plugin.Log.LogInfo(
            "Round has loaded for all -- forcing everyone to reinitialize musicRandomizer."
        );
        forceReinitialize(seedSyncDictionary);
    }

    private static void forceReinitialize(Dictionary<BoomboxItem, bool> seedSyncDictionary)
    {
        foreach (BoomboxItem item in seedSyncDictionary.Keys.ToList())
        {
            seedSyncDictionary[item] = false;
        }
    }
}