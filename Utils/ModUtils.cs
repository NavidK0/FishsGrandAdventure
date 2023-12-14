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

    public static void CopyLevelProperties(ref SelectableLevel level, SelectableLevel original)
    {
        // Properties
        level.planetPrefab = original.planetPrefab;
        level.sceneName = original.sceneName;
        level.spawnEnemiesAndScrap = original.spawnEnemiesAndScrap;
        level.PlanetName = original.PlanetName;
        level.LevelDescription = original.LevelDescription;
        level.videoReel = original.videoReel;
        level.riskLevel = original.riskLevel;
        level.timeToArrive = original.timeToArrive;

        // Time
        level.OffsetFromGlobalTime = original.OffsetFromGlobalTime;
        level.DaySpeedMultiplier = original.DaySpeedMultiplier;
        level.planetHasTime = original.planetHasTime;
        level.randomWeathers = original.randomWeathers;
        level.overrideWeather = original.overrideWeather;
        level.overrideWeatherType = original.overrideWeatherType;

        // Level Values
        level.currentWeather = original.currentWeather;
        level.factorySizeMultiplier = original.factorySizeMultiplier;
        level.dungeonFlowTypes = original.dungeonFlowTypes;
        level.spawnableMapObjects = original.spawnableMapObjects;
        level.spawnableOutsideObjects = original.spawnableOutsideObjects;
        level.spawnableScrap = original.spawnableScrap;
        level.minTotalScrapValue = original.minTotalScrapValue;
        level.maxTotalScrapValue = original.maxTotalScrapValue;
        level.levelAmbienceClips = original.levelAmbienceClips;

        // Don't touch these.
        // level.minScrap = original.minScrap;
        // level.maxScrap = original.maxScrap;
        // level.maxEnemyPowerCount = original.maxEnemyPowerCount;
        // level.maxOutsideEnemyPowerCount = original.maxEnemyPowerCount;
        // level.maxDaytimeEnemyPowerCount = original.maxDaytimeEnemyPowerCount;
        // level.daytimeEnemySpawnChanceThroughDay = original.daytimeEnemySpawnChanceThroughDay;
        // level.enemySpawnChanceThroughoutDay = original.enemySpawnChanceThroughoutDay;

        // Level enemy values
        level.Enemies = original.Enemies;
        level.OutsideEnemies = original.OutsideEnemies;
        level.DaytimeEnemies = original.DaytimeEnemies;
        level.outsideEnemySpawnChanceThroughDay = original.outsideEnemySpawnChanceThroughDay;
        level.spawnProbabilityRange = original.spawnProbabilityRange;
        level.daytimeEnemiesProbabilityRange = original.daytimeEnemiesProbabilityRange;
        level.levelIncludesSnowFootprints = original.levelIncludesSnowFootprints;
        level.levelIconString = original.levelIconString;
    }

    public static void CopyEnemyTypeProperties(ref EnemyType enemy, EnemyType original)
    {
        // Properties
        enemy.enemyName = original.enemyName;

        // Spawning Logic
        enemy.probabilityCurve = original.probabilityCurve;
        enemy.numberSpawnedFalloff = original.numberSpawnedFalloff;
        enemy.useNumberSpawnedFalloff = original.useNumberSpawnedFalloff;
        enemy.enemyPrefab = original.enemyPrefab;
        enemy.PowerLevel = original.PowerLevel;
        enemy.MaxCount = original.MaxCount;
        enemy.numberSpawned = original.numberSpawned;
        enemy.isOutsideEnemy = original.isOutsideEnemy;
        enemy.isDaytimeEnemy = original.isDaytimeEnemy;
        enemy.normalizedTimeInDayToLeave = original.normalizedTimeInDayToLeave;

        // Misc. ingame properties
        enemy.stunTimeMultiplier = original.stunTimeMultiplier;
        enemy.doorSpeedMultiplier = original.doorSpeedMultiplier;
        enemy.canBeStunned = original.canBeStunned;
        enemy.canDie = original.canDie;
        enemy.destroyOnDeath = original.destroyOnDeath;
        enemy.canSeeThroughFog = original.canSeeThroughFog;

        // Vent Properties
        enemy.timeToPlayAudio = original.timeToPlayAudio;
        enemy.loudnessMultiplier = original.loudnessMultiplier;
        enemy.overrideVentSFX = original.overrideVentSFX;
        enemy.hitBodySFX = original.hitBodySFX;
        enemy.hitEnemyVoiceSFX = original.hitEnemyVoiceSFX;
        enemy.deathSFX = original.deathSFX;
        enemy.stunSFX = original.stunSFX;
        enemy.miscAnimations = original.miscAnimations;
        enemy.audioClips = original.audioClips;
    }

    public static void CopyItemProperties(ref Item item, Item original)
    {
        // Properties
        item.itemName = original.itemName;
        item.spawnPositionTypes = original.spawnPositionTypes;
        item.twoHanded = original.twoHanded;
        item.twoHandedAnimation = original.twoHandedAnimation;
        item.canBeGrabbedBeforeGameStart = original.canBeGrabbedBeforeGameStart;
        item.weight = original.weight;
        item.itemIsTrigger = original.itemIsTrigger;
        item.holdButtonUse = original.holdButtonUse;
        item.itemSpawnsOnGround = original.itemSpawnsOnGround;
        item.isConductiveMetal = original.isConductiveMetal;

        // Scrap collection
        item.isScrap = original.isScrap;
        item.creditsWorth = original.creditsWorth;
        item.highestSalePercentage = original.highestSalePercentage;
        item.maxValue = original.maxValue;
        item.minValue = original.minValue;
        item.spawnPrefab = original.spawnPrefab;

        // Battery
        item.requiresBattery = original.requiresBattery;
        item.batteryUsage = original.batteryUsage;
        item.automaticallySetUsingPower = original.automaticallySetUsingPower;
        item.itemIcon = original.itemIcon;
        item.grabAnim = original.grabAnim;

        // Player animations
        item.useAnim = original.useAnim;
        item.pocketAnim = original.pocketAnim;
        item.throwAnim = original.throwAnim;
        item.grabAnimationTime = original.grabAnimationTime;

        // Player SFX
        item.grabSFX = original.grabSFX;
        item.dropSFX = original.dropSFX;
        item.pocketSFX = original.pocketSFX;
        item.throwSFX = original.throwSFX;

        // Netcode
        item.syncGrabFunction = original.syncGrabFunction;
        item.syncDiscardFunction = original.syncDiscardFunction;
        item.syncInteractLRFunction = original.syncInteractLRFunction;

        // Save data
        item.saveItemVariable = original.saveItemVariable;

        // MISC
        item.isDefensiveWeapon = original.isDefensiveWeapon;
        item.toolTips = original.toolTips;
        item.verticalOffset = original.verticalOffset;
        item.floorYOffset = original.floorYOffset;
        item.allowDroppingAheadOfPlayer = original.allowDroppingAheadOfPlayer;
        item.restingRotation = original.restingRotation;
        item.rotationOffset = original.rotationOffset;
        item.positionOffset = original.positionOffset;
        item.meshOffset = original.meshOffset;
        item.meshVariants = original.meshVariants;
        item.materialVariants = original.materialVariants;
        item.usableInSpecialAnimations = original.usableInSpecialAnimations;
        item.canBeInspected = original.canBeInspected;
    }

    public static FieldInfo FloodLevelOffsetField = typeof(FloodWeather).GetField("floodLevelOffset",
        BindingFlags.NonPublic | BindingFlags.Instance);

    public static float GetFloodLevelOffset(this FloodWeather weather)
    {
        return (float)FloodLevelOffsetField.GetValue(weather);
    }
}