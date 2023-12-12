using FishsGrandAdventure.Utils.Converters;
using Newtonsoft.Json;

namespace FishsGrandAdventure.Network;

public static class NetworkUtils
{
    private const string Signature = "FishsGrandAdventure.Packet";

    public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
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
            Plugin.Log.LogError("localPlayerController is null!");
            return;
        }

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