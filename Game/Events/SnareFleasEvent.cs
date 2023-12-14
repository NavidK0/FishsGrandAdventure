using System;
using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class SnareFleasEvent : BaseGameEvent
{
    public override string Description => "Snare Fleas and Friends!";
    public override Color Color => Color.red;
    public override GameEventType GameEventType => GameEventType.SnareFleas;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(CentipedeAI) });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(FlowermanAI) });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(SpringManAI) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            spawnableEnemyWithRarity.rarity = 0;

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 100;
            }

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 100;
            }

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 100;
            }
        }
    }
}