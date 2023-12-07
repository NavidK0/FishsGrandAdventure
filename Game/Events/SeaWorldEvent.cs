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
        // Replace all items with fish
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap)
        {
            spawnableItemWithRarity.rarity = spawnableItemWithRarity.spawnableItem.itemName.ToLower()
                .Contains("plastic fish")
                ? 999
                : 0;
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