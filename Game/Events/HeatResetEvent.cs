using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class HeatResetEvent : IGameEvent
{
    public string Description => "Time to Cooldown! (Heat has been reset)";
    public Color Color => Color.cyan;
    public GameEventType GameEventType => GameEventType.HeatReset;

    // These are handled by the plugin iself, so we don't need to do anything here.
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