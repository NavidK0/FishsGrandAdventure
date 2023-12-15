using System;
using System.Collections.Generic;
using FishsGrandAdventure.Network;
using FishsGrandAdventure.Utils;
using UnityEngine;

namespace FishsGrandAdventure.Game.Events;

public class WendigoEvent : BaseGameEvent
{
    public override string Description => "The Wendigo";
    public override Color Color => new Color(0.2f, 0.07f, 0.09f);
    public override GameEventType GameEventType => GameEventType.Wendigo;

    public override void OnPreModifyLevel(ref SelectableLevel level)
    {
        ModUtils.AddSpecificEnemiesForEvent(level, new List<Type> { typeof(FlowermanAI) });

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.Enemies)
        {
            if (spawnableEnemyWithRarity.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
            {
                spawnableEnemyWithRarity.rarity = 100;
                spawnableEnemyWithRarity.enemyType.MaxCount = 4;
            }
        }
    }

    public override void OnPostFinishGeneratingLevel()
    {
        NetworkUtils.BroadcastAll(new PacketStopBoomboxes());
        NetworkUtils.BroadcastAll(new PacketPlayMusic
        {
            Name = "mc_cave_5"
        });
    }
}