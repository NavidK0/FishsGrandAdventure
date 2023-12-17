using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class WalkieTalkieEvent : BaseGameEvent
{
    public override string Name => "Walkie Talkies";
    public override string Description => "Communication is Key!";
    public override Color Color => new Color(0.5f, 0.5f, 0.5f);
    public override GameEventType GameEventType => GameEventType.WalkieTalkies;

    public override void OnPreModifyLevel(ref SelectableLevel level)
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
}