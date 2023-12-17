using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ShortDayEvent : BaseGameEvent
{
    public override string Name => "Short Day";
    public override string Description => "Feels like there's never enough time...";
    public override Color Color => Color.yellow;
    public override GameEventType GameEventType => GameEventType.ShortDay;

    public override void OnPreFinishGeneratingLevel()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 1.4f
        });
    }

    public override void CleanupServer()
    {
        NetworkUtils.BroadcastAll(new PacketGlobalTimeSpeedMultiplier
        {
            Multiplier = 1f
        });
    }
}