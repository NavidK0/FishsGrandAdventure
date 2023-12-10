using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace FishsGrandAdventure.Utils;

[PublicAPI]
public static class ModUtils
{
    public enum SpawnLocation
    {
        Outside,
        Vent
    }

    public struct EnemySpawnInfo
    {
        public readonly Type EnemyType;
        public readonly int Amount;
        public readonly SpawnLocation Location;
        public readonly bool ForceInside;
        public readonly bool ForceOutside;

        public EnemySpawnInfo(Type enemyType, int amount, SpawnLocation location, bool forceInside, bool forceOutside)
        {
            EnemyType = enemyType;
            Amount = amount;
            Location = location;
            ForceInside = forceInside;
            ForceOutside = forceOutside;
        }
    }

    public static void AddSpecificItemsForEvent(SelectableLevel newLevel, List<string> itemNames)
    {
        SelectableLevel[] levels = StartOfRound.Instance.levels;
        foreach (SelectableLevel sl in levels)
        {
            foreach (SpawnableItemWithRarity item in sl.spawnableScrap)
            {
                Item spawnableItem = item.spawnableItem;
                foreach (string itemName in itemNames)
                {
                    if (spawnableItem.itemName.ToLower() == itemName &&
                        newLevel.spawnableScrap.All(
                            s =>
                                !string.Equals(
                                    s.spawnableItem.itemName,
                                    spawnableItem.itemName,
                                    StringComparison.CurrentCultureIgnoreCase
                                )
                        )
                       )
                    {
                        newLevel.spawnableScrap.Add(item);
                        Plugin.Log.LogInfo($"Added event Item: {item.spawnableItem.itemName} for event");
                    }
                }
            }
        }
    }

    public static void AddSpecificEnemiesForEvent(SelectableLevel newLevel, List<Type> enemyAITypes)
    {
        SelectableLevel[] levels = StartOfRound.Instance.levels;
        foreach (SelectableLevel sl in levels)
        {
            foreach (SpawnableEnemyWithRarity enemy in sl.Enemies)
            {
                GameObject enemyPrefab = enemy.enemyType.enemyPrefab;
                foreach (Type enemyAIType in enemyAITypes)
                {
                    if (enemyPrefab.GetComponent(enemyAIType) != null &&
                        newLevel.Enemies.All(
                            e => e.enemyType.enemyPrefab != enemyPrefab))
                    {
                        newLevel.Enemies.Add(enemy);
                        Plugin.Log.LogInfo($"Added event Enemy: {enemy.enemyType.enemyPrefab.name} for event");
                    }
                }
            }
        }
    }

    public static GameObject FindEnemyPrefabByType(
        Type enemyType,
        List<SpawnableEnemyWithRarity> enemyList,
        SelectableLevel newLevel
    )
    {
        foreach (SpawnableEnemyWithRarity enemy in enemyList)
        {
            if (enemy.enemyType.enemyPrefab.GetComponent(enemyType) != null)
            {
                return enemy.enemyType.enemyPrefab;
            }
        }

        AddSpecificEnemiesForEvent(newLevel, new List<Type> { enemyType });

        foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
        {
            if (enemy.enemyType.enemyPrefab.GetComponent(enemyType) != null)
            {
                return enemy.enemyType.enemyPrefab;
            }
        }

        throw new Exception($"Enemy type {enemyType.Name} not found and could not be added.");
    }


    public static EnemyAI SpawnEnemyOutside(Type enemyType, SelectableLevel level, bool forceOutside)
    {
        GameObject enemyPrefab = !forceOutside
            ? FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.OutsideEnemies, level)
            : FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.Enemies, level);

        GameObject[] array = GameObject.FindGameObjectsWithTag("OutsideAINode");
        Vector3 position = array[Random.Range(0, array.Length)].transform.position;

        GameObject go = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        go.GetComponentInChildren<NetworkObject>().Spawn(true);

        EnemyAI component = go.GetComponent<EnemyAI>();
        if (forceOutside)
        {
            component.enemyType.isOutsideEnemy = true;
            component.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            component.SyncPositionToClients();
        }

        RoundManager.Instance.SpawnedEnemies.Add(component);
        return component;
    }

    public static EnemyAI SpawnEnemyInside(Type enemyType, SelectableLevel level, bool forceInside)
    {
        GameObject enemyPrefab = !forceInside
            ? FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.Enemies, level)
            : FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.OutsideEnemies, level);

        int num = Random.Range(0, RoundManager.Instance.allEnemyVents.Length);
        Vector3 position = RoundManager.Instance.allEnemyVents[num].floorNode.position;
        var rot = Quaternion.Euler(0f, RoundManager.Instance.allEnemyVents[num].floorNode.eulerAngles.y, 0f);

        GameObject go = Object.Instantiate(enemyPrefab, position, rot);
        go.GetComponentInChildren<NetworkObject>().Spawn(true);

        EnemyAI component = go.GetComponent<EnemyAI>();
        if (forceInside)
        {
            component.enemyType.isOutsideEnemy = false;
            component.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            component.SyncPositionToClients();
        }

        RoundManager.Instance.SpawnedEnemies.Add(component);
        return component;
    }

    public static EnemyAI SpawnEnemyOutside(Type enemyType, SelectableLevel level, bool forceOutside, Vector3 position)
    {
        GameObject enemyPrefab = !forceOutside
            ? FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.OutsideEnemies, level)
            : FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.Enemies, level);

        GameObject go = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        go.GetComponentInChildren<NetworkObject>().Spawn(true);

        EnemyAI component = go.GetComponent<EnemyAI>();
        if (forceOutside)
        {
            component.enemyType.isOutsideEnemy = true;
            component.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            component.SyncPositionToClients();
        }

        RoundManager.Instance.SpawnedEnemies.Add(component);
        return component;
    }

    public static EnemyAI SpawnEnemyInside(Type enemyType, SelectableLevel level, bool forceInside, Vector3 position)
    {
        GameObject enemyPrefab = !forceInside
            ? FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.Enemies, level)
            : FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.OutsideEnemies, level);

        int num = Random.Range(0, RoundManager.Instance.allEnemyVents.Length);
        var rot = Quaternion.Euler(0f, RoundManager.Instance.allEnemyVents[num].floorNode.eulerAngles.y, 0f);

        GameObject go = Object.Instantiate(enemyPrefab, position, rot);
        go.GetComponentInChildren<NetworkObject>().Spawn(true);

        EnemyAI component = go.GetComponent<EnemyAI>();
        if (forceInside)
        {
            component.enemyType.isOutsideEnemy = false;
            component.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            component.SyncPositionToClients();
        }

        RoundManager.Instance.SpawnedEnemies.Add(component);
        return component;
    }

    public static EnemyAI SpawnEnemy(Type enemyType, SelectableLevel level,
        Vector3 position, bool isInside
    )
    {
        GameObject enemyPrefab = isInside
            ? FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.Enemies, level)
            : FindEnemyPrefabByType(enemyType, RoundManager.Instance.currentLevel.OutsideEnemies, level);

        int num = Random.Range(0, RoundManager.Instance.allEnemyVents.Length);
        var rot = Quaternion.Euler(0f, RoundManager.Instance.allEnemyVents[num].floorNode.eulerAngles.y, 0f);

        GameObject go = Object.Instantiate(enemyPrefab, position, rot);
        go.GetComponentInChildren<NetworkObject>().Spawn(true);

        EnemyAI component = go.GetComponent<EnemyAI>();
        if (isInside)
        {
            component.enemyType.isOutsideEnemy = false;
            component.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            component.SyncPositionToClients();
        }
        else
        {
            component.enemyType.isOutsideEnemy = true;
            component.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            component.SyncPositionToClients();
        }

        RoundManager.Instance.SpawnedEnemies.Add(component);
        return component;
    }

    public static void SpawnMultipleEnemies(SelectableLevel level, List<EnemySpawnInfo> enemiesToSpawn)
    {
        foreach (EnemySpawnInfo item in enemiesToSpawn)
        {
            for (var i = 0; i < item.Amount; i++)
            {
                switch (item.Location)
                {
                    case SpawnLocation.Outside:
                        SpawnEnemyOutside(item.EnemyType, level, item.ForceOutside);
                        break;
                    case SpawnLocation.Vent:
                        SpawnEnemyInside(item.EnemyType, level, item.ForceInside);
                        break;
                }
            }
        }
    }
}