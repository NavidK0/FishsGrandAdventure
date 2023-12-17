using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ChaosEvent : BaseGameEvent
{
    public override string Name => "Chaos Theory";
    public override string Description => "That's a lot of enemies.";
    public override Color Color => new Color(1f, 0f, 0.99f);
    public override GameEventType GameEventType => GameEventType.Chaos;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        float dangerLevel = GameEventManager.DangerLevels[level];
        level.enemySpawnChanceThroughoutDay =
            new AnimationCurve(new Keyframe(0f, 500f + dangerLevel));

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            spawnableEnemyWithRarity.enemyType.probabilityCurve =
                new AnimationCurve(new Keyframe(0f, 1000f));
        }
    }
}