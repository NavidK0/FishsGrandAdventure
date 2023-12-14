using System;
using System.Collections.Generic;
using FishsGrandAdventure.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace FishsGrandAdventure.Game.Events;

public class AllEvent : BaseGameEvent
{
    public override string Description => "Everything but the kitchen sink!";
    public override Color Color => new Color(0.68f, 0f, 1f);
    public override GameEventType GameEventType => GameEventType.All;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        GameEventManager.DangerLevels.TryGetValue(level, out float dangerLevel);

        level.enemySpawnChanceThroughoutDay =
            new AnimationCurve(new Keyframe(0f, 500f + dangerLevel));

        foreach (SpawnableMapObject spawnableMapObject in level.spawnableMapObjects)
        {
            if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
            {
                spawnableMapObject.numberToSpawn =
                    new AnimationCurve(new Keyframe(0f, 200f), new Keyframe(1f, 25f));
            }
            else if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
            {
                spawnableMapObject.numberToSpawn =
                    new AnimationCurve(new Keyframe(0f, 175f), new Keyframe(1f, 150f));
            }
        }

        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(FlowermanAI) });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(SpringManAI) });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(CentipedeAI) });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(HoarderBugAI) });
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(DressGirlAI) });

        foreach (SpawnableEnemyWithRarity enemy in level.Enemies)
        {
            enemy.enemyType.probabilityCurve =
                new AnimationCurve(new Keyframe(0f, 1000f));

            if (enemy.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
            {
                enemy.rarity = 100;
            }

            if (enemy.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
            {
                enemy.rarity = 100;
            }

            if (enemy.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
            {
                enemy.rarity = 100;
            }

            if (enemy.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
            {
                enemy.rarity = 100;
            }

            if (enemy.enemyType.enemyPrefab.GetComponent<DressGirlAI>() != null)
            {
                enemy.rarity = 1000;
            }
        }

        for (var i = 0; i < Random.Range(2, 9); i++)
        {
            int randomItems = Random.Range(0, 3);
            Object.FindObjectOfType<Terminal>().orderedItemsFromTerminal.Add(randomItems);
        }

        Terminal terminal = Object.FindObjectOfType<Terminal>();
        terminal.orderedItemsFromTerminal.Clear();

        for (var i = 0; i < terminal.orderedItemsFromTerminal.Count; i++)
        {
            terminal.orderedItemsFromTerminal.Add(0);
        }

        // Reset danger level because this event is nuts
        GameEventManager.DangerLevels[level] = 0f;
    }
}