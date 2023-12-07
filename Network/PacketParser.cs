using FishsGrandAdventure.Game;
using Newtonsoft.Json;

namespace FishsGrandAdventure.Network;

public static class PacketParser
{
    public static void Parse(ulong clientId, float receiveTime, string data)
    {
        Plugin.Log.LogInfo("Received data: " + data);
        Packet rawPacket = JsonConvert.DeserializeObject<Packet>(data, NetworkUtils.SerializerSettings);

        Plugin.Log.LogInfo($"Received type: {rawPacket.GetType()}");
        Plugin.Log.LogInfo(
            $"Received object: {JsonConvert.SerializeObject(rawPacket, NetworkUtils.SerializerSettings)}");

        switch (rawPacket)
        {
            case PacketGameEvent packet:
            {
                GameState.CurrentGameEventType = packet.GameEventType;
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
            case PacketResetPlayedSpeed:
            {
                ClientHelper.ResetPlayerMoveSpeed();
                break;
            }
        }
    }
}