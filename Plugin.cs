using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using FishsGrandAdventure.Audio;
using FishsGrandAdventure.Behaviors.Wendigo;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Game.Events;
using FishsGrandAdventure.Network;
using HarmonyLib;
using UnityEngine;
using NetworkTransport = FishsGrandAdventure.Network.NetworkTransport;

namespace FishsGrandAdventure;

public static class ModInfo
{
    public const string Guid = "FishsGrandAdventure";
    public const string Name = "Fish's Grand Adventure";
    public const string Version = "1.2.3";

    public static string[] Dependencies => new string[]
    {
    };
}

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static string FileLocation;
    public static ManualLogSource Log;
    public static MethodInfo Chat;

    private static bool loaded;

    private readonly Harmony harmony = new Harmony("NavidK0.FishsGrandAdventure");

    private void Awake()
    {
        FileLocation = Info.Location;
        Log = Logger;

        Chat = AccessTools.Method(typeof(HUDManager), "AddChatMessage");

        Log.LogInfo("Loading Fish's Grand Adventure...");

        // Game Event Manager
        harmony.PatchAll(typeof(GameEventManager));

        // Network Transport Patch
        harmony.PatchAll(typeof(NetworkTransportPatch));

        // Event Patches
        harmony.PatchAll(typeof(PatchClownWorld));
        harmony.PatchAll(typeof(PatchClownExpo));
        harmony.PatchAll(typeof(PatchSpeedRun));
        harmony.PatchAll(typeof(PatchSeaWorld));
        harmony.PatchAll(typeof(PatchRackAndRoll));
        harmony.PatchAll(typeof(PatchEscapeFactoryEvent));

        // Misc
        harmony.PatchAll(typeof(CommandListener));
        harmony.PatchAll(typeof(CustomMoonManager));
        harmony.PatchAll(typeof(PlayerControllerBPatcher));

        NetworkTransport.GetString += NetworkUtils.OnMessageReceived;
    }

    public void OnDestroy()
    {
        if (!loaded)
        {
            var eventManagerGo = new GameObject("FishsGrandAdventure.GameEventManager");
            eventManagerGo.AddComponent<GameEventManager>();
            DontDestroyOnLoad(eventManagerGo);

            Log.LogInfo("Added GameEventManager");

            var audioManagerGo = new GameObject("FishsGrandAdventure.AudioManager");
            eventManagerGo.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioManagerGo);

            Log.LogInfo("Added AudioManager");

            var wendigoGo = new GameObject("FishsGrandAdventure.WendigoRecorder");
            wendigoGo.AddComponent<WendigoRecorder>();
            DontDestroyOnLoad(wendigoGo);

            Log.LogInfo("Added WendigoRecorder");

            loaded = true;

            Log.LogInfo($"Loaded Fish's Grand Adventure v{ModInfo.Version}");
        }
    }
}