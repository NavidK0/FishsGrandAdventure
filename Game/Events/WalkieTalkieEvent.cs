using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class WalkieTalkieEvent : IGameEvent
{
    public string Description => "Communication is Key!";
    public Color Color => new Color(0.5f, 0.5f, 0.5f);
    public GameEventType GameEventType => GameEventType.WalkieTalkies;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        Terminal terminal = Object.FindObjectOfType<Terminal>();
        int orderedItemCount = terminal.orderedItemsFromTerminal.Count;
        if (orderedItemCount == 0)
        {
            orderedItemCount = 1;
        }

        terminal.orderedItemsFromTerminal.Clear();

        for (var i = 0; i < orderedItemCount; i++)
        {
            terminal.orderedItemsFromTerminal.Add(0);
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}