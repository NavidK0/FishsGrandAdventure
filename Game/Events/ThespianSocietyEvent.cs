using System;
using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ThespianSocietyEvent : IGameEvent
{
    public string Description => "Thespian Society";
    public Color Color => new Color(1f, 0f, 0.73f);
    public GameEventType GameEventType => GameEventType.ThespianSociety;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(MaskedPlayerEnemy) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            MaskedPlayerEnemy enemy =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<MaskedPlayerEnemy>();

            spawnableEnemyWithRarity.rarity = enemy != null ? 999 : 0;
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}