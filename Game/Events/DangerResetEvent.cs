using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class DangerResetEvent : BaseGameEvent
{
    public override string Name => "Danger Reset";
    public override string Description => "Things have calmed down!";
    public override Color Color => Color.cyan;
    public override GameEventType GameEventType => GameEventType.DangerReset;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        GameEventManager.DangerLevels[level] = 0f;
    }
}