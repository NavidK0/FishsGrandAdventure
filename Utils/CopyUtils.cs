using System.Collections.Generic;
using UnityEngine;

namespace FishsGrandAdventure.Utils;

public static class CopyUtils
{
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
        level.randomWeathers = original.randomWeathers.DeepCopy();
        level.overrideWeather = original.overrideWeather;
        level.overrideWeatherType = original.overrideWeatherType;

        // Level Values
        level.currentWeather = original.currentWeather;
        level.factorySizeMultiplier = original.factorySizeMultiplier;
        level.dungeonFlowTypes = original.dungeonFlowTypes.DeepCopy();
        level.spawnableMapObjects = original.spawnableMapObjects.DeepCopy();
        level.spawnableOutsideObjects = original.spawnableOutsideObjects.DeepCopy();
        level.spawnableScrap = original.spawnableScrap.DeepCopy();
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
        level.Enemies = original.Enemies.DeepCopy();
        level.OutsideEnemies = original.OutsideEnemies.DeepCopy();
        level.DaytimeEnemies = original.DaytimeEnemies.DeepCopy();
        level.outsideEnemySpawnChanceThroughDay = original.outsideEnemySpawnChanceThroughDay;
        level.spawnProbabilityRange = original.spawnProbabilityRange;
        level.daytimeEnemiesProbabilityRange = original.daytimeEnemiesProbabilityRange;
        level.levelIncludesSnowFootprints = original.levelIncludesSnowFootprints;
        level.levelIconString = original.levelIconString;
    }

    public static EnemyType DeepCopy(this EnemyType enemy)
    {
        EnemyType newEnemy = ScriptableObject.CreateInstance<EnemyType>();

        // Properties
        newEnemy.enemyName = enemy.enemyName;

        // Spawning Logic
        newEnemy.probabilityCurve = enemy.probabilityCurve;
        newEnemy.numberSpawnedFalloff = enemy.numberSpawnedFalloff;
        newEnemy.useNumberSpawnedFalloff = enemy.useNumberSpawnedFalloff;
        newEnemy.enemyPrefab = enemy.enemyPrefab;
        newEnemy.PowerLevel = enemy.PowerLevel;
        newEnemy.MaxCount = enemy.MaxCount;
        newEnemy.numberSpawned = enemy.numberSpawned;
        newEnemy.isOutsideEnemy = enemy.isOutsideEnemy;
        newEnemy.isDaytimeEnemy = enemy.isDaytimeEnemy;
        newEnemy.normalizedTimeInDayToLeave = enemy.normalizedTimeInDayToLeave;

        // Misc. in-game properties
        newEnemy.stunTimeMultiplier = enemy.stunTimeMultiplier;
        newEnemy.doorSpeedMultiplier = enemy.doorSpeedMultiplier;
        newEnemy.canBeStunned = enemy.canBeStunned;
        newEnemy.canDie = enemy.canDie;
        newEnemy.destroyOnDeath = enemy.destroyOnDeath;
        newEnemy.canSeeThroughFog = enemy.canSeeThroughFog;

        // Vent Properties
        newEnemy.timeToPlayAudio = enemy.timeToPlayAudio;
        newEnemy.loudnessMultiplier = enemy.loudnessMultiplier;
        newEnemy.overrideVentSFX = enemy.overrideVentSFX;
        newEnemy.hitBodySFX = enemy.hitBodySFX;
        newEnemy.hitEnemyVoiceSFX = enemy.hitEnemyVoiceSFX;
        newEnemy.deathSFX = enemy.deathSFX;
        newEnemy.stunSFX = enemy.stunSFX;
        newEnemy.miscAnimations = (MiscAnimation[])enemy.miscAnimations.Clone();
        newEnemy.audioClips = (AudioClip[])enemy.audioClips.Clone();

        return newEnemy;
    }

    public static Item DeepCopy(this Item item)
    {
        Item newItem = ScriptableObject.CreateInstance<Item>();

        // Properties
        newItem.itemName = item.itemName;
        newItem.spawnPositionTypes = new List<ItemGroup>(item.spawnPositionTypes);
        newItem.twoHanded = item.twoHanded;
        newItem.twoHandedAnimation = item.twoHandedAnimation;
        newItem.canBeGrabbedBeforeGameStart = item.canBeGrabbedBeforeGameStart;
        newItem.weight = item.weight;
        newItem.itemIsTrigger = item.itemIsTrigger;
        newItem.holdButtonUse = item.holdButtonUse;
        newItem.itemSpawnsOnGround = item.itemSpawnsOnGround;
        newItem.isConductiveMetal = item.isConductiveMetal;

        // Scrap collection
        newItem.isScrap = item.isScrap;
        newItem.creditsWorth = item.creditsWorth;
        newItem.highestSalePercentage = item.highestSalePercentage;
        newItem.maxValue = item.maxValue;
        newItem.minValue = item.minValue;
        newItem.spawnPrefab = item.spawnPrefab;

        // Battery
        newItem.requiresBattery = item.requiresBattery;
        newItem.batteryUsage = item.batteryUsage;
        newItem.automaticallySetUsingPower = item.automaticallySetUsingPower;
        newItem.itemIcon = item.itemIcon;
        newItem.grabAnim = item.grabAnim;

        // Player animations
        newItem.useAnim = item.useAnim;
        newItem.pocketAnim = item.pocketAnim;
        newItem.throwAnim = item.throwAnim;
        newItem.grabAnimationTime = item.grabAnimationTime;

        // Player SFX
        newItem.grabSFX = item.grabSFX;
        newItem.dropSFX = item.dropSFX;
        newItem.pocketSFX = item.pocketSFX;
        newItem.throwSFX = item.throwSFX;

        // Netcode
        newItem.syncGrabFunction = item.syncGrabFunction;
        newItem.syncDiscardFunction = item.syncDiscardFunction;
        newItem.syncInteractLRFunction = item.syncInteractLRFunction;

        // Save data
        newItem.saveItemVariable = item.saveItemVariable;

        // MISC
        newItem.isDefensiveWeapon = item.isDefensiveWeapon;
        newItem.toolTips = item.toolTips;
        newItem.verticalOffset = item.verticalOffset;
        newItem.floorYOffset = item.floorYOffset;
        newItem.allowDroppingAheadOfPlayer = item.allowDroppingAheadOfPlayer;
        newItem.restingRotation = item.restingRotation;
        newItem.rotationOffset = item.rotationOffset;
        newItem.positionOffset = item.positionOffset;
        newItem.meshOffset = item.meshOffset;
        newItem.meshVariants = (Mesh[])item.meshVariants.Clone();
        newItem.materialVariants = (Material[])item.materialVariants.Clone();
        newItem.usableInSpecialAnimations = item.usableInSpecialAnimations;
        newItem.canBeInspected = item.canBeInspected;

        return newItem;
    }

    public static SelectableLevel DeepCopy(this SelectableLevel level)
    {
        SelectableLevel newLevel = ScriptableObject.CreateInstance<SelectableLevel>();
        CopyLevelProperties(ref newLevel, level);

        return newLevel;
    }

    public static IntWithRarity[] DeepCopy(this IntWithRarity[] array)
    {
        IntWithRarity[] newArray = new IntWithRarity[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            newArray[i] = new IntWithRarity
            {
                id = array[i].id,
                rarity = array[i].rarity
            };
        }

        return newArray;
    }

    public static RandomWeatherWithVariables[] DeepCopy(this RandomWeatherWithVariables[] array)
    {
        RandomWeatherWithVariables[] newArray = new RandomWeatherWithVariables[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            newArray[i] = new RandomWeatherWithVariables
            {
                weatherType = array[i].weatherType,
                weatherVariable = array[i].weatherVariable,
                weatherVariable2 = array[i].weatherVariable2
            };
        }

        return newArray;
    }

    public static SpawnableMapObject[] DeepCopy(this SpawnableMapObject[] array)
    {
        SpawnableMapObject[] newArray = new SpawnableMapObject[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            newArray[i] = new SpawnableMapObject
            {
                prefabToSpawn = array[i].prefabToSpawn,
                numberToSpawn = array[i].numberToSpawn,
                spawnFacingAwayFromWall = array[i].spawnFacingAwayFromWall,
            };
        }

        return newArray;
    }

    public static SpawnableOutsideObjectWithRarity[] DeepCopy(this SpawnableOutsideObjectWithRarity[] array)
    {
        SpawnableOutsideObjectWithRarity[] newArray = new SpawnableOutsideObjectWithRarity[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            newArray[i] = new SpawnableOutsideObjectWithRarity
            {
                spawnableObject = array[i].spawnableObject,
                randomAmount = array[i].randomAmount,
            };
        }

        return newArray;
    }

    public static List<SpawnableEnemyWithRarity> DeepCopy(this List<SpawnableEnemyWithRarity> list)
    {
        List<SpawnableEnemyWithRarity> newList = new List<SpawnableEnemyWithRarity>();
        foreach (SpawnableEnemyWithRarity item in list)
        {
            newList.Add(new SpawnableEnemyWithRarity
            {
                enemyType = item.enemyType.DeepCopy(),
                rarity = item.rarity
            });
        }

        return newList;
    }

    public static List<SpawnableItemWithRarity> DeepCopy(this List<SpawnableItemWithRarity> list)
    {
        List<SpawnableItemWithRarity> newList = new List<SpawnableItemWithRarity>();
        foreach (SpawnableItemWithRarity item in list)
        {
            newList.Add(new SpawnableItemWithRarity
            {
                spawnableItem = item.spawnableItem.DeepCopy(),
                rarity = item.rarity
            });
        }

        return newList;
    }
}