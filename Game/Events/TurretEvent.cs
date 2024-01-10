using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class TurretEvent : BaseGameEvent
{
    public override string Name => "Too Many Turrets";
    public override string Description => "Lots of turrets. Sorry.";
    public override Color Color => Color.red;

    public override GameEventType GameEventType => GameEventType.Turret;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableMapObject spawnableMapObject in level.spawnableMapObjects)
        {
            if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Turret>() != null)
            {
                spawnableMapObject.numberToSpawn =
                    new AnimationCurve(new Keyframe(0f, 200f), new Keyframe(1f, 25f));
            }
        }
    }
}