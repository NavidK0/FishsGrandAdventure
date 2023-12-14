using FishsGrandAdventure.Network;
using HarmonyLib;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class SpeedRunEvent : BaseGameEvent
{
    public override string Description => "It's Speedrunning Time!";
    public override Color Color => new Color(0.78f, 1f, 0f);
    public override GameEventType GameEventType => GameEventType.SpeedRun;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        NetworkUtils.BroadcastAll(new PacketSetPlayerMoveSpeed
        {
            Speed = 8f
        });
    }

    public override void OnPreFinishGeneratingLevel()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 2f
        });
    }

    public override void Cleanup()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 1f
        });
    }
}

public static class PatchSpeedRun
{
    [HarmonyPatch(typeof(EnemyAI), "Start")]
    [HarmonyPostfix]
    public static void PatchEnemySpeed(EnemyAI __instance)
    {
        if (GameState.CurrentGameEvent?.GameEventType == GameEventType.SpeedRun)
        {
            __instance.agent.speed *= 2f;
        }
    }
}