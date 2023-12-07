using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class BrackenAndCoilEvent : IGameEvent
{
    public string Description => "Bracken and Coil";
    public Color Color => new Color(1f, 0f, 0.32f);
    public GameEventType GameEventType => GameEventType.BrackenAndCoil;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            spawnableEnemyWithRarity.rarity = 0;

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