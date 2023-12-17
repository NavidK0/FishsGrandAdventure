using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class DeliveryEvent : BaseGameEvent
{
    public override string Name => "Extra Delivery";
    public override string Description => "This one's on the house!";
    public override Color Color => Color.green;
    public override GameEventType GameEventType => GameEventType.Delivery;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        int itemsCount = Random.Range(2, 9);
        for (var i = 0; i < itemsCount; i++)
        {
            int item2 = Random.Range(0, 6);
            Object.FindObjectOfType<Terminal>().orderedItemsFromTerminal.Add(item2);
        }
    }
}