using FishsGrandAdventure.Network;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class BlazeIt420Event : IGameEvent
{
    public string Description => "420 Blaze It";
    public Color Color => new Color(0.78f, 1f, 0f);
    public GameEventType GameEventType => GameEventType.BlazeIt420;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        NetworkUtils.BroadcastAll(new PacketPlayersBlazed());
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}