﻿using System.Collections.Generic;
using System.Linq;
using FishsGrandAdventure.Game.Events;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using HarmonyLib;
using MEC;
using UnityEngine;

namespace FishsGrandAdventure.Game;

public class GameEventManager : MonoBehaviour
{
    public const float DefaultMovementSpeed = 4.6f;

    public static readonly List<IGameEvent> EnabledEvents = new List<IGameEvent>
    {
        new NoneEvent(),
        new TurretEvent(),
        new LandmineEvent(),
        new HoardingEvent(),
        new BullshitEvent(),
        new SnareFleasEvent(),
        new BrackenAndCoilEvent(),
        new ChaosEvent(),
        new DeliveryEvent(),
        new WalkieTalkieEvent(),
        new PsychosisEvent(),
        new HeatResetEvent(),
        new AllEvent(),

        new SethsFridgeEvent(),
        new NiceDayEvent(),
        new HorribleDayEvent(),
        new LongDayEvent(),
        new ShortDayEvent(),
        new BlazeIt420Event(),
        new SeaWorldEvent(),
        new SpeedRunEvent(),
        new ClownWorldEvent(),
        new ClownExpoEvent()
    };

    private static readonly Dictionary<SelectableLevel, float> LevelHeatVal = new Dictionary<SelectableLevel, float>();
    private static readonly Dictionary<SelectableLevel, GameEventType> MoonEvents =
        new Dictionary<SelectableLevel, GameEventType>();

    private static readonly Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> LevelEnemySpawns =
        new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();

    private static readonly Dictionary<SpawnableEnemyWithRarity, int> EnemyRarities =
        new Dictionary<SpawnableEnemyWithRarity, int>();

    private static readonly Dictionary<SpawnableEnemyWithRarity, AnimationCurve> EnemyPropCurves =
        new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();

    private static Terminal terminal;
    private static bool initialized;

    private static Terminal Terminal => terminal != null ? terminal : terminal = FindObjectOfType<Terminal>();

    private void Awake()
    {
        EnemyRarities.Clear();
        LevelEnemySpawns.Clear();
        EnemyPropCurves.Clear();

        LevelHeatVal.Clear();
    }

    private void OnDestroy()
    {
        EnemyRarities.Clear();
        LevelEnemySpawns.Clear();
        EnemyPropCurves.Clear();

        LevelHeatVal.Clear();

        initialized = false;
    }

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPostfix]
    private static void OnStartOfRoundStart()
    {
        EnemyRarities.Clear();
        LevelEnemySpawns.Clear();
        EnemyPropCurves.Clear();

        LevelHeatVal.Clear();

        Plugin.Log.LogInfo("Clearing out events on StartOfRound Start method");
    }

    [HarmonyPatch(typeof(StartOfRound), "ResetMiscValues")]
    [HarmonyPostfix]
    private static void OnNewRoundStart()
    {
        SetupNewEvent();
    }

    [HarmonyPatch(typeof(StartOfRound), "ChangeLevel")]
    [HarmonyPostfix]
    private static void OnAfterChangeLevel()
    {
        if (GameNetworkManager.Instance.localPlayerController == null)
        {
            return;
        }

        SetupNewEvent();
    }

    [HarmonyPatch(typeof(StartOfRound), "OnClientConnect")]
    [HarmonyPostfix]
    private static void OnClientConnect(ulong clientId)
    {
        if (!RoundManager.Instance.IsServer || GameState.CurrentGameEvent == null) return;

        NetworkUtils.BroadcastAll(new PacketGameEvent
        {
            GameEventType = GameState.CurrentGameEventType
        });
    }

    public static void SetupNewEvent()
    {
        initialized = true;

        ResetEvents();

        if (!RoundManager.Instance.IsServer)
            return;

        if (GameState.ForceLoadEvent != null)
        {
            GameState.CurrentGameEvent = EnabledEvents.Find(e => e.GameEventType == GameState.ForceLoadEvent.Value);
            GameState.ForceLoadEvent = null;
        }
        else
        {
            GameState.CurrentGameEvent =
                EnabledEvents.GetRandomElement();
        }

        if (StartOfRound.Instance.currentLevel.sceneName == "CompanyBuilding")
        {
            GameState.CurrentGameEvent = EnabledEvents.Find(e => e.GameEventType == GameEventType.None);
        }

        NetworkUtils.BroadcastAll(new PacketGameEvent
        {
            GameEventType = GameState.CurrentGameEventType
        });

        SelectableLevel newLevel = RoundManager.Instance.currentLevel;

        HUDManager.Instance.AddTextToChatOnServer(
            $"<color=#{GameState.CurrentGameEvent.Color.ToHex()}>Event: {GameState.CurrentGameEvent.Description}</color>");

        GameState.CurrentGameEvent.OnServerInitialize(newLevel);
    }

    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    [HarmonyPrefix]
    private static bool ModifyLevel(ref SelectableLevel newLevel)
    {
        if (!RoundManager.Instance.IsServer)
            return true;

        if (!LevelHeatVal.ContainsKey(newLevel))
        {
            LevelHeatVal.Add(newLevel, 0f);
        }

        if (!LevelEnemySpawns.ContainsKey(newLevel))
        {
            List<SpawnableEnemyWithRarity> list = new List<SpawnableEnemyWithRarity>();
            foreach (SpawnableEnemyWithRarity item in newLevel.Enemies)
            {
                list.Add(item);
            }

            LevelEnemySpawns.Add(newLevel, list);
        }

        LevelEnemySpawns.TryGetValue(newLevel, out List<SpawnableEnemyWithRarity> enemies);
        newLevel.Enemies = enemies;

        foreach (SelectableLevel key in LevelHeatVal.Keys.ToList())
        {
            LevelHeatVal.TryGetValue(key, out float num);
            LevelHeatVal[key] = Mathf.Clamp(num - 5f, 0f, 100f);

            if (GameState.CurrentGameEventType == GameEventType.HeatReset |
                GameState.CurrentGameEventType == GameEventType.All)
            {
                LevelHeatVal[key] = 0f;
            }
        }

        LevelHeatVal.TryGetValue(newLevel, out float heatLevel);

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

        if (newLevel.Enemies != null)
        {
            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
            {
                if (!EnemyRarities.ContainsKey(spawnableEnemyWithRarity))
                {
                    EnemyRarities.Add(spawnableEnemyWithRarity, spawnableEnemyWithRarity.rarity);
                }

                EnemyRarities.TryGetValue(spawnableEnemyWithRarity, out int rarity);
                spawnableEnemyWithRarity.rarity = rarity;
            }

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity2 in newLevel.Enemies)
            {
                if (!EnemyPropCurves.ContainsKey(spawnableEnemyWithRarity2))
                {
                    EnemyPropCurves.Add(spawnableEnemyWithRarity2,
                        spawnableEnemyWithRarity2.enemyType.probabilityCurve);
                }

                EnemyPropCurves.TryGetValue(spawnableEnemyWithRarity2, out AnimationCurve probabilityCurve);
                spawnableEnemyWithRarity2.enemyType.probabilityCurve = probabilityCurve;
            }
        }

        GameState.CurrentGameEvent.OnBeforeModifyLevel(ref newLevel);

        AddBaseModifiers(newLevel, heatLevel);

        return true;
    }

    [HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
    [HarmonyPostfix]
    private static void EndRound()
    {
        ResetEvents();
    }

    [HarmonyPatch(typeof(RoundManager), "FinishGeneratingLevel")]
    [HarmonyPrefix]
    private static void OnFinishGeneratingLevel()
    {
        GameState.CurrentGameEvent.OnFinishGeneratingLevel();

        if (RoundManager.Instance.IsServer)
        {
            NetworkUtils.BroadcastAll(new PacketGameTip
            {
                Header = "World Event",
                Body = GameState.CurrentGameEvent.Description
            });
        }
    }

    private static void AddBaseModifiers(SelectableLevel selectableLevel, float heatLevel)
    {
        foreach (SpawnableMapObject spawnableMapObject in selectableLevel.spawnableMapObjects)
        {
            if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
            {
                if (GameState.CurrentGameEventType == GameEventType.Turret |
                    GameState.CurrentGameEventType == GameEventType.All)
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
                if (GameState.CurrentGameEventType == GameEventType.Landmine |
                    GameState.CurrentGameEventType == GameEventType.All)
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

            Plugin.Log.LogInfo(spawnableMapObject.prefabToSpawn.ToString());
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity15 in selectableLevel.DaytimeEnemies)
        {
            Plugin.Log.LogInfo(spawnableEnemyWithRarity15.enemyType.enemyName);
        }

        selectableLevel.maxScrap += 45;
        selectableLevel.maxTotalScrapValue += 800;
        selectableLevel.daytimeEnemySpawnChanceThroughDay =
            new AnimationCurve(new Keyframe(0f, 7f), new Keyframe(0.5f, 7f));
        selectableLevel.maxEnemyPowerCount += 2000;
        selectableLevel.maxOutsideEnemyPowerCount += 20;
        selectableLevel.maxDaytimeEnemyPowerCount += 200;

        selectableLevel.enemySpawnChanceThroughoutDay = new AnimationCurve(
            new Keyframe(0f, 0.1f + heatLevel),
            new Keyframe(0.5f, 500f + heatLevel)
        );

        selectableLevel.outsideEnemySpawnChanceThroughDay = new AnimationCurve(
            new Keyframe(0f, -30f + heatLevel),
            new Keyframe(20f, -30f + heatLevel),
            new Keyframe(21f, 10f + heatLevel)
        );

        if (GameState.CurrentGameEventType == GameEventType.Bullshit ||
            GameState.CurrentGameEventType == GameEventType.All)
        {
            selectableLevel.outsideEnemySpawnChanceThroughDay = new AnimationCurve(
                new Keyframe(0f, 10f + heatLevel),
                new Keyframe(20f, 10f + heatLevel),
                new Keyframe(21f, 10f + heatLevel)
            );
        }

        if (GameState.CurrentGameEventType == GameEventType.Hoarding ||
            GameState.CurrentGameEventType == GameEventType.All)
        {
            selectableLevel.enemySpawnChanceThroughoutDay =
                new AnimationCurve(new Keyframe(0f, 500f + heatLevel));
        }

        if (GameState.CurrentGameEventType == GameEventType.Chaos ||
            GameState.CurrentGameEventType == GameEventType.All)
        {
            selectableLevel.enemySpawnChanceThroughoutDay =
                new AnimationCurve(new Keyframe(0f, 500f + heatLevel));
        }

        LevelHeatVal.TryGetValue(selectableLevel, out heatLevel);
        LevelHeatVal[selectableLevel] = Mathf.Clamp(heatLevel + 20f, 0f, 100f);

        Terminal.groupCredits += 120;
        Terminal.SyncGroupCreditsServerRpc(Terminal.groupCredits, Terminal.numberOfItemsInDropship);
    }

    private static void ResetEvents()
    {
        if (!RoundManager.Instance.IsServer || !initialized) return;

        NetworkUtils.BroadcastAll(new PacketResetPlayerSpeed());
        NetworkUtils.BroadcastAll(new PacketDestroyEffects());

        if (GameState.CurrentGameEvent != null)
        {
            GameState.CurrentGameEvent.Cleanup();
            GameState.CurrentGameEvent = null;
        }

        StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
    }
}