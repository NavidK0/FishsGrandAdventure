using System;
using System.Collections.Generic;
using System.Linq;
using FishsGrandAdventure.Utils;
using HarmonyLib;
using UnityEngine;

namespace FishsGrandAdventure.Network;

internal enum NetworkBroadcastDataType
{
    Unknown,
    BDInt,
    BDFloat,
    BDVector3,
    BDString,
    BDListString
}

/// <summary>
/// Networking solution to broadcast and receive data over the server. Use the delegates GetString, GetInt, GetFloat, and GetVector3 for receiving data. Note that the local player will not receive data that they broadcast.
/// <para>The second parameter for each of the events is the signature string.</para>
/// </summary>
public static class NetworkTransport
{
    public const string TransportPrefix = "NWT";

    /// <summary>
    /// Delegate for receiving a string value. Second parameter is the signature.
    /// <para/> (that signature would have been the signature given to <see cref="Broadcast(string, string)"/>, for example)
    /// </summary>
    public static Action<string, string> GetString = (_, _) => { };

    /// <summary>
    /// Delegate for receiving a list of string values. Second parameter is the signature.
    /// <para/> (that signature would have been the signature given to <see cref="Broadcast(string, string)"/>, for example)
    /// </summary>
    public static Action<List<string>, string> GetListString = (_, _) => { };

    /// <summary>
    /// Delegate for receiving a int value. Second parameter is the signature.
    /// <para/> (that signature would have been the signature given to <see cref="Broadcast(string, string)"/>, for example)
    /// </summary>
    public static Action<int, string> GetInt = (_, _) => { };

    /// <summary>
    /// Delegate for receiving a float value. Second parameter is the signature.
    /// <para/> (that signature would have been the signature given to <see cref="Broadcast(string, string)"/>, for example)
    /// </summary>
    public static Action<float, string> GetFloat = (_, _) => { };

    /// <summary>
    /// Delegate for receiving a Vector3 value. Second parameter is the signature.
    /// <para/> (that signature would have been the signature given to <see cref="Broadcast(string, string)"/>, for example)
    /// </summary>
    public static Action<Vector3, string> GetVector3 = (_, _) => { };

    /// <summary>
    /// Send data across the network. The signature is an identifier for use when receiving data.
    /// </summary>
    public static void Broadcast(string data, string signature)
    {
        if (data.Contains("/"))
        {
            Plugin.Log.LogError("Invalid character in broadcasted string event! ( / )");
            return;
        }

        HUDManager.Instance.AddTextToChatOnServer(
            $"<size=0>{TransportPrefix}/{data}/{signature}/{NetworkBroadcastDataType.BDString}/{GameNetworkManager.Instance.localPlayerController.playerClientId}/</size>");
    }

    /// <summary>
    /// Send data across the network. The signature is an identifier for use when receiving data.
    /// </summary>
    public static void Broadcast(List<string> data, string signature)
    {
        foreach (string item in data)
        {
            if (item.Contains("/"))
            {
                Plugin.Log.LogError("Invalid character in broadcasted string event! ( / )");
                return;
            }

            if (item.Contains("\n"))
            {
                Plugin.Log.LogError("Invalid character in broadcasted string event! ( NewLine )");
                return;
            }
        }

        HUDManager.Instance.AddTextToChatOnServer(
            $"<size=0>{TransportPrefix}/{data}/{signature}/{NetworkBroadcastDataType.BDListString}/{GameNetworkManager.Instance.localPlayerController.playerClientId}/</size>");
    }

    /// <summary>
    /// Send data across the network. The signature is an identifier for use when receiving data.
    /// </summary>
    public static void Broadcast(int data, string signature)
    {
        HUDManager.Instance.AddTextToChatOnServer(
            $"<size=0>{TransportPrefix}/{data}/{signature}/{NetworkBroadcastDataType.BDInt}/{GameNetworkManager.Instance.localPlayerController.playerClientId}/</size>");
    }

    /// <summary>
    /// Send data across the network. The signature is an identifier for use when receiving data.
    /// </summary>
    public static void Broadcast(float data, string signature)
    {
        HUDManager.Instance.AddTextToChatOnServer(
            $"<size=0>{TransportPrefix}/{data}/{signature}/{NetworkBroadcastDataType.BDFloat}/{GameNetworkManager.Instance.localPlayerController.playerClientId}/</size>");
    }

    /// <summary>
    /// Send data across the network. The signature is an identifier for use when receiving data.
    /// </summary>
    public static void Broadcast(Vector3 data, string signature)
    {
        HUDManager.Instance.AddTextToChatOnServer(
            $"<size=0>{TransportPrefix}/{data}/{signature}/{NetworkBroadcastDataType.BDVector3}/{GameNetworkManager.Instance.localPlayerController.playerClientId}/</size>");
    }
}

internal static class NetworkTransportPatch
{
    [HarmonyPatch(typeof(HUDManager), "AddChatMessage")]
    [HarmonyPrefix]
    internal static bool ChatInterpreter(HUDManager __instance, string chatMessage)
    {
        if (!chatMessage.Contains($"{NetworkTransport.TransportPrefix}") || !chatMessage.Contains("<size=0>"))
            return true;

        string[] dataFragments = chatMessage.Split('/');

        if (dataFragments.Length < 5)
        {
            if (dataFragments.Length >= 3)
            {
                bool success = int.TryParse(dataFragments[4], out int parsedPLayer);
                if (!success)
                {
                    Plugin.Log.LogWarning("Failed to parse player ID!!");
                    return false;
                }

                if (parsedPLayer == (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
                {
                    return false;
                }

                Enum.TryParse(dataFragments[3], out NetworkBroadcastDataType dataType);
                switch (dataType)
                {
                    case NetworkBroadcastDataType.BDString:
                        NetworkTransport.GetString(dataFragments[1], dataFragments[2]);
                        break;

                    case NetworkBroadcastDataType.BDInt:
                        NetworkTransport.GetInt(int.Parse(dataFragments[1]), dataFragments[2]);
                        break;

                    case NetworkBroadcastDataType.BDFloat:
                        NetworkTransport.GetFloat(float.Parse(dataFragments[1]), dataFragments[2]);
                        break;

                    case NetworkBroadcastDataType.BDVector3:
                        string[] components = dataFragments[1].Replace("(", "").Replace(")", "").Split(',');
                        var convertedString = new Vector3();
                        if (components.Length == 3)
                        {
                            if (float.TryParse(components[0], out float x) &&
                                float.TryParse(components[1], out float y) &&
                                float.TryParse(components[2], out float z))
                            {
                                convertedString.x = x;
                                convertedString.y = y;
                                convertedString.z = z;
                            }
                            else
                            {
                                Plugin.Log.LogError(
                                    "Vector3 Network receive fail. This is a failure of the API, and it should be reported as a bug.");
                            }
                        }
                        else
                        {
                            Plugin.Log.LogError(
                                "Vector3 Network receive fail. This is a failure of the API, and it should be reported as a bug.");
                        }

                        NetworkTransport.GetVector3(convertedString, dataFragments[2]);
                        break;

                    case NetworkBroadcastDataType.BDListString:
                        string[] items = dataFragments[1].Split('\n');
                        NetworkTransport.GetListString(items.ToList(), dataFragments[2]);
                        break;
                }

                return false;
            }

            Plugin.Log.LogError(
                "Generic Network receive fail. This is a failure of the API, and it should be reported as a bug.");

            Plugin.Log.LogError(
                $"Generic Network receive fail (expected 5+ data fragments, got {dataFragments.Length}). This is a failure of the API, and it should be reported as a bug.");
            return true;
        }

        if (!int.TryParse(dataFragments[4], out int parsedPlayer))
        {
            Plugin.Log.LogWarning($"Failed to parse player ID '{dataFragments[4]}'!!");
            return false;
        }

        if (parsedPlayer == (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            return false;
        }

        if (!Enum.TryParse(dataFragments[3], out NetworkBroadcastDataType type))
        {
            Plugin.Log.LogError($"Unknown datatype - unable to parse '{dataFragments[3]}' into a known data type!");
            return false;
        }

        switch (type)
        {
            case NetworkBroadcastDataType.BDString:
                NetworkTransport.GetString.InvokeActionSafe(dataFragments[1], dataFragments[2]);
                break;

            case NetworkBroadcastDataType.BDInt:
                NetworkTransport.GetInt.InvokeActionSafe(int.Parse(dataFragments[1]), dataFragments[2]);
                break;

            case NetworkBroadcastDataType.BDFloat:
                NetworkTransport.GetFloat.InvokeActionSafe(float.Parse(dataFragments[1]), dataFragments[2]);
                break;

            case NetworkBroadcastDataType.BDVector3:
                // technically creating garbage by creating a 2-long char arr every time this executes
                string vectorStr = dataFragments[1].Trim('(', ')');
                string[] components = vectorStr.Split(',');
                Vector3
                    convertedString =
                        default; // could also use Vector3.zero, but that copies memory instead of just initializing it to all 0's

                if (components.Length != 3)
                {
                    Plugin.Log.LogError(
                        $"Vector3 Network receive fail (expected 3 numbers, got {components.Length} number(?)(s) instead). This is a failure of the API, and it should be reported as a bug. (passing an empty Vector3 in its place)");
                }
                else
                {
                    if (float.TryParse(components[0], out float x) && float.TryParse(components[1], out float y) &&
                        float.TryParse(components[2], out float z))
                    {
                        convertedString.x = x;
                        convertedString.y = y;
                        convertedString.z = z;
                    }
                    else
                    {
                        Plugin.Log.LogError(
                            $"Vector3 Network receive fail (failed to parse '{vectorStr}' as numbers). This is a failure of the API, and it should be reported as a bug.");
                    }
                }

                NetworkTransport.GetVector3.InvokeActionSafe(convertedString, dataFragments[2]);
                break;
        }

        return false;
    }
}