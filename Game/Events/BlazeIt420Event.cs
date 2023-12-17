using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class BlazeIt420Event : BaseGameEvent
{
    public override string Name => "420 Blaze It";
    public override string Description => "Yooooooooooooooooooooo";
    public override Color Color => new Color(0.78f, 1f, 0f);
    public override GameEventType GameEventType => GameEventType.BlazeIt420;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        NetworkUtils.BroadcastAll(new PacketPlayersBlazed());
    }
}