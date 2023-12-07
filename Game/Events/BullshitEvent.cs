using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class BullshitEvent : IGameEvent
{
    public string Description => "WTF That's BULLSHIT";
    public Color Color => new Color(0.5f, 0.25f, 0.05f);
    public GameEventType GameEventType => GameEventType.Bullshit;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}