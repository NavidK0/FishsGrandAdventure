using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class HoardingEvent : IGameEvent
{
    public string Description => "The Backstreets";
    public Color Color => Color.gray;
    public GameEventType GameEventType => GameEventType.Hoarding;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 999;
            }
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}