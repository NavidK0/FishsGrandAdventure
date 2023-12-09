﻿using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class SeaWorldEvent : IGameEvent
{
    public string Description => "Welcome to Sea World! Enjoy your souvenirs!";
    public Color Color => Color.cyan;
    public GameEventType GameEventType => GameEventType.SeaWorld;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
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

    public void OnFinishGeneratingLevel()
    {
        ClientHelper.SetWeather(LevelWeatherType.Flooded);
    }

    public void Cleanup()
    {
    }
}