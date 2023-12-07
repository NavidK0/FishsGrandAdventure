using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class DeliveryEvent : IGameEvent
{
    public string Description => "Extra Delivery!";
    public Color Color => Color.green;
    public GameEventType GameEventType => GameEventType.Delivery;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        int itemsCount = Random.Range(2, 9);
        for (var i = 0; i < itemsCount; i++)
        {
            int item2 = Random.Range(0, 6);
            Object.FindObjectOfType<Terminal>().orderedItemsFromTerminal.Add(item2);
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}