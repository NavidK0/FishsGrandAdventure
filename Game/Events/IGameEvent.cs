using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public interface IGameEvent
{
    public string Description { get; }
    public Color Color { get; }
    public GameEventType GameEventType { get; }

    public void OnServerInitialize(SelectableLevel level);
    public void OnBeforeModifyLevel(ref SelectableLevel level);
    public void OnFinishGeneratingLevel();
    public void Cleanup();
}