using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class PsychosisEvent : IGameEvent
{
    public string Description => "It's not psychosis.";
    public Color Color => new Color(0.53f, 0f, 0.06f);
    public GameEventType GameEventType => GameEventType.Psychosis;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            DressGirlAI dressGirlAI =
                spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<DressGirlAI>();
            if (dressGirlAI != null)
            {
                spawnableEnemyWithRarity.rarity = 9999;
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