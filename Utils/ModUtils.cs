using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
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
                        Plugin.Log.LogInfo($"Added event Item: {item.spawnableItem.itemName}");
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
                        Plugin.Log.LogInfo($"Added event Enemy: {enemy.enemyType.enemyPrefab.name}");
                    }
                }
            }
        }
    }

    public static void AddSpecificOutsideObjectsForEvent(SelectableLevel newLevel, List<string> outsideObjectNames)
    {
        SelectableLevel[] levels = StartOfRound.Instance.levels;
        foreach (SelectableLevel sl in levels)
        {
            foreach (SpawnableOutsideObjectWithRarity outsideObject in sl.spawnableOutsideObjects)
            {
                SpawnableOutsideObject spawnableObject = outsideObject.spawnableObject;
                foreach (string objectName in outsideObjectNames)
                {
                    if (spawnableObject.prefabToSpawn.name.ToLower() == objectName &&
                        newLevel.spawnableOutsideObjects.All(
                            s =>
                                !string.Equals(
                                    s.spawnableObject.prefabToSpawn.name,
                                    spawnableObject.prefabToSpawn.name,
                                    StringComparison.CurrentCultureIgnoreCase
                                )
                        )
                       )
                    {
                        newLevel.spawnableOutsideObjects = newLevel.spawnableOutsideObjects.AddToArray(outsideObject);
                        Plugin.Log.LogInfo(
                            $"Added event Outside Object: {outsideObject.spawnableObject.prefabToSpawn.name}");
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

    public static int FirstEmptyItemSlot(this PlayerControllerB playerController)
    {
        int num = -1;

        if (playerController.ItemSlots[playerController.currentItemSlot] == null)
        {
            num = playerController.currentItemSlot;
        }
        else
        {
            for (var index = 0; index < playerController.ItemSlots.Length; ++index)
            {
                if (playerController.ItemSlots[index] == null)
                {
                    num = index;
                    break;
                }
            }
        }

        return num;
    }

    // PlayerController Helpers and Reflected Properties

    public static MethodInfo SwitchToItemSlotMethod =
        typeof(PlayerControllerB).GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);

    public static MethodInfo GrabObjectServerRpcMethod =
        typeof(PlayerControllerB).GetMethod("GrabObjectServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);

    public static MethodInfo GrabObjectClientRpcMethod =
        typeof(PlayerControllerB).GetMethod("GrabObjectClientRpc", BindingFlags.NonPublic | BindingFlags.Instance);

    public static FieldInfo GrabbedObjectValidatedField = typeof(PlayerControllerB).GetField("grabbedObjectValidated",
        BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly int GrabValidated = Animator.StringToHash("GrabValidated");

    public static void SwitchToItemSlot(this PlayerControllerB playerController, int slot,
        GrabbableObject grabbableObject = null)
    {
        SwitchToItemSlotMethod.Invoke(playerController, new object[] { slot, grabbableObject });
    }

    public static void GrabObject(this PlayerControllerB playerController,
        NetworkObjectReference grabbedObjectRef)
    {
        grabbedObjectRef.TryGet(out NetworkObject netObject, NetworkManager.Singleton);
        GrabbableObject grabbableObject = netObject.GetComponent<GrabbableObject>();
        GrabObjectServerRpcMethod.Invoke(playerController, new object[] { grabbedObjectRef });
        GrabbedObjectValidatedField.SetValue(playerController, true);

        playerController.currentlyHeldObjectServer = grabbableObject;
        playerController.currentlyHeldObjectServer.GrabItemOnClient();
        playerController.isHoldingObject = true;
        playerController.playerBodyAnimator.SetBool(GrabValidated, true);

        GrabObjectClientRpcMethod.Invoke(playerController, new object[] { true, grabbedObjectRef });
    }

    public static FieldInfo FloodLevelOffsetField = typeof(FloodWeather).GetField("floodLevelOffset",
        BindingFlags.NonPublic | BindingFlags.Instance);

    public static float GetFloodLevelOffset(this FloodWeather weather)
    {
        return (float)FloodLevelOffsetField.GetValue(weather);
    }

    public static MethodInfo ThrowObjectServerRpcMethodInfo =
        typeof(PlayerControllerB).GetMethod("ThrowObjectServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);

    public static MethodInfo SetSpecialGrabAnimationBoolMethodInfo =
        typeof(PlayerControllerB).GetMethod("SetSpecialGrabAnimationBool",
            BindingFlags.NonPublic | BindingFlags.Instance);

    public static FieldInfo ThrowingObjectFieldInfo = typeof(PlayerControllerB).GetField("throwingObject",
        BindingFlags.NonPublic | BindingFlags.Instance);

    public static void ThrowObjectServerRpc(
        this PlayerControllerB playerController,
        NetworkObjectReference grabbedObject,
        bool droppedInElevator,
        bool droppedInShipRoom,
        Vector3 targetFloorPosition,
        int floorYRot
    )
    {
        ThrowObjectServerRpcMethodInfo.Invoke(playerController, new object[]
        {
            grabbedObject,
            droppedInElevator,
            droppedInShipRoom,
            targetFloorPosition,
            floorYRot
        });
    }

    public static void SetSpecialGrabAnimationBool(
        this PlayerControllerB playerController,
        bool setTrue,
        GrabbableObject currentItem = null
    )
    {
        SetSpecialGrabAnimationBoolMethodInfo.Invoke(playerController, new object[] { setTrue, currentItem });
    }

    public static void SetThrowingObject(this PlayerControllerB playerController, bool throwingObject)
    {
        ThrowingObjectFieldInfo.SetValue(playerController, throwingObject);
    }
}