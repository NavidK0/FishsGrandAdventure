using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class NoneEvent : BaseGameEvent
{
    public override string Description => "None";
    public override Color Color => Color.green;
    public override GameEventType GameEventType => GameEventType.None;
}