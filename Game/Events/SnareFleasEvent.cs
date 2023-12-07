using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class SnareFleasEvent : IGameEvent
{
    public string Description => "Snare Fleas and Friends!";
    public Color Color => Color.red;
    public GameEventType GameEventType => GameEventType.SnareFleas;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            spawnableEnemyWithRarity.rarity = 0;

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 999;
            }

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 999;
            }

            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
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