using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class SpeedRunEvent : IGameEvent
{
    public string Description => "It's Speedrunning Time!";
    public Color Color => new Color(0.78f, 1f, 0f);
    public GameEventType GameEventType => GameEventType.Speedrun;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        NetworkUtils.BroadcastAll(new PacketSetPlayerMoveSpeed
        {
            Speed = 8f
        });
    }

    public void OnFinishGeneratingLevel()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 2f
        });
    }

    public void Cleanup()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 1f
        });
    }
}