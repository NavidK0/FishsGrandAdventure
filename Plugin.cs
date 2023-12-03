using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using FishsGrandAdventure.Effects;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Patches;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FishsGrandAdventure
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const float DefaultMovementSpeed = 4.6f;

        public static ManualLogSource Log;
        public static MethodInfo Chat;

        private readonly Harmony harmony = new Harmony("FishsGrandAdventure");

        private static readonly List<SelectableLevel> LevelsModified = new List<SelectableLevel>();

        private static Dictionary<SelectableLevel, float> levelHeatVal;
        private static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> levelEnemySpawns;
        private static Dictionary<SpawnableEnemyWithRarity, int> enemyRarities;
        private static Dictionary<SpawnableEnemyWithRarity, AnimationCurve> enemyPropCurves;

        private static bool loaded;

        private void Awake()
        {
            Log = Logger;

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(PatchCommandListener));
            harmony.PatchAll(typeof(PatchClownWorld));
            harmony.PatchAll(typeof(PatchPlayerControllerB));

            levelHeatVal = new Dictionary<SelectableLevel, float>();
            enemyRarities = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();

            Chat = AccessTools.Method(typeof(HUDManager), "AddChatMessage");

            Log.LogInfo("Loaded Fish's Grand Adventure!");
        }

        public void OnDestroy()
        {
            if (!loaded)
            {
                var go = new GameObject("QuotaChanger");
                go.AddComponent<ObjectSpawner>();

                DontDestroyOnLoad(go);

                loaded = true;
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        [HarmonyPrefix]
        private static void StartRound()
        {
            if (!RoundManager.Instance.IsHost)
                return;

            if (GameState.ForceLoadEvent != null)
            {
                GameState.CurrentEvent = GameState.ForceLoadEvent.Value;
                GameState.ForceLoadEvent = null;
            }
            else
            {
                GameState.CurrentEvent = (GameEvent)Random.Range(0, Enum.GetNames(typeof(GameEvent)).Length);
            }

            if (StartOfRound.Instance.currentLevel.sceneName == "CompanyBuilding")
            {
                GameState.CurrentEvent = GameEvent.None;
            }

            ObjectSpawner.CleanupAllSpawns();

            switch (GameState.CurrentEvent)
            {
                case GameEvent.SethsFridge:
                {
                    SelectableLevel rendLevel =
                        StartOfRound.Instance.levels.FirstOrDefault(l => l.PlanetName.Contains("Rend"));

                    EventManager.SetupLevelDataClient(rendLevel);
                    EventSyncer.SetLevelDataSyncAll(rendLevel);

                    break;
                }

                case GameEvent.SeaWorld:
                {
                    EventManager.SetWeatherClient(LevelWeatherType.Flooded);
                    EventSyncer.SetWeatherSyncAll(LevelWeatherType.Flooded);

                    break;
                }

                case GameEvent.Speedrun:
                {
                    SelectableLevel currentLevel = Instantiate(StartOfRound.Instance.currentLevel);
                    currentLevel.DaySpeedMultiplier = 2f;

                    EventManager.SetupLevelDataClient(currentLevel);
                    EventSyncer.SetLevelDataSyncAll(currentLevel);

                    break;
                }

                case GameEvent.LongDay:
                {
                    SelectableLevel currentLevel = Instantiate(StartOfRound.Instance.currentLevel);
                    currentLevel.DaySpeedMultiplier = 0.5f;

                    EventManager.SetupLevelDataClient(currentLevel);
                    EventSyncer.SetLevelDataSyncAll(currentLevel);

                    break;
                }

                case GameEvent.ShortDay:
                {
                    SelectableLevel currentLevel = Instantiate(StartOfRound.Instance.currentLevel);
                    currentLevel.DaySpeedMultiplier = 1.45f;

                    EventManager.SetupLevelDataClient(currentLevel);
                    EventSyncer.SetLevelDataSyncAll(currentLevel);

                    break;
                }
            }

            EventSyncer.GameEventSyncAll(GameState.CurrentEvent);
        }

        [HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
        [HarmonyPostfix]
        private static void EndRound()
        {
            ResetEvents();
        }

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ModifyLevel(ref SelectableLevel newLevel)
        {
            if (!RoundManager.Instance.IsHost)
                return true;

            if (!levelHeatVal.ContainsKey(newLevel))
            {
                levelHeatVal.Add(newLevel, 0f);
            }

            if (!levelEnemySpawns.ContainsKey(newLevel))
            {
                List<SpawnableEnemyWithRarity> list = new List<SpawnableEnemyWithRarity>();
                foreach (SpawnableEnemyWithRarity item in newLevel.Enemies)
                {
                    list.Add(item);
                }

                levelEnemySpawns.Add(newLevel, list);
            }

            levelEnemySpawns.TryGetValue(newLevel, out List<SpawnableEnemyWithRarity> enemies);
            newLevel.Enemies = enemies;

            foreach (SelectableLevel key in levelHeatVal.Keys.ToList())
            {
                levelHeatVal.TryGetValue(key, out float num);
                levelHeatVal[key] = Mathf.Clamp(num - 5f, 0f, 100f);

                if (GameState.CurrentEvent == GameEvent.HeatReset | GameState.CurrentEvent == GameEvent.All)
                {
                    levelHeatVal[key] = 0f;
                }
            }

            levelHeatVal.TryGetValue(newLevel, out float heatLevel);

            HUDManager.Instance.AddTextToChatOnServer(
                "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n"
            );
            HUDManager.Instance.AddTextToChatOnServer(
                $"<color=orange>Moon is at {heatLevel}% heat.</color>"
            );

            if (heatLevel > 49f)
            {
                HUDManager.Instance.AddTextToChatOnServer(
                    "<color=red>The heat level is comically high. <color=white>\nVisit other moons to lower the heat level.</color>"
                );
            }

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
            {
                if (!enemyRarities.ContainsKey(spawnableEnemyWithRarity))
                {
                    enemyRarities.Add(spawnableEnemyWithRarity, spawnableEnemyWithRarity.rarity);
                }

                enemyRarities.TryGetValue(spawnableEnemyWithRarity, out int rarity);
                spawnableEnemyWithRarity.rarity = rarity;
            }

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity2 in newLevel.Enemies)
            {
                if (!enemyPropCurves.ContainsKey(spawnableEnemyWithRarity2))
                {
                    enemyPropCurves.Add(spawnableEnemyWithRarity2,
                        spawnableEnemyWithRarity2.enemyType.probabilityCurve);
                }

                enemyPropCurves.TryGetValue(spawnableEnemyWithRarity2, out AnimationCurve probabilityCurve);
                spawnableEnemyWithRarity2.enemyType.probabilityCurve = probabilityCurve;
            }

            switch (GameState.CurrentEvent)
            {
                case GameEvent.None:
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Level event: None</color>");
                    break;

                case GameEvent.NiceDay:
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Level event: Have a Nice Day! :)</color>");

                    EventManager.SetWeatherClient(LevelWeatherType.None);
                    EventSyncer.SetWeatherSyncAll(LevelWeatherType.None);

                    break;

                case GameEvent.HorribleDay:
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: What a Horrible Night to Have a Curse</color>");

                    EventManager.SetWeatherClient(LevelWeatherType.Eclipsed);
                    EventSyncer.SetWeatherSyncAll(LevelWeatherType.Eclipsed);

                    break;

                case GameEvent.SethsFridge:
                {
                    SelectableLevel rendLevel =
                        Instantiate(StartOfRound.Instance.levels.FirstOrDefault(l => l.PlanetName.Contains("Rend")));

                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: Free luxury vacation to Seth's Fridge!</color>"
                    );

                    EventManager.SetupLevelDataClient(rendLevel);
                    EventSyncer.SetLevelDataSyncAll(rendLevel);

                    break;
                }

                case GameEvent.LongDay:
                {
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=#FCCB14>Level event: It's been one of those days...</color>");
                    break;
                }

                case GameEvent.ShortDay:
                {
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=#FCCB14>Level event: Feels like there's never enough time...</color>");
                    break;
                }


                case GameEvent.SeaWorld:
                {
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=#14FCE7>Level event: Welcome to SeaWorld!</color>"
                    );

                    // Replace all items with fish
                    foreach (SpawnableItemWithRarity spawnableItemWithRarity in newLevel.spawnableScrap)
                    {
                        spawnableItemWithRarity.rarity = spawnableItemWithRarity.spawnableItem.itemName.ToLower()
                            .Contains("plastic fish")
                            ? 999
                            : 0;
                    }

                    break;
                }

                case GameEvent.ClownWorld:
                {
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=#FC9A14>Level event: Clown World</color>"
                    );

                    // Replace all items with horns :)
                    foreach (SpawnableItemWithRarity spawnableItemWithRarity in newLevel.spawnableScrap)
                    {
                        spawnableItemWithRarity.rarity =
                            spawnableItemWithRarity.spawnableItem.itemName.ToLower() switch
                            {
                                "clown horn" => 999,
                                "airhorn" => 999,
                                _ => 0
                            };
                    }

                    break;
                }

                case GameEvent.Speedrun:
                {
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: It's Speedrunning Time!</color>");

                    EventManager.SetPlayerMovementSpeedClient(8f);
                    EventSyncer.SetPlayerMovementSpeedSyncAll(8f);

                    break;
                }

                case GameEvent.Turret:
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: Turrets. Lots of Turrets.</color>");
                    break;

                case GameEvent.Landmine:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: Landmines Abound!</color>");
                    break;

                case GameEvent.Hoarding:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: The Backstreets</color>");

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
                    {
                        if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
                        {
                            spawnableEnemyWithRarity.rarity = 999;
                        }
                    }

                    break;

                case GameEvent.Bullshit:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: That's BULLSHIT</color>");
                    break;

                case GameEvent.SnareFleas:
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: Snare Flea and Friends!</color>"
                    );

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
                    {
                        spawnableEnemyWithRarity.rarity = 0;

                        if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
                        {
                            spawnableEnemyWithRarity.rarity = 999;
                        }

                        if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
                        {
                            spawnableEnemyWithRarity.rarity = 999;
                        }

                        if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
                        {
                            spawnableEnemyWithRarity.rarity = 999;
                        }
                    }

                    break;

                case GameEvent.BrackenAndCoil:
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: Polar Opposites</color>"
                    );

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
                    {
                        spawnableEnemyWithRarity.rarity = 0;

                        if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
                        {
                            spawnableEnemyWithRarity.rarity = 999;
                        }

                        if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
                        {
                            spawnableEnemyWithRarity.rarity = 999;
                        }
                    }

                    break;

                case GameEvent.Chaos:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: Chaos Theory</color>");

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
                    {
                        spawnableEnemyWithRarity.enemyType.probabilityCurve =
                            new AnimationCurve(new Keyframe(0f, 1000f));
                    }

                    break;

                case GameEvent.BlazeIt420:
                {
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Level event: 420 Blaze It</color>");

                    EventManager.SetupBlazedClient();
                    EventSyncer.PlayersBlazedSyncAll();

                    break;
                }

                case GameEvent.Delivery:
                {
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Level event: Extra Delivery!</color>");

                    int itemsCount = Random.Range(2, 9);
                    for (var i = 0; i < itemsCount; i++)
                    {
                        int item2 = Random.Range(0, 6);
                        FindObjectOfType<Terminal>().orderedItemsFromTerminal.Add(item2);
                    }

                    break;
                }

                case GameEvent.ReplaceItems:
                {
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: Communication is Key!</color>"
                    );

                    Terminal terminal = FindObjectOfType<Terminal>();
                    int orderedItemCount = terminal.orderedItemsFromTerminal.Count;
                    if (orderedItemCount == 0)
                    {
                        orderedItemCount = 1;
                    }

                    terminal.orderedItemsFromTerminal.Clear();

                    for (var i = 0; i < orderedItemCount; i++)
                    {
                        terminal.orderedItemsFromTerminal.Add(0);
                    }

                    break;
                }

                case GameEvent.Psychosis:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: It's not psychosis.</color>");

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
                    {
                        DressGirlAI dressGirlAI =
                            spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<DressGirlAI>();
                        if (dressGirlAI != null)
                        {
                            spawnableEnemyWithRarity.rarity = 9999;
                        }
                    }

                    break;

                case GameEvent.HeatReset:
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=blue>Level event: Time to Cooldown! (Heat has been reset)</color>");

                    break;

                case GameEvent.AutomatedTurretDefenseSystem:
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: The Company's Automated Defense System</color>");
                    ObjectSpawner.ShouldSpawnTurret = true;

                    break;

                case GameEvent.All:
                    HUDManager.Instance.AddTextToChatOnServer(
                        "<color=red>Level event: Everything but the kitchen sink</color>");
                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity8 in newLevel.Enemies)
                    {
                        spawnableEnemyWithRarity8.enemyType.probabilityCurve =
                            new AnimationCurve(new Keyframe(0f, 1000f));
                    }

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity9 in newLevel.Enemies)
                    {
                        spawnableEnemyWithRarity9.rarity = 0;
                        if (spawnableEnemyWithRarity9.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
                        {
                            spawnableEnemyWithRarity9.rarity = 999;
                        }

                        if (spawnableEnemyWithRarity9.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
                        {
                            spawnableEnemyWithRarity9.rarity = 999;
                        }
                    }

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity10 in newLevel.Enemies)
                    {
                        if (spawnableEnemyWithRarity10.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
                        {
                            spawnableEnemyWithRarity10.rarity = 999;
                        }
                    }

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity11 in newLevel.Enemies)
                    {
                        if (spawnableEnemyWithRarity11.enemyType.enemyPrefab.GetComponent<LassoManAI>() != null)
                        {
                            spawnableEnemyWithRarity11.rarity = 999;
                        }
                    }

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity12 in newLevel.Enemies)
                    {
                        if (spawnableEnemyWithRarity12.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
                        {
                            spawnableEnemyWithRarity12.rarity = 999;
                        }
                    }

                    foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity13 in newLevel.Enemies)
                    {
                        if (spawnableEnemyWithRarity13.enemyType.enemyPrefab.GetComponent<DressGirlAI>() != null)
                        {
                            spawnableEnemyWithRarity13.rarity = 9999;
                        }
                    }

                    int itemCount = Random.Range(2, 9);
                    for (var k = 0; k < itemCount; k++)
                    {
                        int randomItems = Random.Range(0, 3);
                        FindObjectOfType<Terminal>().orderedItemsFromTerminal.Add(randomItems);
                    }

                    Terminal terminal2 = FindObjectOfType<Terminal>();
                    int count = terminal2.orderedItemsFromTerminal.Count;
                    terminal2.orderedItemsFromTerminal.Clear();
                    for (var l = 0; l < count; l++)
                    {
                        terminal2.orderedItemsFromTerminal.Add(0);
                    }

                    break;
            }

            SelectableLevel selectableLevel = newLevel;
            Log.LogWarning("Map Objects");

            foreach (SpawnableMapObject spawnableMapObject in selectableLevel.spawnableMapObjects)
            {
                if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                {
                    ObjectSpawner.Turret = spawnableMapObject.prefabToSpawn;
                    if (GameState.CurrentEvent == GameEvent.Turret | GameState.CurrentEvent == GameEvent.All)
                    {
                        spawnableMapObject.numberToSpawn =
                            new AnimationCurve(new Keyframe(0f, 200f), new Keyframe(1f, 25f));
                    }
                    else
                    {
                        spawnableMapObject.numberToSpawn =
                            new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 10f));
                    }
                }
                else if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
                {
                    if (GameState.CurrentEvent == GameEvent.Landmine | GameState.CurrentEvent == GameEvent.All)
                    {
                        spawnableMapObject.numberToSpawn =
                            new AnimationCurve(new Keyframe(0f, 175f), new Keyframe(1f, 150f));
                    }
                    else
                    {
                        spawnableMapObject.numberToSpawn =
                            new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 70f));
                    }
                }

                Log.LogInfo(spawnableMapObject.prefabToSpawn.ToString());
            }

            Log.LogWarning("Enemies");
            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity14 in selectableLevel.Enemies)
            {
                Log.LogInfo(spawnableEnemyWithRarity14.enemyType.enemyName + "--rarity = " +
                            spawnableEnemyWithRarity14.rarity);
            }

            Log.LogWarning("Daytime Enemies");
            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity15 in selectableLevel.DaytimeEnemies)
            {
                Log.LogInfo(spawnableEnemyWithRarity15.enemyType.enemyName);
            }

            if (!LevelsModified.Contains(newLevel))
            {
                LevelsModified.Add(newLevel);

                selectableLevel.maxScrap += 45;
                selectableLevel.maxTotalScrapValue += 800;
                selectableLevel.daytimeEnemySpawnChanceThroughDay =
                    new AnimationCurve(new Keyframe(0f, 7f), new Keyframe(0.5f, 7f));
                selectableLevel.maxEnemyPowerCount += 2000;
                selectableLevel.maxOutsideEnemyPowerCount += 20;
                selectableLevel.maxDaytimeEnemyPowerCount += 200;

                newLevel = selectableLevel;
            }

            selectableLevel.enemySpawnChanceThroughoutDay = new AnimationCurve(
                new Keyframe(0f, 0.1f + heatLevel),
                new Keyframe(0.5f, 500f + heatLevel)
            );

            selectableLevel.outsideEnemySpawnChanceThroughDay = new AnimationCurve(
                new Keyframe(0f, -30f + heatLevel),
                new Keyframe(20f, -30f + heatLevel),
                new Keyframe(21f, 10f + heatLevel)
            );

            if (GameState.CurrentEvent == GameEvent.Bullshit || GameState.CurrentEvent == GameEvent.All)
            {
                selectableLevel.outsideEnemySpawnChanceThroughDay = new AnimationCurve(
                    new Keyframe(0f, 10f + heatLevel),
                    new Keyframe(20f, 10f + heatLevel),
                    new Keyframe(21f, 10f + heatLevel)
                );
            }

            if (GameState.CurrentEvent == GameEvent.Hoarding || GameState.CurrentEvent == GameEvent.All)
            {
                selectableLevel.enemySpawnChanceThroughoutDay =
                    new AnimationCurve(new Keyframe(0f, 500f + heatLevel));
            }

            if (GameState.CurrentEvent == GameEvent.Chaos || GameState.CurrentEvent == GameEvent.All)
            {
                selectableLevel.enemySpawnChanceThroughoutDay =
                    new AnimationCurve(new Keyframe(0f, 500f + heatLevel));
            }

            levelHeatVal.TryGetValue(newLevel, out heatLevel);
            levelHeatVal[newLevel] = Mathf.Clamp(heatLevel + 20f, 0f, 100f);

            Terminal terminal3 = FindObjectOfType<Terminal>();
            terminal3.groupCredits += 120;
            terminal3.SyncGroupCreditsServerRpc(terminal3.groupCredits, terminal3.numberOfItemsInDropship);

            return true;
        }

        [HarmonyPatch(typeof(RoundManager), "FinishGeneratingLevel")]
        [HarmonyPrefix]
        private static void OnFinishGeneratingLevel()
        {
            if (GameState.ForceLoadLevel == null)
                return;

            SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;

            RoundManager.Instance.currentLevel = GameState.ForceLoadLevel;
            RoundManager.Instance.currentLevel.currentWeather = currentLevel.currentWeather;

            GameState.ForceLoadLevel = null;
        }

        private static void ResetEvents()
        {
            GameState.ForcePlayerMovementSpeed = null;
            GameState.ForceLoadLevel = null;

            // Reset player movement speed
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                player.movementSpeed = DefaultMovementSpeed;
            }

            GameNetworkManager.Instance.localPlayerController.movementSpeed = DefaultMovementSpeed;

            // Remove effect components
            foreach (PlayerEffectBlazed effect in FindObjectsOfType<PlayerEffectBlazed>())
            {
                Destroy(effect);
            }

            Log.LogInfo("Resetting all events.");
        }
    }
}