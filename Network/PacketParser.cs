using System;
using FishsGrandAdventure.Game;
using FishsGrandAdventure.Utils;
using GameNetcodeStuff;
using Newtonsoft.Json;

namespace FishsGrandAdventure.Network;

public static class PacketParser
{
    public static void Parse(string data)
    {
        Packet rawPacket = JsonConvert.DeserializeObject<Packet>(data, NetworkUtils.SerializerSettings);

        Plugin.Log.LogInfo(
            $"Received object: {JsonConvert.SerializeObject(rawPacket, NetworkUtils.SerializerSettings)}");

        switch (rawPacket)
        {
            case PacketGameEvent packet:
            {
                GameState.CurrentGameEvent =
                    GameEventManager.EnabledEvents.Find(e => e.GameEventType == packet.GameEventType);
                break;
            }
            case PacketPlayersBlazed:
            {
                ClientHelper.SetupBlazedPlayer();
                break;
            }
            case PacketSetPlayerMoveSpeed packet:
            {
                ClientHelper.SetPlayerMoveSpeed(packet.Speed);
                break;
            }
            case PacketSpawnExplosion packet:
            {
                ClientHelper.SpawnExplosion(packet.Position, packet.KillRange, packet.DamageRange);
                break;
            }
            case PacketStrikeLightning packet:
            {
                ClientHelper.StrikeLightning(packet.Position);
                break;
            }
            case PacketGlobalTimeSpeedMultiplier packet:
            {
                TimeOfDay.Instance.globalTimeSpeedMultiplier = packet.Multiplier;
                break;
            }
            case PacketDestroyEffects:
            {
                ClientHelper.DestroyEffects();
                break;
            }
            case PacketResetPlayerSpeed:
            {
                ClientHelper.ResetPlayerMoveSpeed();
                break;
            }
            case PacketGameTip packet:
            {
                HUDManager.Instance.DisplayTip(packet.Header, packet.Body);
                break;
            }
            case PacketSpawnEnemy packet:
            {
                if (!RoundManager.Instance.IsServer) break;

                PlayerControllerB playerControllerB =
                    StartOfRound.Instance.allPlayerScripts.Find(ps => ps.actualClientId == packet.ClientId);
                EnemyAI spawnedEnemy = ModUtils.SpawnEnemy(
                    packet.EnemyType,
                    RoundManager.Instance.currentLevel,
                    playerControllerB.transform.position,
                    playerControllerB.isInsideFactory
                );

                if (packet.ComponentsToAttach != null)
                {
                    foreach (Type componentType in packet.ComponentsToAttach)
                    {
                        spawnedEnemy.gameObject.AddComponent(componentType);
                    }
                }

                break;
            }
            case PacketSpawnEnemyOutside packet:
            {
                if (!RoundManager.Instance.IsServer) break;

                if (packet.Position.HasValue)
                {
                    EnemyAI spawnedEnemy = ModUtils.SpawnEnemyOutside(
                        packet.EnemyType,
                        StartOfRound.Instance.levels[packet.LevelId],
                        packet.ForceOutside,
                        packet.Position.Value
                    );

                    if (packet.ComponentsToAttach != null)
                    {
                        foreach (Type componentType in packet.ComponentsToAttach)
                        {
                            spawnedEnemy.gameObject.AddComponent(componentType);
                        }
                    }
                }
                else
                {
                    EnemyAI spawnedEnemy = ModUtils.SpawnEnemyOutside(
                        packet.EnemyType,
                        StartOfRound.Instance.levels[packet.LevelId],
                        packet.ForceOutside
                    );

                    if (packet.ComponentsToAttach != null)
                    {
                        foreach (Type componentType in packet.ComponentsToAttach)
                        {
                            spawnedEnemy.gameObject.AddComponent(componentType);
                        }
                    }
                }

                break;
            }
            case PacketSpawnEnemyInside packet:
            {
                if (!RoundManager.Instance.IsServer) break;

                if (packet.Position.HasValue)
                {
                    EnemyAI spawnedEnemy = ModUtils.SpawnEnemyInside(
                        packet.EnemyType,
                        StartOfRound.Instance.levels[packet.LevelId],
                        packet.ForceInside,
                        packet.Position.Value
                    );

                    if (packet.ComponentsToAttach != null)
                    {
                        foreach (Type componentType in packet.ComponentsToAttach)
                        {
                            spawnedEnemy.gameObject.AddComponent(componentType);
                        }
                    }
                }
                else
                {
                    EnemyAI spawnedEnemy = ModUtils.SpawnEnemyInside(
                        packet.EnemyType,
                        StartOfRound.Instance.levels[packet.LevelId],
                        packet.ForceInside
                    );

                    if (packet.ComponentsToAttach != null)
                    {
                        foreach (Type componentType in packet.ComponentsToAttach)
                        {
                            spawnedEnemy.gameObject.AddComponent(componentType);
                        }
                    }
                }

                break;
            }
        }
    }
}