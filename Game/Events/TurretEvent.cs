using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class TurretEvent : IGameEvent
{
    public string Description => "Turrets. Lots of Turrets.";
    public Color Color => Color.red;

    public GameEventType GameEventType => GameEventType.Turret;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}