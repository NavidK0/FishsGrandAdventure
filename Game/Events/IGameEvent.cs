using JetBrains.Annotations;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

[PublicAPI]
public abstract class BaseGameEvent
{
    public abstract string Description { get; }
    public abstract Color Color { get; }
    public abstract GameEventType GameEventType { get; }

    /**
     * This is called when the server is initializing the level.
     * Server-side.
     */
    public virtual void OnServerInitialize(SelectableLevel level)
    {
    }

    /**
     * This is called before the level is generated.
     * Server-side.
     */
    public virtual void OnPreModifyLevel(ref SelectableLevel level)
    {
    }

    public virtual void OnPostModifyLevel(int randomSeed, SelectableLevel level)
    {
    }

    /**
     * This is called after the level has been generated.
     * Client-side and server-side.
     */
    public virtual void OnPreFinishGeneratingLevel()
    {
    }

    public virtual void OnPostFinishGeneratingLevel()
    {
    }

    /**
     * This is called when the event is being cleaned up.
     * Client-side and server-side.
     */
    public virtual void Cleanup()
    {
    }
}