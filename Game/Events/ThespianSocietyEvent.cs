using System;
using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ThespianSocietyEvent : BaseGameEvent
{
    public override string Name => "Thespian Society";
    public override string Description => "We are of a higher class.";
    public override Color Color => new Color(1f, 0f, 0.73f);
    public override GameEventType GameEventType => GameEventType.ThespianSociety;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(MaskedPlayerEnemy) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            MaskedPlayerEnemy enemy =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<MaskedPlayerEnemy>();

            spawnableEnemyWithRarity.rarity = enemy != null ? 999 : 0;
        }
    }
}