using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Game.Events;
using FishsGrandAdventure.Patches;
using HarmonyLib;
using UnityEngine;

namespace FishsGrandAdventure;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public static MethodInfo Chat;

    private static bool loaded;

    private readonly Harmony harmony = new Harmony("NavidK0.FishsGrandAdventure");

    private void Awake()
    {
        Log = Logger;

        Chat = AccessTools.Method(typeof(HUDManager), "AddChatMessage");

        Log.LogInfo("Loading Fish's Grand Adventure...");

        // Game Event Manager
        harmony.PatchAll(typeof(GameEventManager));

        // Patches
        harmony.PatchAll(typeof(ClownWorld));
        harmony.PatchAll(typeof(CommandListener));
        harmony.PatchAll(typeof(CustomMoonManager));
        harmony.PatchAll(typeof(PlayerControllerBPatcher));
    }

    public void OnDestroy()
    {
        if (!loaded)
        {
            var eventManagerGo = new GameObject("FishsGrandAdventure.GameEventManager");
            eventManagerGo.AddComponent<GameEventManager>();
            DontDestroyOnLoad(eventManagerGo);
            Log.LogInfo("Added GameEventManager");

            loaded = true;

            Log.LogInfo("Loaded Fish's Grand Adventure!");
        }
    }
}