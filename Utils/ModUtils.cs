using System.Collections.Generic;
using System.Linq;

namespace FishsGrandAdventure.Utils;

public static class ModUtils
{
    /**
     * Searches all levels for a spawnable enemy type.
     */
    public static SpawnableEnemyWithRarity FindSpawnableEnemy<T>() where T : EnemyAI
    {
        List<SpawnableEnemyWithRarity> allEnemies =
            StartOfRound.Instance.levels.SelectMany(l => l.Enemies).ToList();

        return allEnemies
            .FirstOrDefault(e => e.enemyType.enemyPrefab.GetComponent<T>() != null);
    }
}