using System;
using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class HoardingEvent : BaseGameEvent
{
    public override string Name => "The Ghettos";
    public override string Description => "Not the safest part of town... hey, where's my scrap?!";
    public override Color Color => Color.gray;
    public override GameEventType GameEventType => GameEventType.Hoarding;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        float dangerLevel = GameEventManager.DangerLevels[level];

        level.enemySpawnChanceThroughoutDay =
            new AnimationCurve(new Keyframe(0f, 500f + dangerLevel));

        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(HoarderBugAI) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 999;
            }
        }
    }
}