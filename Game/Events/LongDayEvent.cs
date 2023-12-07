using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class LongDayEvent : IGameEvent
{
    public string Description => "It's been one of those days...";
    public Color Color => Color.yellow;
    public GameEventType GameEventType => GameEventType.LongDay;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
    }

    public void OnFinishGeneratingLevel()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 0.5f
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