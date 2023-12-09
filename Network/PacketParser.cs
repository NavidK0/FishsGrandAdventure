using FishsGrandAdventure.Game;
using Newtonsoft.Json;

namespace FishsGrandAdventure.Network;

public static class PacketParser
{
    public static void Parse(string data)
    {
        Packet rawPacket = JsonConvert.DeserializeObject<Packet>(data, NetworkUtils.SerializerSettings);

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
        }
    }
}