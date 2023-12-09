using System;
using FishsGrandAdventure.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishsGrandAdventure.Game.Events;

public class ClownExpoEvent : IGameEvent
{
    public string Description => $"Welcome to Clown Expo {DateTime.Today.Year}!";
    public Color Color => new Color32(252, 10, 0, 255);
    public GameEventType GameEventType => GameEventType.ClownExpo;

    private bool forcefullyAddedJesterToLevel;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        // Replace all items with horns :)
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in level.spawnableScrap)
        {
            spawnableItemWithRarity.rarity =
                spawnableItemWithRarity.spawnableItem.itemName.ToLower() switch
                {
                    "cash register" => 999,
                    _ => 0
                };
        }

        var hasJester = false;

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            JesterAI jesterAI =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<JesterAI>();

            if (jesterAI != null)
            {
                hasJester = true;
                spawnableEnemyWithRarity.rarity = jesterAI != null ? 9999 : 0;
            }
        }

        if (!hasJester)
        {
            SpawnableEnemyWithRarity jesterEnemy = ModUtils.FindSpawnableEnemy<JesterAI>();
            jesterEnemy.rarity = 9999;
            level.Enemies.Add(jesterEnemy);
            forcefullyAddedJesterToLevel = true;
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
        if (forcefullyAddedJesterToLevel)
        {
            StartOfRound.Instance.currentLevel
                .Enemies
                .RemoveAll(e => e.enemyType.enemyPrefab.GetComponent<JesterAI>() != null);
        }
    }
}