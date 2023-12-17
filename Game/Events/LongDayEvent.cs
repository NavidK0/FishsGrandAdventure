using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class LongDayEvent : BaseGameEvent
{
    public override string Name => "Long Day";
    public override string Description => "It's been one of those days...";
    public override Color Color => Color.yellow;
    public override GameEventType GameEventType => GameEventType.LongDay;

    public override void OnPreFinishGeneratingLevel()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 0.5f
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