using System;
using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class BrackenAndCoilEvent : BaseGameEvent
{
    public override string Name => "Bracken and Coil";
    public override string Description => "Best of friends.";
    public override Color Color => new Color(1f, 0f, 0.32f);
    public override GameEventType GameEventType => GameEventType.BrackenAndCoil;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(FlowermanAI) });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(SpringManAI) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            spawnableEnemyWithRarity.rarity = 0;

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 100;
                spawnableEnemyWithRarity.enemyType.MaxCount = 4;
            }

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 100;
            }
        }
    }
}