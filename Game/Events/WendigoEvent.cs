using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class WendigoEvent : BaseGameEvent
{
    public override string Description => "The Wendigo";
    public override Color Color => new Color(0.2f, 0.07f, 0.09f);
    public override GameEventType GameEventType => GameEventType.Wendigo;

    public override void OnPostFinishGeneratingLevel()
    {
        NetworkUtils.BroadcastAll(new PacketStopBoomboxes());
        NetworkUtils.BroadcastAll(new PacketPlayMusic
        {
            Name = "mc_cave_5"
        });
    }
}