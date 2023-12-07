using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ShortDayEvent : IGameEvent
{
    public string Description => "Feels like there's never enough time...";
    public Color Color => Color.yellow;
    public GameEventType GameEventType => GameEventType.ShortDay;

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
            Multiplier = 1.4f
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