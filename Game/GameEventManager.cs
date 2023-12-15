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

    public static readonly List<BaseGameEvent> EnabledEvents = new List<BaseGameEvent>
    {
        // Standard original Brutal-company based events
        new NoneEvent(),
        new TurretEvent(),
        new LandmineEvent(),
        new HoardingEvent(),
        new SnareFleasEvent(),
        new BrackenAndCoilEvent(),
        new ChaosEvent(),
        new DeliveryEvent(),
        new WalkieTalkieEvent(),
        new PsychosisEvent(),
        new DangerResetEvent(),
        new AllEvent(),

        // Travel special events
        new GoToSethsFridgeEvent(),
        new GoToTitanEvent(),

        // Time/weather events
        new NiceDayEvent(),
        new HorribleDayEvent(),
        new LongDayEvent(),
        new ShortDayEvent(),

        // Unique/Minigame style events
        new ThespianSocietyEvent(),
        new BlazeIt420Event(),
        new SeaWorldEvent(),
        new SpeedRunEvent(),
        new ClownWorldEvent(),
        new ClownExpoEvent(),
        new RackAndRollEvent(),
        new WendigoEvent(),
    };

    public static readonly Dictionary<SelectableLevel, float> DangerLevels = new Dictionary<SelectableLevel, float>();

    private static Queue<BaseGameEvent> EventQueue = new Queue<BaseGameEvent>();

    private static readonly Dictionary<int, SelectableLevel> ClonedLevels = new Dictionary<int, SelectableLevel>();

    private static Terminal terminal;

    private static bool initialized;

    private static Terminal Terminal => terminal != null ? terminal : terminal = FindObjectOfType<Terminal>();

    private void OnDestroy()
    {
        DangerLevels.Clear();
        foreach (SelectableLevel selectableLevel in StartOfRound.Instance.levels)
        {
            ResetLevel(selectableLevel);
        }

        initialized = false;
    }

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPostfix]
    private static void OnStartOfRoundStart()
    {
        DangerLevels.Clear();
        foreach (SelectableLevel selectableLevel in StartOfRound.Instance.levels)
        {
            ResetLevel(selectableLevel);
        }

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
            if (EventQueue.IsNullOrEmpty())
            {
                var clonedList = new List<BaseGameEvent>(EnabledEvents);
                clonedList.Shuffle();

                EventQueue = new Queue<BaseGameEvent>(clonedList);
            }

            GameState.CurrentGameEvent = EventQueue.Dequeue();
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

            // Reset the enemy spawns
            ResetLevelForRound(newLevel);

            HUDManager.Instance.AddTextToChatOnServer(
                $"<color=#{GameState.CurrentGameEvent.Color.ToHex()}>Event: {GameState.CurrentGameEvent.Description}</color>"
            );

            GameState.CurrentGameEvent.OnServerInitialize(newLevel);
        }
    }

    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    [HarmonyPrefix]
    private static bool ModifyLevel(ref int randomSeed, ref SelectableLevel newLevel)
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

            if (key.levelID != newLevel.levelID)
            {
                DangerLevels[key] = Mathf.Clamp(num - 5f, 0f, 100f);
            }
        }

        DangerLevels.TryGetValue(newLevel, out float dangerLevel);

        newLevel.maxScrap += 45;
        newLevel.maxTotalScrapValue += 800;
        newLevel.daytimeEnemySpawnChanceThroughDay =
            new AnimationCurve(new Keyframe(0f, 7f), new Keyframe(0.5f, 7f));
        newLevel.maxEnemyPowerCount += 2000;
        newLevel.maxOutsideEnemyPowerCount += 20;
        newLevel.maxDaytimeEnemyPowerCount += 200;

        newLevel.enemySpawnChanceThroughoutDay = new AnimationCurve(
            new Keyframe(0f, 0.1f + dangerLevel),
            new Keyframe(0.5f, 500f + dangerLevel)
        );

        DangerLevels[newLevel] = Mathf.Clamp(dangerLevel + 20f, 0f, 100f);

        Terminal.groupCredits += 120;
        Terminal.SyncGroupCreditsServerRpc(Terminal.groupCredits, Terminal.numberOfItemsInDropship);

        if (GameState.CurrentGameEvent != null)
        {
            GameState.CurrentGameEvent.OnPreModifyLevel(ref newLevel);
        }

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

        return true;
    }

    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    [HarmonyPostfix]
    private static void PostModifyLevel(int randomSeed, SelectableLevel newLevel)
    {
        if (GameState.CurrentGameEvent != null)
        {
            GameState.CurrentGameEvent.OnPostModifyLevel(randomSeed, newLevel);
        }
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
        GameState.CurrentGameEvent?.OnPreFinishGeneratingLevel();
    }

    [HarmonyPatch(typeof(RoundManager), "FinishGeneratingLevel")]
    [HarmonyPostfix]
    private static void OnPostFinishGeneratingLevel()
    {
        GameState.CurrentGameEvent?.OnPostFinishGeneratingLevel();

        if (RoundManager.Instance.IsServer && GameState.CurrentGameEvent?.GameEventType != GameEventType.None)
        {
            NetworkUtils.BroadcastAll(new PacketGameTip
            {
                Header = """Adventure Alert™""",
                Body = GameState.CurrentGameEvent?.Description ?? "<unknown>"
            });
        }
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

    private static void ResetLevel(SelectableLevel level)
    {
        if (ClonedLevels.TryGetValue(level.levelID, out SelectableLevel originalLevel))
        {
            level.minScrap = originalLevel.minScrap;
            level.maxScrap = originalLevel.maxScrap;
            level.maxEnemyPowerCount = originalLevel.maxEnemyPowerCount;
            level.maxOutsideEnemyPowerCount = originalLevel.maxEnemyPowerCount;
            level.maxDaytimeEnemyPowerCount = originalLevel.maxDaytimeEnemyPowerCount;

            level.daytimeEnemySpawnChanceThroughDay = originalLevel.daytimeEnemySpawnChanceThroughDay;
            level.enemySpawnChanceThroughoutDay = originalLevel.enemySpawnChanceThroughoutDay;
        }

        ResetLevelForRound(level);
    }

    private static void ResetLevelForRound(SelectableLevel level)
    {
        if (!ClonedLevels.ContainsKey(level.levelID))
        {
            Plugin.Log.LogInfo($"Creating copy for level {level.levelID}");
            SelectableLevel clonedLevel = level.DeepCopy();
            ClonedLevels.Add(level.levelID, clonedLevel);
        }

        if (ClonedLevels.TryGetValue(level.levelID, out SelectableLevel originalLevel))
        {
            CopyUtils.CopyLevelProperties(ref level, originalLevel);
        }
    }
}