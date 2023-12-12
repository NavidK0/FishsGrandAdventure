using System.Collections.Generic;
using System.Linq;
using FishsGrandAdventure.Game.Events;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using HarmonyLib;
using UnityEngine;

namespace FishsGrandAdventure.Game;

public class GameEventManager : MonoBehaviour
{
    public const float DefaultMovementSpeed = 4.6f;

    public static readonly List<IGameEvent> EnabledEvents = new List<IGameEvent>
    {
        // Standard events (server-side)
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
        new DangerResetEvent(),
        new AllEvent(),
        new ThespianSocietyEvent(),

        // Special events
        new SethsFridgeEvent(),
        new NiceDayEvent(),
        new HorribleDayEvent(),
        new LongDayEvent(),
        new ShortDayEvent(),
        new BlazeIt420Event(),
        new SeaWorldEvent(),
        new SpeedRunEvent(),
        new ClownWorldEvent(),
        new ClownExpoEvent(),
        new RackAndRuinEvent()
    };

    private static readonly Dictionary<SelectableLevel, float> DangerLevels = new Dictionary<SelectableLevel, float>();

    private static readonly Dictionary<SelectableLevel, List<SpawnableItemWithRarity>> OriginalItems =
        new Dictionary<SelectableLevel, List<SpawnableItemWithRarity>>();

    private static readonly Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> OriginalEnemies =
        new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();

    private static readonly Dictionary<SelectableLevel, List<SpawnableOutsideObjectWithRarity>> OriginalOutsideObjects =
        new Dictionary<SelectableLevel, List<SpawnableOutsideObjectWithRarity>>();

    private static readonly Dictionary<SpawnableEnemyWithRarity, AnimationCurve> OriginalEnemyPropCurves =
        new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();

    private static Terminal terminal;
    private static bool initialized;

    private static Terminal Terminal => terminal != null ? terminal : terminal = FindObjectOfType<Terminal>();

    private void OnDestroy()
    {
        DangerLevels.Clear();

        initialized = false;
    }

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPostfix]
    private static void OnStartOfRoundStart()
    {
        DangerLevels.Clear();

        Plugin.Log.LogInfo("Clearing out events on StartOfRound Start method");

        if (RoundManager.Instance.IsServer)
        {
            SetupNewEvent();
        }
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

        if (GameState.CurrentGameEvent != null)
        {
            Timing.CallDelayed(1f, () =>
            {
                NetworkUtils.BroadcastAll(new PacketGameEvent
                {
                    GameEventType = GameState.CurrentGameEvent.GameEventType
                });
            });
        }
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

        if (GameState.CurrentGameEvent != null)
        {
            NetworkUtils.BroadcastAll(new PacketGameEvent
            {
                GameEventType = GameState.CurrentGameEvent.GameEventType
            });

            SelectableLevel newLevel = RoundManager.Instance.currentLevel;

            HUDManager.Instance.AddTextToChatOnServer(
                $"<color=#{GameState.CurrentGameEvent.Color.ToHex()}>Event: {GameState.CurrentGameEvent.Description}</color>");

            GameState.CurrentGameEvent.OnServerInitialize(newLevel);
        }
    }

    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    [HarmonyPrefix]
    private static bool ModifyLevel(ref SelectableLevel newLevel)
    {
        if (!RoundManager.Instance.IsServer)
            return true;

        if (!DangerLevels.ContainsKey(newLevel))
        {
            DangerLevels.Add(newLevel, 0f);
        }

        foreach (SelectableLevel key in DangerLevels.Keys.ToList())
        {
            DangerLevels.TryGetValue(key, out float num);
            DangerLevels[key] = Mathf.Clamp(num - 5f, 0f, 100f);

            if (GameState.CurrentGameEvent?.GameEventType == GameEventType.DangerReset |
                GameState.CurrentGameEvent?.GameEventType == GameEventType.All)
            {
                DangerLevels[key] = 0f;
            }
        }

        DangerLevels.TryGetValue(newLevel, out float dangerLevel);

        HUDManager.Instance.AddTextToChatOnServer(
            "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n"
        );
        HUDManager.Instance.AddTextToChatOnServer(
            $"<color=orange>Danger Level: {dangerLevel}%</color>"
        );

        if (dangerLevel > 49f)
        {
            HUDManager.Instance.AddTextToChatOnServer(
                "<color=red>WARNING: Critical danger levels! Proceed with extreme caution.</color>"
            );
        }

        // Reset the enemy spawns
        ResetItems(newLevel);
        ResetEnemies(newLevel);
        ResetOutsideObjects(newLevel);

        if (newLevel.Enemies != null)
        {
            foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
            {
                if (!OriginalEnemyPropCurves.ContainsKey(enemy))
                {
                    OriginalEnemyPropCurves.Add(enemy,
                        enemy.enemyType.probabilityCurve);
                }

                OriginalEnemyPropCurves.TryGetValue(enemy, out AnimationCurve probabilityCurve);
                enemy.enemyType.probabilityCurve = probabilityCurve;
            }
        }

        if (GameState.CurrentGameEvent != null)
        {
            GameState.CurrentGameEvent.OnBeforeModifyLevel(ref newLevel);
        }

        AddBaseModifiers(newLevel, dangerLevel);

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
        GameState.CurrentGameEvent?.OnFinishGeneratingLevel();

        if (RoundManager.Instance.IsServer && GameState.CurrentGameEvent?.GameEventType != GameEventType.None)
        {
            NetworkUtils.BroadcastAll(new PacketGameTip
            {
                Header = """Adventure Alert™""",
                Body = GameState.CurrentGameEvent?.Description ?? "<unknown>"
            });
        }
    }

    private static void AddBaseModifiers(SelectableLevel selectableLevel, float dangerLevel)
    {
        foreach (SpawnableMapObject spawnableMapObject in selectableLevel.spawnableMapObjects)
        {
            if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
            {
                if (GameState.CurrentGameEvent?.GameEventType == GameEventType.Turret |
                    GameState.CurrentGameEvent?.GameEventType == GameEventType.All)
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
                if (GameState.CurrentGameEvent?.GameEventType == GameEventType.Landmine |
                    GameState.CurrentGameEvent?.GameEventType == GameEventType.All)
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

        selectableLevel.maxScrap += 45;
        selectableLevel.maxTotalScrapValue += 800;
        selectableLevel.daytimeEnemySpawnChanceThroughDay =
            new AnimationCurve(new Keyframe(0f, 7f), new Keyframe(0.5f, 7f));
        selectableLevel.maxEnemyPowerCount += 2000;
        selectableLevel.maxOutsideEnemyPowerCount += 20;
        selectableLevel.maxDaytimeEnemyPowerCount += 200;

        selectableLevel.enemySpawnChanceThroughoutDay = new AnimationCurve(
            new Keyframe(0f, 0.1f + dangerLevel),
            new Keyframe(0.5f, 500f + dangerLevel)
        );

        if (GameState.CurrentGameEvent?.GameEventType == GameEventType.Hoarding ||
            GameState.CurrentGameEvent?.GameEventType == GameEventType.All)
        {
            selectableLevel.enemySpawnChanceThroughoutDay =
                new AnimationCurve(new Keyframe(0f, 500f + dangerLevel));
        }

        if (GameState.CurrentGameEvent?.GameEventType == GameEventType.Chaos ||
            GameState.CurrentGameEvent?.GameEventType == GameEventType.All)
        {
            selectableLevel.enemySpawnChanceThroughoutDay =
                new AnimationCurve(new Keyframe(0f, 500f + dangerLevel));
        }

        DangerLevels.TryGetValue(selectableLevel, out dangerLevel);
        DangerLevels[selectableLevel] = Mathf.Clamp(dangerLevel + 20f, 0f, 100f);

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

    private static void ResetItems(SelectableLevel newLevel)
    {
        if (!OriginalItems.ContainsKey(newLevel))
        {
            List<SpawnableItemWithRarity> list = new List<SpawnableItemWithRarity>();
            foreach (SpawnableItemWithRarity item in newLevel.spawnableScrap)
            {
                list.Add(item);
            }

            OriginalItems.Add(newLevel, list);
        }

        OriginalItems.TryGetValue(newLevel, out List<SpawnableItemWithRarity> spawnableScrap);
        newLevel.spawnableScrap = spawnableScrap;
    }

    private static void ResetEnemies(SelectableLevel newLevel)
    {
        if (!OriginalEnemies.ContainsKey(newLevel))
        {
            List<SpawnableEnemyWithRarity> list = new List<SpawnableEnemyWithRarity>();
            foreach (SpawnableEnemyWithRarity item in newLevel.Enemies)
            {
                list.Add(item);
            }

            OriginalEnemies.Add(newLevel, list);
        }

        OriginalEnemies.TryGetValue(newLevel, out List<SpawnableEnemyWithRarity> enemies);
        newLevel.Enemies = enemies;
    }

    private static void ResetOutsideObjects(SelectableLevel newLevel)
    {
        if (!OriginalOutsideObjects.ContainsKey(newLevel))
        {
            List<SpawnableOutsideObjectWithRarity> list = new List<SpawnableOutsideObjectWithRarity>();
            foreach (SpawnableOutsideObjectWithRarity item in newLevel.spawnableOutsideObjects)
            {
                list.Add(item);
            }

            OriginalOutsideObjects.Add(newLevel, list);
        }

        OriginalOutsideObjects.TryGetValue(newLevel, out List<SpawnableOutsideObjectWithRarity> outsideObjects);

        if (outsideObjects != null)
        {
            newLevel.spawnableOutsideObjects = outsideObjects.ToArray();
        }
    }
}