using System.Collections.Generic;
using FishsGrandAdventure.Audio;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class SeaWorldEvent : BaseGameEvent
{
    public override string Description => "Welcome to Sea World! Enjoy your souvenirs!";
    public override Color Color => Color.cyan;
    public override GameEventType GameEventType => GameEventType.SeaWorld;

    public override void OnServerInitialize(SelectableLevel level)
    {
        PatchSeaWorld.Reset();
    }

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificItemsForEvent(level, new List<string> { "plastic fish" });

        // Replace all items with fish
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap)
        {
            spawnableItemWithRarity.rarity =
                spawnableItemWithRarity.spawnableItem.itemName.ToLower() switch
                {
                    "plastic fish" => 100,
                    _ => 0
                };
        }
    }

    public override void OnPreFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.Flooded);
    }

    public override void OnPostFinishGeneratingLevel()
    {
        TimeOfDay.Instance.currentWeatherVariable2 += 12;
    }

    public override void Cleanup()
    {
        AudioManager.StopMusic();
    }
}

public static class PatchSeaWorld
{
    public static float TimeSpentUnderwater;
    public static bool PlayingMusic;

    public static void Reset()
    {
        TimeSpentUnderwater = 0f;
        PlayingMusic = false;
    }

    [HarmonyPatch(typeof(StartOfRound), "Update")]
    [HarmonyPostfix]
    private static void StartOfRoundUpdate(StartOfRound __instance)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.SeaWorld) return;

        __instance.drowningTimer = 10f;
    }

    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    [HarmonyPostfix]
    private static void PlayerControllerBUpdate(PlayerControllerB __instance)
    {
        if (GameState.CurrentGameEvent?.GameEventType != GameEventType.SeaWorld) return;

        float drunkValue = Mathf.Abs(StartOfRound.Instance.drunknessSpeedEffect.Evaluate(__instance.drunkness) - 1.25f);

        if (__instance.isUnderwater)
        {
            if (!PlayingMusic && TimeSpentUnderwater > 1f)
            {
                AudioManager.PlayMusic("oras_dive", 0.45f);
                PlayingMusic = true;
            }

            if (TimeSpentUnderwater > 1f)
            {
                __instance.sprintMeter += Time.deltaTime / (__instance.sprintTime + 4f) * drunkValue;

                if (__instance.sprintMeter > 1f)
                {
                    __instance.sprintMeter = 1f;
                }
            }

            if (TimeSpentUnderwater > 3f)
            {
                __instance.isMovementHindered = 0;
                HUDManager.Instance.underwaterScreenFilter.weight = .05f;

                __instance.nightVision.enabled = true;
            }

            TimeSpentUnderwater += Time.deltaTime;
        }
        else
        {
            if (TimeSpentUnderwater > 1f)
            {
                TimeSpentUnderwater = 1f;
            }

            if (PlayingMusic && TimeSpentUnderwater <= 0f)
            {
                AudioManager.StopMusic(true);

                PlayingMusic = false;

                __instance.nightVision.enabled = false;
            }

            TimeSpentUnderwater = Mathf.Max(TimeSpentUnderwater - Time.deltaTime, 0);
        }
    }
}