using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public interface IGameEvent
{
    public string Description { get; }
    public Color Color { get; }
    public GameEventType GameEventType { get; }

    /**
     * This is called when the server is initializing the level.
     * Server-side.
     */
    public void OnServerInitialize(SelectableLevel level);

    /**
     * This is called before the level is generated.
     * Server-side.
     */
    public void OnBeforeModifyLevel(ref SelectableLevel level);

    /**
     * This is called after the level has been generated.
     * Client-side and server-side.
     */
    public void OnFinishGeneratingLevel();

    /**
     * This is called when the event is being cleaned up.
     * Client-side and server-side.
     */
    public void Cleanup();
}