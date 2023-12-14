using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class LandmineEvent : BaseGameEvent
{
    public override string Description => "Landmines Abound!";
    public override Color Color => Color.red;
    public override GameEventType GameEventType => GameEventType.Landmine;

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