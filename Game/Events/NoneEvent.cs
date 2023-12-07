using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class NoneEvent : IGameEvent
{
    public string Description => "None";
    public Color Color => Color.green;
    public GameEventType GameEventType => GameEventType.None;

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