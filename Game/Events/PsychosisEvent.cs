using System;
using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class PsychosisEvent : BaseGameEvent
{
    public override string Description => "It's not psychosis.";
    public override Color Color => new Color(0.53f, 0f, 0.06f);
    public override GameEventType GameEventType => GameEventType.Psychosis;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(DressGirlAI) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            DressGirlAI dressGirlAI =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<DressGirlAI>();
            if (dressGirlAI != null)
            {
                spawnableEnemyWithRarity.rarity = 9999;
            }
        }
    }
}