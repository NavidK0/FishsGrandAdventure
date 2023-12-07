using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class LandmineEvent : IGameEvent
{
    public string Description => "Landmines Abound!";
    public Color Color => Color.red;
    public GameEventType GameEventType => GameEventType.Landmine;

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