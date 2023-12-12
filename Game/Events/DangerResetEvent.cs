using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class DangerResetEvent : IGameEvent
{
    public string Description => "Things have calmed down! (Danger has been reset)";
    public Color Color => Color.cyan;
    public GameEventType GameEventType => GameEventType.DangerReset;

    // These are handled by the plugin itself, so we don't need to do anything here.
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