using System.Collections.Generic;
using FishsGrandAdventure.Utils.Converters;
using Newtonsoft.Json;

namespace FishsGrandAdventure.Network;

public static class NetworkUtils
{
    private const string Signature = "FishsGrandAdventure.Packet";
    private static List<Packet> packetQueue = new List<Packet>();

    public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Include,
        Converters = new JsonConverter[]
        {
            new Vector2IntConverter(),
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector3IntConverter(),
            new Vector4Converter(),
            new ColorConverter(),
            new Color32Converter(),
            new QuaternionConverter(),
        }
    };

    public static void BroadcastAll<T>(T packet) where T : Packet
    {
        if (!RoundManager.Instance.IsServer) return;

        if (GameNetworkManager.Instance.localPlayerController == null)
        {
            Plugin.Log.LogInfo("localPlayerController has not been initialized yet! Storing packet when ready...");
            packetQueue.Add(packet);
            return;
        }

        if (packetQueue.Count > 0)
        {
            Plugin.Log.LogInfo($"Sending {packetQueue.Count} stored packets...");

            foreach (Packet p in packetQueue)
            {
                SendPacket(p);
            }

            packetQueue.Clear();
        }

        SendPacket(packet);
    }

    public static void SendPacket<T>(T packet)
    {
        string json = JsonConvert.SerializeObject(packet, SerializerSettings);

        Plugin.Log.LogInfo($"Sending packet: {json}");

        PacketParser.Parse(json);
        NetworkTransport.Broadcast(json, Signature);
    }

    public static void OnMessageReceived(string data, string signature)
    {
        if (signature == Signature)
        {
            PacketParser.Parse(data);
        }
    }
}