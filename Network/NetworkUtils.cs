using System;
using FishsGrandAdventure.Utils.Converters;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace FishsGrandAdventure.Network;

public class NetworkUtils : MonoBehaviour
{
    public static NetworkManager NetworkManager => NetworkManager.Singleton;
    public static NetworkTransport Transport => NetworkManager.Singleton.NetworkConfig.NetworkTransport;

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

    private void Awake()
    {
        NetworkManager.Singleton
                .NetworkConfig
                .NetworkTransport
                .OnTransportEvent +=
            TransportEvent;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton
                    .NetworkConfig
                    .NetworkTransport
                    .OnTransportEvent -=
                TransportEvent;
        }
    }

    public static void BroadcastAll<T>(T packet) where T : Packet
    {
        Plugin.Log.LogInfo("Broadcasting data: " + packet.GetType());

        string json = JsonConvert.SerializeObject(packet, SerializerSettings);

        Plugin.Log.LogInfo("Broadcasting JSON: " + json);

        Plugin.Log.LogInfo("ClientId: " + NetworkManager.LocalClientId);
        Plugin.Log.LogInfo("ServerClientId: " + NetworkManager.ServerClientId);
        Plugin.Log.LogInfo("ConnectedClientsIds: " + NetworkManager.ConnectedClientsIds.Count);

        foreach (ulong id in NetworkManager.ConnectedClientsIds)
        {
            Plugin.Log.LogInfo("Sending to client: " + id);
            Send(id, packet);
        }
    }

    public static void Send<T>(ulong clientId, T packet) where T : Packet
    {
        string json = JsonConvert.SerializeObject(packet, SerializerSettings);
        var jsonByteArray = new ArraySegment<byte>(System.Text.Encoding.Default.GetBytes(json));

        if (clientId == NetworkManager.LocalClientId)
        {
            ParseTransportEvent(NetworkEvent.Data, 0, jsonByteArray, 0);
        }
        else
        {
            Transport.Send(clientId, jsonByteArray, NetworkDelivery.ReliableSequenced);
        }
    }

    private static void TransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload,
        float receiveTime)
    {
        ParseTransportEvent(eventType, clientId, payload, receiveTime);
    }

    private static void ParseTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload,
        float receiveTime)
    {
        if (eventType != NetworkEvent.Data) return;
        if (payload.Array == null) return;

        string data = System.Text.Encoding.Default.GetString(payload.Array, payload.Offset, payload.Count);

        Plugin.Log.LogInfo($"Received data: {data}");

        PacketParser.Parse(clientId, receiveTime, data);
    }
}