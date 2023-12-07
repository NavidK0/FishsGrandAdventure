using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class ChaosEvent : IGameEvent
{
    public string Description => "Chaos Theory";
    public Color Color => new Color(1f, 0f, 0.99f);
    public GameEventType GameEventType => GameEventType.Chaos;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            spawnableEnemyWithRarity.enemyType.probabilityCurve =
                new AnimationCurve(new Keyframe(0f, 1000f));
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}