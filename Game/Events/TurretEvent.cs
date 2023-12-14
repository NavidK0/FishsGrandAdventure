﻿using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class TurretEvent : BaseGameEvent
{
    public override string Description => "Turrets. Lots of Turrets.";
    public override Color Color => Color.red;

    public override GameEventType GameEventType => GameEventType.Turret;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableMapObject spawnableMapObject in level.spawnableMapObjects)
        {
            if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
            {
                spawnableMapObject.numberToSpawn =
                    new AnimationCurve(new Keyframe(0f, 175f), new Keyframe(1f, 150f));
            }
        }
    }
}