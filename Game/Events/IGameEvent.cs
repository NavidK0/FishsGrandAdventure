using JetBrains.Annotations;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

[PublicAPI]
public abstract class BaseGameEvent
{
    public abstract string Name { get; }
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
     * This is called before the level is generated.
     * Client-side.
     */
    public virtual void OnPreGenerateLevel(int randomSeed, int levelID)
    {
    }


    public virtual void OnPostGenerateLevel(int randomSeed, int levelID)
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
     * Called when the round starts, before the doors have opened.
     * Client-side.
     */
    public virtual void OnPreRoundStart()
    {
    }

    /**
     * Called when the round starts after everything else and after the ship has landed.
     * Client-side.
     */
    public virtual void OnPostRoundStart()
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