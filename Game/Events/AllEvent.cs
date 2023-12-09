using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class AllEvent : IGameEvent
{
    public string Description => "Everything but the kitchen sink!";
    public Color Color => new Color(0.68f, 0f, 1f);
    public GameEventType GameEventType => GameEventType.All;

    public void OnServerInitialize(SelectableLevel level)
    {
    }

    public void OnBeforeModifyLevel(ref SelectableLevel level)
    {
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity8 in level.Enemies)
        {
            spawnableEnemyWithRarity8.enemyType.probabilityCurve =
                new AnimationCurve(new Keyframe(0f, 1000f));
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity9 in level.Enemies)
        {
            spawnableEnemyWithRarity9.rarity = 0;

            if (spawnableEnemyWithRarity9.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
            {
                spawnableEnemyWithRarity9.rarity = 100;
            }

            if (spawnableEnemyWithRarity9.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
            {
                spawnableEnemyWithRarity9.rarity = 100;
            }
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity10 in level.Enemies)
        {
            if (spawnableEnemyWithRarity10.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
            {
                spawnableEnemyWithRarity10.rarity = 100;
            }
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity11 in level.Enemies)
        {
            if (spawnableEnemyWithRarity11.enemyType.enemyPrefab.GetComponent<LassoManAI>() != null)
            {
                spawnableEnemyWithRarity11.rarity = 100;
            }
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity12 in level.Enemies)
        {
            if (spawnableEnemyWithRarity12.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
            {
                spawnableEnemyWithRarity12.rarity = 100;
            }
        }

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity13 in level.Enemies)
        {
            if (spawnableEnemyWithRarity13.enemyType.enemyPrefab.GetComponent<DressGirlAI>() != null)
            {
                spawnableEnemyWithRarity13.rarity = 1000;
            }
        }

        int itemCount = Random.Range(2, 9);
        for (var k = 0; k < itemCount; k++)
        {
            int randomItems = Random.Range(0, 3);
            Object.FindObjectOfType<Terminal>().orderedItemsFromTerminal.Add(randomItems);
        }

        Terminal terminal2 = Object.FindObjectOfType<Terminal>();
        int count = terminal2.orderedItemsFromTerminal.Count;
        terminal2.orderedItemsFromTerminal.Clear();
        for (var l = 0; l < count; l++)
        {
            terminal2.orderedItemsFromTerminal.Add(0);
        }
    }

    public void OnFinishGeneratingLevel()
    {
    }

    public void Cleanup()
    {
    }
}